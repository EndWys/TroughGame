using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectCore.Template
{
    internal sealed class DebugConsoleLogsWidgetView : VisualElement
    {
        private readonly Action<string> _commandSubmitted;
        private readonly Action<string> _commandChanged;
        private readonly Action<int> _historyNavigationRequested;
        private readonly Action<DebugConsoleLogFiltersData> _filtersChanged;
        private readonly Action _clearRequested;
        private readonly Action<int> _logSelected;
        private readonly int _fontSize;
        private readonly List<Label> _logLines = new();

        private ScrollView _logsScroll;
        private Label _detailsLabel;
        private Toggle _messagesToggle;
        private Toggle _warningsToggle;
        private Toggle _errorsToggle;
        private Toggle _exceptionsToggle;
        private TextField _filterInput;
        private TextField _commandInput;
        private VisualElement _autocompleteContainer;

        public DebugConsoleLogsWidgetView(
            Action<string> commandSubmitted,
            Action<string> commandChanged,
            Action<int> historyNavigationRequested,
            Action<DebugConsoleLogFiltersData> filtersChanged,
            Action clearRequested,
            Action<int> logSelected,
            int fontSize)
        {
            _commandSubmitted = commandSubmitted;
            _commandChanged = commandChanged;
            _historyNavigationRequested = historyNavigationRequested;
            _filtersChanged = filtersChanged;
            _clearRequested = clearRequested;
            _logSelected = logSelected;
            _fontSize = fontSize;
            Build();
        }

        public VisualElement Root => this;

        public void RenderLogs(
            IReadOnlyList<DebugConsoleLogData> logs,
            int? selectedLogID)
        {
            VisualElement selected = null;

            for (int i = 0; i < logs.Count; i++)
            {
                DebugConsoleLogData log = logs[i];
                Label line = GetOrCreateLogLine(i);
                line.text = log.Message;
                line.style.color = GetColor(log.Type);
                line.style.display = DisplayStyle.Flex;
                line.userData = log.ID;
                line.RemoveFromClassList("console-log-entry-selected");

                if (selectedLogID.HasValue && selectedLogID.Value == log.ID)
                {
                    line.AddToClassList("console-log-entry-selected");
                    selected = line;
                }
            }

            for (int i = logs.Count; i < _logLines.Count; i++)
                _logLines[i].style.display = DisplayStyle.None;

            _logsScroll.schedule.Execute(() =>
            {
                if (selected != null && IsChildOf(selected, _logsScroll.contentContainer))
                {
                    _logsScroll.ScrollTo(selected);
                    return;
                }

                _logsScroll.scrollOffset = new Vector2(0, float.MaxValue);
            });
        }

        public void RenderDetails(string details)
        {
            _detailsLabel.text = details ?? string.Empty;
        }

        public void RenderAutocomplete(IReadOnlyList<CheatCommandDescriptor> commands)
        {
            _autocompleteContainer.Clear();
            foreach (CheatCommandDescriptor command in commands)
            {
                string displayText = BuildFullCheatCommand(command);
                var button = new Button(() => SelectAutocomplete(command.Name))
                {
                    text = displayText
                };
                button.AddToClassList("console-autocomplete-item");
                _autocompleteContainer.Add(button);
            }
        }

        public void SetCommandInput(string command)
        {
            _commandInput.SetValueWithoutNotify(command ?? string.Empty);
        }

        public void SetFilters(DebugConsoleLogFiltersData filters)
        {
            _messagesToggle.SetValueWithoutNotify(filters.ShowMessages);
            _warningsToggle.SetValueWithoutNotify(filters.ShowWarnings);
            _errorsToggle.SetValueWithoutNotify(filters.ShowErrors);
            _exceptionsToggle.SetValueWithoutNotify(filters.ShowExceptions);
            _filterInput.SetValueWithoutNotify(filters.Query);
        }

        public void FocusInput()
        {
            _commandInput?.Focus();
        }

        private void Build()
        {
            name = "Logs";
            AddToClassList("console-tab");

            var toolbar = new VisualElement();
            toolbar.AddToClassList("console-log-toolbar");
            Add(toolbar);

            _messagesToggle = CreateFilterToggle("Message", toolbar);
            _warningsToggle = CreateFilterToggle("Warning", toolbar);
            _errorsToggle = CreateFilterToggle("Error", toolbar);
            _exceptionsToggle = CreateFilterToggle("Exception", toolbar);

            _filterInput = new TextField("Filter");
            _filterInput.AddToClassList("console-log-filter");
            _filterInput.RegisterValueChangedCallback(_ => EmitFilters());
            toolbar.Add(_filterInput);

            var clearButton = new Button(() => _clearRequested?.Invoke()) { text = "Clear" };
            clearButton.AddToClassList("console-button");
            toolbar.Add(clearButton);

            _logsScroll = new ScrollView();
            _logsScroll.AddToClassList("console-logs-scroll");

            var detailsScroll = new ScrollView();
            detailsScroll.AddToClassList("console-log-details-scroll");
            _detailsLabel = new Label("Select a log entry to see stacktrace/details.");
            _detailsLabel.AddToClassList("console-log-details");
            detailsScroll.Add(_detailsLabel);

            var split = new TwoPaneSplitView(
                1,
                100,
                TwoPaneSplitViewOrientation.Vertical);
            split.AddToClassList("console-logs-split");
            split.Add(_logsScroll);
            split.Add(detailsScroll);
            Add(split);

            _autocompleteContainer = new VisualElement();
            _autocompleteContainer.AddToClassList("console-autocomplete");
            Add(_autocompleteContainer);

            var inputRow = new VisualElement();
            inputRow.AddToClassList("console-input-row");
            Add(inputRow);

            _commandInput = new TextField();
            _commandInput.AddToClassList("console-command-input");
            _commandInput.RegisterCallback<KeyDownEvent>(OnCommandKeyDown);
            _commandInput.RegisterValueChangedCallback(changeEvent =>
                _commandChanged?.Invoke(changeEvent.newValue ?? string.Empty));
            inputRow.Add(_commandInput);

            var runButton = new Button(SubmitCommand) { text = "Run" };
            runButton.AddToClassList("console-run-button");
            inputRow.Add(runButton);
        }

        private Toggle CreateFilterToggle(string label, VisualElement parent)
        {
            var toggle = new Toggle(label) { value = true };
            toggle.AddToClassList("console-filter-toggle");
            toggle.RegisterValueChangedCallback(_ => EmitFilters());
            parent.Add(toggle);
            return toggle;
        }

        private Label GetOrCreateLogLine(int index)
        {
            if (index < _logLines.Count)
                return _logLines[index];

            var line = new Label();
            line.style.fontSize = _fontSize;
            line.AddToClassList("console-log-line");
            line.AddToClassList("console-log-entry");
            line.RegisterCallback<ClickEvent>(OnLogLineClicked);
            _logLines.Add(line);
            _logsScroll.Add(line);
            return line;
        }

        private void OnLogLineClicked(ClickEvent clickEvent)
        {
            if (clickEvent.currentTarget is Label { userData: int logID })
                _logSelected?.Invoke(logID);
        }

        private void EmitFilters()
        {
            _filtersChanged?.Invoke(new DebugConsoleLogFiltersData(
                _messagesToggle.value,
                _warningsToggle.value,
                _errorsToggle.value,
                _exceptionsToggle.value,
                _filterInput.value));
        }

        private void OnCommandKeyDown(KeyDownEvent keyEvent)
        {
            if (keyEvent.keyCode == KeyCode.Return
                || keyEvent.keyCode == KeyCode.KeypadEnter)
            {
                SubmitCommand();
                keyEvent.StopImmediatePropagation();
                return;
            }

            if (keyEvent.keyCode == KeyCode.UpArrow)
            {
                _historyNavigationRequested?.Invoke(-1);
                keyEvent.StopPropagation();
                return;
            }

            if (keyEvent.keyCode == KeyCode.DownArrow)
            {
                _historyNavigationRequested?.Invoke(1);
                keyEvent.StopPropagation();
            }
        }

        private void SubmitCommand()
        {
            string command = _commandInput.value;
            if (!string.IsNullOrWhiteSpace(command))
                _commandSubmitted?.Invoke(command);
        }

        private void SelectAutocomplete(string command)
        {
            _commandInput.SetValueWithoutNotify(command);
            _commandChanged?.Invoke(command);
            _commandInput.Focus();
            _commandInput.SelectRange(command.Length, command.Length);
        }

        private static bool IsChildOf(VisualElement element, VisualElement parent)
        {
            VisualElement current = element;
            while (current != null)
            {
                if (current == parent)
                    return true;

                current = current.hierarchy.parent;
            }

            return false;
        }

        private static string BuildFullCheatCommand(CheatCommandDescriptor descriptor)
        {
            if (descriptor.Arguments == null || descriptor.Arguments.Count == 0)
                return descriptor.Name;

            IEnumerable<string> arguments = descriptor.Arguments.Select(argument =>
                $"<{argument.Name}>");
            return $"{descriptor.Name} {string.Join(" ", arguments)}";
        }

        private static Color GetColor(LogType type)
        {
            return type switch
            {
                LogType.Warning => new Color(1f, 0.8f, 0.2f),
                LogType.Error => new Color(1f, 0.45f, 0.35f),
                LogType.Exception => new Color(1f, 0.35f, 0.35f),
                _ => new Color(0.9f, 0.9f, 0.9f)
            };
        }
    }
}
