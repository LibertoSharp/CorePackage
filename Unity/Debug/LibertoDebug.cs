using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

/// Values shown in the Liberto Debug window. Editor only: every call is stripped from player builds.
/// [ExposedField] fields of MonoBehaviours in a loaded scene are registered automatically;
/// for anything spawned at runtime (or that isn't a MonoBehaviour) call ExposeFields or Expose.
public static class LibertoDebug
{
    public readonly struct Watch
    {
        public readonly string Name;
        public readonly Func<object> Getter;
        private readonly Object _owner;
        private readonly bool _hasOwner;

        public Watch(string name, Func<object> getter, Object owner)
        {
            Name = name;
            Getter = getter;
            _owner = owner;
            _hasOwner = owner != null;
        }

        // Unity's == reports destroyed objects as null
        public bool IsAlive => !_hasOwner || _owner != null;
        public bool IsOwnedBy(Object owner) => _hasOwner && ReferenceEquals(_owner, owner);
    }

    private static readonly List<Watch> _watches = new();
    private static readonly Dictionary<Type, FieldInfo[]> _exposedFieldsCache = new();

    public static IReadOnlyList<Watch> Watches => _watches;
    /// Raised whenever watches are added or removed
    public static event Action Changed;

    /// Shows getter's value under name. The watch is removed automatically once owner is destroyed.
    [Conditional("UNITY_EDITOR")]
    public static void Expose(string name, Func<object> getter, Object owner = null)
    {
        _watches.Add(new Watch(name, getter, owner));
        Changed?.Invoke();
    }

    /// Exposes every [ExposedField] field of target. Structs get boxed, so they are a snapshot taken now:
    /// reference fields (like a stats object) still show live data, plain value fields won't update.
    [Conditional("UNITY_EDITOR")]
    public static void ExposeFields(object target, Object owner = null, string prefix = "")
    {
        foreach (FieldInfo field in GetExposedFields(target.GetType()))
            Expose(prefix + field.Name, () => field.GetValue(target), owner);
    }

    [Conditional("UNITY_EDITOR")]
    public static void Unexpose(Object owner)
    {
        if (_watches.RemoveAll(w => w.IsOwnedBy(owner)) > 0)
            Changed?.Invoke();
    }

    /// Removes the watches whose owner has been destroyed
    [Conditional("UNITY_EDITOR")]
    public static void RemoveDead()
    {
        if (_watches.RemoveAll(w => !w.IsAlive) > 0)
            Changed?.Invoke();
    }

    [Conditional("UNITY_EDITOR")]
    public static void Clear()
    {
        _watches.Clear();
        Changed?.Invoke();
    }

    private static FieldInfo[] GetExposedFields(Type type)
    {
        if (_exposedFieldsCache.TryGetValue(type, out FieldInfo[] fields))
            return fields;

        var list = new List<FieldInfo>();
        for (Type t = type; t != null; t = t.BaseType)
        {
            foreach (FieldInfo field in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (field.IsDefined(typeof(ExposedFieldAttribute), true))
                    list.Add(field);
            }
        }

        return _exposedFieldsCache[type] = list.ToArray();
    }

#if UNITY_EDITOR
    // Static state survives between play sessions when domain reload is disabled, so reset it on every play
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay()
    {
        _watches.Clear();
        SceneManager.sceneLoaded -= ExposeScene;
        SceneManager.sceneLoaded += ExposeScene;
        Changed?.Invoke();
    }

    private static void ExposeScene(Scene scene, LoadSceneMode mode)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (MonoBehaviour mono in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                // null when the script is missing
                if (mono != null)
                    ExposeFields(mono, mono);
            }
        }
    }
#endif
}
