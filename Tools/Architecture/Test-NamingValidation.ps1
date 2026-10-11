[CmdletBinding()]
param()

Set-StrictMode -Version 2.0
$ErrorActionPreference = 'Stop'
$validator = Join-Path $PSScriptRoot 'Validate-Architecture.ps1'
$tempParent = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$fixtureRoot = Join-Path $tempParent ('TroughGame-Naming-' + [guid]::NewGuid().ToString('N'))
$cases = New-Object System.Collections.ArrayList

function Add-Case {
    param([string]$Name, [string]$Path, [string]$Declaration, [string[]]$Expected = @())
    $namespace = if ($Path -match '/Contexts/([^/]+)/') { 'ProjectCore.' + $matches[1] } else { 'Shared' }
    [void]$script:cases.Add([pscustomobject]@{
        Name = $Name; Path = $Path
        Content = "namespace $namespace`n{`n    $Declaration`n    {`n    }`n}`n"
        Expected = $Expected
    })
}

foreach ($role in @('Editor', 'EditorWindow', 'AttributeDrawer')) {
    foreach ($owner in @('Scripts', 'Features/Fixture/Scripts')) {
        Add-Case "$owner accepts $role" `
            "Assets/ProjectCore/Contexts/Template/$owner/Other/Editor/Fixture$role.cs" `
            "public sealed class Fixture$role"
    }
}
Add-Case 'Unknown editor suffix is rejected' `
    'Assets/ProjectCore/Contexts/Template/Scripts/Other/Editor/FixtureTool.cs' `
    'public sealed class FixtureTool' @('SUFFIX001')
Add-Case 'Feature state base outside Abstract still requires Base' `
    'Assets/ProjectCore/Contexts/GameCore/Features/Fixture/Scripts/Components/States/IdleState.cs' `
    'public abstract class IdleState : UnityEngine.MonoBehaviour' @('NAME003')
Add-Case 'Context handler base outside Abstract still requires Base' `
    'Assets/ProjectCore/Contexts/Template/Scripts/Components/Handlers/MemberHandler.cs' `
    'public abstract class MemberHandler : UnityEngine.MonoBehaviour' @('NAME003')
Add-Case 'Abstract contract folder retains the Base check' `
    'Assets/ProjectCore/Contexts/GameCore/Features/Fixture/Scripts/Abstract/MovementService.cs' `
    'public abstract class MovementService' @('NAME003')
Add-Case 'Named feature state base is accepted' `
    'Assets/ProjectCore/Contexts/GameCore/Features/Fixture/Scripts/Components/States/BaseIdleState.cs' `
    'public abstract class BaseIdleState : UnityEngine.MonoBehaviour'
Add-Case 'Named context handler base is accepted' `
    'Assets/ProjectCore/Contexts/Template/Scripts/Components/Handlers/BaseMemberHandler.cs' `
    'public abstract class BaseMemberHandler : UnityEngine.MonoBehaviour'
Add-Case 'Shared primitives do not inherit the context Base rule' `
    'Assets/ProjectCore/Shared/Patterns/Mediator/HandlerPayload.cs' `
    'public abstract class HandlerPayload'

$definitionFamilies = @(
    @{ Name = 'ScreenDefinition'; Feature = 'ScreenNavigation'; Arguments = 'TScreen, TSettings' },
    @{ Name = 'SceneDefinition'; Feature = 'SceneFlow'; Arguments = 'TScene, TSettings' },
    @{ Name = 'PopupDefinition'; Feature = 'Popup'; Arguments = 'TPopup, TPayload, TResponse' },
    @{ Name = 'LoadingScreenDefinition'; Feature = 'LoadingScreen'; Arguments = 'TScreen, TSettings' }
)
foreach ($family in $definitionFamilies) {
    Add-Case ("Documented generic family " + $family.Name) `
        ("Assets/ProjectCore/Contexts/Template/Features/" + $family.Feature + '/Scripts/DataHolders/Definitions/' + $family.Name + 'T.cs') `
        ("public abstract class " + $family.Name + '<' + $family.Arguments + '> : Base' + $family.Name)
}
Add-Case 'Definition exception cannot be reused by another feature' `
    'Assets/ProjectCore/Contexts/Template/Features/Fixture/Scripts/DataHolders/Definitions/SceneDefinitionT.cs' `
    'public abstract class SceneDefinition<TScene, TSettings> : BaseSceneDefinition' @('NAME003')
Add-Case 'Definition exception requires its documented arity' `
    'Assets/ProjectCore/Contexts/Template/Features/SceneFlow/Scripts/DataHolders/Definitions/SceneDefinitionT.cs' `
    'public abstract class SceneDefinition<TScene> : BaseSceneDefinition' @('NAME003')
Add-Case 'Definition exception does not accept non-generic bases' `
    'Assets/ProjectCore/Contexts/Template/Features/SceneFlow/Scripts/DataHolders/Definitions/SceneDefinition.cs' `
    'public abstract class SceneDefinition : BaseSceneDefinition' @('NAME003')
Add-Case 'Definition exception requires the corresponding BaseDefinition' `
    'Assets/ProjectCore/Contexts/Template/Features/SceneFlow/Scripts/DataHolders/Definitions/SceneDefinitionT.cs' `
    'public abstract class SceneDefinition<TScene, TSettings> : OtherDefinition' @('NAME003')

try {
    New-Item -ItemType Directory -Path $fixtureRoot | Out-Null
    for ($index = 0; $index -lt $cases.Count; $index++) {
        $case = $cases[$index]
        $root = Join-Path $fixtureRoot $index.ToString()
        $path = Join-Path $root $case.Path
        New-Item -ItemType Directory -Path (Split-Path -Parent $path) -Force | Out-Null
        Set-Content -LiteralPath $path -Value $case.Content -Encoding UTF8
        $reportPath = Join-Path $fixtureRoot ($index.ToString() + '.json')
        $output = & powershell -NoProfile -ExecutionPolicy Bypass -File $validator `
            -ProjectRoot $root -Mode Strict -SummaryOnly -OutputPath $reportPath
        $exitCode = $LASTEXITCODE
        $report = Get-Content -LiteralPath $reportPath -Raw | ConvertFrom-Json
        $actual = @($report.Diagnostics | ForEach-Object { $_.Rule + '|' + $_.Path } | Sort-Object)
        $expected = @($case.Expected | ForEach-Object { $_ + '|' + $case.Path } | Sort-Object)
        $expectedExit = if ($expected.Count -gt 0) { 1 } else { 0 }
        if ($exitCode -ne $expectedExit -or $report.Warnings -ne 0 -or
            ($actual -join "`n") -cne ($expected -join "`n")) {
            throw ("{0}: exit {1}, diagnostics [{2}], expected [{3}]. {4}" -f `
                $case.Name, $exitCode, ($actual -join ', '), ($expected -join ', '), ($output -join "`n"))
        }
        Write-Output ('[PASS] ' + $case.Name)
    }
    Write-Output ("Naming regression checks: {0} passed." -f $cases.Count)
} finally {
    if (Test-Path -LiteralPath $fixtureRoot) {
        $resolvedRoot = (Resolve-Path -LiteralPath $fixtureRoot).Path
        if (-not $resolvedRoot.StartsWith($tempParent, [StringComparison]::OrdinalIgnoreCase) -or
            (Split-Path -Leaf $resolvedRoot) -notmatch '^TroughGame-Naming-[a-f0-9]{32}$') {
            throw 'Refusing to remove a fixture directory outside the expected temporary location.'
        }
        Remove-Item -LiteralPath $resolvedRoot -Recurse -Force
    }
}
