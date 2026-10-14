[CmdletBinding()]
param()
Set-StrictMode -Version 2.0
$ErrorActionPreference = 'Stop'
$validator = Join-Path $PSScriptRoot 'Validate-Architecture.ps1'
$tempParent = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$fixtureRoot = Join-Path $tempParent ('TroughGame-Resources-' + [guid]::NewGuid().ToString('N'))
$cases = @(
    @{ Name = 'Feature config'; Path = 'Contexts/GameCore/Features/Movement/Assets/Configs/Config_GameCore_Movement_Default.asset'; Rules = @() },
    @{ Name = 'Application catalog'; Path = 'Contexts/Project/Assets/Catalogs/Config_Project_SceneFlow_Catalog.asset'; Rules = @() },
    @{ Name = 'Global rendering'; Path = 'Assets/Rendering/Config_Project_Rendering_DesktopPipeline.asset'; Rules = @() },
    @{ Name = 'Root common UI widget'; Path = 'Assets/UI/UXML/CommonUI/Widget_CommonUI_Button.uxml'; Rules = @() },
    @{ Name = 'Root common UI styles'; Path = 'Assets/UI/USS/CommonUI/Document_CommonUI_Tokens.uss'; Rules = @() },
    @{ Name = 'Root common UI theme'; Path = 'Assets/UI/USS/CommonUI/Theme_CommonUI_Runtime.tss'; Rules = @() },
    @{ Name = 'Root common UI sprite'; Path = 'Assets/Sprites/CommonUI/Buttons/Sprite_CommonUI_Button_Primary.png'; Rules = @() },
    @{ Name = 'Root common TTF font'; Path = 'Assets/Fonts/CommonUI/Font_CommonUI_Body.ttf'; Rules = @() },
    @{ Name = 'Root common OTF font'; Path = 'Assets/Fonts/CommonUI/Font_CommonUI_Heading.otf'; Rules = @() },
    @{ Name = 'Root common UI keeps semantic folders'; Path = 'Assets/UI/CommonUI/UXML/Widget_CommonUI_Button.uxml'; Rules = @('ASSET004') },
    @{ Name = 'Font requires its naming prefix'; Path = 'Assets/Fonts/CommonUI/Body.ttf'; Rules = @('ASSET003') },
    @{ Name = 'Font requires its semantic folder'; Path = 'Assets/Sprites/CommonUI/Font_CommonUI_Body.otf'; Rules = @('ASSET004') },
    @{ Name = 'Context input'; Path = 'Contexts/TechnicalPrototype/Assets/InputActions/InputActions_TechnicalPrototype.inputactions'; Rules = @() },
    @{ Name = 'UI document'; Path = 'Contexts/Template/Features/Popup/Assets/UI/UXML/Document_Template_Popup_Root.uxml'; Rules = @() },
    @{ Name = 'Panel settings'; Path = 'Contexts/Template/Features/Popup/Assets/UI/PanelSettings/Config_Template_Popup_PanelSettings.asset'; Rules = @() },
    @{ Name = 'ProjectContext exception'; Path = 'Resources/ProjectContext.prefab'; Rules = @() },
    @{ Name = 'Third party exception'; Path = 'ThirdParty/Sample.asset'; Rules = @(); NoMeta = $true },
    @{ Name = 'Missing meta'; Path = 'Assets/InputActions/InputActions_Project_Default.inputactions'; Rules = @('ASSET001'); NoMeta = $true },
    @{ Name = 'Legacy resource root'; Path = 'Contexts/GameCore/Features/Movement/GraphicResources/Configs/Config_GameCore_Movement_Default.asset'; Rules = @('ASSET002','ASSET004') },
    @{ Name = 'Config among sprites'; Path = 'Contexts/GameCore/Features/Movement/Assets/Sprites/Config_GameCore_Movement_Default.asset'; Rules = @('ASSET004') },
    @{ Name = 'Wrong resource name'; Path = 'Contexts/GameCore/Features/Movement/Assets/Configs/Movement.asset'; Rules = @('ASSET003','ASSET005') },
    @{ Name = 'Wrong config owner'; Path = 'Contexts/GameCore/Features/Movement/Assets/Configs/Config_Project_Movement_Default.asset'; Rules = @('ASSET005') },
    @{ Name = 'Panel settings outside UI'; Path = 'Contexts/Template/Features/Popup/Assets/Configs/Config_Template_Popup_PanelSettings.asset'; Rules = @('ASSET004') },
    @{ Name = 'Definition without scene'; Path = 'Contexts/GameCore/Assets/Scenes/Config_GameCore_SceneFlow_Example.asset'; Rules = @('ASSET004') },
    @{ Name = 'Definition beside matching scene'; Path = 'Contexts/GameCore/Assets/Scenes/Config_GameCore_SceneFlow_Example.asset'; Companion = 'Contexts/GameCore/Assets/Scenes/Scene_GameCore_Example.unity'; Rules = @() },
    @{ Name = 'Empty obsolete folder'; Path = 'Contexts/GameCore/Assets/Unused'; Directory = $true; Rules = @('ASSET006'); Warning = $true }
)
try {
    New-Item -ItemType Directory -Path $fixtureRoot | Out-Null
    for ($index = 0; $index -lt $cases.Count; $index++) {
        $case = $cases[$index]
        $root = Join-Path $fixtureRoot $index.ToString()
        $path = Join-Path $root ('Assets/ProjectCore/' + $case.Path)
        if ($case.ContainsKey('Directory')) {
            New-Item -ItemType Directory -Path $path -Force | Out-Null
        } else {
            $paths = @($path)
            if ($case.ContainsKey('Companion')) { $paths += Join-Path $root ('Assets/ProjectCore/' + $case.Companion) }
            foreach ($file in $paths) {
                New-Item -ItemType Directory -Path (Split-Path -Parent $file) -Force | Out-Null
                Set-Content -LiteralPath $file -Value 'Fixture resource' -Encoding UTF8
                if (-not $case.ContainsKey('NoMeta')) {
                    Set-Content -LiteralPath ($file + '.meta') -Value ('guid: ' + [guid]::NewGuid().ToString('N')) -Encoding UTF8
                }
            }
        }
        $reportPath = Join-Path $fixtureRoot ($index.ToString() + '.json')
        $output = & powershell -NoProfile -ExecutionPolicy Bypass -File $validator `
            -ProjectRoot $root -Mode Strict -SummaryOnly -OutputPath $reportPath
        $exitCode = $LASTEXITCODE
        if (-not (Test-Path -LiteralPath $reportPath)) { throw ($output -join "`n") }
        $report = Get-Content -LiteralPath $reportPath -Raw | ConvertFrom-Json
        $actual = @($report.Diagnostics | ForEach-Object Rule | Sort-Object -Unique)
        $expected = @($case.Rules | Sort-Object)
        $expectedExit = if ($expected.Count -gt 0 -and -not $case.ContainsKey('Warning')) { 1 } else { 0 }
        if ($exitCode -ne $expectedExit -or ($actual -join ',') -cne ($expected -join ',')) {
            throw ("{0}: exit {1}, diagnostics [{2}], expected [{3}]. {4}" -f `
                $case.Name, $exitCode, ($actual -join ','), ($expected -join ','), ($output -join "`n"))
        }
        Write-Output ('[PASS] ' + $case.Name)
    }
    Write-Output ("Resource validation regression checks: {0} passed." -f $cases.Count)
} finally {
    if (Test-Path -LiteralPath $fixtureRoot) {
        $resolvedRoot = (Resolve-Path -LiteralPath $fixtureRoot).Path
        if (-not $resolvedRoot.StartsWith($tempParent, [StringComparison]::OrdinalIgnoreCase) -or
            (Split-Path -Leaf $resolvedRoot) -notmatch '^TroughGame-Resources-[a-f0-9]{32}$') {
            throw 'Refusing to remove a fixture directory outside the expected temporary location.'
        }
        Remove-Item -LiteralPath $resolvedRoot -Recurse -Force
    }
}
