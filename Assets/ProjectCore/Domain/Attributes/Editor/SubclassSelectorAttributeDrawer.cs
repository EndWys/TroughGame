using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Domain
{
    [CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
    public sealed class SubclassSelectorAttributeDrawer : PropertyDrawer
    {
        private const float LineSpacing = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUI.PropertyField(position, property, label, true);
                EditorGUI.EndProperty();
                return;
            }

            Rect headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            Rect popupRect = EditorGUI.PrefixLabel(headerRect, label);
            Type[] concreteTypes = GetConcreteTypes();
            string[] typeNames = new string[concreteTypes.Length + 1];
            typeNames[0] = "None";

            for (int i = 0; i < concreteTypes.Length; i++)
            {
                typeNames[i + 1] = concreteTypes[i].FullName;
            }

            int selectedIndex = GetSelectedIndex(property, concreteTypes);
            int nextIndex = EditorGUI.Popup(popupRect, selectedIndex, typeNames);

            if (nextIndex != selectedIndex)
            {
                property.managedReferenceValue = nextIndex == 0
                    ? null
                    : Activator.CreateInstance(concreteTypes[nextIndex - 1]);
                property.serializedObject.ApplyModifiedProperties();
            }

            if (property.managedReferenceValue != null)
            {
                DrawChildren(position, property);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference ||
                property.managedReferenceValue == null)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            float height = EditorGUIUtility.singleLineHeight;
            SerializedProperty child = property.Copy();
            SerializedProperty end = child.GetEndProperty();
            bool enterChildren = true;

            while (child.NextVisible(enterChildren) &&
                   !SerializedProperty.EqualContents(child, end))
            {
                height += LineSpacing + EditorGUI.GetPropertyHeight(child, true);
                enterChildren = false;
            }

            return height;
        }

        private void DrawChildren(Rect position, SerializedProperty property)
        {
            SerializedProperty child = property.Copy();
            SerializedProperty end = child.GetEndProperty();
            bool enterChildren = true;
            float y = position.y + EditorGUIUtility.singleLineHeight + LineSpacing;

            while (child.NextVisible(enterChildren) &&
                   !SerializedProperty.EqualContents(child, end))
            {
                float childHeight = EditorGUI.GetPropertyHeight(child, true);
                Rect childRect = new Rect(position.x, y, position.width, childHeight);
                EditorGUI.PropertyField(childRect, child, true);
                y += childHeight + LineSpacing;
                enterChildren = false;
            }
        }

        private int GetSelectedIndex(SerializedProperty property, Type[] concreteTypes)
        {
            if (property.managedReferenceValue == null)
            {
                return 0;
            }

            Type selectedType = property.managedReferenceValue.GetType();
            int typeIndex = Array.IndexOf(concreteTypes, selectedType);
            return typeIndex < 0 ? 0 : typeIndex + 1;
        }

        private Type[] GetConcreteTypes()
        {
            Type baseType = GetManagedReferenceBaseType(fieldInfo.FieldType);
            IEnumerable<Type> types = TypeCache.GetTypesDerivedFrom(baseType)
                .Where(type => !type.IsAbstract && !type.IsInterface)
                .Where(type => !type.IsGenericType)
                .Where(type => type.IsDefined(typeof(SerializableAttribute), false));

            if (!baseType.IsAbstract && !baseType.IsInterface &&
                baseType.IsDefined(typeof(SerializableAttribute), false))
            {
                types = types.Append(baseType);
            }

            return types
                .Distinct()
                .OrderBy(type => type.FullName)
                .ToArray();
        }

        private static Type GetManagedReferenceBaseType(Type fieldType)
        {
            if (fieldType.IsArray)
            {
                return fieldType.GetElementType();
            }

            if (fieldType.IsGenericType &&
                fieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                return fieldType.GetGenericArguments()[0];
            }

            return fieldType;
        }
    }
}
