using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BindableList<T>
{
    private List<T> _collection;
    
    private HashSet<T> _cachedNew = new HashSet<T>();
    private HashSet<T> _cachedOld = new HashSet<T>();

    public event Action<T> OnItemAdded;
    public event Action<T> OnItemRemoved;

    public List<T> Collection => _collection;
    public bool EnableDuplicates = true;

    public BindableList(List<T> defaultValue = null)
    {
        _collection = defaultValue ?? new List<T>();
    }

    public void Add(T element)
    {
        if (!EnableDuplicates && _collection.Contains(element))
            return;

        _collection.Add(element);
        OnItemAdded?.Invoke(element);
    }

    public void AddRange(IEnumerable<T> Elements)
    {
        foreach (T element in Elements)
            Add(element);
    }

    public void SilentAdd(T element)
    {
        _collection.Add(element);
    }

    public void SilentAddRange(IEnumerable<T> Elements)
    {
        foreach (T element in Elements)
            SilentAdd(element);
    }

    public void SilentRemove(T Element)
    {
        _collection.Remove(Element);
    }

    public void Remove(T Element)
    {
        _collection.Remove(Element);
        OnItemRemoved?.Invoke(Element);
    }

    public void SilentRemoveRange(IEnumerable<T> Elements)
    {
        foreach (T element in Elements)
            SilentRemove(element);
    }

    public bool Contains(T element) => _collection.Contains(element);

    public void Set(IEnumerable<T> elements)
    {
        _cachedNew.Clear();
        _cachedNew.UnionWith(elements);

        _cachedOld.Clear();
        _cachedOld.UnionWith(_collection);

        foreach (T el in elements)
        {
            if (!_cachedOld.Contains(el))
                OnItemAdded?.Invoke(el);
        }

        foreach (T el in _collection)
        {
            if (!_cachedNew.Contains(el))
                OnItemRemoved?.Invoke(el);
        }

        _collection.Clear();
        _collection.AddRange(elements);
    }

    public IEnumerator<T> GetEnumerator() => _collection.GetEnumerator();
}

