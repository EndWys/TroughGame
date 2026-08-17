using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectCore.Template
{
    internal sealed class DebugVisualizationRegistry : IDebugVisualizationRegistry
    {
        private const BindingFlags MemberFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly MethodInfo CreateTypedPropertyGetterMethod =
            typeof(DebugVisualizationRegistry).GetMethod(
                nameof(CreateTypedPropertyGetter),
                BindingFlags.Static | BindingFlags.NonPublic);

        private readonly List<RegisteredTarget> _targets = new();
        private readonly HashSet<int> _targetIDs = new();
        private readonly Dictionary<Type, IReadOnlyList<InspectedMember>> _memberCache = new();

        public void Register(Object target)
        {
            if (target == null || !_targetIDs.Add(target.GetInstanceID()))
                return;

            IReadOnlyList<InspectedMember> inspectedMembers = GetInspectedMembers(target.GetType());
            var drawable = target as IDebugDrawable;

            if (inspectedMembers.Count == 0 && drawable == null)
            {
                _targetIDs.Remove(target.GetInstanceID());
                return;
            }

            _targets.Add(new RegisteredTarget(target, inspectedMembers, drawable));
        }

        public void Unregister(Object target)
        {
            if (target == null)
                return;

            int targetID = target.GetInstanceID();
            if (!_targetIDs.Remove(targetID))
                return;

            for (int i = _targets.Count - 1; i >= 0; i--)
            {
                if (_targets[i].TargetID == targetID)
                    _targets.RemoveAt(i);
            }
        }

        public void Clear()
        {
            _targets.Clear();
            _targetIDs.Clear();
        }

        public void Refresh(IDebugVisualizationBackend backend)
        {
            for (int i = _targets.Count - 1; i >= 0; i--)
            {
                RegisteredTarget registered = _targets[i];
                if (registered.Target == null)
                {
                    _targetIDs.Remove(registered.TargetID);
                    _targets.RemoveAt(i);
                    continue;
                }

                registered.Drawable?.DrawDebug(
                    new DebugVisualizationContextAdapter(registered.Target, backend));

                foreach (InspectedMember member in registered.Members)
                    member.Draw(registered.Target, backend);
            }
        }

        private IReadOnlyList<InspectedMember> GetInspectedMembers(Type type)
        {
            if (_memberCache.TryGetValue(type, out IReadOnlyList<InspectedMember> members))
                return members;

            var result = new List<InspectedMember>();

            foreach (FieldInfo field in type.GetFields(MemberFlags))
                TryAdd(result, field, field.FieldType, target => field.GetValue(target));

            foreach (PropertyInfo property in type.GetProperties(MemberFlags))
            {
                if (property.GetIndexParameters().Length > 0 || property.GetMethod == null)
                    continue;

                TryAdd(
                    result,
                    property,
                    property.PropertyType,
                    CreatePropertyGetter(property));
            }

            IReadOnlyList<InspectedMember> cachedMembers = result.AsReadOnly();
            _memberCache.Add(type, cachedMembers);
            return cachedMembers;
        }

        private static void TryAdd(
            ICollection<InspectedMember> members,
            MemberInfo member,
            Type valueType,
            Func<object, object> getter)
        {
            DebugValueAttribute valueAttribute = member.GetCustomAttribute<DebugValueAttribute>();
            if (valueAttribute != null)
            {
                members.Add(InspectedMember.Value(member.Name, valueAttribute, getter));
                return;
            }

            DebugRadiusAttribute radiusAttribute = member.GetCustomAttribute<DebugRadiusAttribute>();
            if (radiusAttribute != null && IsNumeric(valueType))
                members.Add(InspectedMember.Radius(member.Name, radiusAttribute, getter));
        }

        private static bool IsNumeric(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;

            return type == typeof(byte)
                   || type == typeof(sbyte)
                   || type == typeof(short)
                   || type == typeof(ushort)
                   || type == typeof(int)
                   || type == typeof(uint)
                   || type == typeof(long)
                   || type == typeof(ulong)
                   || type == typeof(float)
                   || type == typeof(double)
                   || type == typeof(decimal);
        }

        private static Func<object, object> CreatePropertyGetter(PropertyInfo property)
        {
            try
            {
                MethodInfo factory = CreateTypedPropertyGetterMethod.MakeGenericMethod(
                    property.DeclaringType,
                    property.PropertyType);
                return (Func<object, object>)factory.Invoke(
                    null,
                    new object[] { property.GetMethod });
            }
            catch (Exception)
            {
                return target => property.GetValue(target);
            }
        }

        private static Func<object, object> CreateTypedPropertyGetter<TTarget, TValue>(
            MethodInfo getterMethod)
        {
            var getter = (Func<TTarget, TValue>)getterMethod.CreateDelegate(
                typeof(Func<TTarget, TValue>));
            return target => getter((TTarget)target);
        }

        private readonly struct RegisteredTarget
        {
            public readonly Object Target;
            public readonly IReadOnlyList<InspectedMember> Members;
            public readonly IDebugDrawable Drawable;
            public readonly int TargetID;

            public RegisteredTarget(
                Object target,
                IReadOnlyList<InspectedMember> members,
                IDebugDrawable drawable)
            {
                Target = target;
                Members = members;
                Drawable = drawable;
                TargetID = target.GetInstanceID();
            }
        }

        private readonly struct InspectedMember
        {
            private readonly string _label;
            private readonly string _channel;
            private readonly bool _isRadius;
            private readonly bool _isFilledRadius;
            private readonly DebugVisualizationValueDisplay _display;
            private readonly float _worldYOffset;
            private readonly Func<object, object> _getter;

            private InspectedMember(
                string fallbackLabel,
                string label,
                string channel,
                bool isRadius,
                bool isFilledRadius,
                DebugVisualizationValueDisplay display,
                float worldYOffset,
                Func<object, object> getter)
            {
                _label = string.IsNullOrWhiteSpace(label) ? fallbackLabel : label;
                _channel = string.IsNullOrWhiteSpace(channel)
                    ? DebugVisualizationUtility.DefaultChannel
                    : channel;
                _isRadius = isRadius;
                _isFilledRadius = isFilledRadius;
                _display = display;
                _worldYOffset = worldYOffset;
                _getter = getter;
            }

            public static InspectedMember Value(
                string fallbackLabel,
                DebugValueAttribute attribute,
                Func<object, object> getter)
            {
                return new InspectedMember(
                    fallbackLabel,
                    attribute.Label,
                    attribute.Channel,
                    false,
                    false,
                    attribute.Display,
                    attribute.WorldYOffset,
                    getter);
            }

            public static InspectedMember Radius(
                string fallbackLabel,
                DebugRadiusAttribute attribute,
                Func<object, object> getter)
            {
                return new InspectedMember(
                    fallbackLabel,
                    attribute.Label,
                    attribute.Channel,
                    true,
                    attribute.Filled,
                    attribute.ValueDisplay,
                    attribute.WorldYOffset,
                    getter);
            }

            public void Draw(Object target, IDebugVisualizationBackend backend)
            {
                object value;
                try
                {
                    value = _getter(target);
                }
                catch (Exception exception)
                {
                    DrawValue(target, exception.GetType().Name, backend);
                    return;
                }

                DrawValue(target, value, backend);
                if (!_isRadius
                    || !TryGetPosition(target, out Vector3 position)
                    || !TryConvertFloat(value, out float radius))
                {
                    return;
                }

                DebugVisualizationStyleData style = backend.GetChannelStyle(_channel);
                DebugVisualizationDrawCommand command = _isFilledRadius
                    ? DebugVisualizationDrawCommand.Disc(
                        position,
                        radius,
                        _channel,
                        style,
                        0f,
                        64)
                    : DebugVisualizationDrawCommand.Circle(
                        position,
                        radius,
                        _channel,
                        style,
                        0f,
                        64);
                backend.Add(command);
            }

            private void DrawValue(
                Object target,
                object value,
                IDebugVisualizationBackend backend)
            {
                if ((_display & DebugVisualizationValueDisplay.Hud) != 0)
                    backend.AddPersistentValue(target, _channel, _label, value);

                if ((_display & DebugVisualizationValueDisplay.World) == 0
                    || !TryGetPosition(target, out Vector3 position))
                {
                    return;
                }

                Vector3 labelPosition = position + Vector3.up * Mathf.Max(0f, _worldYOffset);
                backend.AddPersistentWorldLabel(
                    target,
                    _channel,
                    _label,
                    value,
                    labelPosition);
            }

            private static bool TryGetPosition(Object target, out Vector3 position)
            {
                if (target is Component component)
                {
                    position = component.transform.position;
                    return true;
                }

                if (target is GameObject gameObject)
                {
                    position = gameObject.transform.position;
                    return true;
                }

                position = default;
                return false;
            }

            private static bool TryConvertFloat(object value, out float result)
            {
                try
                {
                    result = Convert.ToSingle(value);
                    return true;
                }
                catch (Exception)
                {
                    result = 0f;
                    return false;
                }
            }
        }
    }
}
