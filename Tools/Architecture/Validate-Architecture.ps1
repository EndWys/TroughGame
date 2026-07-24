[CmdletBinding()]
param(
    [ValidateSet('Report', 'Strict')]
    [string]$Mode = 'Report',

    [string]$ProjectRoot,

    [string]$OutputPath,

    [switch]$SummaryOnly
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
    $ProjectRoot = [System.IO.Path]::GetFullPath(
        (Join-Path $PSScriptRoot '..\..'))
}

$ProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
$diagnostics = New-Object System.Collections.ArrayList

function Add-Diagnostic {
    param(
        [string]$Severity,
        [string]$Rule,
        [string]$Path,
        [int]$Line,
        [string]$Message
    )

    [void]$script:diagnostics.Add([pscustomobject][ordered]@{
        Severity = $Severity
        Rule = $Rule
        Path = $Path
        Line = $Line
        Message = $Message
    })
}

function Get-RelativePath {
    param([string]$FullName)

    return $FullName.Substring($script:ProjectRoot.Length).
        TrimStart([char]'\', [char]'/').Replace('\', '/')
}

function Get-LineNumber {
    param(
        [string]$Text,
        [int]$Index
    )

    if ($Index -le 0) {
        return 1
    }

    return ([regex]::Matches($Text.Substring(0, $Index), "`n").Count + 1)
}

function Get-TypeInfo {
    param([string]$Text)

    $namespaceMatch = [regex]::Match(
        $Text,
        '(?m)^\s*namespace\s+(?<name>[A-Za-z_][A-Za-z0-9_\.]*)\s*[;{]')

    $typePattern = '(?m)^[ \t]{0,4}(?<mods>(?:(?:public|internal|private|protected|abstract|sealed|static|partial|readonly)\s+)*)' +
        '(?<kind>class|interface|struct|enum|record(?:\s+class|\s+struct)?)\s+' +
        '(?<name>[A-Za-z_][A-Za-z0-9_]*)(?:\s*<[^\r\n{]+?>)?' +
        '(?:\s*:\s*(?<bases>[^\r\n{]+))?'

    $typeMatches = [regex]::Matches($Text, $typePattern)
    $primary = $null

    if ($typeMatches.Count -gt 0) {
        $match = $typeMatches[0]
        $bases = @()
        if ($match.Groups['bases'].Success) {
            $baseText = $match.Groups['bases'].Value -replace '\s+where\s+.*$', ''
            $bases = @($baseText.Split(',') | ForEach-Object {
                $value = $_.Trim()
                $value = $value -replace '<.*$', ''
                if ($value.Contains('.')) {
                    $value = $value.Substring($value.LastIndexOf('.') + 1)
                }
                $value
            } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
        }

        $primary = [pscustomobject]@{
            Name = $match.Groups['name'].Value
            Kind = $match.Groups['kind'].Value
            Modifiers = $match.Groups['mods'].Value.Trim()
            Bases = $bases
            Line = Get-LineNumber -Text $Text -Index $match.Index
        }
    }

    return [pscustomobject]@{
        Namespace = if ($namespaceMatch.Success) {
            $namespaceMatch.Groups['name'].Value
        } else {
            $null
        }
        NamespaceLine = if ($namespaceMatch.Success) {
            Get-LineNumber -Text $Text -Index $namespaceMatch.Index
        } else {
            1
        }
        PrimaryType = $primary
        TopLevelTypeCount = $typeMatches.Count
    }
}

function Test-EndsWithAny {
    param(
        [string]$Name,
        [string[]]$Suffixes
    )

    foreach ($suffix in $Suffixes) {
        if ($Name.EndsWith($suffix, [System.StringComparison]::Ordinal)) {
            return $true
        }
    }

    return $false
}

function Add-SuffixDiagnostic {
    param(
        [pscustomobject]$FileRecord,
        [string[]]$Suffixes,
        [string]$Folder
    )

    if (-not (Test-EndsWithAny -Name $FileRecord.BaseName -Suffixes $Suffixes)) {
        $line = if ($null -ne $FileRecord.TypeInfo.PrimaryType) {
            $FileRecord.TypeInfo.PrimaryType.Line
        } else {
            1
        }
        Add-Diagnostic -Severity 'Error' -Rule 'SUFFIX001' `
            -Path $FileRecord.RelativePath -Line $line `
            -Message ("Folder '{0}' accepts only suffixes: {1}." -f `
                $Folder, ($Suffixes -join ', '))
    }
}

function Test-IsMonoBehaviourType {
    param(
        [string]$TypeName,
        [hashtable]$Visited
    )

    if ([string]::IsNullOrWhiteSpace($TypeName)) {
        return $false
    }

    if ($TypeName -in @('MonoBehaviour', 'NetworkBehaviour')) {
        return $true
    }

    if ($Visited.ContainsKey($TypeName)) {
        return $false
    }

    $Visited[$TypeName] = $true

    if (-not $script:typeMap.ContainsKey($TypeName)) {
        return $false
    }

    foreach ($baseType in $script:typeMap[$TypeName].Bases) {
        if (Test-IsMonoBehaviourType -TypeName $baseType -Visited $Visited) {
            return $true
        }
    }

    return $false
}

function Test-StaticClass {
    param([pscustomobject]$TypeInfo)

    if ($null -eq $TypeInfo.PrimaryType) {
        return $false
    }

    return ($TypeInfo.PrimaryType.Kind -eq 'class' -and
        $TypeInfo.PrimaryType.Modifiers -match '(^|\s)static($|\s)')
}

function Test-FeatureScriptPath {
    param([pscustomobject]$FileRecord)

    $match = [regex]::Match(
        $FileRecord.RelativePath,
        '^Assets/ProjectCore/Contexts/(?<context>[^/]+)/Features/(?<rest>.+)$')

    if (-not $match.Success) {
        return $false
    }

    $segments = @($match.Groups['rest'].Value.Split('/'))
    $featureKinds = @('Modules', 'Implementations', 'Infrastructure', 'Bridges')

    if ($segments.Count -lt 1 -or $segments[0] -notin $featureKinds) {
        Add-Diagnostic -Severity 'Error' -Rule 'FEATURE001' `
            -Path $FileRecord.RelativePath -Line 1 `
            -Message 'Feature must be placed under Features/Modules, Features/Implementations, Features/Infrastructure, or Features/Bridges.'
        return $true
    }

    if ($segments.Count -lt 4 -or $segments[2] -ne 'Scripts') {
        Add-Diagnostic -Severity 'Error' -Rule 'SCRIPT001' `
            -Path $FileRecord.RelativePath -Line 1 `
            -Message 'Feature C# files must be placed under <Feature>/Scripts/<Category>.'
        return $true
    }

    $scriptSegments = @($segments[3..($segments.Count - 1)])
    $category = $scriptSegments[0]
    $allowedRoots = @(
        'Abstract', 'DataHolders', 'Enums', 'Init', 'Managers',
        'Other', 'Static', 'Tests', 'Views')

    if ($category -notin $allowedRoots) {
        Add-Diagnostic -Severity 'Error' -Rule 'SCRIPT002' `
            -Path $FileRecord.RelativePath -Line 1 `
            -Message ("Unsupported Scripts category '{0}'." -f $category)
        return $true
    }

    $primary = $FileRecord.TypeInfo.PrimaryType
    if ($null -eq $primary) {
        return $true
    }

    if ($category -eq 'Abstract') {
        $isInterface = $primary.Kind -eq 'interface'
        $isAbstract = $primary.Kind -eq 'class' -and
            $primary.Modifiers -match '(^|\s)abstract($|\s)'

        if (-not $isInterface -and -not $isAbstract) {
            Add-Diagnostic -Severity 'Error' -Rule 'ABSTRACT001' `
                -Path $FileRecord.RelativePath -Line $primary.Line `
                -Message 'Abstract contains only interfaces and abstract classes.'
        }

        if ($isInterface -and -not $primary.Name.StartsWith('I')) {
            Add-Diagnostic -Severity 'Error' -Rule 'NAME002' `
                -Path $FileRecord.RelativePath -Line $primary.Line `
                -Message 'Interface names must start with I.'
        }

        if ($isAbstract -and -not $primary.Name.StartsWith('Base')) {
            Add-Diagnostic -Severity 'Error' -Rule 'NAME003' `
                -Path $FileRecord.RelativePath -Line $primary.Line `
                -Message 'Abstract class names must start with Base.'
        }

        return $true
    }

    if ($category -eq 'Enums') {
        if ($primary.Kind -ne 'enum') {
            Add-Diagnostic -Severity 'Error' -Rule 'ENUM001' `
                -Path $FileRecord.RelativePath -Line $primary.Line `
                -Message 'Enums contains only enum declarations.'
        }
        return $true
    }

    if ($category -eq 'Init') {
        Add-SuffixDiagnostic -FileRecord $FileRecord `
            -Suffixes @('FeatureGroup', 'Feature', 'Installer', 'Initializer', 'EntryPoint') `
            -Folder 'Init'
        return $true
    }

    if ($scriptSegments.Count -lt 2) {
        Add-Diagnostic -Severity 'Error' -Rule 'SCRIPT003' `
            -Path $FileRecord.RelativePath -Line 1 `
            -Message ("Category root '{0}' cannot contain C# files directly." -f $category)
        return $true
    }

    $subfolder = $scriptSegments[1]
    $suffixMaps = @{
        DataHolders = @{
            Configs = @('Config')
            Data = @('Data')
            DTOs = @('Dto')
            Payloads = @('Payload')
        }
        Managers = @{
            Controllers = @('Controller')
            Coordinators = @('Coordinator')
            Factories = @('Factory')
            Flows = @('Flow')
            Handlers = @('Handler')
            Mediators = @('Mediator')
            Providers = @('Provider')
            Registries = @('Registry')
            Repositories = @('Repository')
            Services = @('Service')
            Spawners = @('Spawner')
            Systems = @('System')
        }
        Other = @{
            Adapters = @('Adapter')
            Builders = @('Builder')
            Commands = @('Command')
            Converters = @('Converter')
            Decorators = @('Decorator')
            Mappers = @('Mapper')
            Processors = @('Processor')
            StateMachines = @('StateMachine')
            States = @('State')
            Strategies = @('Strategy')
        }
        Static = @{
            Constants = @('Constants', 'Keys', 'Defaults')
            Errors = @('Errors')
            Extensions = @('Extensions')
            Utilities = @('Utility')
            Validation = @('Validation')
        }
        Views = @{
            Components = @('Component')
            Navigation = @('NavigationView')
            Popups = @('PopupView')
            Screens = @('ScreenView')
            Widgets = @('WidgetView')
        }
    }

    if ($category -eq 'Tests') {
        if ($subfolder -notin @('Editor', 'Play')) {
            Add-Diagnostic -Severity 'Error' -Rule 'TEST001' `
                -Path $FileRecord.RelativePath -Line 1 `
                -Message 'Tests must be placed under Tests/Editor or Tests/Play.'
        }
        Add-SuffixDiagnostic -FileRecord $FileRecord -Suffixes @('Tests') -Folder 'Tests'
        return $true
    }

    if (-not $suffixMaps.ContainsKey($category) -or
        -not $suffixMaps[$category].ContainsKey($subfolder)) {
        Add-Diagnostic -Severity 'Error' -Rule 'SCRIPT004' `
            -Path $FileRecord.RelativePath -Line 1 `
            -Message ("Unsupported {0} subfolder '{1}'." -f $category, $subfolder)
        return $true
    }

    Add-SuffixDiagnostic -FileRecord $FileRecord `
        -Suffixes $suffixMaps[$category][$subfolder] -Folder ("{0}/{1}" -f $category, $subfolder)

    if ($category -eq 'Static' -and -not (Test-StaticClass -TypeInfo $FileRecord.TypeInfo)) {
        Add-Diagnostic -Severity 'Error' -Rule 'STATIC001' `
            -Path $FileRecord.RelativePath -Line $primary.Line `
            -Message 'Every production type under Static must be declared as a static class.'
    }

    if ($category -eq 'Managers') {
        if (Test-IsMonoBehaviourType -TypeName $primary.Name -Visited @{}) {
            Add-Diagnostic -Severity 'Error' -Rule 'MANAGER001' `
                -Path $FileRecord.RelativePath -Line $primary.Line `
                -Message 'Managers must not inherit from MonoBehaviour.'
        }

        $bindingPattern = '(?s)' + [regex]::Escape($primary.Name) + '.{0,500}?\.AsSingle\s*\('
        if (-not [regex]::IsMatch($script:allSourceText, $bindingPattern)) {
            Add-Diagnostic -Severity 'Warning' -Rule 'DI001' `
                -Path $FileRecord.RelativePath -Line $primary.Line `
                -Message 'Manager type was not found in a nearby AsSingle binding. Verify its DI lifetime.'
        }
    }

    if ($category -eq 'Views') {
        if (-not (Test-IsMonoBehaviourType -TypeName $primary.Name -Visited @{})) {
            Add-Diagnostic -Severity 'Error' -Rule 'VIEW001' `
                -Path $FileRecord.RelativePath -Line $primary.Line `
                -Message 'Views must inherit from MonoBehaviour, directly or through a known base type.'
        }
    }

    return $true
}

function Test-ContextSharedScriptPath {
    param([pscustomobject]$FileRecord)

    $match = [regex]::Match(
        $FileRecord.RelativePath,
        '^Assets/ProjectCore/Contexts/(?<context>[^/]+)/Scripts/(?<rest>.+)$')

    if (-not $match.Success) {
        return $false
    }

    $segments = @($match.Groups['rest'].Value.Split('/'))
    if ($segments.Count -lt 2) {
        Add-Diagnostic -Severity 'Error' -Rule 'CONTEXT001' `
            -Path $FileRecord.RelativePath -Line 1 `
            -Message 'Context Scripts files must be placed under a semantic category folder.'
        return $true
    }

    $category = $segments[0]
    $primary = $FileRecord.TypeInfo.PrimaryType
    $staticSuffixes = @{
        Constants = @('Constants', 'Keys', 'Defaults')
        Errors = @('Errors')
        Extensions = @('Extensions')
        Utilities = @('Utility')
        Validation = @('Validation')
    }

    if ($staticSuffixes.ContainsKey($category)) {
        Add-SuffixDiagnostic -FileRecord $FileRecord `
            -Suffixes $staticSuffixes[$category] -Folder ("Context/Scripts/{0}" -f $category)

        if (-not (Test-StaticClass -TypeInfo $FileRecord.TypeInfo)) {
            $line = if ($null -ne $FileRecord.TypeInfo.PrimaryType) {
                $FileRecord.TypeInfo.PrimaryType.Line
            } else {
                1
            }
            Add-Diagnostic -Severity 'Error' -Rule 'STATIC002' `
                -Path $FileRecord.RelativePath -Line $line `
                -Message ("Shared context category '{0}' contains only static classes." -f $category)
        }
        return $true
    }

    if ($category -eq 'Init') {
        Add-SuffixDiagnostic -FileRecord $FileRecord `
            -Suffixes @('FeatureGroup', 'Feature', 'Installer', 'Initializer', 'EntryPoint') `
            -Folder 'Context/Scripts/Init'
        return $true
    }

    if ($category -eq 'Abstract') {
        if ($null -ne $primary) {
            $isInterface = $primary.Kind -eq 'interface'
            $isAbstract = $primary.Kind -eq 'class' -and
                $primary.Modifiers -match '(^|\s)abstract($|\s)'

            if (-not $isInterface -and -not $isAbstract) {
                Add-Diagnostic -Severity 'Error' -Rule 'ABSTRACT002' `
                    -Path $FileRecord.RelativePath -Line $primary.Line `
                    -Message 'Context Scripts/Abstract contains only interfaces and abstract classes.'
            }

            if ($isInterface -and -not $primary.Name.StartsWith('I')) {
                Add-Diagnostic -Severity 'Error' -Rule 'NAME002' `
                    -Path $FileRecord.RelativePath -Line $primary.Line `
                    -Message 'Interface names must start with I.'
            }

            if ($isAbstract -and -not $primary.Name.StartsWith('Base')) {
                Add-Diagnostic -Severity 'Error' -Rule 'NAME003' `
                    -Path $FileRecord.RelativePath -Line $primary.Line `
                    -Message 'Abstract class names must start with Base.'
            }
        }
    } elseif ($category -eq 'Enums') {
        if ($null -ne $primary -and $primary.Kind -ne 'enum') {
            Add-Diagnostic -Severity 'Error' -Rule 'ENUM002' `
                -Path $FileRecord.RelativePath -Line $primary.Line `
                -Message 'Context Scripts/Enums contains only enum declarations.'
        }
    } elseif ($category -in @('DataHolders', 'Other', 'Views', 'Tests')) {
        if ($segments.Count -lt 3) {
            Add-Diagnostic -Severity 'Error' -Rule 'CONTEXT002' `
                -Path $FileRecord.RelativePath -Line 1 `
                -Message ("Context Scripts/{0} requires a documented subfolder." -f $category)
            return $true
        }

        $subfolder = $segments[1]
        $contextSuffixMaps = @{
            DataHolders = @{
                Configs = @('Config')
                Data = @('Data')
                DTOs = @('Dto')
                Payloads = @('Payload')
            }
            Other = @{
                Adapters = @('Adapter')
                Builders = @('Builder')
                Commands = @('Command')
                Converters = @('Converter')
                Decorators = @('Decorator')
                Mappers = @('Mapper')
                Processors = @('Processor')
                StateMachines = @('StateMachine')
                States = @('State')
                Strategies = @('Strategy')
            }
            Views = @{
                Components = @('Component')
                Navigation = @('NavigationView')
                Popups = @('PopupView')
                Screens = @('ScreenView')
                Widgets = @('WidgetView')
            }
        }

        if ($category -eq 'Tests') {
            if ($subfolder -notin @('Editor', 'Play')) {
                Add-Diagnostic -Severity 'Error' -Rule 'TEST001' `
                    -Path $FileRecord.RelativePath -Line 1 `
                    -Message 'Context tests must be placed under Tests/Editor or Tests/Play.'
            }
            Add-SuffixDiagnostic -FileRecord $FileRecord -Suffixes @('Tests') -Folder 'Context/Tests'
        } elseif (-not $contextSuffixMaps[$category].ContainsKey($subfolder)) {
            Add-Diagnostic -Severity 'Error' -Rule 'CONTEXT002' `
                -Path $FileRecord.RelativePath -Line 1 `
                -Message ("Unsupported Context Scripts/{0} subfolder '{1}'." -f $category, $subfolder)
        } else {
            Add-SuffixDiagnostic -FileRecord $FileRecord `
                -Suffixes $contextSuffixMaps[$category][$subfolder] `
                -Folder ("Context/Scripts/{0}/{1}" -f $category, $subfolder)

            if ($category -eq 'Views' -and -not (Test-IsMonoBehaviourType -TypeName $primary.Name -Visited @{})) {
                Add-Diagnostic -Severity 'Error' -Rule 'VIEW002' `
                    -Path $FileRecord.RelativePath -Line $primary.Line `
                    -Message 'Context Views must inherit from MonoBehaviour.'
            }
        }
    } else {
        Add-Diagnostic -Severity 'Error' -Rule 'CONTEXT002' `
            -Path $FileRecord.RelativePath -Line 1 `
            -Message ("Unsupported Context Scripts category '{0}'." -f $category)
        return $true
    }

    if ($category -ne 'Init' -and $FileRecord.Text -match
        '(?m)^\s*using\s+Zenject\s*;|\[\s*Inject\s*\]|\bDiContainer\b|\.Bind(?:Interfaces|InterfacesAndSelf|InterfacesTo)?\s*[<(]') {
        Add-Diagnostic -Severity 'Error' -Rule 'CONTEXT003' `
            -Path $FileRecord.RelativePath -Line 1 `
            -Message 'Context-shared scripts outside Init must not participate in DI.'
    }

    return $true
}

$sourceRoots = New-Object System.Collections.ArrayList
foreach ($candidate in @(
    (Join-Path $ProjectRoot 'Assets\ProjectCore'),
    (Join-Path $ProjectRoot 'Assets\Domain')
)) {
    if (Test-Path -LiteralPath $candidate) {
        [void]$sourceRoots.Add($candidate)
    }
}

$sourceFiles = @($sourceRoots | ForEach-Object {
    Get-ChildItem -LiteralPath $_ -Recurse -Filter '*.cs' -File
} | Where-Object {
    $normalized = $_.FullName.Replace('\', '/')
    $normalized -notmatch '/ThirdParty/' -and
    $normalized -notmatch '/Plugins/'
} | Sort-Object FullName -Unique)

$fileRecords = New-Object System.Collections.ArrayList
$typeMap = @{}

foreach ($file in $sourceFiles) {
    $text = Get-Content -LiteralPath $file.FullName -Raw
    $typeInfo = Get-TypeInfo -Text $text
    $record = [pscustomobject]@{
        File = $file
        RelativePath = Get-RelativePath -FullName $file.FullName
        BaseName = $file.BaseName
        Text = $text
        TypeInfo = $typeInfo
    }
    [void]$fileRecords.Add($record)

    if ($null -ne $typeInfo.PrimaryType) {
        $typeMap[$typeInfo.PrimaryType.Name] = $typeInfo.PrimaryType
    }
}

$allSourceText = ($fileRecords | ForEach-Object { $_.Text }) -join "`n"

$knownTypos = [ordered]@{
    Proyotype = 'Prototype'
    Enteties = 'Entities'
    Paterns = 'Patterns'
    Extansions = 'Extensions'
    Paloads = 'Payloads'
}

foreach ($record in $fileRecords) {
    $typeInfo = $record.TypeInfo

    foreach ($typo in $knownTypos.Keys) {
        $pathMatch = [regex]::Match($record.RelativePath, [regex]::Escape($typo), 'IgnoreCase')
        $textMatch = [regex]::Match($record.Text, '\b' + [regex]::Escape($typo) + '\b', 'IgnoreCase')

        if ($pathMatch.Success -or $textMatch.Success) {
            $line = if ($textMatch.Success) {
                Get-LineNumber -Text $record.Text -Index $textMatch.Index
            } else {
                1
            }

            Add-Diagnostic -Severity 'Error' -Rule 'NAME001' `
                -Path $record.RelativePath -Line $line `
                -Message ("Spelling '{0}' must be replaced with '{1}'." -f $typo, $knownTypos[$typo])
        }
    }

    $changerMatch = [regex]::Match($record.Text, '\b[A-Za-z_][A-Za-z0-9_]*Changer\b')
    if ($changerMatch.Success) {
        Add-Diagnostic -Severity 'Error' -Rule 'NAME004' `
            -Path $record.RelativePath `
            -Line (Get-LineNumber -Text $record.Text -Index $changerMatch.Index) `
            -Message "Legacy suffix 'Changer' must be replaced with 'Mutator'."
    }

    if ($null -eq $typeInfo.PrimaryType) {
        Add-Diagnostic -Severity 'Error' -Rule 'TYPE001' `
            -Path $record.RelativePath -Line 1 `
            -Message 'No top-level type declaration was found.'
    } else {
        if ($typeInfo.PrimaryType.Name -ne $record.BaseName) {
            Add-Diagnostic -Severity 'Error' -Rule 'TYPE002' `
                -Path $record.RelativePath -Line $typeInfo.PrimaryType.Line `
                -Message ("File name must match primary type '{0}'." -f $typeInfo.PrimaryType.Name)
        }

        if ($typeInfo.TopLevelTypeCount -gt 1) {
            Add-Diagnostic -Severity 'Error' -Rule 'TYPE003' `
                -Path $record.RelativePath -Line $typeInfo.PrimaryType.Line `
                -Message 'Only one top-level type is allowed per file.'
        }
    }

    $expectedNamespace = $null
    $contextMatch = [regex]::Match(
        $record.RelativePath,
        '^Assets/ProjectCore/Contexts/(?<context>[^/]+)/')

    if ($contextMatch.Success) {
        $expectedNamespace = 'ProjectCore.' + $contextMatch.Groups['context'].Value
    } elseif ($record.RelativePath -match '^Assets/ProjectCore/Domain/' -or
        $record.RelativePath -match '^Assets/Domain/') {
        $expectedNamespace = 'Domain'
    } elseif ($record.RelativePath -match '^Assets/ProjectCore/') {
        Add-Diagnostic -Severity 'Error' -Rule 'PATH002' `
            -Path $record.RelativePath -Line 1 `
            -Message 'Project-owned C# file is outside Contexts, Domain, or ThirdParty.'
    }

    if ($record.RelativePath -match '^Assets/Domain/') {
        Add-Diagnostic -Severity 'Error' -Rule 'PATH001' `
            -Path $record.RelativePath -Line 1 `
            -Message 'Legacy Assets/Domain code must move to Assets/ProjectCore/Domain.'
    }

    if ($null -ne $expectedNamespace -and $typeInfo.Namespace -ne $expectedNamespace) {
        $actual = if ([string]::IsNullOrWhiteSpace($typeInfo.Namespace)) {
            '<missing>'
        } else {
            $typeInfo.Namespace
        }
        Add-Diagnostic -Severity 'Error' -Rule 'NS001' `
            -Path $record.RelativePath -Line $typeInfo.NamespaceLine `
            -Message ("Expected namespace '{0}', found '{1}'." -f $expectedNamespace, $actual)
    }

    $isDomainPath = $record.RelativePath -match '^Assets/ProjectCore/Domain/' -or
        $record.RelativePath -match '^Assets/Domain/'

    if ($isDomainPath) {
        $usingMatches = [regex]::Matches(
            $record.Text,
            '(?m)^\s*using\s+(?:static\s+)?(?:(?:[A-Za-z_][A-Za-z0-9_]*)\s*=\s*)?(?<namespace>[A-Za-z_][A-Za-z0-9_\.]*)(?:\s*;|\s*=)')

        foreach ($usingMatch in $usingMatches) {
            $usedNamespace = $usingMatch.Groups['namespace'].Value
            $allowed = $usedNamespace -eq 'System' -or $usedNamespace.StartsWith('System.') -or
                $usedNamespace -eq 'UnityEngine' -or $usedNamespace.StartsWith('UnityEngine.') -or
                $usedNamespace -eq 'UnityEditor' -or $usedNamespace.StartsWith('UnityEditor.') -or
                $usedNamespace -eq 'Domain' -or $usedNamespace.StartsWith('Domain.')

            if (-not $allowed) {
                Add-Diagnostic -Severity 'Error' -Rule 'DOMAIN001' `
                    -Path $record.RelativePath `
                    -Line (Get-LineNumber -Text $record.Text -Index $usingMatch.Index) `
                    -Message ("Domain cannot depend on external namespace '{0}'." -f $usedNamespace)
            }
        }

        foreach ($externalPrefix in @('Fusion.', 'Zenject.', 'Cysharp.', 'Newtonsoft.', 'Photon.')) {
            $externalMatch = [regex]::Match($record.Text, '\b' + [regex]::Escape($externalPrefix))
            if ($externalMatch.Success) {
                Add-Diagnostic -Severity 'Error' -Rule 'DOMAIN002' `
                    -Path $record.RelativePath `
                    -Line (Get-LineNumber -Text $record.Text -Index $externalMatch.Index) `
                    -Message ("Domain contains external API reference '{0}'." -f $externalPrefix.TrimEnd('.'))
            }
        }
    }

    $asyncVoidMatch = [regex]::Match($record.Text, '(?m)\basync\s+void\s+[A-Za-z_][A-Za-z0-9_]*\s*\(')
    if ($asyncVoidMatch.Success) {
        Add-Diagnostic -Severity 'Error' -Rule 'INIT002' `
            -Path $record.RelativePath `
            -Line (Get-LineNumber -Text $record.Text -Index $asyncVoidMatch.Index) `
            -Message 'async void is forbidden for project initialization and runtime workflows.'
    }

    $lines = @($record.Text -split "`r?`n")
    $lifecycleDeclarationPattern = '^\s*(?:public|private|protected|internal)?\s*' +
        '(?:(?:override|virtual|sealed|async)\s+)*void\s+' +
        '(?:Awake|Start|OnEnable)\s*\('

    for ($index = 0; $index -lt $lines.Count; $index++) {
        if ($lines[$index] -match $lifecycleDeclarationPattern -and
            $record.BaseName -ne 'ApplicationEntryPoint') {
            $last = [Math]::Min($lines.Count - 1, $index + 30)
            $window = ($lines[$index..$last] -join "`n")
            if ($window -match '\b(?:Init|Initialize|Setup|Resolve|AddFeature)[A-Za-z0-9_]*\s*\(' -or
                $window -match '\.Forget\s*\(') {
                Add-Diagnostic -Severity 'Warning' -Rule 'INIT001' `
                    -Path $record.RelativePath -Line ($index + 1) `
                    -Message 'Unity lifecycle callback appears to start or resolve an initialization chain. Review manually.'
            }
        }
    }

    $handled = Test-FeatureScriptPath -FileRecord $record
    if (-not $handled) {
        [void](Test-ContextSharedScriptPath -FileRecord $record)
    }
}

$orderedDiagnostics = @($diagnostics | Sort-Object `
    @{ Expression = { if ($_.Severity -eq 'Error') { 0 } else { 1 } } }, `
    Rule, Path, Line)

$errorCount = @($orderedDiagnostics | Where-Object { $_.Severity -eq 'Error' }).Count
$warningCount = @($orderedDiagnostics | Where-Object { $_.Severity -eq 'Warning' }).Count

if (-not $SummaryOnly) {
    foreach ($diagnostic in $orderedDiagnostics) {
        Write-Output ("[{0}] {1} {2}:{3} - {4}" -f `
            $diagnostic.Severity.ToUpperInvariant(),
            $diagnostic.Rule,
            $diagnostic.Path,
            $diagnostic.Line,
            $diagnostic.Message)
    }
}

Write-Output ''
Write-Output ("Architecture validation ({0}): {1} files, {2} errors, {3} warnings." -f `
    $Mode, $fileRecords.Count, $errorCount, $warningCount)

$ruleSummary = @($orderedDiagnostics | Group-Object Rule | Sort-Object Name)
foreach ($group in $ruleSummary) {
    Write-Output ("  {0}: {1}" -f $group.Name, $group.Count)
}

if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
    $resolvedOutput = if ([System.IO.Path]::IsPathRooted($OutputPath)) {
        $OutputPath
    } else {
        Join-Path $ProjectRoot $OutputPath
    }

    $outputDirectory = Split-Path -Parent $resolvedOutput
    if (-not [string]::IsNullOrWhiteSpace($outputDirectory) -and
        -not (Test-Path -LiteralPath $outputDirectory)) {
        New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
    }

    $report = [pscustomobject][ordered]@{
        Mode = $Mode
        ProjectRoot = $ProjectRoot
        FilesChecked = $fileRecords.Count
        Errors = $errorCount
        Warnings = $warningCount
        Diagnostics = $orderedDiagnostics
    }
    $report | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $resolvedOutput -Encoding UTF8
    Write-Output ("JSON report: {0}" -f $resolvedOutput)
}

if ($Mode -eq 'Strict' -and $errorCount -gt 0) {
    exit 1
}

exit 0
