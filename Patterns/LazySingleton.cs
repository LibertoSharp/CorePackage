using Unity.Scripting.LifecycleManagement;
using UnityEngine;

public class LazySingleton<T> : MonoBehaviour where T : LazySingleton<T>
{
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

            _instance = new GameObject(typeof(T).Name).AddComponent<T>();
            _instance.gameObject.hideFlags = HideFlags.HideInHierarchy;
            return _instance;
        }
    }
}