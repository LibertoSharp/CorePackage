using System;
using UnityEditor;
using UnityEngine;
using static ShowIfAttribute;

[CustomPropertyDrawer(typeof(ShowIfAttribute))]
public class ShowIfDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (ShouldShow(property))
            EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (ShouldShow(property))
            return EditorGUI.GetPropertyHeight(property, label, true);
        
        return -EditorGUIUtility.standardVerticalSpacing;
    }

    private bool ShouldShow(SerializedProperty property)
    {
        ShowIfAttribute showIf = (ShowIfAttribute)attribute;

        string conditionPath = property.propertyPath.Replace(property.name, showIf.FieldName);
        SerializedProperty conditionProp = property.serializedObject.FindProperty(conditionPath);

        if (conditionProp == null) return true;

        if (conditionProp.propertyType == SerializedPropertyType.Enum)
            return conditionProp.intValue == (int)Convert.ChangeType(showIf.ExpectedValue, typeof(int));

        if (conditionProp.propertyType == SerializedPropertyType.Integer || conditionProp.propertyType == SerializedPropertyType.Float)
        {
            float propValue = System.Convert.ToSingle(conditionProp.boxedValue);
            float expected = System.Convert.ToSingle(showIf.ExpectedValue);

            return showIf.Op switch
            {
                Operator.Equals => Mathf.Approximately(propValue, expected),
                Operator.NotEquals => !Mathf.Approximately(propValue, expected),
                Operator.Greater => propValue > expected,
                Operator.Less => propValue < expected,
                Operator.GreaterOrEqual => propValue >= expected,
                Operator.LessOrEqual => propValue <= expected,
                _ => false
            };
        }
            
        return Equals(conditionProp.boxedValue, showIf.ExpectedValue);
    }
}
