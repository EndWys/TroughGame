[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^[a-z0-9]+(?:-[a-z0-9]+)*$')]
    [string]$Feature,

    [string]$FeaturesRoot = '.ai/features',

    [string]$OutputDirectory = '.ai/evidence/workflow-audit'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$projectRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))

function Resolve-ProjectPath {
    param([string]$Path)

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $projectRoot $Path))
}

function Get-YamlScalar {
    param(
        [string[]]$Lines,
        [string]$Key
    )

    $escapedKey = [regex]::Escape($Key)
    $match = $Lines | Where-Object { $_ -match "^${escapedKey}:\s*(?<value>[^#]*?)\s*(?:#.*)?$" } | Select-Object -First 1
    if ($null -eq $match) {
        return $null
    }

    $value = ([regex]::Match($match, "^${escapedKey}:\s*(?<value>[^#]*?)\s*(?:#.*)?$")).Groups['value'].Value.Trim()
    if ([string]::IsNullOrWhiteSpace($value) -or $value -eq 'null' -or $value -eq '[]') {
        return $null
    }

    return $value.Trim([char[]]@([char]39, [char]34))
}

function Get-WorkflowIds {
    param(
        [string[]]$Lines,
        [string]$Prefix
    )

    $escapedPrefix = [regex]::Escape($Prefix)
    $pattern = '^\s*-\s+id:\s*(' + $escapedPrefix + '-\d+)\b'
    return @($Lines | ForEach-Object {
        $match = [regex]::Match($_, $pattern)
        if ($match.Success) { $match.Groups[1].Value }
    })
}

function Add-Finding {
    param(
        [System.Collections.Generic.List[object]]$Findings,
        [string]$Id,
        [ValidateSet('info', 'warning', 'error')]
        [string]$Severity,
        [string]$Category,
        [string]$Summary,
        [string]$Evidence,
        [string]$Recommendation
    )

    $Findings.Add([pscustomobject][ordered]@{
        id = $Id
        severity = $Severity
        category = $Category
        summary = $Summary
        evidence = $Evidence
        recommendation = $Recommendation
    })
}

function ConvertTo-MarkdownCell {
    param([AllowNull()][object]$Value)

    if ($null -eq $Value) { return '-' }
    return ([string]$Value).Replace('|', '\|').Replace("`r", ' ').Replace("`n", '<br>')
}

function Get-GitSnapshot {
    $snapshot = [ordered]@{
        available = $false
        staged_paths = @()
        unstaged_paths = @()
        untracked_paths = @()
        error = $null
    }

    try {
        $porcelain = @(git -C $projectRoot status --porcelain=v1 2>&1)
        if ($LASTEXITCODE -ne 0) {
            $snapshot.error = ($porcelain -join "\n")
            return [pscustomobject]$snapshot
        }

        $snapshot.available = $true
        foreach ($line in $porcelain) {
            if ($line.Length -lt 4) { continue }
            $indexStatus = $line.Substring(0, 1)
            $workTreeStatus = $line.Substring(1, 1)
            $path = $line.Substring(3)

            if ($indexStatus -eq '?' -and $workTreeStatus -eq '?') {
                $snapshot.untracked_paths += $path
                continue
            }

            if ($indexStatus -ne ' ') { $snapshot.staged_paths += $path }
            if ($workTreeStatus -ne ' ') { $snapshot.unstaged_paths += $path }
        }
    }
    catch {
        $snapshot.error = $_.Exception.Message
    }

    return [pscustomobject]$snapshot
}

$featureRoot = Join-Path (Resolve-ProjectPath $FeaturesRoot) $Feature
$statePath = Join-Path $featureRoot 'state.yaml'
$decisionsPath = Join-Path $featureRoot 'decisions.md'
$runId = (Get-Date -Format 'yyyyMMdd-HHmmss-fff') + '-' + [guid]::NewGuid().ToString('N').Substring(0, 8)
$runDirectory = Join-Path (Join-Path (Resolve-ProjectPath $OutputDirectory) $Feature) $runId
New-Item -ItemType Directory -Path $runDirectory -Force | Out-Null
$resultPath = Join-Path $runDirectory 'result.json'
$reportPath = Join-Path $runDirectory 'report.md'

$findings = [System.Collections.Generic.List[object]]::new()
$stateLines = @()
$decisionText = ''
$stateDecisionIds = @()
$stateValidationIds = @()
$stateApprovalIds = @()

if (-not (Test-Path -LiteralPath $featureRoot -PathType Container)) {
    Add-Finding -Findings $findings -Id 'AUD-001' -Severity 'error' -Category 'feature-state' -Summary 'Feature directory is missing.' -Evidence $featureRoot -Recommendation 'Create the feature state from .ai/features/_template before running the workflow audit.'
}

if (-not (Test-Path -LiteralPath $statePath -PathType Leaf)) {
    Add-Finding -Findings $findings -Id 'AUD-002' -Severity 'error' -Category 'feature-state' -Summary 'state.yaml is missing.' -Evidence $statePath -Recommendation 'Restore or create the feature state before relying on this audit.'
}
else {
    $stateLines = @(Get-Content -LiteralPath $statePath)
    $recordedFeature = Get-YamlScalar -Lines $stateLines -Key 'feature'
    $stage = Get-YamlScalar -Lines $stateLines -Key 'stage'
    $workflowStatus = Get-YamlScalar -Lines $stateLines -Key 'status'
    $lastUpdatedAt = Get-YamlScalar -Lines $stateLines -Key 'last_updated_at'
    $workflowVersion = Get-YamlScalar -Lines $stateLines -Key 'workflow_version'
    if ($workflowVersion -ne '1.4-tg.1') {
        Add-Finding -Findings $findings -Id 'AUD-VERSION' -Severity 'warning' -Category 'workflow-version' -Summary 'Workflow version differs from the installed TroughGame workflow.' -Evidence "Recorded: $workflowVersion; installed: 1.4-tg.1" -Recommendation 'Review gate/state compatibility before resuming; do not silently migrate existing state.'
    }

    if ($recordedFeature -ne $Feature) {
        Add-Finding -Findings $findings -Id 'AUD-003' -Severity 'warning' -Category 'feature-state' -Summary 'State feature identifier does not match the audited feature.' -Evidence "Requested: $Feature; state: $recordedFeature" -Recommendation 'Confirm the intended feature directory and correct the identifier only with a developer decision.'
    }

    if ([string]::IsNullOrWhiteSpace($stage) -or [string]::IsNullOrWhiteSpace($workflowStatus)) {
        Add-Finding -Findings $findings -Id 'AUD-004' -Severity 'warning' -Category 'feature-state' -Summary 'Current workflow stage or status is not recorded.' -Evidence $statePath -Recommendation 'Record the current factual state before continuing the feature.'
    }

    if ([string]::IsNullOrWhiteSpace($lastUpdatedAt)) {
        Add-Finding -Findings $findings -Id 'AUD-005' -Severity 'info' -Category 'state-freshness' -Summary 'state.yaml has no last_updated_at value.' -Evidence $statePath -Recommendation 'Use this field on future state updates so audits can show the freshness of workflow data.'
    }

    $placeholders = @($stateLines | Where-Object { $_ -notmatch '^\s*#' -and $_ -match '<feature-kebab-case>|<short title>|<[^>]+>' })
    if ($placeholders.Count -gt 0) {
        Add-Finding -Findings $findings -Id 'AUD-006' -Severity 'warning' -Category 'state-quality' -Summary 'Template placeholders remain in state.yaml.' -Evidence ($placeholders -join '; ') -Recommendation 'Replace placeholders or remove unused template records before relying on the state.'
    }

    $stateDecisionIds = @(Get-WorkflowIds -Lines $stateLines -Prefix 'DEC')
    $stateValidationIds = @(Get-WorkflowIds -Lines $stateLines -Prefix 'VAL')
    $stateApprovalIds = @(Get-WorkflowIds -Lines $stateLines -Prefix 'APR')
    $resultPaths = @($stateLines | ForEach-Object {
        $match = [regex]::Match($_, '^\s*result_path:\s*(?<path>[^#]*?)\s*(?:#.*)?$')
        if ($match.Success) {
            $candidate = $match.Groups['path'].Value.Trim().Trim([char[]]@([char]39, [char]34))
            if (-not [string]::IsNullOrWhiteSpace($candidate) -and $candidate -ne 'null') { $candidate }
        }
    })

    foreach ($evidencePath in $resultPaths) {
        $resolvedEvidencePath = Resolve-ProjectPath $evidencePath
        if (-not (Test-Path -LiteralPath $resolvedEvidencePath -PathType Leaf)) {
            Add-Finding -Findings $findings -Id "AUD-EVIDENCE-$($findings.Count + 1)" -Severity 'warning' -Category 'validation-evidence' -Summary 'A validation result path recorded in state does not exist.' -Evidence $evidencePath -Recommendation 'Record a valid evidence path or mark the validation result accurately as unavailable/not run.'
        }
    }
}

if (-not (Test-Path -LiteralPath $decisionsPath -PathType Leaf)) {
    Add-Finding -Findings $findings -Id 'AUD-007' -Severity 'warning' -Category 'decisions' -Summary 'decisions.md is missing.' -Evidence $decisionsPath -Recommendation 'Restore or create the paired decision log before recording long-form decisions.'
}
else {
    $decisionText = Get-Content -LiteralPath $decisionsPath -Raw
    $documentDecisionIds = @([regex]::Matches($decisionText, '(?m)^###\s+(DEC-\d+)\b') | ForEach-Object { $_.Groups[1].Value })
    foreach ($decisionId in $stateDecisionIds) {
        if ($documentDecisionIds -notcontains $decisionId) {
            Add-Finding -Findings $findings -Id "AUD-DECISION-$decisionId" -Severity 'warning' -Category 'decisions' -Summary 'A decision indexed in state.yaml has no matching decision record.' -Evidence $decisionId -Recommendation 'Add the long-form decision record or remove the stale index entry.'
        }
    }
    foreach ($decisionId in $documentDecisionIds) {
        if ($stateDecisionIds -notcontains $decisionId) {
            Add-Finding -Findings $findings -Id "AUD-DECISION-$decisionId" -Severity 'warning' -Category 'decisions' -Summary 'A decision record is not indexed in state.yaml.' -Evidence $decisionId -Recommendation 'Add the compact decision index entry to state.yaml.'
        }
    }
}

if ($stateValidationIds.Count -eq 0) {
    Add-Finding -Findings $findings -Id 'AUD-008' -Severity 'info' -Category 'validation-evidence' -Summary 'No validation evidence records are indexed in state.yaml.' -Evidence $statePath -Recommendation 'This is expected before implementation; after production changes, record each applicable validation result.'
}

if ($stateApprovalIds.Count -eq 0) {
    Add-Finding -Findings $findings -Id 'AUD-009' -Severity 'info' -Category 'approvals' -Summary 'No approval records are indexed in state.yaml.' -Evidence $statePath -Recommendation 'This is expected before the first gate; record explicit approvals as the workflow progresses.'
}

$gitSnapshot = Get-GitSnapshot
if (-not $gitSnapshot.available) {
    Add-Finding -Findings $findings -Id 'AUD-010' -Severity 'warning' -Category 'git' -Summary 'Git working-tree status could not be collected.' -Evidence $gitSnapshot.error -Recommendation 'Run the normal Git reconciliation before any write stage.'
}
elseif (($gitSnapshot.staged_paths.Count + $gitSnapshot.unstaged_paths.Count + $gitSnapshot.untracked_paths.Count) -gt 0) {
    Add-Finding -Findings $findings -Id 'AUD-011' -Severity 'info' -Category 'git' -Summary 'The working tree contains changes at audit time.' -Evidence "Staged: $($gitSnapshot.staged_paths.Count); unstaged: $($gitSnapshot.unstaged_paths.Count); untracked: $($gitSnapshot.untracked_paths.Count)" -Recommendation 'Use the feature Git-reconciliation protocol before the next write or commit.'
}

$errorCount = @($findings | Where-Object { $_.severity -eq 'error' }).Count
$warningCount = @($findings | Where-Object { $_.severity -eq 'warning' }).Count
$infoCount = @($findings | Where-Object { $_.severity -eq 'info' }).Count
$auditStatus = if (($errorCount + $warningCount) -gt 0) { 'attention_required' } else { 'complete' }

$result = [ordered]@{
    schema_version = 1
    audit_status = $auditStatus
    feature = $Feature
    project_path = $projectRoot
    feature_state = [ordered]@{
        directory = $featureRoot
        state_path = $statePath
        decisions_path = $decisionsPath
        stage = if ($null -ne $stateLines) { Get-YamlScalar -Lines $stateLines -Key 'stage' } else { $null }
        status = if ($null -ne $stateLines) { Get-YamlScalar -Lines $stateLines -Key 'status' } else { $null }
        approval_count = $stateApprovalIds.Count
        decision_count = $stateDecisionIds.Count
        validation_evidence_count = $stateValidationIds.Count
    }
    git = $gitSnapshot
    summary = [ordered]@{
        errors = $errorCount
        warnings = $warningCount
        info = $infoCount
    }
    findings = @($findings)
    evidence = [ordered]@{
        directory = $runDirectory
        result = $resultPath
        report = $reportPath
    }
    recorded_at = (Get-Date).ToUniversalTime().ToString('o')
}

$markdown = @(
    "# Workflow Audit - $Feature",
    '',
    '> Read-only workflow evidence report. Findings are not approvals, implementation instructions or automatic process changes.',
    '',
    '## Summary',
    '',
    '| Status | Errors | Warnings | Info | Stage | Workflow status |',
    '| --- | ---: | ---: | ---: | --- | --- |',
    "| $auditStatus | $errorCount | $warningCount | $infoCount | $(ConvertTo-MarkdownCell $result.feature_state.stage) | $(ConvertTo-MarkdownCell $result.feature_state.status) |",
    '',
    '## Observed Git Context',
    '',
    '| Staged | Unstaged | Untracked |',
    '| ---: | ---: | ---: |',
    "| $($gitSnapshot.staged_paths.Count) | $($gitSnapshot.unstaged_paths.Count) | $($gitSnapshot.untracked_paths.Count) |",
    '',
    '## Findings',
    ''
)

if ($findings.Count -eq 0) {
    $markdown += 'No structural workflow findings were detected by this minimal audit.'
}
else {
    $markdown += '| ID | Severity | Category | Finding | Evidence | Suggested follow-up |'
    $markdown += '| --- | --- | --- | --- | --- | --- |'
    foreach ($finding in $findings) {
        $markdown += "| $($finding.id) | $($finding.severity) | $($finding.category) | $(ConvertTo-MarkdownCell $finding.summary) | $(ConvertTo-MarkdownCell $finding.evidence) | $(ConvertTo-MarkdownCell $finding.recommendation) |"
    }
}

$markdown += @(
    '',
    '## Boundary',
    '',
    'This audit does not validate Unity code, run tests, infer approvals, restore Git data or modify workflow state. Review its findings during the retrospective and make persistent workflow changes only after explicit developer approval.'
)

Set-Content -LiteralPath $reportPath -Value ($markdown -join [Environment]::NewLine) -Encoding utf8
Set-Content -LiteralPath $resultPath -Value ($result | ConvertTo-Json -Depth 8) -Encoding utf8
Write-Output ($result | ConvertTo-Json -Depth 8)
if ($errorCount -gt 0) { exit 6 }
