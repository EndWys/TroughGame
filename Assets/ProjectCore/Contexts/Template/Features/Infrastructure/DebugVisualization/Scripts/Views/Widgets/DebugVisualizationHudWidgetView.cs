using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ProjectCore.Template
{
    internal sealed class DebugVisualizationHudWidgetView : VisualElement
    {
        private readonly DebugVisualizationHudAdapter _controller;

        private UIDocument _document;
        private VisualElement _window;
        private VisualElement _breadcrumbRow;
        private VisualElement _body;
        private VisualElement _channelsContent;
        private VisualElement _valuesContent;
        private IVisualElementScheduledItem _scheduledRender;
        private bool _fullRenderRequested;
        private bool _valuesRenderRequested;
        private bool _isFolded;
        private float _expandedWindowHeight = 520f;

        public DebugVisualizationHudWidgetView(DebugVisualizationHudAdapter controller)
        {
            _controller = controller;
            _controller.View = this;
        }

        public void Attach(GameObject host, PanelSettings panelSettings, StyleSheet styleSheet)
        {
            _document = host.GetComponent<UIDocument>();
            if (_document == null)
                _document = host.AddComponent<UIDocument>();

            _document.panelSettings = panelSettings;

            VisualElement documentRoot = _document.rootVisualElement;
            documentRoot.Clear();
            documentRoot.styleSheets.Clear();
            documentRoot.styleSheets.Add(styleSheet);
            documentRoot.Add(this);
            AddToClassList("debugviz-root");

            BuildUi();
            RenderFull();
        }

        public void SetVisible(bool visible)
        {
            if (_window == null)
                return;

            if (visible)
                _window.RemoveFromClassList("debugviz-hidden");
            else
                _window.AddToClassList("debugviz-hidden");
        }

        public void RenderFull()
        {
            if (_window == null)
                return;

            RenderBreadcrumb();
            RenderChannels();
            RenderValues();
        }

        public void RenderValuesOnly()
        {
            if (_window == null)
                return;

            RenderValues();
        }

        public void RequestRenderFull()
        {
            _fullRenderRequested = true;
            ScheduleRender();
        }

        public void RequestRenderValuesOnly()
        {
            _valuesRenderRequested = true;
            ScheduleRender();
        }

        public void Dispose()
        {
            RemoveFromHierarchy();
            Clear();
        }

        private void BuildUi()
        {
            Clear();
            AddToClassList("debugviz-root");

            _window = new VisualElement();
            _window.AddToClassList("debugviz-window");

            _breadcrumbRow = new VisualElement();
            _breadcrumbRow.AddToClassList("debugviz-breadcrumb-row");
            _window.Add(_breadcrumbRow);

            _body = new VisualElement();
            _body.AddToClassList("debugviz-body");
            _window.Add(_body);

            var channelsPanel = new VisualElement();
            channelsPanel.AddToClassList("debugviz-channels-panel");
            channelsPanel.Add(CreateHeaderLabel("Channels"));
            var channelsScroll = new ScrollView();
            channelsScroll.AddToClassList("debugviz-scroll");
            _channelsContent = channelsScroll.contentContainer;
            channelsPanel.Add(channelsScroll);
            _body.Add(channelsPanel);

            var valuesPanel = new VisualElement();
            valuesPanel.AddToClassList("debugviz-values-panel");
            valuesPanel.Add(CreateHeaderLabel("Values"));
            var valuesScroll = new ScrollView();
            valuesScroll.AddToClassList("debugviz-scroll");
            _valuesContent = valuesScroll.contentContainer;
            valuesPanel.Add(valuesScroll);
            _body.Add(valuesPanel);

            Add(_window);
        }

        private void RenderBreadcrumb()
        {
            _breadcrumbRow.Clear();

            var foldButton = new Button(ToggleFold) { text = _isFolded ? "+" : "-" };
            foldButton.AddToClassList("debugviz-small-button");
            _breadcrumbRow.Add(foldButton);

            var channelsButton = new Button(_controller.NavigateRoot) { text = "Channels" };
            channelsButton.AddToClassList("debugviz-small-button");
            _breadcrumbRow.Add(channelsButton);

            var breadcrumbParts = _controller.GetBreadcrumbParts();
            if (breadcrumbParts.Count > 0)
            {
                var current = string.Empty;

                for (var i = 0; i < breadcrumbParts.Count; i++)
                {
                    var part = breadcrumbParts[i];
                    current = string.IsNullOrEmpty(current) ? part : $"{current}/{part}";

                    _breadcrumbRow.Add(new Label(">"));
                    var path = current;
                    var button = new Button(() => _controller.NavigateTo(path)) { text = part };
                    button.AddToClassList("debugviz-small-button");
                    _breadcrumbRow.Add(button);
                }
            }

            var spacer = new VisualElement();
            spacer.AddToClassList("debugviz-spacer");
            _breadcrumbRow.Add(spacer);

            var clearButton = new Button(_controller.ClearDraws) { text = "Clear Draws" };
            clearButton.AddToClassList("debugviz-small-button");
            _breadcrumbRow.Add(clearButton);

            var colorsToggle = new Toggle("Value Colors") { value = _controller.UseChannelValueColors };
            colorsToggle.AddToClassList("debugviz-color-toggle");
            colorsToggle.RegisterValueChangedCallback(evt => _controller.SetUseChannelValueColors(evt.newValue));
            _breadcrumbRow.Add(colorsToggle);
        }

        private void RenderChannels()
        {
            _channelsContent.Clear();

            var root = _controller.BuildTree();
            var current = _controller.ResolveCurrentNode(root);

            if (current.Children.Count == 0)
            {
                _channelsContent.Add(new Label("No channels on this page."));
                return;
            }

            for (var i = 0; i < current.Children.Count; i++)
                DrawChannelRow(current.Children[i]);
        }

        private void DrawChannelRow(DebugVisualizationHudAdapter.ChannelTreeNode node)
        {
            var row = new VisualElement();
            row.AddToClassList("debugviz-channel-row");

            var enabled = _controller.AreAllChannelsEnabled(node);
            var toggle = new Toggle { value = enabled };
            toggle.AddToClassList("debugviz-channel-toggle");
            toggle.RegisterValueChangedCallback(change =>
            {
                _controller.SetChannelsEnabled(node, change.newValue);
                _controller.RequestFullRefresh();
            });
            row.Add(toggle);

            var label = node.HasChildren ? $"{node.Name} >" : node.Name;
            var button = new Button(() =>
            {
                if (node.HasChildren)
                    _controller.NavigateTo(node.FullPath);
                else if (node.Channel != null)
                    _controller.SetChannelEnabled(node.Channel, !_controller.IsChannelEnabled(node.Channel));
            })
            {
                text = label
            };
            button.AddToClassList("debugviz-channel-button");
            row.Add(button);

            _channelsContent.Add(row);
        }

        private void RenderValues()
        {
            _valuesContent.Clear();

            var groups = _controller.BuildValueGroups();
            if (groups.Count == 0)
            {
                _valuesContent.Add(new Label("No values for this page."));
                return;
            }

            foreach (var group in groups)
                DrawValueGroup(group.Key, group.Value);
        }

        private void DrawValueGroup(string ownerName, List<DebugVisualizationHudValueData> values)
        {
            if (values.Count == 0)
                return;

            var ownerHeader = new Label(ownerName);
            ownerHeader.AddToClassList("debugviz-owner-header");
            _valuesContent.Add(ownerHeader);

            for (var i = 0; i < values.Count; i++)
            {
                var value = values[i];
                Label label;
                if (_controller.UseChannelValueColors)
                {
                    var color = ColorUtility.ToHtmlStringRGBA(_controller.GetStyle(value.Channel).VisibleColor);
                    label = new Label($"  <color=#{color}>{value.Label}: {FormatValue(value.Value)}</color>");
                    label.enableRichText = true;
                }
                else
                {
                    label = new Label($"  {value.Label}: {FormatValue(value.Value)}");
                }

                label.AddToClassList("debugviz-value-label");
                _valuesContent.Add(label);
            }
        }

        private static Label CreateHeaderLabel(string text)
        {
            var label = new Label(text);
            label.AddToClassList("debugviz-header");
            return label;
        }

        private static string FormatValue(object value)
        {
            return value switch
            {
                null => "null",
                float number => number.ToString("0.###"),
                double number => number.ToString("0.###"),
                _ => value.ToString()
            };
        }

        private void ScheduleRender()
        {
            if (panel == null || _scheduledRender != null)
                return;

            _scheduledRender = schedule.Execute(FlushRenderRequests);
        }

        private void FlushRenderRequests()
        {
            _scheduledRender = null;

            if (_window == null)
                return;

            if (IsPrimaryPointerPressed())
            {
                ScheduleRender();
                return;
            }

            if (_fullRenderRequested)
            {
                _fullRenderRequested = false;
                _valuesRenderRequested = false;
                RenderFull();
                return;
            }

            if (_valuesRenderRequested)
            {
                _valuesRenderRequested = false;
                RenderValuesOnly();
            }
        }

        private void ToggleFold()
        {
            _isFolded = !_isFolded;

            if (_isFolded)
            {
                _expandedWindowHeight = Mathf.Max(120f, _window.resolvedStyle.height);
                _window.AddToClassList("debugviz-window-folded");
                _body.AddToClassList("debugviz-hidden");
                _window.style.height = 36f;
            }
            else
            {
                _window.RemoveFromClassList("debugviz-window-folded");
                _body.RemoveFromClassList("debugviz-hidden");
                _window.style.height = _expandedWindowHeight;
            }

            RequestRenderFull();
        }

        private static bool IsPrimaryPointerPressed()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
                return true;

            if (Touchscreen.current != null)
            {
                var touches = Touchscreen.current.touches;
                for (var i = 0; i < touches.Count; i++)
                {
                    if (touches[i].isInProgress)
                        return true;
                }
            }
#endif
            return false;
        }
    }
}
