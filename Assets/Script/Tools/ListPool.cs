using System.Collections.Generic;

namespace HuaHua
{
    public static class ListPool<T>
    {
        private static Queue<List<T>> mListQueue = new Queue<List<T>>();

        public static List<T> Get()
        {
            if(mListQueue.Count == 0)
            {
                mListQueue.Enqueue(new List<T>());
            }
            return mListQueue.Dequeue();
        }

        public static List<T> Get(List<T> source)
        {
            List<T> temp = Get();
            temp.Clear();
            if(temp.Capacity < source.Capacity) { temp.Capacity = source.Capacity; }
            temp.AddRange(source);
            return temp;
        }
        
        public static List<T> Get(int capacity)
        {
            List<T> temp = Get();
            temp.Clear();
            if(temp.Capacity < capacity) { temp.Capacity = capacity; }
            return temp;
        }

        public static void Release(List<T> list)
        {
            list.Clear();
            mListQueue.Enqueue(list);
        }
    }
}
