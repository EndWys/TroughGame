[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$previousScenario = $env:TROUGH_WORKFLOW_TEST_SCENARIO
$testRoot = Join-Path $env:TEMP ('trough-workflow-tests-' + [guid]::NewGuid().ToString('N'))
$fixtureRoot = Join-Path $testRoot 'project'
$fixtureTools = Join-Path $fixtureRoot 'Tools/AI'
$fixtureFeature = Join-Path $fixtureRoot '.ai/features/smoke-test'
$fixtureEvidence = Join-Path $fixtureRoot '.ai/evidence'
New-Item -ItemType Directory -Path $fixtureTools, $fixtureFeature, $fixtureEvidence -Force | Out-Null
Copy-Item -Path (Join-Path $PSScriptRoot '*.ps1') -Destination $fixtureTools

function Assert-True {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) { throw $Message }
}

function Invoke-Fixture {
    param([string]$Script, [string[]]$Arguments)
    $ErrorActionPreference = 'Continue' # Capture expected native stderr in negative cases.
    $output = & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $fixtureTools $Script) @Arguments 2>&1
    return [pscustomobject]@{ ExitCode = $LASTEXITCODE; Output = ($output -join "`n") }
}

try {
    $null = git -C $fixtureRoot init --quiet
    Assert-True ($LASTEXITCODE -eq 0) 'Could not initialize fixture repository.'
    Set-Content -LiteralPath (Join-Path $fixtureFeature 'state.yaml') -Encoding utf8 -Value @'
workflow_version: "1.3-tg.1"
feature: smoke-test
stage: requirements
status: in_progress
last_updated_at: "2026-10-12T00:00:00Z"
decisions: []
approvals: []
validation_evidence:
'@
    Set-Content -LiteralPath (Join-Path $fixtureFeature 'decisions.md') -Value '# Feature Decisions'
    $audit = Invoke-Fixture 'workflow-audit.ps1' @('-Feature', 'smoke-test')
    $auditResult = $audit.Output | ConvertFrom-Json
    Assert-True ($audit.ExitCode -eq 0 -and $auditResult.audit_status -eq 'complete') 'Valid state audit failed.'
    $missing = Invoke-Fixture 'workflow-audit.ps1' @('-Feature', 'missing-feature')
    $missingResult = $missing.Output | ConvertFrom-Json
    Assert-True ($missing.ExitCode -eq 6 -and $missingResult.summary.errors -gt 0) 'Missing state was not detected.'

    $runRoot = Join-Path $fixtureEvidence 'validation/smoke-run'
    $otherRoot = Join-Path $fixtureEvidence 'validation/other-run'
    New-Item -ItemType Directory -Path $runRoot, $otherRoot -Force | Out-Null
    Set-Content -LiteralPath (Join-Path $runRoot 'result.json') -Value '{}'
    Set-Content -LiteralPath (Join-Path $otherRoot 'result.json') -Value '{}'
    Add-Content -LiteralPath (Join-Path $fixtureFeature 'state.yaml') -Value @'
  - id: VAL-001
    result_path: .ai/evidence/validation/smoke-run/result.json
'@
    $preview = Invoke-Fixture 'cleanup-feature-evidence.ps1' @('-Feature', 'smoke-test', '-WhatIf')
    Assert-True ($preview.ExitCode -eq 0 -and (Test-Path -LiteralPath $runRoot)) 'Dry-run deleted evidence.'
    Assert-True ($preview.Output -match 'dry_run') 'Dry-run status was not reported.'
    $cleanup = Invoke-Fixture 'cleanup-feature-evidence.ps1' @('-Feature', 'smoke-test')
    Assert-True ($cleanup.ExitCode -eq 0 -and -not (Test-Path -LiteralPath $runRoot)) 'Scoped cleanup failed.'
    Assert-True (Test-Path -LiteralPath $otherRoot) 'Cleanup removed unrelated evidence.'
    $unsafe = Invoke-Fixture 'cleanup-feature-evidence.ps1' @('-Feature', 'smoke-test', '-EvidenceRoot', $fixtureRoot)
    Assert-True ($unsafe.ExitCode -ne 0 -and (Test-Path -LiteralPath $fixtureFeature)) 'Unsafe root was accepted.'
    Add-Content -LiteralPath (Join-Path $fixtureFeature 'state.yaml') -Value @'
  - id: VAL-002
    result_path: .ai/evidence/validation/result.json
'@
    $bucket = Invoke-Fixture 'cleanup-feature-evidence.ps1' @('-Feature', 'smoke-test')
    Assert-True ($bucket.Output -match 'Shared evidence bucket' -and (Test-Path -LiteralPath $otherRoot)) 'Shared evidence bucket was not protected.'

    $cliScript = Join-Path $testRoot 'fake-cli.ps1'
    Set-Content -LiteralPath $cliScript -Value @'
$command = $args[1]
$scenario = $env:TROUGH_WORKFLOW_TEST_SCENARIO
$success = -not ($command -eq 'recompile' -and $scenario -eq 'reject')
$payload = switch ($command) {
    'editor_status' { @{ status = 'ready' } }
    'recompile_status' { @{ status = 'completed'; failed = $false; compilationFailed = $false } }
    'test_status' {
        if ($scenario -eq 'empty') { @{ status = 'completed' } }
        elseif ($scenario -eq 'failed') { @{ status = 'completed'; summary = @{ passed = 1; failed = 1 } } }
        else { @{ status = 'completed'; summary = @{ passed = 2; failed = 0 } } }
    }
    default { @{ status = 'accepted' } }
}
@{ success = $success; data = @{ result = ($payload | ConvertTo-Json -Compress -Depth 5) }; errors = @() } | ConvertTo-Json -Depth 8
exit 0
'@
    $fakeCli = Join-Path $testRoot 'fake-cli.cmd'
    Set-Content -LiteralPath $fakeCli -Value ('@powershell -NoProfile -ExecutionPolicy Bypass -File "' + $cliScript + '" %*') -Encoding ascii
    foreach ($case in @(
        @{ Scenario = 'pass'; Status = 'passed'; Exit = 0 },
        @{ Scenario = 'reject'; Status = 'not_completed'; Exit = 6 },
        @{ Scenario = 'empty'; Status = 'not_completed'; Exit = 6 },
        @{ Scenario = 'failed'; Status = 'failed'; Exit = 8 }
    )) {
        $env:TROUGH_WORKFLOW_TEST_SCENARIO = $case.Scenario
        $validation = Invoke-Fixture 'validate-unity.ps1' @('-Mode', 'EditMode', '-UnityCliPath', $fakeCli, '-TimeoutSeconds', '5')
        try { $result = $validation.Output | ConvertFrom-Json }
        catch { throw "Invalid validation JSON for $($case.Scenario) (exit $($validation.ExitCode)): $($validation.Output)" }
        Assert-True ($validation.ExitCode -eq $case.Exit -and $result.status -eq $case.Status) "Validation scenario failed: $($case.Scenario)"
    }
    Write-Output 'Workflow tool regression checks: passed (audit, cleanup boundaries/dry-run, validation success/rejection/inconclusive/failure).'
}
finally {
    if ($null -eq $previousScenario) { Remove-Item Env:TROUGH_WORKFLOW_TEST_SCENARIO -ErrorAction SilentlyContinue }
    else { $env:TROUGH_WORKFLOW_TEST_SCENARIO = $previousScenario }
    $resolvedTestRoot = [IO.Path]::GetFullPath($testRoot)
    $resolvedTemp = [IO.Path]::GetFullPath($env:TEMP).TrimEnd('\') + '\'
    if ($resolvedTestRoot.StartsWith($resolvedTemp, [StringComparison]::OrdinalIgnoreCase) -and (Split-Path $resolvedTestRoot -Leaf) -like 'trough-workflow-tests-*') {
        Remove-Item -LiteralPath $resolvedTestRoot -Recurse -Force
    }
}
