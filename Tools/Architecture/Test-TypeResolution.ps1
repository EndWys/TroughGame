[CmdletBinding()]
param()

Set-StrictMode -Version 2.0
$ErrorActionPreference = 'Stop'

$validator = Join-Path $PSScriptRoot 'Validate-Architecture.ps1'
$tempParent = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
$fixtureRoot = Join-Path $tempParent ('TroughGame-TypeResolution-' + [guid]::NewGuid().ToString('N'))
$cases = New-Object System.Collections.ArrayList

function New-FixtureFile {
    param(
        [string]$Context,
        [string]$Folder,
        [string]$Name,
        [string]$Declaration,
        [string[]]$Usings = @()
    )

    $imports = ($Usings | ForEach-Object { 'using ' + $_ + ';' }) -join "`n"
    return [pscustomobject]@{
        Path = "Assets/ProjectCore/Contexts/$Context/Features/Fixture/Scripts/$Folder/$Name.cs"
        Content = "$imports`nnamespace ProjectCore.$Context`n{`n    $Declaration`n    {`n    }`n}`n"
    }
}

function Add-Case {
    param(
        [string]$Name,
        [object[]]$Files = @(),
        [string[]]$Expected = @(),
        [string]$ExistingRoot
    )

    [void]$script:cases.Add([pscustomobject]@{
        Name = $Name; Files = $Files; Expected = $Expected; ExistingRoot = $ExistingRoot
    })
}

function Get-ExpectedDiagnostic {
    param([string]$Rule, [pscustomobject]$File)

    return ($Rule + '|' + $File.Path)
}

$gamePlainBase = New-FixtureFile GameCore Abstract BaseCollisionComponent `
    'public abstract class BaseCollisionComponent'
$prototypeMonoBase = New-FixtureFile TechnicalPrototype Abstract BaseCollisionComponent `
    'public abstract class BaseCollisionComponent : MonoBehaviour' @('UnityEngine')

Add-Case -Name 'Existing positive fixture' -ExistingRoot (Join-Path $PSScriptRoot 'Fixtures/Valid')
Add-Case -Name 'Same name cannot hide invalid view' `
    -ExistingRoot (Join-Path $PSScriptRoot 'Fixtures/TypeNamesInvalid') `
    -Expected @('VIEW001|Assets/ProjectCore/Contexts/TechnicalPrototype/Features/Movement/Scripts/Components/CollisionComponent.cs')

$gameInvalid = New-FixtureFile GameCore Components CollisionComponent 'public class CollisionComponent'
$prototypeValid = New-FixtureFile TechnicalPrototype Components CollisionComponent `
    'public class CollisionComponent : Fusion.NetworkBehaviour'
Add-Case -Name 'Same names with the invalid context reversed' -Files @($gameInvalid, $prototypeValid) `
    -Expected @(Get-ExpectedDiagnostic VIEW001 $gameInvalid)

$contextInvalid = New-FixtureFile GameCore Components CollisionComponent 'public class CollisionComponent'
$contextInvalid.Path = 'Assets/ProjectCore/Contexts/GameCore/Scripts/Components/CollisionComponent.cs'
Add-Case -Name 'Context-owned Components use the same namespace-aware check' `
    -Files @($contextInvalid, $prototypeValid) -Expected @(Get-ExpectedDiagnostic VIEW002 $contextInvalid)

$validLocal = New-FixtureFile TechnicalPrototype Components DerivedComponent `
    'public class DerivedComponent : BaseCollisionComponent' @('ProjectCore.GameCore')
$invalidLocal = New-FixtureFile GameCore Components DerivedComponent `
    'public class DerivedComponent : BaseCollisionComponent' @('ProjectCore.TechnicalPrototype')
Add-Case -Name 'Local base takes precedence over an imported namesake' `
    -Files @($gamePlainBase, $prototypeMonoBase, $validLocal, $invalidLocal) `
    -Expected @(Get-ExpectedDiagnostic VIEW001 $invalidLocal)

$qualified = New-FixtureFile GameCore Components QualifiedComponent `
    'public class QualifiedComponent : ProjectCore.TechnicalPrototype.BaseCollisionComponent'
$global = New-FixtureFile GameCore Components GlobalComponent `
    'public class GlobalComponent : global::ProjectCore.TechnicalPrototype.BaseCollisionComponent'
$namespaceAlias = New-FixtureFile GameCore Components AliasComponent `
    'public class AliasComponent : Peer.BaseCollisionComponent' @('Peer = ProjectCore.TechnicalPrototype')
$typeAlias = New-FixtureFile GameCore Components TypeAliasComponent `
    'public class TypeAliasComponent : Peer' @('Peer = global::ProjectCore.TechnicalPrototype.BaseCollisionComponent')
$wrongQualified = New-FixtureFile TechnicalPrototype Components PlainComponent `
    'public class PlainComponent : ProjectCore.GameCore.BaseCollisionComponent'
Add-Case -Name 'Qualified bases and namespace or type aliases' `
    -Files @($gamePlainBase, $prototypeMonoBase, $qualified, $global, $namespaceAlias, $typeAlias, $wrongQualified) `
    -Expected @(Get-ExpectedDiagnostic VIEW001 $wrongQualified)

$templateBase = New-FixtureFile Template Abstract BaseVisualComponent `
    'public abstract class BaseVisualComponent : VisualElement' @('UnityEngine.UIElements')
$importedBase = New-FixtureFile TechnicalPrototype Abstract BaseImportedComponent `
    'public abstract class BaseImportedComponent : BaseVisualComponent' @('ProjectCore.Template')
$importedView = New-FixtureFile GameCore Components ImportedComponent `
    'public class ImportedComponent : BaseImportedComponent' @('ProjectCore.TechnicalPrototype')
Add-Case -Name 'Each link in an inheritance chain uses its own imports' `
    -Files @($templateBase, $importedBase, $importedView)

$genericBase = New-FixtureFile GameCore Abstract BaseCollisionComponentT `
    'public abstract class BaseCollisionComponent<TFirst, TSecond> : UnityEngine.MonoBehaviour'
$genericView = New-FixtureFile GameCore Components GenericComponent `
    "public class GenericComponent : BaseCollisionComponent<`n        System.Collections.Generic.Dictionary<int, string>,`n        System.Tuple<int, string>>"
$plainView = New-FixtureFile GameCore Components PlainComponent `
    'public class PlainComponent : BaseCollisionComponent'
Add-Case -Name 'Generic arity and multiline nested generic arguments' `
    -Files @($gamePlainBase, $genericBase, $genericView, $plainView) `
    -Expected @(Get-ExpectedDiagnostic VIEW001 $plainView)

$ambiguous = New-FixtureFile Template Components AmbiguousComponent `
    'public class AmbiguousComponent : BaseCollisionComponent' @('ProjectCore.GameCore', 'ProjectCore.TechnicalPrototype')
Add-Case -Name 'Ambiguous imports produce an explicit diagnostic' `
    -Files @($gamePlainBase, $prototypeMonoBase, $ambiguous) `
    -Expected @((Get-ExpectedDiagnostic TYPE004 $ambiguous), (Get-ExpectedDiagnostic VIEW001 $ambiguous))

$sharedNamesakes = @('MonoBehaviour', 'NetworkBehaviour', 'VisualElement') | ForEach-Object {
    [pscustomobject]@{
        Path = "Assets/ProjectCore/Shared/$_.cs"
        Content = "namespace Shared`n{`n    public class $_`n    {`n    }`n}`n"
    }
}
$fakeMono = New-FixtureFile GameCore Components FakeMonoComponent `
    'public class FakeMonoComponent : Shared.MonoBehaviour'
$fakeNetwork = New-FixtureFile GameCore Components FakeNetworkComponent `
    'public class FakeNetworkComponent : Shared.NetworkBehaviour'
$fakeVisual = New-FixtureFile GameCore Components FakeVisualComponent `
    'public class FakeVisualComponent : VisualElement' @('Shared')
Add-Case -Name 'Framework roots are recognized only by their full names' `
    -Files (@($sharedNamesakes) + @($fakeMono, $fakeNetwork, $fakeVisual)) `
    -Expected @((Get-ExpectedDiagnostic VIEW001 $fakeMono), (Get-ExpectedDiagnostic VIEW001 $fakeNetwork),
        (Get-ExpectedDiagnostic VIEW001 $fakeVisual))

$monoManager = New-FixtureFile GameCore Managers/Services CollisionService `
    'public class CollisionService : UnityEngine.MonoBehaviour'
$plainManager = New-FixtureFile TechnicalPrototype Managers/Services CollisionService `
    'public class CollisionService'
# The binding is real source text for the validator's existing DI heuristic.
$binding = New-FixtureFile GameCore Init FixtureFeature 'public class FixtureFeature'
$binding.Content = $binding.Content.Replace('    {', @'
    {
        public void Bind(Zenject.DiContainer container)
        {
            container.Bind<CollisionService>().AsSingle();
        }
'@)
Add-Case -Name 'Namesake manager cannot hide forbidden MonoBehaviour inheritance' `
    -Files @($monoManager, $plainManager, $binding) `
    -Expected @(Get-ExpectedDiagnostic MANAGER001 $monoManager)

$cycleA = New-FixtureFile GameCore Abstract BaseCycleComponent 'public abstract class BaseCycleComponent : BaseOtherComponent'
$cycleB = New-FixtureFile GameCore Abstract BaseOtherComponent 'public abstract class BaseOtherComponent : BaseCycleComponent'
$cycleView = New-FixtureFile GameCore Components CycleComponent 'public class CycleComponent : BaseCycleComponent'
Add-Case -Name 'Cyclic inheritance terminates without accepting a view' -Files @($cycleA, $cycleB, $cycleView) `
    -Expected @(Get-ExpectedDiagnostic VIEW001 $cycleView)

try {
    New-Item -ItemType Directory -Path $fixtureRoot | Out-Null
    for ($index = 0; $index -lt $cases.Count; $index++) {
        $case = $cases[$index]
        $root = $case.ExistingRoot
        if ([string]::IsNullOrWhiteSpace($root)) {
            $root = Join-Path $fixtureRoot $index.ToString()
            foreach ($file in $case.Files) {
                $path = Join-Path $root $file.Path
                New-Item -ItemType Directory -Path (Split-Path -Parent $path) -Force | Out-Null
                Set-Content -LiteralPath $path -Value $file.Content -Encoding UTF8
            }
        }
        $reportPath = Join-Path $fixtureRoot ($index.ToString() + '.json')
        $output = & powershell -NoProfile -ExecutionPolicy Bypass -File $validator `
            -ProjectRoot $root -Mode Strict -SummaryOnly -OutputPath $reportPath
        $exitCode = $LASTEXITCODE
        if (-not (Test-Path -LiteralPath $reportPath)) {
            throw ("{0}: validator did not write a report. {1}" -f $case.Name, ($output -join "`n"))
        }
        $report = Get-Content -LiteralPath $reportPath -Raw | ConvertFrom-Json
        $actual = @($report.Diagnostics | ForEach-Object { $_.Rule + '|' + $_.Path } | Sort-Object)
        $expected = @($case.Expected | Sort-Object)
        $expectedExit = if ($expected.Count -gt 0) { 1 } else { 0 }
        if ($exitCode -ne $expectedExit -or $report.Warnings -ne 0 -or
            ($actual -join "`n") -cne ($expected -join "`n")) {
            throw ("{0}: exit {1}, diagnostics [{2}], expected [{3}]. {4}" -f `
                $case.Name, $exitCode, ($actual -join ', '), ($expected -join ', '), ($output -join "`n"))
        }
        Write-Output ('[PASS] ' + $case.Name)
    }
    Write-Output ("Type resolution regression checks: {0} passed." -f $cases.Count)
} finally {
    if (Test-Path -LiteralPath $fixtureRoot) {
        $resolvedRoot = (Resolve-Path -LiteralPath $fixtureRoot).Path
        if (-not $resolvedRoot.StartsWith($tempParent, [System.StringComparison]::OrdinalIgnoreCase) -or
            (Split-Path -Leaf $resolvedRoot) -notmatch '^TroughGame-TypeResolution-[a-f0-9]{32}$') {
            throw 'Refusing to remove a fixture directory outside the expected temporary location.'
        }
        Remove-Item -LiteralPath $resolvedRoot -Recurse -Force
    }
}
