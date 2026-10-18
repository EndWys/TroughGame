[CmdletBinding()]
param(
    [ValidateSet('Relevant', 'Compile', 'EditMode', 'PlayMode', 'Diagnose')]
    [string]$Mode = 'Relevant',

    [string]$Filter,

    [string[]]$ChangedFiles = @(),

    [string]$OutputDirectory = '.ai/evidence/validation',

    [ValidateRange(1, 3600)]
    [int]$TimeoutSeconds = 600,

    [string]$UnityCliPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$projectRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$resolvedOutputDirectory = if ([System.IO.Path]::IsPathRooted($OutputDirectory)) {
    $OutputDirectory
}
else {
    Join-Path $projectRoot $OutputDirectory
}

$runId = (Get-Date -Format 'yyyyMMdd-HHmmss-fff') + '-' + [guid]::NewGuid().ToString('N').Substring(0, 8)
$runDirectory = Join-Path $resolvedOutputDirectory "$runId-$Mode"
New-Item -ItemType Directory -Path $runDirectory -Force | Out-Null
$resultPath = Join-Path $runDirectory 'result.json'
$script:resolvedUnityCliPath = $null
$script:commandRecords = [System.Collections.Generic.List[object]]::new()

function Write-ValidationResult {
    param(
        [ValidateSet('available', 'passed', 'failed', 'not_available', 'editor_unavailable', 'not_completed')]
        [string]$Status,
        [int]$ExitCode,
        [string]$Message,
        [string]$ResolvedMode = $Mode,
        [AllowNull()][object]$Details = $null
    )

    $result = [ordered]@{
        schema_version = 2
        status = $Status
        requested_mode = $Mode
        resolved_mode = $ResolvedMode
        validation_backend = 'connected_editor_pipeline'
        project_path = $projectRoot
        unity_cli_path = $script:resolvedUnityCliPath
        changed_files = @($ChangedFiles)
        exit_code = $ExitCode
        message = $Message
        details = $Details
        commands = @($script:commandRecords)
        evidence = [ordered]@{
            directory = $runDirectory
            result = $resultPath
        }
        recorded_at = (Get-Date).ToUniversalTime().ToString('o')
    }

    $json = $result | ConvertTo-Json -Depth 10
    Set-Content -LiteralPath $resultPath -Value $json -Encoding utf8
    Write-Output $json
    exit $ExitCode
}

function Resolve-UnityCliPath {
    $candidate = if (-not [string]::IsNullOrWhiteSpace($UnityCliPath)) {
        $UnityCliPath
    }
    elseif (-not [string]::IsNullOrWhiteSpace($env:TROUGHGAME_UNITY_CLI_PATH)) {
        $env:TROUGHGAME_UNITY_CLI_PATH
    }
    else {
        $command = Get-Command unity -ErrorAction SilentlyContinue
        if ($null -ne $command) {
            $command.Source
        }
        else {
            $defaultWindowsCli = if ($env:LOCALAPPDATA) { Join-Path $env:LOCALAPPDATA 'Unity\bin\unity.exe' } else { $null }
            if ($defaultWindowsCli -and (Test-Path -LiteralPath $defaultWindowsCli -PathType Leaf)) { $defaultWindowsCli } else { $null }
        }
    }

    if ([string]::IsNullOrWhiteSpace($candidate)) { return $null }
    if (-not (Test-Path -LiteralPath $candidate -PathType Leaf)) {
        throw "Configured Unity CLI executable was not found: $candidate"
    }

    return [System.IO.Path]::GetFullPath($candidate)
}

function Read-JsonFile {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { return $null }
    try { return (Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json) }
    catch { return $null }
}

function Get-PropertyValue {
    param([AllowNull()][object]$Object, [string]$Name)

    if ($null -eq $Object) { return $null }
    $property = $Object.PSObject.Properties | Where-Object { $_.Name -eq $Name } | Select-Object -First 1
    if ($null -eq $property) { return $null }
    return $property.Value
}

function Get-PipelinePayload {
    param([AllowNull()][object]$Envelope)

    $rawResult = Get-PropertyValue -Object (Get-PropertyValue -Object $Envelope -Name 'data') -Name 'result'
    if ($null -eq $rawResult) { return $null }
    if ($rawResult -isnot [string]) { return $rawResult }
    if ([string]::IsNullOrWhiteSpace($rawResult)) { return $null }
    try { return ($rawResult | ConvertFrom-Json) }
    catch { return $null }
}

function Invoke-UnityCli {
    param([string]$Label, [string[]]$Arguments)

    $sequence = '{0:D2}' -f ($script:commandRecords.Count + 1)
    $stdoutPath = Join-Path $runDirectory "$sequence-$Label.stdout.json"
    $stderrPath = Join-Path $runDirectory "$sequence-$Label.stderr.log"
    & $script:resolvedUnityCliPath @Arguments 1> $stdoutPath 2> $stderrPath
    $exitCode = $LASTEXITCODE
    $record = [pscustomobject][ordered]@{
        label = $Label
        command = @($script:resolvedUnityCliPath) + @($Arguments)
        exit_code = $exitCode
        stdout_path = $stdoutPath
        stderr_path = $stderrPath
    }
    $script:commandRecords.Add($record)
    $envelope = Read-JsonFile -Path $stdoutPath
    if ($exitCode -eq 0 -and (Get-PropertyValue -Object $envelope -Name 'success') -ne $true) {
        $exitCode = 6
    }
    return [pscustomobject]@{ ExitCode = $exitCode; Envelope = $envelope }
}

function Invoke-PipelineCommand {
    param([string]$Label, [string[]]$CommandArguments)

    $arguments = @('command') + $CommandArguments + @('--project-path', $projectRoot, '--caller', 'plugin', '--skill', 'unity-feature-implementation-v2', '--format', 'json', '--no-banner', '--no-pager')
    return Invoke-UnityCli -Label $Label -Arguments $arguments
}

function Get-FirstCliError {
    param([AllowNull()][object]$Envelope)

    $errors = Get-PropertyValue -Object $Envelope -Name 'errors'
    if ($null -eq $errors -or @($errors).Count -eq 0) { return $null }
    return @($errors)[0]
}

function Wait-ForRecompile {
    param([datetime]$Deadline)

    do {
        Start-Sleep -Seconds 1
        $statusCommand = Invoke-PipelineCommand -Label 'recompile-status' -CommandArguments @('recompile_status')
        if ($statusCommand.ExitCode -ne 0) { continue }

        $payload = Get-PipelinePayload -Envelope $statusCommand.Envelope
        $statusValue = [string](Get-PropertyValue -Object $payload -Name 'status')
        if ((Get-PropertyValue -Object $payload -Name 'failed') -eq $true -or (Get-PropertyValue -Object $payload -Name 'compilationFailed') -eq $true -or $statusValue -eq 'failed') {
            return [pscustomobject]@{ Outcome = 'failed'; Payload = $payload }
        }
        if ($statusValue -in @('completed', 'up_to_date', 'idle')) {
            return [pscustomobject]@{ Outcome = 'passed'; Payload = $payload }
        }
    }
    while ((Get-Date) -lt $Deadline)

    return [pscustomobject]@{ Outcome = 'timeout'; Payload = $null }
}

function Wait-ForTests {
    param([datetime]$Deadline)

    do {
        Start-Sleep -Seconds 1
        $statusCommand = Invoke-PipelineCommand -Label 'test-status' -CommandArguments @('test_status')
        if ($statusCommand.ExitCode -ne 0) { continue }

        $payload = Get-PipelinePayload -Envelope $statusCommand.Envelope
        $statusValue = [string](Get-PropertyValue -Object $payload -Name 'status')
        if ($statusValue -in @('running', 'queued', 'starting')) { continue }
        if ($statusValue -in @('failed', 'failure')) {
            return [pscustomobject]@{ Outcome = 'failed'; Payload = $payload }
        }
        if ($statusValue -in @('completed', 'passed', 'success')) {
            $success = Get-PropertyValue -Object $payload -Name 'success'
            $failedCount = Get-PropertyValue -Object $payload -Name 'failed'
            if ($null -eq $failedCount) {
                $failedCount = Get-PropertyValue -Object (Get-PropertyValue -Object $payload -Name 'summary') -Name 'failed'
            }
            if ($success -eq $false -or ($failedCount -is [ValueType] -and [int]$failedCount -gt 0)) {
                return [pscustomobject]@{ Outcome = 'failed'; Payload = $payload }
            }
            $passedCount = Get-PropertyValue -Object $payload -Name 'passed'
            if ($null -eq $passedCount) {
                $passedCount = Get-PropertyValue -Object (Get-PropertyValue -Object $payload -Name 'summary') -Name 'passed'
            }
            if ($success -eq $true -or ($failedCount -is [ValueType] -and [int]$failedCount -eq 0 -and $passedCount -is [ValueType] -and [int]$passedCount -gt 0)) {
                return [pscustomobject]@{ Outcome = 'passed'; Payload = $payload }
            }
            return [pscustomobject]@{ Outcome = 'not_completed'; Payload = $payload }
        }
        if ($statusValue -in @('no_tests', 'cancelled', 'timeout')) {
            return [pscustomobject]@{ Outcome = 'not_completed'; Payload = $payload }
        }
    }
    while ((Get-Date) -lt $Deadline)

    return [pscustomobject]@{ Outcome = 'timeout'; Payload = $null }
}

try {
    $script:resolvedUnityCliPath = Resolve-UnityCliPath
}
catch {
    Write-ValidationResult -Status 'not_available' -ExitCode 4 -Message $_.Exception.Message
}

if ($null -eq $script:resolvedUnityCliPath) {
    Write-ValidationResult -Status 'not_available' -ExitCode 4 -Message 'Unity CLI was not found. Set -UnityCliPath, TROUGHGAME_UNITY_CLI_PATH, or add the Unity CLI executable to PATH.'
}

$editorStatus = Invoke-UnityCli -Label 'editor-status' -Arguments @('status', '--format', 'json', '--no-banner', '--no-pager')
if ($editorStatus.ExitCode -ne 0) {
    $cliError = Get-FirstCliError -Envelope $editorStatus.Envelope
    $errorCode = Get-PropertyValue -Object $cliError -Name 'code'
    $errorMessage = Get-PropertyValue -Object $cliError -Name 'message'
    $message = 'A connected Unity Editor with Pipeline is required; no fallback Editor will be started. Unity CLI status failed'
    if ($errorCode) { $message += " ($errorCode)" }
    if ($errorMessage) { $message += ": $errorMessage" }
    Write-ValidationResult -Status 'editor_unavailable' -ExitCode 4 -Message $message -ResolvedMode 'connected_editor'
}

$pipelineStatus = Invoke-PipelineCommand -Label 'pipeline-editor-status' -CommandArguments @('editor_status')
if ($pipelineStatus.ExitCode -ne 0) {
    $cliError = Get-FirstCliError -Envelope $pipelineStatus.Envelope
    $errorMessage = Get-PropertyValue -Object $cliError -Name 'message'
    $message = 'A connected Unity Editor was discovered but its Pipeline commands are unavailable. No fallback Editor will be started.'
    if ($errorMessage) { $message += " $errorMessage" }
    Write-ValidationResult -Status 'editor_unavailable' -ExitCode 4 -Message $message -ResolvedMode 'connected_editor'
}

if ($Mode -eq 'Diagnose') {
    Write-ValidationResult -Status 'available' -ExitCode 0 -Message 'Connected Unity Editor and Pipeline are available.' -ResolvedMode 'connected_editor'
}

$autotick = Invoke-PipelineCommand -Label 'set-autotick' -CommandArguments @('set_autotick', '--enable', 'true')
if ($autotick.ExitCode -ne 0) {
    Write-ValidationResult -Status 'not_completed' -ExitCode 6 -Message 'Could not enable Pipeline auto-tick for the connected Editor.' -ResolvedMode 'connected_editor'
}

$recompileCommand = Invoke-PipelineCommand -Label 'recompile' -CommandArguments @('recompile')
if ($recompileCommand.ExitCode -ne 0) {
    Write-ValidationResult -Status 'not_completed' -ExitCode 6 -Message 'Connected Editor did not accept the recompile command.' -ResolvedMode 'connected_editor'
}
$deadline = (Get-Date).AddSeconds($TimeoutSeconds)
$recompileResult = Wait-ForRecompile -Deadline $deadline
if ($recompileResult.Outcome -eq 'failed') {
    Write-ValidationResult -Status 'failed' -ExitCode 6 -Message 'Connected Editor compilation failed.' -ResolvedMode 'connected_editor' -Details $recompileResult.Payload
}
if ($recompileResult.Outcome -eq 'timeout') {
    Write-ValidationResult -Status 'not_completed' -ExitCode 6 -Message 'Connected Editor compilation did not complete before the timeout.' -ResolvedMode 'connected_editor'
}

if ($Mode -eq 'Compile') {
    Write-ValidationResult -Status 'passed' -ExitCode 0 -Message 'Connected Editor compilation completed.' -ResolvedMode 'connected_editor' -Details $recompileResult.Payload
}

$testMode = if ($Mode -eq 'PlayMode') { 'playmode' } else { 'editor' }
$testArguments = @('run_tests', '--mode', $testMode, '--async_tests', 'true', '--timeout', $TimeoutSeconds)
if (-not [string]::IsNullOrWhiteSpace($Filter)) {
    $testArguments += @('--filter', $Filter, '--filter_type', 'testName')
}

$testRun = Invoke-PipelineCommand -Label "run-tests-$testMode" -CommandArguments $testArguments
if ($testRun.ExitCode -ne 0) {
    Write-ValidationResult -Status 'not_completed' -ExitCode 6 -Message 'Connected Editor did not accept the test run.' -ResolvedMode $testMode
}

$testResult = Wait-ForTests -Deadline (Get-Date).AddSeconds($TimeoutSeconds)
if ($testResult.Outcome -eq 'failed') {
    Write-ValidationResult -Status 'failed' -ExitCode 8 -Message "Connected Editor $testMode tests failed." -ResolvedMode $testMode -Details $testResult.Payload
}
if ($testResult.Outcome -ne 'passed') {
    Write-ValidationResult -Status 'not_completed' -ExitCode 6 -Message "Connected Editor $testMode tests did not complete." -ResolvedMode $testMode -Details $testResult.Payload
}

$message = if ($Mode -eq 'Relevant') {
    'Relevant validation ran connected-Editor compilation and the full Editor test suite because no approved selective-test mapping exists.'
}
else {
    "Connected Editor $testMode tests passed."
}
Write-ValidationResult -Status 'passed' -ExitCode 0 -Message $message -ResolvedMode $testMode -Details $testResult.Payload
