using UnityEngine;

public class LazyResource<T> where T : Object
{
    private string _path;
    private T _value = null;
    public T Value
    {
        get
        {
            if (_value == null)
                _value = Resources.Load<T>(_path);
            return _value;
        }
    }

    public LazyResource(string path)
    {
        _path = path;
    }
}