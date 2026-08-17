using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine.UIElements;

namespace ProjectCore.Template
{
    internal sealed class DebugConsoleCheatsWidgetView : VisualElement
    {
        private readonly Action<DebugConsoleCheatPayload> _submitted;
        private readonly VisualElement _container;

        public DebugConsoleCheatsWidgetView(
            Action<DebugConsoleCheatPayload> submitted)
        {
            _submitted = submitted;
            Build();
            _container = ((ScrollView)this[0]).contentContainer;
        }

        public VisualElement Root => this;

        public void Render(IReadOnlyList<CheatCommandDescriptor> descriptors)
        {
            _container.Clear();
            if (descriptors == null || descriptors.Count == 0)
            {
                _container.Add(new Label("No cheats registered"));
                return;
            }

            IEnumerable<IGrouping<string, CheatCommandDescriptor>> groups = descriptors
                .GroupBy(descriptor => string.IsNullOrWhiteSpace(descriptor.GroupName)
                    ? "General"
                    : descriptor.GroupName)
                .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase);

            foreach (IGrouping<string, CheatCommandDescriptor> group in groups)
            {
                var groupLabel = new Label(group.Key);
                groupLabel.AddToClassList("console-cheat-group-title");
                _container.Add(groupLabel);

                foreach (CheatCommandDescriptor descriptor in group.OrderBy(
                             item => item.Name,
                             StringComparer.OrdinalIgnoreCase))
                {
                    DrawCommand(descriptor);
                }
            }
        }

        private void Build()
        {
            name = "Cheats";
            AddToClassList("console-tab");
            style.display = DisplayStyle.None;

            var scroll = new ScrollView();
            scroll.style.flexGrow = 1;
            Add(scroll);
        }

        private void DrawCommand(CheatCommandDescriptor descriptor)
        {
            var row = new VisualElement();
            row.AddToClassList("console-cheat-row");

            var title = new Label(descriptor.Name);
            title.AddToClassList("console-cheat-title");
            row.Add(title);

            var inputs = new List<TextField>();
            foreach (CheatArgumentDescriptor argument in descriptor.Arguments)
            {
                var field = new TextField { label = argument.Name };
                field.AddToClassList("console-cheat-arg");
                field.tooltip = argument.ValueType?.Name ?? "string";
                if (argument.IsOptional && argument.DefaultValue != null)
                {
                    field.value = Convert.ToString(
                        argument.DefaultValue,
                        CultureInfo.InvariantCulture);
                }

                row.Add(field);
                inputs.Add(field);
            }

            var runButton = new Button(() => Submit(descriptor, inputs)) { text = "Run" };
            runButton.AddToClassList("console-run-button");
            row.Add(runButton);
            _container.Add(row);
        }

        private void Submit(
            CheatCommandDescriptor descriptor,
            IReadOnlyList<TextField> inputs)
        {
            var values = new string[inputs.Count];
            for (int i = 0; i < inputs.Count; i++)
                values[i] = inputs[i].value ?? string.Empty;

            _submitted?.Invoke(new DebugConsoleCheatPayload(
                descriptor,
                Array.AsReadOnly(values)));
        }
    }
}
