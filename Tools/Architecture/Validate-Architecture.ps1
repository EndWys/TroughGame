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

function Split-TypeList {
    param([string]$Text)

    # Commas inside generic arguments do not separate base types.
    $depth = 0
    $start = 0
    for ($index = 0; $index -lt $Text.Length; $index++) {
        if ($Text[$index] -in @('<', '(')) { $depth++ }
        elseif ($Text[$index] -in @('>', ')')) { $depth-- }
        elseif ($Text[$index] -eq ',' -and $depth -eq 0) {
            $Text.Substring($start, $index - $start).Trim()
            $start = $index + 1
        }
    }
    if ($start -lt $Text.Length) {
        $Text.Substring($start).Trim()
    }
}

function Get-TypeReference {
    param([string]$Text)

    $value = $Text.Trim()
    $genericStart = $value.IndexOf('<')
    $arity = 0
    $name = $value
    if ($genericStart -ge 0) {
        $name = $value.Substring(0, $genericStart).Trim()
        $arguments = $value.Substring($genericStart + 1)
        if ($arguments.EndsWith('>')) {
            $arguments = $arguments.Substring(0, $arguments.Length - 1)
        }
        $arity = @(Split-TypeList -Text $arguments).Count
    }
    return [pscustomobject]@{ Name = $name; Arity = $arity; Text = $value }
}

function Get-TypeIdentity {
    param([string]$Name, [int]$Arity)

    return ('{0}#{1}' -f $Name, $Arity)
}

function Get-TypeInfo {
    param([string]$Text)

    $namespaceMatch = [regex]::Match(
        $Text,
        '(?m)^\s*namespace\s+(?<name>[A-Za-z_][A-Za-z0-9_\.]*)\s*[;{]')

    $typePattern = '(?m)^[ \t]{0,4}(?<mods>(?:(?:public|internal|private|protected|abstract|sealed|static|partial|readonly)\s+)*)' +
        '(?<kind>class|interface|struct|enum|record(?:\s+class|\s+struct)?)\s+' +
        '(?<name>[A-Za-z_][A-Za-z0-9_]*)(?<generic>\s*<[^{};]+?>)?' +
        '(?:\s*:\s*(?<bases>[^{;]+))?'

    $typeMatches = [regex]::Matches($Text, $typePattern)
    $primary = $null
    $namespace = if ($namespaceMatch.Success) { $namespaceMatch.Groups['name'].Value } else { '' }
    $usings = @()
    $aliases = @{}
    $usingMatches = [regex]::Matches($Text,
        '(?m)^\s*using\s+(?:(?<alias>[A-Za-z_][A-Za-z0-9_]*)\s*=\s*)?(?<name>(?:global::)?[A-Za-z_][A-Za-z0-9_\.]*)\s*;')
    foreach ($usingMatch in $usingMatches) {
        $usedName = $usingMatch.Groups['name'].Value
        if ($usingMatch.Groups['alias'].Success) {
            $aliases[$usingMatch.Groups['alias'].Value] = $usedName
        } else {
            $usings += $usedName -replace '^global::', ''
        }
    }

    if ($typeMatches.Count -gt 0) {
        $match = $typeMatches[0]
        $baseReferences = @()
        if ($match.Groups['bases'].Success) {
            $baseText = $match.Groups['bases'].Value -replace '(?s)\s+where\s+.*$', ''
            $baseReferences = @(Split-TypeList -Text $baseText | ForEach-Object {
                Get-TypeReference -Text $_
            })
        }

        $declaration = Get-TypeReference -Text ($match.Groups['name'].Value + $match.Groups['generic'].Value)
        $fullName = if ($namespace) { $namespace + '.' + $declaration.Name } else { $declaration.Name }
        $primary = [pscustomobject]@{
            Name = $match.Groups['name'].Value
            Namespace = $namespace
            Identity = Get-TypeIdentity -Name $fullName -Arity $declaration.Arity
            Usings = $usings
            Aliases = $aliases
            Path = $null
            Kind = $match.Groups['kind'].Value
            Modifiers = $match.Groups['mods'].Value.Trim()
            # Short names remain available for the existing feature-registration heuristic.
            Bases = @($baseReferences | ForEach-Object { ($_.Name -split '\.')[-1] })
            BaseReferences = $baseReferences
            IsGeneric = $match.Groups['generic'].Success
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

    $name = $FileRecord.BaseName
    if ($null -ne $FileRecord.TypeInfo.PrimaryType -and
        $FileRecord.TypeInfo.PrimaryType.IsGeneric -and
        $name -eq ($FileRecord.TypeInfo.PrimaryType.Name + 'T')) {
        $name = $FileRecord.TypeInfo.PrimaryType.Name
    }

    if (-not (Test-EndsWithAny -Name $name -Suffixes $Suffixes)) {
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

function Test-KnownTypeIdentity {
    param([string]$Identity)

    return ($script:typeMap.ContainsKey($Identity) -or $Identity -in @(
        'UnityEngine.MonoBehaviour#0', 'Fusion.NetworkBehaviour#0',
        'UnityEngine.UIElements.VisualElement#0'))
}

function Resolve-BaseTypeIdentity {
    param(
        [pscustomobject]$Reference,
        [pscustomobject]$DeclaringType
    )

    $cacheKey = $DeclaringType.Path + '|' + $Reference.Text
    if ($script:typeResolutionCache.ContainsKey($cacheKey)) {
        return $script:typeResolutionCache[$cacheKey]
    }

    $name = $Reference.Name
    $absolute = $name.StartsWith('global::')
    $name = $name -replace '^global::', ''
    $firstSegment = ($name -split '\.')[0]
    if (-not $absolute -and $DeclaringType.Aliases.ContainsKey($firstSegment)) {
        $name = ($DeclaringType.Aliases[$firstSegment] -replace '^global::', '') +
            $name.Substring($firstSegment.Length)
        $absolute = $true
    }

    $resolved = $null
    # Search the declaring namespace and its parents before file-level imports.
    $scope = if ($absolute) { '' } else { $DeclaringType.Namespace }
    while ($true) {
        $candidate = if ($scope) { $scope + '.' + $name } else { $name }
        $identity = Get-TypeIdentity -Name $candidate -Arity $Reference.Arity
        if (Test-KnownTypeIdentity -Identity $identity) {
            $resolved = $identity
            break
        }
        if (-not $scope) { break }
        $lastDot = $scope.LastIndexOf('.')
        $scope = if ($lastDot -ge 0) { $scope.Substring(0, $lastDot) } else { '' }
    }

    if (-not $absolute -and $null -eq $resolved) {
        $candidates = @($DeclaringType.Usings | ForEach-Object {
            $identity = Get-TypeIdentity -Name ($_ + '.' + $name) -Arity $Reference.Arity
            if (Test-KnownTypeIdentity -Identity $identity) { $identity }
        } | Sort-Object -Unique)
        if ($candidates.Count -eq 1) {
            $resolved = $candidates[0]
        } elseif ($candidates.Count -gt 1) {
            Add-Diagnostic -Severity 'Error' -Rule 'TYPE004' `
                -Path $DeclaringType.Path -Line $DeclaringType.Line `
                -Message ("Base type '{0}' is ambiguous between: {1}. Qualify the name or use an alias." -f `
                    $Reference.Text, ($candidates -join ', '))
        }
    }

    $script:typeResolutionCache[$cacheKey] = $resolved
    return $resolved
}

function Test-InheritsKnownType {
    param(
        [string]$Identity,
        [string[]]$Roots,
        [hashtable]$Visited
    )

    if ([string]::IsNullOrWhiteSpace($Identity) -or $Visited.ContainsKey($Identity)) {
        return $false
    }
    $Visited[$Identity] = $true
    if (-not $script:typeMap.ContainsKey($Identity)) {
        return ($Identity -in $Roots)
    }
    foreach ($type in $script:typeMap[$Identity]) {
        foreach ($reference in $type.BaseReferences) {
            $baseIdentity = Resolve-BaseTypeIdentity -Reference $reference -DeclaringType $type
            if (Test-InheritsKnownType -Identity $baseIdentity -Roots $Roots -Visited $Visited) {
                return $true
            }
        }
    }
    return $false
}

function Test-IsMonoBehaviourType {
    param(
        [string]$Identity,
        [hashtable]$Visited
    )

    return (Test-InheritsKnownType -Identity $Identity -Visited $Visited `
        -Roots @('UnityEngine.MonoBehaviour#0', 'Fusion.NetworkBehaviour#0'))
}

function Test-IsViewType {
    param(
        [string]$Identity,
        [hashtable]$Visited
    )

    return (Test-InheritsKnownType -Identity $Identity -Visited $Visited `
        -Roots @('UnityEngine.MonoBehaviour#0', 'Fusion.NetworkBehaviour#0',
            'UnityEngine.UIElements.VisualElement#0'))
}

function Test-StaticClass {
    param([pscustomobject]$TypeInfo)

    if ($null -eq $TypeInfo.PrimaryType) {
        return $false
    }

    return ($TypeInfo.PrimaryType.Kind -eq 'class' -and
        $TypeInfo.PrimaryType.Modifiers -match '(^|\s)static($|\s)')
}

function Test-ComponentScriptPath {
    param([pscustomobject]$FileRecord, [string[]]$Segments, [string]$Rule)
    $roles = @{
        Navigation = @('NavigationView'); Popups = @('PopupView'); Screens = @('ScreenView')
        StateMachines = @('StateMachine'); States = @('State'); Widgets = @('WidgetView')
        Handlers = @('Handler'); Networking = @('Component')
    }
    $suffixes = @('Component')
    if ($Segments.Count -gt 2) {
        if (-not $roles.ContainsKey($Segments[1])) {
            Add-Diagnostic -Severity 'Error' -Rule 'SCRIPT004' -Path $FileRecord.RelativePath -Line 1 `
                -Message ('Unsupported Components subfolder: ' + $Segments[1])
            return
        }
        $suffixes = $roles[$Segments[1]]
    }
    Add-SuffixDiagnostic -FileRecord $FileRecord -Suffixes $suffixes -Folder 'Scripts/Components'
    $primary = $FileRecord.TypeInfo.PrimaryType
    if ($null -ne $primary -and -not (Test-IsViewType -Identity $primary.Identity -Visited @{})) {
        Add-Diagnostic -Severity 'Error' -Rule $Rule -Path $FileRecord.RelativePath -Line $primary.Line `
            -Message 'Components must inherit from MonoBehaviour or VisualElement.'
    }
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
    $legacyRoleFolders = @('Modules', 'Implementations', 'Infrastructure', 'Bridges')

    if ($segments.Count -ge 3 -and $segments[0] -in $legacyRoleFolders -and
        $segments[1] -ne 'Scripts') {
        Add-Diagnostic -Severity 'Error' -Rule 'FEATURE001' `
            -Path $FileRecord.RelativePath -Line 1 `
            -Message 'Feature role folders are obsolete; use Features/<Feature>/Scripts/<Category>.'
        return $true
    }

    if ($segments.Count -lt 3 -or $segments[1] -ne 'Scripts') {
        Add-Diagnostic -Severity 'Error' -Rule 'SCRIPT001' `
            -Path $FileRecord.RelativePath -Line 1 `
            -Message 'Feature C# files must be placed under <Feature>/Scripts/<Category>.'
        return $true
    }

    $scriptSegments = @($segments[2..($segments.Count - 1)])
    $category = $scriptSegments[0]
    if ($category -eq 'Tests') {
        Add-Diagnostic -Severity 'Error' -Rule 'TEST001' `
            -Path $FileRecord.RelativePath -Line 1 `
            -Message 'Feature tests must be moved to Contexts/<Context>/Tests/Editor or Tests/Play.'
        return $true
    }

    $allowedRoots = @(
        'Abstract', 'DataHolders', 'Enums', 'Init', 'Managers', 'Navigation',
        'Other', 'Static', 'Components')

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

    if ($category -eq 'Components') {
        Test-ComponentScriptPath -FileRecord $FileRecord -Segments $scriptSegments -Rule 'VIEW001'
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
            Attributes = @('Attribute')
            Configs = @('Config')
            Data = @('Data')
            Definitions = @('Definition')
            Descriptors = @('Descriptor')
            DTOs = @('DTO')
            Models = @('Model')
            Payloads = @('Payload')
            Settings = @('Settings')
        }
        Managers = @{
            Accumulators = @('Accumulator')
            Controllers = @('Controller')
            Coordinators = @('Coordinator')
            Factories = @('Factory')
            Flows = @('Flow')
            Handlers = @('Handler')
            Mediators = @('Mediator')
            Providers = @('Provider')
            Registries = @('Registry')
            Repositories = @('Repository')
            Serializers = @('Serializer')
            Services = @('Service')
            Storages = @('Storage')
            Spawners = @('Spawner')
            Systems = @('System')
        }
        Other = @{
            Adapters = @('Adapter')
            Builders = @('Builder')
            Commands = @('Command')
            Converters = @('Converter')
            Decorators = @('Decorator')
            Editor = @('Editor', 'EditorWindow', 'AttributeDrawer')
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
        if (Test-IsMonoBehaviourType -Identity $primary.Identity -Visited @{}) {
            Add-Diagnostic -Severity 'Error' -Rule 'MANAGER001' `
                -Path $FileRecord.RelativePath -Line $primary.Line `
                -Message 'Managers must not inherit from MonoBehaviour.'
        }

        $typeNamePattern = [regex]::Escape($primary.Name)
        $directBindingPattern = '(?s)' + $typeNamePattern + '.{0,500}?\.AsSingle\s*\('
        $baseFeatureBindingPattern = '(?s)Bind(?:Interfaces(?:AndSelf)?)?(?:FromComponentInHierarchy)?AsSingle\s*<[^>]*\b' + $typeNamePattern + '\b[^>]*>'
        $cachedGenericBindingPattern = '(?s)\.To\s*\(\s*typeof\s*\(\s*' + $typeNamePattern + '<>\s*\)\s*\).{0,100}?\.AsCached\s*\('

        if (-not [regex]::IsMatch($script:allSourceText, $directBindingPattern) -and
            -not [regex]::IsMatch($script:allSourceText, $baseFeatureBindingPattern) -and
            -not [regex]::IsMatch($script:allSourceText, $cachedGenericBindingPattern)) {
            Add-Diagnostic -Severity 'Warning' -Rule 'DI001' `
                -Path $FileRecord.RelativePath -Line $primary.Line `
                -Message 'Manager type was not found in a nearby AsSingle or Feature binding. Verify its DI lifetime.'
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

    if ($category -eq 'Components') {
        Test-ComponentScriptPath -FileRecord $FileRecord -Segments $segments -Rule 'VIEW002'
        if ($FileRecord.Text -match '(?m)^\s*using\s+Zenject\s*;|\[\s*Inject\s*\]|\bDiContainer\b|\.Bind(?:Interfaces|InterfacesAndSelf|InterfacesTo)?\s*[<(]') {
            Add-Diagnostic -Severity 'Error' -Rule 'CONTEXT003' -Path $FileRecord.RelativePath -Line 1 `
                -Message 'Context-shared components must not participate in DI.'
        }
        return $true
    }

    if ($category -eq 'Initialization') {
        if ($segments.Count -lt 3) {
            Add-Diagnostic -Severity 'Error' -Rule 'CONTEXT002' `
                -Path $FileRecord.RelativePath -Line 1 `
                -Message 'Context Scripts/Initialization requires a semantic subfolder.'
            return $true
        }

        $subfolder = $segments[1]
        if ($subfolder -eq 'Abstract') {
            if ($null -ne $primary) {
                $isInterface = $primary.Kind -eq 'interface'
                $isAbstract = $primary.Kind -eq 'class' -and
                    $primary.Modifiers -match '(^|\s)abstract($|\s)'

                if (-not $isInterface -and -not $isAbstract) {
                    Add-Diagnostic -Severity 'Error' -Rule 'ABSTRACT002' `
                        -Path $FileRecord.RelativePath -Line $primary.Line `
                        -Message 'Context Scripts/Initialization/Abstract contains only interfaces and abstract classes.'
                }
            }
        } elseif ($subfolder -eq 'Init') {
            Add-SuffixDiagnostic -FileRecord $FileRecord `
                -Suffixes @('FeatureGroup', 'Feature', 'Installer', 'Initializer', 'EntryPoint') `
                -Folder 'Context/Scripts/Initialization/Init'
        } elseif ($subfolder -eq 'Managers' -and $segments[2] -eq 'Flows') {
            Add-SuffixDiagnostic -FileRecord $FileRecord `
                -Suffixes @('Flow') -Folder 'Context/Scripts/Initialization/Managers/Flows'
        } elseif ($subfolder -eq 'Other' -and $segments[2] -eq 'Adapters') {
            Add-SuffixDiagnostic -FileRecord $FileRecord `
                -Suffixes @('Adapter') -Folder 'Context/Scripts/Initialization/Other/Adapters'
        } else {
            Add-Diagnostic -Severity 'Error' -Rule 'CONTEXT002' `
                -Path $FileRecord.RelativePath -Line 1 `
                -Message ("Unsupported Context Scripts/Initialization subfolder '{0}'." -f $subfolder)
        }

        return $true
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

        }
    } elseif ($category -eq 'Enums') {
        if ($null -ne $primary -and $primary.Kind -ne 'enum') {
            Add-Diagnostic -Severity 'Error' -Rule 'ENUM002' `
                -Path $FileRecord.RelativePath -Line $primary.Line `
                -Message 'Context Scripts/Enums contains only enum declarations.'
        }
    } elseif ($category -eq 'Navigation') {
        Add-SuffixDiagnostic -FileRecord $FileRecord `
            -Suffixes @('Scene', 'SceneDefinition') -Folder 'Context/Scripts/Navigation'
    } elseif ($category -in @('DataHolders', 'Other', 'Tests')) {
        if ($segments.Count -lt 3) {
            Add-Diagnostic -Severity 'Error' -Rule 'CONTEXT002' `
                -Path $FileRecord.RelativePath -Line 1 `
                -Message ("Context Scripts/{0} requires a documented subfolder." -f $category)
            return $true
        }

        $subfolder = $segments[1]
        $contextSuffixMaps = @{
            DataHolders = @{
                Attributes = @('Attribute')
                Configs = @('Config')
                Data = @('Data')
                Definitions = @('Definition')
                DTOs = @('DTO')
                Models = @('Model')
                Payloads = @('Payload')
            }
            Other = @{
                Editor = @('Editor', 'EditorWindow', 'AttributeDrawer')
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

        }

        if ($category -eq 'Tests') {
            if ($subfolder -notin @('Editor', 'Play') -or $segments.Count -gt 3) {
                Add-Diagnostic -Severity 'Error' -Rule 'TEST001' `
                    -Path $FileRecord.RelativePath -Line 1 `
                    -Message 'Context tests must be placed directly under Tests/Editor or Tests/Play.'
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

        }
    } else {
        Add-Diagnostic -Severity 'Error' -Rule 'CONTEXT002' `
            -Path $FileRecord.RelativePath -Line 1 `
            -Message ("Unsupported Context Scripts category '{0}'." -f $category)
        return $true
    }

    if ($category -notin @('Init', 'Initialization') -and $FileRecord.Text -match
        '(?m)^\s*using\s+Zenject\s*;|\[\s*Inject\s*\]|\bDiContainer\b|\.Bind(?:Interfaces|InterfacesAndSelf|InterfacesTo)?\s*[<(]') {
        Add-Diagnostic -Severity 'Error' -Rule 'CONTEXT003' `
            -Path $FileRecord.RelativePath -Line 1 `
            -Message 'Context-shared scripts outside Init must not participate in DI.'
    }

    return $true
}

function Test-ContextTestPath {
    param([pscustomobject]$FileRecord)

    $match = [regex]::Match(
        $FileRecord.RelativePath,
        '^Assets/ProjectCore/Contexts/(?<context>[^/]+)/Tests/(?<rest>.+)$')

    if (-not $match.Success) {
        return $false
    }

    $segments = @($match.Groups['rest'].Value.Split('/'))
    if ($segments.Count -ne 2 -or $segments[0] -notin @('Editor', 'Play')) {
        Add-Diagnostic -Severity 'Error' -Rule 'TEST001' `
            -Path $FileRecord.RelativePath -Line 1 `
            -Message 'Context tests must be placed directly under Tests/Editor or Tests/Play.'
    }

    Add-SuffixDiagnostic -FileRecord $FileRecord -Suffixes @('Tests') -Folder 'Context/Tests'
    return $true
}

$sourceRoots = New-Object System.Collections.ArrayList
foreach ($candidate in @(
    (Join-Path $ProjectRoot 'Assets\ProjectCore'),
    (Join-Path $ProjectRoot 'Assets\Shared'),
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
$typeResolutionCache = @{}

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
        $primary = $typeInfo.PrimaryType
        $primary.Path = $record.RelativePath
        # Partial declarations share an identity; names from different namespaces do not.
        $typeMap[$primary.Identity] = @($typeMap[$primary.Identity]) + @($primary) |
            Where-Object { $null -ne $_ }
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
        $isGenericFileName = $typeInfo.PrimaryType.IsGeneric -and
            $record.BaseName -eq ($typeInfo.PrimaryType.Name + 'T')

        if ($typeInfo.PrimaryType.Name -ne $record.BaseName -and -not $isGenericFileName) {
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
        $primary = $typeInfo.PrimaryType
        if ($null -ne $primary -and $primary.Kind -eq 'class' -and
            $primary.Modifiers -match '(^|\s)abstract($|\s)' -and
            -not $primary.Name.StartsWith('Base')) {
            # These generic API families intentionally sit above a non-generic
            # Base*Definition. The exception does not permit arbitrary *Definition types.
            $genericDefinitionFamilies = @{
                ScreenDefinition = @{ Feature = 'ScreenNavigation'; Arity = 2 }
                SceneDefinition = @{ Feature = 'SceneFlow'; Arity = 2 }
                PopupDefinition = @{ Feature = 'Popup'; Arity = 3 }
                LoadingScreenDefinition = @{ Feature = 'LoadingScreen'; Arity = 2 }
            }
            $isGenericDefinition = $false
            if ($genericDefinitionFamilies.ContainsKey($primary.Name)) {
                $family = $genericDefinitionFamilies[$primary.Name]
                $familyPath = 'Assets/ProjectCore/Contexts/Template/Features/' +
                    $family.Feature + '/Scripts/DataHolders/Definitions/'
                $isGenericDefinition = $primary.IsGeneric -and
                    $primary.Identity.EndsWith('#' + $family.Arity) -and
                    $record.RelativePath.StartsWith($familyPath, [System.StringComparison]::Ordinal) -and
                    ('Base' + $primary.Name) -in $primary.Bases
            }
            if (-not $isGenericDefinition) {
                Add-Diagnostic -Severity 'Error' -Rule 'NAME003' `
                    -Path $record.RelativePath -Line $primary.Line `
                    -Message 'Top-level abstract context classes must start with Base, except documented generic definition families.'
            }
        }
    } elseif ($record.RelativePath -match '^Assets/ProjectCore/Shared/' -or
        $record.RelativePath -match '^Assets/Shared/') {
        $expectedNamespace = 'Shared'
    } elseif ($record.RelativePath -match '^Assets/ProjectCore/') {
        Add-Diagnostic -Severity 'Error' -Rule 'PATH002' `
            -Path $record.RelativePath -Line 1 `
            -Message 'Project-owned C# file is outside Contexts, Shared, or ThirdParty.'
    }

    if ($record.RelativePath -match '^Assets/Shared/' -or
        $record.RelativePath -match '^Assets/(?:ProjectCore/)?Domain/') {
        Add-Diagnostic -Severity 'Error' -Rule 'PATH001' `
            -Path $record.RelativePath -Line 1 `
            -Message 'Legacy shared code must move to Assets/ProjectCore/Shared; Unity-dependent types belong to Template.'
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

    $isSharedPath = $record.RelativePath -match '^Assets/ProjectCore/Shared/' -or
        $record.RelativePath -match '^Assets/Shared/'

    if ($isSharedPath) {
        $usingMatches = [regex]::Matches(
            $record.Text,
            '(?m)^\s*using\s+(?:static\s+)?(?:(?:[A-Za-z_][A-Za-z0-9_]*)\s*=\s*)?(?<namespace>[A-Za-z_][A-Za-z0-9_\.]*)(?:\s*;|\s*=)')

        foreach ($usingMatch in $usingMatches) {
            $usedNamespace = $usingMatch.Groups['namespace'].Value
            $allowed = $usedNamespace -eq 'System' -or $usedNamespace.StartsWith('System.') -or
                $usedNamespace -eq 'Shared' -or $usedNamespace.StartsWith('Shared.')

            if (-not $allowed) {
                Add-Diagnostic -Severity 'Error' -Rule 'SHARED001' `
                    -Path $record.RelativePath `
                    -Line (Get-LineNumber -Text $record.Text -Index $usingMatch.Index) `
                    -Message ("Shared cannot depend on external namespace '{0}'." -f $usedNamespace)
            }
        }

        foreach ($externalPrefix in @('UnityEngine.', 'UnityEditor.', 'ProjectCore.', 'Fusion.', 'Zenject.', 'Cysharp.', 'Newtonsoft.', 'Photon.')) {
            $externalMatch = [regex]::Match($record.Text, '\b' + [regex]::Escape($externalPrefix))
            if ($externalMatch.Success) {
                Add-Diagnostic -Severity 'Error' -Rule 'SHARED002' `
                    -Path $record.RelativePath `
                    -Line (Get-LineNumber -Text $record.Text -Index $externalMatch.Index) `
                    -Message ("Shared contains external API reference '{0}'." -f $externalPrefix.TrimEnd('.'))
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

    $primary = $record.TypeInfo.PrimaryType
    if ($null -ne $primary -and
        ($primary.Bases -contains 'BaseMonoBehaviourFeature' -or
         $primary.Bases -contains 'BaseMonoBehaviourFeatureGroup')) {
        $componentFeaturePattern = 'AddFeatureFromComponent\s*<\s*' +
            [regex]::Escape($primary.Name) + '\s*>'
        $isRegisteredInComponentGroup = $false

        foreach ($groupRecord in $fileRecords) {
            $groupPrimary = $groupRecord.TypeInfo.PrimaryType
            if ($null -eq $groupPrimary -or
                -not ($groupPrimary.Bases -contains 'BaseMonoBehaviourFeatureGroup')) {
                continue
            }

            $groupFieldPattern = '\b' + [regex]::Escape($primary.Name) +
                '\s+(?<field>_[A-Za-z_][A-Za-z0-9_]*)\s*;'
            $groupFieldMatches = [regex]::Matches($groupRecord.Text, $groupFieldPattern)

            foreach ($groupFieldMatch in $groupFieldMatches) {
                $fieldName = $groupFieldMatch.Groups['field'].Value
                $groupRegistrationPattern = 'AddFeature\s*\(\s*' +
                    [regex]::Escape($fieldName) + '\s*\)'

                if ([regex]::IsMatch($groupRecord.Text, $groupRegistrationPattern)) {
                    $isRegisteredInComponentGroup = $true
                    break
                }
            }

            if ($isRegisteredInComponentGroup) {
                break
            }
        }

        if (-not [regex]::IsMatch($allSourceText, $componentFeaturePattern) -and
            -not $isRegisteredInComponentGroup) {
            Add-Diagnostic -Severity 'Error' -Rule 'INIT003' `
                -Path $record.RelativePath -Line $primary.Line `
                -Message 'MonoBehaviour Feature or FeatureGroup must be registered through AddFeatureFromComponent<TFeature>() or a serialized BaseMonoBehaviourFeatureGroup reference.'
        }
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
        $handled = Test-ContextTestPath -FileRecord $record
    }
    if (-not $handled) {
        [void](Test-ContextSharedScriptPath -FileRecord $record)
    }
}

# Own Unity resources: dependencies and imported SDK assets are excluded.
$assetRoot = Join-Path $ProjectRoot 'Assets/ProjectCore'
$resourceCount = 0
if (Test-Path -LiteralPath $assetRoot) {
    $resources = @(Get-ChildItem -LiteralPath $assetRoot -Recurse -File | Where-Object {
        $_.Extension -notin @('.meta', '.cs', '.md', '.asmdef', '.asmref') -and
        $_.FullName.Replace('\', '/') -notmatch '/(ThirdParty|Plugins)/'
    })
    $resourceCount = $resources.Count
    foreach ($asset in $resources) {
        $path = Get-RelativePath $asset.FullName
        if (-not (Test-Path -LiteralPath ($asset.FullName + '.meta'))) {
            Add-Diagnostic 'Error' 'ASSET001' $path 1 'Unity resources require a matching .meta file.'
        }
        if ($path -eq 'Assets/ProjectCore/Resources/ProjectContext.prefab') { continue }
        if ($path -notmatch '^Assets/ProjectCore/(Assets/|Contexts/[^/]+/(Features/[^/]+/)?Assets/)') {
            Add-Diagnostic 'Error' 'ASSET002' $path 1 'Own resources belong in project, context or feature Assets.'
        }
        $pattern = switch ($asset.Extension.ToLowerInvariant()) {
            '.unity' { '^Scene_[A-Z][A-Za-z0-9]*(?:_[A-Za-z0-9]+)*$' }
            '.prefab' { '^Prefab_[A-Z][A-Za-z0-9]*(?:_[A-Za-z0-9]+)+$' }
            '.asset' { '^Config_[A-Z][A-Za-z0-9]*(?:_[A-Za-z0-9]+){2,}$' }
            '.inputactions' { '^InputActions_[A-Z][A-Za-z0-9]*(?:_[A-Za-z0-9]+)*$' }
            '.controller' { '^AnimatorController_[A-Z][A-Za-z0-9]*(?:_[A-Za-z0-9]+)+$' }
            '.anim' { '^Animation_[A-Z][A-Za-z0-9]*(?:_[A-Za-z0-9]+)+$' }
            '.mat' { '^Material_[A-Z][A-Za-z0-9]*(?:_[A-Za-z0-9]+)*$' }
            '.png' { '^(Sprite|Reference)_[A-Z][A-Za-z0-9]*(?:_[A-Za-z0-9]+)+$' }
            '.uxml' { '^(Document|Screen|Popup|Widget|LoadingScreen)_[A-Z][A-Za-z0-9]*(?:_[A-Za-z0-9]+)+$' }
            '.uss' { '^(Document|Screen|Popup|Widget|LoadingScreen)_[A-Z][A-Za-z0-9]*(?:_[A-Za-z0-9]+)+$' }
            '.tss' { '^Theme_[A-Z][A-Za-z0-9]*(?:_[A-Za-z0-9]+)+$' }
            default { $null }
        }
        if ($null -ne $pattern -and $asset.BaseName -notmatch $pattern) {
            Add-Diagnostic 'Error' 'ASSET003' $path 1 'Resource name does not match its type prefix and naming convention.'
        }
        $location = switch ($asset.Extension.ToLowerInvariant()) {
            '.unity' { '/Assets/Scenes/' }
            '.prefab' { '/Assets/Prefabs/' }
            '.inputactions' { '/Assets/InputActions/' }
            '.controller' { '/Assets/Animations/' }
            '.anim' { '/Assets/Animations/' }
            '.mat' { '/Assets/Materials/|/Assets/Rendering/' }
            '.png' { '/Assets/(Sprites|Animations/Frames|References)/' }
            '.uxml' { '/Assets/UI/UXML/' }
            '.uss' { '/Assets/UI/USS/' }
            '.tss' { '/Assets/UI/USS/' }
            '.asset' {
                if ($asset.BaseName -match '_PanelSettings$') { '/Assets/UI/PanelSettings/' }
                elseif ($asset.BaseName -match 'Catalog$') { '/Assets/Catalogs/' }
                elseif ($asset.BaseName -match '^Config_Project_Rendering_') { '/Assets/Rendering/' }
                else { '/Assets/(Configs|Definitions)/' }
            }
            default { $null }
        }
        $sceneDefinition = $asset.Extension -eq '.asset' -and $path -match '/Assets/Scenes/Config_([^_]+)_SceneFlow_(.+)\.asset$'
        if ($sceneDefinition) {
            $context = $Matches[1]; $detail = $Matches[2]
            $sceneName = if ($detail -eq $context) { "Scene_$context.unity" } else { "Scene_${context}_$detail.unity" }
            $sceneDefinition = Test-Path -LiteralPath (Join-Path $asset.DirectoryName $sceneName)
        }
        if ($null -ne $location -and $path -notmatch $location -and -not $sceneDefinition) {
            Add-Diagnostic 'Error' 'ASSET004' $path 1 'Resource is outside the semantic folder for its type; SceneDefinition must be beside its scene.'
        }
        if ($asset.Extension -eq '.asset' -and $path -match '/Contexts/([^/]+)/') {
            $owner = $Matches[1]
            if ($asset.BaseName -notmatch ('^Config_' + [regex]::Escape($owner) + '_')) {
                Add-Diagnostic 'Error' 'ASSET005' $path 1 'Config owner in the name must match its containing context.'
            }
        }
    }
    foreach ($directory in Get-ChildItem -LiteralPath $assetRoot -Recurse -Directory | Where-Object {
        $_.FullName.Replace('\', '/') -notmatch '/(ThirdParty|Plugins)/'
    }) {
        $contents = @(Get-ChildItem -LiteralPath $directory.FullName -Recurse -File | Where-Object Extension -ne '.meta')
        if ($contents.Count -eq 0) {
            Add-Diagnostic 'Warning' 'ASSET006' (Get-RelativePath $directory.FullName) 1 'Empty folder: remove obsolete structure rather than creating unused categories.'
        }
    }
}
foreach ($record in $fileRecords) {
    if ($record.Text.Contains('CreateAssetMenu') -and $record.RelativePath -match '/Contexts/([^/]+)/') {
        $context = $Matches[1]
        foreach ($menu in [regex]::Matches($record.Text, 'menuName\s*=\s*"SO/([^/]+)/')) {
            if ($menu.Groups[1].Value -cne $context) {
                Add-Diagnostic 'Error' 'ASSET007' $record.RelativePath (Get-LineNumber $record.Text $menu.Index) `
                    'CreateAssetMenu context must match the script context spelling.'
            }
        }
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
Write-Output ("Architecture validation ({0}): {1} scripts, {2} resources, {3} errors, {4} warnings." -f `
    $Mode, $fileRecords.Count, $resourceCount, $errorCount, $warningCount)

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
        ResourcesChecked = $resourceCount
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
