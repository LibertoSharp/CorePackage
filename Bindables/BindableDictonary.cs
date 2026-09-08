using System;
using System.Collections.Generic;
using UnityEngine;

public class BindableDictonary<T, T2>
{
    private Dictionary<T, T2> _collection;
    
    public event Func<T, T2, bool> OnItemAdded;
    public event Func<T, T2, bool> OnItemRemoved;

    public Dictionary<T, T2> Collection => _collection;

    public BindableDictonary(Dictionary<T, T2> defaultValue = null)
    {
        _collection = defaultValue ?? new Dictionary<T, T2>();
    }

    public void Add(T key, T2 value)
    {
        if (_collection.ContainsKey(key))
            return;

        if (OnItemAdded?.Invoke(key, value) ?? true)
            _collection.Add(key, value);
    }

    public void Remove(T key)
    {
        if (!_collection.ContainsKey(key))
            return;

        T2 value = _collection[key];
        if (OnItemRemoved?.Invoke(key, value) ?? true)
            _collection.Remove(key);
    }

    public T2 GetValue(T key)
    {
        return _collection[key];
    }

    private HashSet<T> _keysToRemove = new HashSet<T>();

    public void Set(IEnumerable<T> elements, Func<T, T2> ktov, Func<T2, bool> canRemove = null)
    {
        _keysToRemove.Clear();
        _keysToRemove.UnionWith(_collection.Keys);

        foreach (var key in elements)
        {
            T2 value = ktov(key);
            
            if (!_keysToRemove.Remove(key))
            {
                if (OnItemAdded?.Invoke(key, value) ?? true)
                    _collection[key] = value;
            }
            else
            {
                T2 oldValue = _collection[key];
                if (!EqualityComparer<T2>.Default.Equals(oldValue, value))
                {
                    bool allowRemove = canRemove == null || canRemove(oldValue);
                    bool confirmRemove = allowRemove && (OnItemRemoved?.Invoke(key, oldValue) ?? true);
                    
                    if (confirmRemove)
                    {
                        if (OnItemAdded?.Invoke(key, value) ?? true)
                            _collection[key] = value;
                        else
                            _collection.Remove(key);
                    }
                }
            }
        }

        foreach (var key in _keysToRemove)
        {
            T2 oldValue = _collection[key];
            
            bool allowRemove = canRemove == null || canRemove(oldValue);
            bool confirmRemove = allowRemove && (OnItemRemoved?.Invoke(key, oldValue) ?? true);
            
            if (confirmRemove)
            {
                _collection.Remove(key);
            }
        }
    }
    public bool ContainsKey(T key) => _collection.ContainsKey(key);
    public bool ContainsValue(T2 value) => _collection.ContainsValue(value);

    public Dictionary<T, T2>.Enumerator GetEnumerator() => _collection.GetEnumerator();

    public void SilentRemove(T key)
    {
        if (!_collection.ContainsKey(key))
            return;

        _collection.Remove(key);
    }

    public void AddRange(IEnumerable<T> elements, Func<T, T2> ktov)
    {
        foreach(T key in elements)
        {
            T2 value = ktov(key);
            Add(key, value);
        }
    }
}