using System.Collections.Generic;

namespace HeartUnity
{
    public class Pool<T>
    {
        public List<T> activeObjects = new List<T>();
        public List<T> freeObjects = new List<T>();

        public bool hasFree() {
            return freeObjects.Count != 0;
        }

        public T Activate() {
            var obj = freeObjects[freeObjects.Count - 1];
            freeObjects.RemoveAt(freeObjects.Count-1);
            activeObjects.Add(obj);
            return obj;
        }

        public void FreeAll() {
            foreach (var item in activeObjects)
            {
                freeObjects.Add(item);
            }
            activeObjects.Clear();
        }

        public void AddFree(T freeObject)
        {
            freeObjects.Add(freeObject);
        }
    }
}