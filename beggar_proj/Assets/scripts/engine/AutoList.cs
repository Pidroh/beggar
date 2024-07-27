using System.Collections.Generic;

namespace HeartUnity
{
    public class AutoList<T> : List<T>
    {
        private readonly T _defaultValue;
        private readonly bool _useCustomDefault;

        public AutoList()
        {
            _useCustomDefault = false;
        }

        public AutoList(T defaultValue)
        {
            _defaultValue = defaultValue;
            _useCustomDefault = true;
        }

        public new T this[int index]
        {
            get
            {
                while (index >= base.Count)
                {
                    base.Add(_useCustomDefault ? _defaultValue : default(T));
                }
                return base[index];
            }
            set
            {
                while (index >= base.Count)
                {
                    base.Add(_useCustomDefault ? _defaultValue : default(T));
                }
                base[index] = value;
            }
        }
    }

    public class AutoNewList<T> : List<T> where T:new()
    {
        private readonly T _defaultValue;
        private readonly bool _useCustomDefault;

        public AutoNewList()
        {
            _useCustomDefault = false;
        }

        public AutoNewList(T defaultValue)
        {
            _defaultValue = defaultValue;
            _useCustomDefault = true;
        }

        public new T this[int index]
        {
            get
            {
                while (index >= base.Count)
                {
                    base.Add(_useCustomDefault ? _defaultValue : new T());
                }
                return base[index];
            }
            set
            {
                while (index >= base.Count)
                {
                    base.Add(_useCustomDefault ? _defaultValue : new T());
                }
                base[index] = value;
            }
        }
    }
}