[CmdletBinding(SupportsShouldProcess, ConfirmImpact = 'Low')]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^[a-z0-9]+(?:-[a-z0-9]+)*$')]
    [string]$Feature,

    [string]$FeaturesRoot = '.ai/features',

    [string]$EvidenceRoot = '.ai/evidence'
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

function Test-PathWithinRoot {
    param(
        [string]$Path,
        [string]$Root
    )

    $normalizedPath = [System.IO.Path]::GetFullPath($Path).TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)
    $normalizedRoot = [System.IO.Path]::GetFullPath($Root).TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)
    return $normalizedPath.StartsWith($normalizedRoot + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)
}

function Test-ReparseTraversal {
    param([string]$Path)

    $cursor = [System.IO.Path]::GetFullPath($Path)
    while (-not [string]::IsNullOrWhiteSpace($cursor)) {
        if (Test-Path -LiteralPath $cursor) {
            $item = Get-Item -LiteralPath $cursor -Force
            if (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) { return $true }
        }
        $cursor = Split-Path -Parent $cursor
    }
    if (Test-Path -LiteralPath $Path -PathType Container) {
        $links = @(Get-ChildItem -LiteralPath $Path -Recurse -Force | Where-Object {
            ($_.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0
        })
        if ($links.Count -gt 0) { return $true }
    }
    return $false
}

$featureRoot = Join-Path (Resolve-ProjectPath $FeaturesRoot) $Feature
$statePath = Join-Path $featureRoot 'state.yaml'
$resolvedEvidenceRoot = Resolve-ProjectPath $EvidenceRoot
$projectEvidenceRoot = Resolve-ProjectPath '.ai/evidence'
if ($resolvedEvidenceRoot -ne $projectEvidenceRoot -and -not (Test-PathWithinRoot -Path $resolvedEvidenceRoot -Root $projectEvidenceRoot)) {
    throw 'EvidenceRoot must be .ai/evidence or a directory within it.'
}
if (Test-ReparseTraversal -Path $resolvedEvidenceRoot) {
    throw 'Evidence cleanup refuses symbolic links or junctions.'
}
$candidates = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
$skipped = [System.Collections.Generic.List[string]]::new()
$removed = [System.Collections.Generic.List[string]]::new()
$errors = [System.Collections.Generic.List[string]]::new()

if (-not (Test-Path -LiteralPath $statePath -PathType Leaf)) {
    $result = [ordered]@{
        schema_version = 1
        status = 'not_completed'
        feature = $Feature
        message = 'Feature state.yaml is required to scope evidence cleanup.'
        removed_directories = @()
        skipped_paths = @()
        errors = @($statePath)
        recorded_at = (Get-Date).ToUniversalTime().ToString('o')
    }
    Write-Output ($result | ConvertTo-Json -Depth 5)
    exit 4
}

$stateLines = @(Get-Content -LiteralPath $statePath)
foreach ($line in $stateLines) {
    $match = [regex]::Match($line, '^\s*(?:result_path|report_path):\s*(?<path>[^#]*?)\s*(?:#.*)?$')
    if (-not $match.Success) { continue }

    $recordedPath = $match.Groups['path'].Value.Trim().Trim([char[]]@([char]39, [char]34))
    if ([string]::IsNullOrWhiteSpace($recordedPath) -or $recordedPath -eq 'null') { continue }
    $resolvedPath = Resolve-ProjectPath $recordedPath
    if (-not (Test-PathWithinRoot -Path $resolvedPath -Root $resolvedEvidenceRoot)) {
        $skipped.Add("Outside evidence root: $recordedPath")
        continue
    }

    $candidateDirectory = Split-Path -Parent $resolvedPath
    $relativeDirectory = $candidateDirectory.Substring($resolvedEvidenceRoot.TrimEnd('\', '/').Length).TrimStart('\', '/')
    $segments = @($relativeDirectory -split '[\\/]')
    if ($segments.Count -lt 2 -or ($segments[0] -eq 'workflow-audit' -and $segments[1] -ne $Feature)) {
        $skipped.Add("Shared evidence bucket or another feature: $recordedPath")
        continue
    }
    $candidates.Add($candidateDirectory) | Out-Null
}

$featureAuditRoot = Join-Path (Join-Path $resolvedEvidenceRoot 'workflow-audit') $Feature
if (Test-Path -LiteralPath $featureAuditRoot -PathType Container) {
    $candidates.Add($featureAuditRoot) | Out-Null
}

$scopedCandidates = [System.Collections.Generic.List[string]]::new()
foreach ($candidate in $candidates) {
    $hasCandidateParent = $false
    foreach ($otherCandidate in $candidates) {
        if ($candidate -eq $otherCandidate) { continue }
        if (Test-PathWithinRoot -Path $candidate -Root $otherCandidate) {
            $hasCandidateParent = $true
            break
        }
    }
    if (-not $hasCandidateParent) { $scopedCandidates.Add($candidate) }
}

foreach ($candidate in $scopedCandidates | Sort-Object) {
    if (-not (Test-PathWithinRoot -Path $candidate -Root $resolvedEvidenceRoot)) {
        $skipped.Add("Outside evidence root: $candidate")
        continue
    }
    if (-not (Test-Path -LiteralPath $candidate -PathType Container)) {
        $skipped.Add("Already absent: $candidate")
        continue
    }

    if (Test-ReparseTraversal -Path $candidate) {
        $errors.Add("Reparse-point traversal refused: $candidate")
        continue
    }

    try {
        if ($PSCmdlet.ShouldProcess($candidate, 'Remove temporary workflow evidence directory')) {
            Remove-Item -LiteralPath $candidate -Recurse -Force
            $removed.Add($candidate)
        }
    }
    catch {
        $errors.Add("${candidate}: $($_.Exception.Message)")
    }
}

$status = if ($WhatIfPreference) { 'dry_run' } elseif ($errors.Count -gt 0 -or $skipped.Count -gt 0) { 'partial' } else { 'complete' }
$result = [ordered]@{
    schema_version = 1
    status = $status
    feature = $Feature
    evidence_root = $resolvedEvidenceRoot
    removed_directories = @($removed)
    skipped_paths = @($skipped)
    errors = @($errors)
    recorded_at = (Get-Date).ToUniversalTime().ToString('o')
}
Write-Output ($result | ConvertTo-Json -Depth 5)
if ($errors.Count -gt 0) { exit 6 }
