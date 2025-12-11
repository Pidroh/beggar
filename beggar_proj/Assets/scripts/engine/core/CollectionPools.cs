using System;
using System.Collections.Generic;

// Minimal replacement for UnityEngine.Pool.* so the codebase can pool collections
// outside of Unity.
namespace HeartEngineCore
{
    public struct PooledObject<T> : IDisposable
    {
        private readonly ObjectPool<T> _pool;
        private readonly T _value;

        internal PooledObject(T value, ObjectPool<T> pool)
        {
            _value = value;
            _pool = pool;
        }

        public void Dispose()
        {
            _pool.Release(_value);
        }
    }

    public class ObjectPool<T>
    {
        private readonly Stack<T> _stack;
        private readonly Func<T> _createFunc;
        private readonly Action<T>? _actionOnGet;
        private readonly Action<T>? _actionOnRelease;
        private readonly int _maxSize;

        public ObjectPool(
            Func<T> createFunc,
            Action<T>? actionOnGet = null,
            Action<T>? actionOnRelease = null,
            int maxSize = 10000)
        {
            _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            _actionOnGet = actionOnGet;
            _actionOnRelease = actionOnRelease;
            _maxSize = maxSize > 0 ? maxSize : throw new ArgumentOutOfRangeException(nameof(maxSize));
            _stack = new Stack<T>();
        }

        public int CountInactive => _stack.Count;

        public T Get()
        {
            var element = _stack.Count == 0 ? _createFunc() : _stack.Pop();
            _actionOnGet?.Invoke(element);
            return element;
        }

        public PooledObject<T> Get(out T element)
        {
            element = Get();
            return new PooledObject<T>(element, this);
        }

        public void Release(T element)
        {
            _actionOnRelease?.Invoke(element);
            if (_stack.Count < _maxSize)
            {
                _stack.Push(element);
            }
        }

        public void Clear()
        {
            _stack.Clear();
        }
    }

    public static class ListPool<T>
    {
        private static readonly ObjectPool<List<T>> Pool = new ObjectPool<List<T>>(
            createFunc: () => new List<T>(),
            actionOnRelease: l => l.Clear());

        public static List<T> Get()
        {
            return Pool.Get();
        }

        public static PooledObject<List<T>> Get(out List<T> list)
        {
            return Pool.Get(out list);
        }

        public static void Release(List<T> toRelease)
        {
            Pool.Release(toRelease);
        }
    }

    public static class DictionaryPool<TKey, TValue>
        where TKey : notnull
    {
        private static readonly ObjectPool<Dictionary<TKey, TValue>> Pool = new ObjectPool<Dictionary<TKey, TValue>>(
            createFunc: () => new Dictionary<TKey, TValue>(),
            actionOnRelease: d => d.Clear());

        public static Dictionary<TKey, TValue> Get()
        {
            return Pool.Get();
        }

        public static PooledObject<Dictionary<TKey, TValue>> Get(out Dictionary<TKey, TValue> dictionary)
        {
            return Pool.Get(out dictionary);
        }

        public static void Release(Dictionary<TKey, TValue> toRelease)
        {
            Pool.Release(toRelease);
        }
    }
}
