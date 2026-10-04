using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Reflection;

public class LibertoDebugWindow : EditorWindow
{
    private static LibertoDebugWindow _instance = null;
    private VisualTreeAsset _baseModel;
    private VisualTreeAsset _watchElement;

    private static readonly List<(string Name, Func<object> Getter)> _exposedVariables = new();
    private readonly List<(Label ValueLabel, Func<object> Getter)> _activeWatches = new();

    private static readonly Color ColorDark = new Color(0.18f, 0.18f, 0.18f, 1f);
    private static readonly Color ColorGray = new Color(0.24f, 0.24f, 0.24f, 1f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInitialize()
    {
        #if UNITY_EDITOR
        _exposedVariables.Clear();

        #if UNITY_2022_2_OR_NEWER
            MonoBehaviour[] sceneActive = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        #else
            MonoBehaviour[] sceneActive = FindObjectsOfType<MonoBehaviour>();
        #endif

	    foreach (MonoBehaviour mono in sceneActive) {
            FieldInfo[] objectFields = mono.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
		    foreach(FieldInfo field in objectFields)
            {
                if (!field.IsDefined(typeof(ExposedFieldAttribute), inherit: true))
                    continue;

                _exposedVariables.Add((field.Name, () => field.GetValue(mono)));
            }
        }

        if (_exposedVariables.Count > 0)
            ShowWindow();
        else
            _instance?.Close();
        #endif
    }

    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            _exposedVariables.Clear();
            _activeWatches.Clear();
            rootVisualElement.Clear(); 
        }
    }

    public static void ShowWindow()
    {
        _instance = GetWindow<LibertoDebugWindow>();
        _instance.titleContent = new GUIContent("Liberto Debug");
        _instance.RebuildUI();
    }

    public void CreateGUI()
    {
        RebuildUI();

        _activeWatches.Clear();
        rootVisualElement.schedule.Execute(() =>
            {
                foreach (var (label, getter) in _activeWatches)
                {
                    if (label != null)
                    {
                        object val = getter();
                        label.text = val != null ? val.ToString() : "null";
                    }
                }
            }).Every(100);
    }

    public void RebuildUI()
    {
        rootVisualElement.Clear();

        if (_baseModel == null)
            _baseModel = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.liberto.core/Unity/Debug/UXML/DebugScreen.uxml");
        if (_watchElement == null)
            _watchElement = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.liberto.core/Unity/Debug/UXML/RegisteredValue.uxml");

        if (_baseModel == null || _watchElement == null)
            return;

        rootVisualElement.Add(_baseModel.Instantiate());
        VisualElement watchesContainer = rootVisualElement.Q("watches_container");
        if (watchesContainer == null)
            return;

        int index = 0;
        foreach ((string Name, Func<object> Getter) watch in _exposedVariables)
        {
            VisualElement watchInstance = _watchElement.Instantiate();

            watchInstance.style.backgroundColor = (index % 2 == 0) ? ColorDark : ColorGray;
            index++;

            watchInstance.Q<Label>("name").text = watch.Name;
            Label valueLabel = watchInstance.Q<Label>("value");

            watchesContainer.Add(watchInstance);
            _activeWatches.Add((valueLabel, watch.Getter));
        }
    }
}