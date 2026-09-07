using Unity.Scripting.LifecycleManagement;
using UnityEngine;

public partial class LazySingleton<T> : MonoBehaviour where T : LazySingleton<T>
{
    [AutoStaticsCleanup]
    protected static T _instance = null;
    public static T Instance
    {
        get
        {
            if (_instance != null)
                return _instance;

            _instance = FindAnyObjectByType<T>();
            if (_instance != null)
                return _instance;

            _instance = new GameObject(nameof(T)).AddComponent<T>();
            _instance.gameObject.hideFlags = HideFlags.HideInHierarchy;
            return _instance;
        }
    }
}