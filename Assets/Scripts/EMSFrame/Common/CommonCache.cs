using System;
using System.Text;
using System.Collections.Generic;

namespace UnityFrame
{
	internal static class StrBuilderCache
	{
		// Static Fields
		[ThreadStatic]
		private static StringBuilder m_Cache = new StringBuilder ();

		// Static Methods
		public static StringBuilder Acquire (int capacity = 512)
		{
			StringBuilder cache = StrBuilderCache.m_Cache;
			if (cache != null) {
				StrBuilderCache.m_Cache = null;
				cache.Remove (0, cache.Length);
				return cache;
			}
			return new StringBuilder (capacity);
		}

		public static string GetStringAndRelease (StringBuilder sb)
		{
			string ret = sb.ToString ();
			StrBuilderCache.Release (sb);
			return ret;
		}

		public static void Release (StringBuilder sb)
		{
			StrBuilderCache.m_Cache = sb;
		}
	}

	internal static class ListCache<T>
	{
		// Static Fields
		[ThreadStatic]
		private static List<T> m_Cache = new List<T> ();

		// Static Methods
		public static List<T> Acquire ()
		{
			List<T> cache = ListCache<T>.m_Cache;
			if (cache != null) {
				ListCache<T>.m_Cache = null;
				cache.Clear ();
				return cache;
			}
			return new List<T> ();
		}
			
		public static void Release (List<T> list)
		{
			ListCache<T>.m_Cache = list;
		}
	}


	internal static class DictionaryCache<K,V>
	{
		// Static Fields
		[ThreadStatic]
		private static Dictionary<K,V> m_Cache = new Dictionary<K,V> ();

		// Static Methods
		public static Dictionary<K,V> Acquire ()
		{
			Dictionary<K,V> cache = DictionaryCache<K,V>.m_Cache;
			if (cache != null) {
				DictionaryCache<K,V>.m_Cache = null;
				cache.Clear ();
				return cache;
			}
			return new Dictionary<K,V> ();
		}

		public static void Release (Dictionary<K,V> list)
		{
			DictionaryCache<K,V>.m_Cache = list;
		}
	}

    internal static class HashCache<K>
    {
        // Static Fields
        [ThreadStatic]
        private static HashSet<K> m_Cache = new HashSet<K>();

        // Static Methods
        public static HashSet<K> Acquire()
        {
            HashSet<K> cache = HashCache<K>.m_Cache;
            if (cache != null)
            {
                HashCache<K>.m_Cache = null;
                cache.Clear();
                return cache;
            }
            return new HashSet<K>();
        }

        public static void Release(HashSet<K> list)
        {
            HashCache<K>.m_Cache = list;
        }
    }


    internal class ObjectPool<T> where T : new()
	{
        //需要改为弱引用
        private readonly TStack<T> m_Stack = new TStack<T>(256);

		public int countAll { get; private set; }
		public int countActive { get { return countAll - countInactive; } }
		public int countInactive { get { return m_Stack.Count; } }

		public ObjectPool(){}

		public ObjectPool(int max){
			m_Stack.MaxCount = max;
		}

		public T Get()
		{
			T element;
			if (m_Stack.Count == 0)
			{
				element = new T();
				countAll++;
			}
			else
			{
				element = m_Stack.Pop();
			}
			return element;
		}

		public void Release(T element)
		{
			if (element == null)
				return;
			if (m_Stack.Count > 0 && ReferenceEquals(m_Stack.Peek(), element))
				Debugger.UF_Error("Internal error. Trying to destroy object that is already released to pool.");
			m_Stack.Push(element);
		}
	}

	// List Object pool
	internal static class ListPool<T>
	{
		private static readonly ObjectPool<List<T>> s_ListPool = new ObjectPool<List<T>>();

		public static int Count{get{ return s_ListPool.countInactive;}}

		public static List<T> Get()
		{
			return s_ListPool.Get();
		}

		public static void Release(List<T> toRelease)
		{
			if (toRelease != null) {
				toRelease.Clear ();
				s_ListPool.Release(toRelease);	
			}
		}
	}

	// Dictionary Object pool
	internal static class DictionaryPool<K,V>
	{
		private static readonly ObjectPool<Dictionary<K,V>> s_ListPool = new ObjectPool<Dictionary<K,V>>();
		public static int Count{get{ return s_ListPool.countInactive;}}
		public static Dictionary<K,V> Get()
		{
			return s_ListPool.Get();
		}

		public static void Release(Dictionary<K,V> toRelease)
		{
			if (toRelease != null) {
				toRelease.Clear ();
				s_ListPool.Release(toRelease);	
			}
		}

	}

	// Queue Object pool
	internal static class QueuePool<T>
	{
		private static readonly ObjectPool<Queue<T>> s_ListPool = new ObjectPool<Queue<T>>();
		public static int Count{get{ return s_ListPool.countInactive;}}
		public static Queue<T> Get()
		{
			return s_ListPool.Get();
		}

		public static void Release(Queue<T> toRelease)
		{
			if (toRelease != null) {
				toRelease.Clear ();
				s_ListPool.Release(toRelease);	
			}
		}
	}

}
