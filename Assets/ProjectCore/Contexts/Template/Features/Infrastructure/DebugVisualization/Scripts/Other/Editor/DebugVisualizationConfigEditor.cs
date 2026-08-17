#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectCore.Template
{
    [CustomEditor(typeof(DebugVisualizationSettingsConfig))]
    public sealed class DebugVisualizationConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty _showHudOnStart;
        private SerializedProperty _maxTransientDraws;
        private SerializedProperty _defaultStyle;
        private SerializedProperty _channelStyles;
        private string _currentPath = string.Empty;
        private int _pendingRemoveIndex = -1;

        private void OnEnable()
        {
            _showHudOnStart = serializedObject.FindProperty("_showHudOnStart");
            _maxTransientDraws = serializedObject.FindProperty("_maxTransientDraws");
            _defaultStyle = serializedObject.FindProperty("_defaultStyle");
            _channelStyles = serializedObject.FindProperty("_channelStyles");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_showHudOnStart);
            EditorGUILayout.PropertyField(_maxTransientDraws);
            EditorGUILayout.PropertyField(_defaultStyle, true);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Channel Styles", EditorStyles.boldLabel);

            _pendingRemoveIndex = -1;
            DrawChannelStylePages();

            if (_pendingRemoveIndex >= 0 && _pendingRemoveIndex < _channelStyles.arraySize)
                _channelStyles.DeleteArrayElementAtIndex(_pendingRemoveIndex);

            EditorGUILayout.Space(6f);
            if (GUILayout.Button("Add Channel Style Here"))
                AddChannelStyle();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawChannelStylePages()
        {
            if (_channelStyles.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No channel styles configured. Add one to override the default style for a channel or folder path.", MessageType.Info);
                return;
            }

            var root = ChannelStyleTreeNode.Build(_channelStyles);
            var current = root.Find(_currentPath) ?? root;
            if (current == root && !string.IsNullOrEmpty(_currentPath))
                _currentPath = string.Empty;

            DrawBreadcrumb();
            DrawCurrentPage(current);
        }

        private void DrawBreadcrumb()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Channels", EditorStyles.miniButtonLeft))
                _currentPath = string.Empty;

            if (!string.IsNullOrEmpty(_currentPath))
            {
                var current = string.Empty;
                var parts = _currentPath.Split('/');

                for (var i = 0; i < parts.Length; i++)
                {
                    var part = parts[i];
                    current = string.IsNullOrEmpty(current) ? part : $"{current}/{part}";

                    GUILayout.Label(">", GUILayout.Width(12f));

                    if (GUILayout.Button(part, EditorStyles.miniButtonMid))
                        _currentPath = current;
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4f);
        }

        private void DrawCurrentPage(ChannelStyleTreeNode node)
        {
            if (node.Children.Count > 0)
            {
                EditorGUILayout.LabelField("Folders", EditorStyles.miniBoldLabel);

                foreach (var child in node.Children.Values)
                {
                    if (GUILayout.Button($"{child.Name} >", EditorStyles.miniButton))
                        _currentPath = child.FullPath;
                }

                EditorGUILayout.Space(6f);
            }

            EditorGUILayout.LabelField("Styles", EditorStyles.miniBoldLabel);

            if (node.StyleIndices.Count == 0)
                EditorGUILayout.HelpBox("No style entry on this channel page.", MessageType.None);

            for (var i = 0; i < node.StyleIndices.Count; i++)
                DrawStyleEntry(node.StyleIndices[i], node.Name);
        }

        private void DrawStyleEntry(int index, string fallbackName)
        {
            if (index < 0 || index >= _channelStyles.arraySize)
                return;

            var element = _channelStyles.GetArrayElementAtIndex(index);
            var channel = element.FindPropertyRelative("_channel");
            var style = element.FindPropertyRelative("_style");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(fallbackName, EditorStyles.boldLabel);

            if (GUILayout.Button("Remove", GUILayout.Width(72f)))
            {
                _pendingRemoveIndex = index;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            var nextChannel = EditorGUILayout.DelayedTextField("Channel", channel.stringValue);
            if (EditorGUI.EndChangeCheck())
                channel.stringValue = nextChannel;

            EditorGUILayout.PropertyField(style, true);
            EditorGUI.indentLevel--;

            EditorGUILayout.EndVertical();
        }

        private void AddChannelStyle()
        {
            var index = _channelStyles.arraySize;
            _channelStyles.InsertArrayElementAtIndex(index);

            var element = _channelStyles.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("_channel").stringValue = string.IsNullOrEmpty(_currentPath)
                ? "General/New Channel"
                : $"{_currentPath}/New Channel";
        }

        private sealed class ChannelStyleTreeNode
        {
            public readonly string Name;
            public readonly string FullPath;
            public readonly SortedDictionary<string, ChannelStyleTreeNode> Children = new();
            public readonly List<int> StyleIndices = new();

            public bool HasChildren => Children.Count > 0;

            private ChannelStyleTreeNode(string name, string fullPath)
            {
                Name = name;
                FullPath = fullPath;
            }

            public static ChannelStyleTreeNode Build(SerializedProperty channelStyles)
            {
                var root = new ChannelStyleTreeNode(string.Empty, string.Empty);

                for (var i = 0; i < channelStyles.arraySize; i++)
                {
                    var element = channelStyles.GetArrayElementAtIndex(i);
                    var channel = element.FindPropertyRelative("_channel").stringValue;
                    root.Add(channel, i);
                }

                return root;
            }

            private void Add(string channel, int index)
            {
                if (string.IsNullOrWhiteSpace(channel))
                    channel = DebugVisualizationUtility.DefaultChannel;

                var parts = channel.Split('/');
                var current = this;
                var currentPath = string.Empty;

                for (var i = 0; i < parts.Length; i++)
                {
                    var part = string.IsNullOrWhiteSpace(parts[i]) ? DebugVisualizationUtility.DefaultChannel : parts[i];
                    currentPath = string.IsNullOrEmpty(currentPath) ? part : $"{currentPath}/{part}";

                    if (!current.Children.TryGetValue(part, out var child))
                    {
                        child = new ChannelStyleTreeNode(part, currentPath);
                        current.Children.Add(part, child);
                    }

                    current = child;
                }

                current.StyleIndices.Add(index);
            }

            public ChannelStyleTreeNode Find(string path)
            {
                if (string.IsNullOrEmpty(path))
                    return this;

                var parts = path.Split('/');
                var current = this;

                for (var i = 0; i < parts.Length; i++)
                {
                    if (!current.Children.TryGetValue(parts[i], out current))
                        return null;
                }

                return current;
            }
        }
    }
}
#endif
