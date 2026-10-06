using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Object = UnityEngine.Object;

/// Displays the values registered in LibertoDebug while in play mode
public class LibertoDebugWindow : EditorWindow
{
    private const int REFRESH_MS = 100;
    private static readonly Color ColorDark = new Color(0.18f, 0.18f, 0.18f, 1f);
    private static readonly Color ColorGray = new Color(0.24f, 0.24f, 0.24f, 1f);
    private static readonly Dictionary<Type, PropertyInfo[]> _displayPropertiesCache = new();
    private static bool _autoOpenedThisSession;

    private VisualTreeAsset _baseModel;
    private VisualTreeAsset _watchElement;
    private readonly List<(Label ValueLabel, Func<object> Getter)> _activeWatches = new();
    private bool _dirty = true;

    [InitializeOnLoadMethod]
    private static void RegisterAutoOpen()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.ExitingEditMode)
                _autoOpenedThisSession = false;
        };

        // Open the window the first time something gets exposed, but only once per play session so closing it sticks
        LibertoDebug.Changed += () =>
        {
            if (_autoOpenedThisSession || LibertoDebug.Watches.Count == 0)
                return;

            _autoOpenedThisSession = true;
            if (!HasOpenInstances<LibertoDebugWindow>())
                ShowWindow();
        };
    }

    [MenuItem("Window/Liberto Debug")]
    public static void ShowWindow()
    {
        // Don't steal focus from the Game view
        GetWindow<LibertoDebugWindow>(false, "Liberto Debug", false);
    }

    private void OnEnable()
    {
        LibertoDebug.Changed += MarkDirty;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnDisable()
    {
        LibertoDebug.Changed -= MarkDirty;
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    private void MarkDirty() => _dirty = true;

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // The getters point at scene objects that are about to be destroyed
        if (state == PlayModeStateChange.ExitingPlayMode)
            LibertoDebug.Clear();
    }

    public void CreateGUI()
    {
        // Load the UXML from next to this script, so it keeps working if the package is moved or renamed
        string scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
        string uxmlFolder = Path.GetDirectoryName(scriptPath).Replace('\\', '/') + "/UXML";
        _baseModel = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlFolder + "/DebugScreen.uxml");
        _watchElement = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlFolder + "/RegisteredValue.uxml");

        if (_baseModel == null || _watchElement == null)
        {
            rootVisualElement.Add(new Label($"Liberto Debug: UXML files not found in {uxmlFolder}"));
            return;
        }

        _dirty = true;
        rootVisualElement.schedule.Execute(Refresh).Every(REFRESH_MS);
    }

    private void Refresh()
    {
        LibertoDebug.RemoveDead();
        if (_dirty)
            Rebuild();

        foreach (var (label, getter) in _activeWatches)
            label.text = Format(getter);
    }

    private void Rebuild()
    {
        _dirty = false;
        _activeWatches.Clear();
        rootVisualElement.Clear();

        TemplateContainer screen = _baseModel.Instantiate();
        screen.style.flexGrow = 1;
        rootVisualElement.Add(screen);

        VisualElement container = rootVisualElement.Q("watches_container");
        if (container == null)
            return;

        IReadOnlyList<LibertoDebug.Watch> watches = LibertoDebug.Watches;
        for (int i = 0; i < watches.Count; i++)
        {
            VisualElement element = _watchElement.Instantiate();
            element.style.backgroundColor = i % 2 == 0 ? ColorDark : ColorGray;
            element.Q<Label>("name").text = watches[i].Name;

            container.Add(element);
            _activeWatches.Add((element.Q<Label>("value"), watches[i].Getter));
        }
    }

    private static string Format(Func<object> getter)
    {
        object value;
        try { value = getter(); }
        catch (Exception e) { return e.GetType().Name; }

        if (value == null)
            return "null";
        if (value is Object unityObject && unityObject == null)
            return "destroyed";

        // Types with their own ToString (numbers, vectors, Bindables...) show that,
        // everything else (like a stats ScriptableObject) lists its properties
        Type type = value.GetType();
        if (OverridesToString(type))
            return value.ToString();

        PropertyInfo[] properties = GetDisplayProperties(type);
        if (properties.Length == 0)
            return value.ToString();

        var sb = new StringBuilder();
        foreach (PropertyInfo property in properties)
        {
            if (sb.Length > 0)
                sb.Append('\n');

            sb.Append(property.Name).Append(": ");
            try { sb.Append(property.GetValue(value)); }
            catch (Exception e) { sb.Append(e.GetType().Name); }
        }
        return sb.ToString();
    }

    private static bool OverridesToString(Type type)
    {
        Type declaring = type.GetMethod("ToString", Type.EmptyTypes).DeclaringType;
        return declaring != typeof(object) && declaring != typeof(ValueType) && declaring != typeof(Object);
    }

    private static PropertyInfo[] GetDisplayProperties(Type type)
    {
        if (_displayPropertiesCache.TryGetValue(type, out PropertyInfo[] properties))
            return properties;

        // Stop at Unity's own types, their properties (name, hideFlags, transform...) are just noise here
        var list = new List<PropertyInfo>();
        for (Type t = type; t != null && t != typeof(object) && t.Namespace?.StartsWith("UnityEngine") != true; t = t.BaseType)
        {
            foreach (PropertyInfo property in t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (property.CanRead && property.GetIndexParameters().Length == 0)
                    list.Add(property);
            }
        }

        return _displayPropertiesCache[type] = list.ToArray();
    }
}
