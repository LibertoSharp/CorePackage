using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class LibertoDebug : LazySingleton<LibertoDebug>
{
    private readonly List<(string Name, Func<object> Getter)> _watches = new();
    public IReadOnlyList<(string Name, Func<object> Getter)> Watches => _watches;

    private void Start()
    {
        #if UNITY_EDITOR
            LibertoDebugWindow.ShowWindow();
        #endif
    }

    public void Register(string name, Func<object> getter)
    {
        _watches.Add((name, getter));
    }
}
