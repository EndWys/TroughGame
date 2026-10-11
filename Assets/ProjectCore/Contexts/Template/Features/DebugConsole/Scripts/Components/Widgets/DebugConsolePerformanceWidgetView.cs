using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectCore.Template
{
    internal sealed class DebugConsolePerformanceWidgetView : VisualElement
    {
        private readonly Label _fpsValue;
        private readonly Label _frameTimeValue;
        private readonly Label _monoUsedValue;
        private readonly Label _totalReservedValue;
        private readonly Label _gcHeapValue;

        public DebugConsolePerformanceWidgetView()
        {
            name = "Performance";
            AddToClassList("console-tab");
            style.display = DisplayStyle.None;

            var scroll = new ScrollView();
            scroll.style.flexGrow = 1;
            Add(scroll);

            _fpsValue = AddMetric(scroll.contentContainer, "FPS");
            _frameTimeValue = AddMetric(scroll.contentContainer, "Frame Time (ms)");
            _monoUsedValue = AddMetric(scroll.contentContainer, "Mono Used (MB)");
            _totalReservedValue = AddMetric(scroll.contentContainer, "Total Reserved (MB)");
            _gcHeapValue = AddMetric(scroll.contentContainer, "GC Heap (MB)");
        }

        public VisualElement Root => this;

        public void Render(DebugConsolePerformanceData data)
        {
            _fpsValue.text = data.FramesPerSecond.ToString("0.0");
            _frameTimeValue.text = data.FrameTimeMilliseconds.ToString("0.00");
            _monoUsedValue.text = data.MonoUsedMegabytes.ToString("0.00");
            _totalReservedValue.text = data.TotalReservedMegabytes.ToString("0.00");
            _gcHeapValue.text = data.GCHeapMegabytes.ToString("0.00");
        }

        private static Label AddMetric(VisualElement parent, string title)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginBottom = 6;
            row.style.alignItems = Align.Center;
            parent.Add(row);

            var titleLabel = new Label(title);
            titleLabel.style.minWidth = 180;
            titleLabel.style.color = Color.white;
            row.Add(titleLabel);

            var valueLabel = new Label("-");
            valueLabel.style.color = Color.white;
            row.Add(valueLabel);
            return valueLabel;
        }
    }
}
