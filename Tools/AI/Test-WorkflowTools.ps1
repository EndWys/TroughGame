[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$previousScenario = $env:TROUGH_WORKFLOW_TEST_SCENARIO
$testRoot = Join-Path $env:TEMP ('trough-workflow-tests-' + [guid]::NewGuid().ToString('N'))
$fixtureRoot = Join-Path $testRoot 'project'
$fixtureTools = Join-Path $fixtureRoot 'Tools/AI'
New-Item -ItemType Directory -Path $fixtureTools -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'validate-unity.ps1') -Destination $fixtureTools

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
    Write-Output 'Workflow tool regression checks: passed (validation success/rejection/inconclusive/failure).'
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
