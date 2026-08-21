using System;
using Game.Saving;

/// <summary>
/// A simple cache slot that can hold a value of type T. It can be used to cache values that are expensive to compute or retrieve.
/// </summary>
public class CacheSlot<T>
{
    private T _value;
    private bool _hasValue;

    public void Set(T value)
    {
        _value = value;
        _hasValue = true;
    }

    public T Get()
    {
        if (!_hasValue)
            throw new InvalidOperationException($"CacheSlot<{typeof(T).Name}>: no value set.");
        return _value;
    }

    public bool TryGet(out T value)
    {
        value = _value;
        return _hasValue;
    }

    public bool IsNull() => !_hasValue;

    public void Clear()
    {
        _value = default;
        _hasValue = false;
    }
}

/// <summary>
///  A static class that holds cache slots for various types of data.
/// </summary>
public static class Cache
{
    public static CacheSlot<GameSave> SaveInfo = new CacheSlot<GameSave>();
}