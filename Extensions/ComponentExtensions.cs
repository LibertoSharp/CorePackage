using System;
using System.Reflection;
using UnityEngine;

public static class ComponentExtensions
{
    public static T MoveTo<T>(this T original, GameObject target) where T : Component
    {
        // 1. Add new component of the same type to target
        T copy = target.AddComponent<T>();

        // 2. Copy field values via Reflection
        Type type = typeof(T);
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default;
        
        foreach (PropertyInfo prop in type.GetProperties(flags))
        {
            if (prop.CanWrite && prop.Name != "name")
                prop.SetValue(copy, prop.GetValue(original, null), null);
        }
        foreach (FieldInfo field in type.GetFields(flags))
        {
            field.SetValue(copy, field.GetValue(original));
        }

        // 3. Destroy old component
        UnityEngine.Object.DestroyImmediate(original);

        return copy;
    }

    public static T CopyTo<T>(this T original, GameObject target) where T : Component
    {
        // 1. Add new component of the same type to target
        T copy = target.AddComponent<T>();

        // 2. Copy field values via Reflection
        Type type = typeof(T);
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default;
        
        foreach (PropertyInfo prop in type.GetProperties(flags))
        {
            if (prop.CanWrite && prop.Name != "name")
                prop.SetValue(copy, prop.GetValue(original, null), null);
        }
        foreach (FieldInfo field in type.GetFields(flags))
        {
            field.SetValue(copy, field.GetValue(original));
        }

        return copy;
    }
}