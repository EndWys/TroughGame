using System;
using System.Collections.Generic;
using Domain;
using UnityEngine;

namespace ProjectCore.Template
{
    public sealed class DebugToolsService : IDebugToolsService
    {
        private readonly HashSet<DebugToolTypes> _enabledTools = new();

        private DebugToolsSettingsConfig _settings;
        private bool _isBuildAllowed;
        private bool _isInitialized;

        public event Action<DebugToolTypes, bool> ToolStateChanged;

        public DebugConsoleSettings ConsoleSettings
        {
            get
            {
                EnsureInitialized();
                return _settings.ConsoleSettings;
            }
        }

        public void Initialize(
            DebugToolsSettingsConfig settings,
            ICommandLineService commandLineService)
        {
            if (_isInitialized)
                throw new InvalidOperationException($"{nameof(DebugToolsService)} is already initialized.");

            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _isBuildAllowed = ResolveBuildAvailability(settings);
            _isInitialized = true;

            if (!_isBuildAllowed)
                return;

            ApplyInitialState(DebugToolTypes.Console);
            ApplyInitialState(DebugToolTypes.Cheats);
            ApplyInitialState(DebugToolTypes.Visualization);

            ApplyCommandLineOverrides(commandLineService);
        }

        public bool IsAllowed(DebugToolTypes tool)
        {
            return _isInitialized
                   && _isBuildAllowed
                   && DebugToolsValidation.IsValidTool(tool);
        }

        public bool IsEnabled(DebugToolTypes tool)
        {
            return IsAllowed(tool) && _enabledTools.Contains(tool);
        }

        public Result Enable(DebugToolTypes tool)
        {
            return SetEnabled(tool, true);
        }

        public Result Disable(DebugToolTypes tool)
        {
            return SetEnabled(tool, false);
        }

        private Result SetEnabled(DebugToolTypes tool, bool enabled)
        {
            if (!_isInitialized)
                return Result.Failure(DebugToolsErrors.NotInitialized);

            if (!DebugToolsValidation.IsValidTool(tool))
                return Result.Failure(DebugToolsErrors.InvalidTool(tool));

            if (!_isBuildAllowed)
                return Result.Failure(DebugToolsErrors.BuildNotAllowed(tool));

            bool changed = enabled
                ? _enabledTools.Add(tool)
                : _enabledTools.Remove(tool);

            if (changed)
                ToolStateChanged?.Invoke(tool, enabled);

            return Result.Success();
        }

        private void ApplyInitialState(DebugToolTypes tool)
        {
            if (_settings.IsInitiallyEnabled(tool))
                _enabledTools.Add(tool);
        }

        private void ApplyCommandLineOverrides(ICommandLineService commandLineService)
        {
            if (commandLineService == null)
                return;

            if (commandLineService.HasArgument("--disable-debug-tools"))
            {
                _enabledTools.Clear();
                return;
            }

            if (commandLineService.HasArgument("--debug-tools"))
            {
                _enabledTools.Add(DebugToolTypes.Console);
                _enabledTools.Add(DebugToolTypes.Cheats);
                _enabledTools.Add(DebugToolTypes.Visualization);
            }

            ApplyCommandLineOverride(commandLineService, DebugToolTypes.Console, "debug-console");
            ApplyCommandLineOverride(commandLineService, DebugToolTypes.Cheats, "debug-cheats");
            ApplyCommandLineOverride(commandLineService, DebugToolTypes.Visualization, "debug-visualization");
        }

        private void ApplyCommandLineOverride(
            ICommandLineService commandLineService,
            DebugToolTypes tool,
            string argument)
        {
            if (commandLineService.HasArgument($"--{argument}"))
                _enabledTools.Add(tool);

            if (commandLineService.HasArgument($"--disable-{argument}"))
                _enabledTools.Remove(tool);
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized)
                throw new InvalidOperationException(DebugToolsErrors.NotInitialized.Message);
        }

        private static bool ResolveBuildAvailability(DebugToolsSettingsConfig settings)
        {
#if UNITY_SERVER
            return settings.EnabledOnDedicatedServer;
#elif UNITY_EDITOR
            return settings.EnabledInEditor;
#else
            return Debug.isDebugBuild
                ? settings.EnabledInDevelopmentBuild
                : settings.EnabledInReleaseBuild;
#endif
        }
    }
}
