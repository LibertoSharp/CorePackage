using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

public class LibertoDebugWindow : EditorWindow
{
    private IVisualElementScheduledItem _updateSchedule;
    private VisualTreeAsset _baseModel;
    private VisualTreeAsset _watchElement;

    private readonly List<(Label ValueLabel, Func<object> Getter)> _activeWatches = new();

    private static readonly Color ColorDark = new Color(0.18f, 0.18f, 0.18f, 1f);
    private static readonly Color ColorGray = new Color(0.24f, 0.24f, 0.24f, 1f);

    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        _updateSchedule?.Pause();
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.EnteredEditMode)
        {
            _updateSchedule?.Pause();
            _activeWatches.Clear();
        }
    }

    public static void ShowWindow()
    {
        LibertoDebugWindow wnd = GetWindow<LibertoDebugWindow>();
        wnd.titleContent = new GUIContent("Liberto Debug");
        wnd.RebuildUI();
    }

    public void CreateGUI()
    {
        RebuildUI();

        _activeWatches.Clear();
        _updateSchedule = rootVisualElement.schedule.Execute(() =>
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
        foreach ((string Name, Func<object> Getter) watch in LibertoDebug.Instance.Watches)
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