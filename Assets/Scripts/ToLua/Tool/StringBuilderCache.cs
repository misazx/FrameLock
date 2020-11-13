using System;
using System.Text;

namespace LuaInterface {
    public static class StringBuilderCache
    {
        [ThreadStatic]
        private static StringBuilder _cache = new StringBuilder();

        private const int MAX_BUILDER_SIZE = 512;

        public static StringBuilder Acquire(int capacity = 256)
        {
            StringBuilder cache = _cache;
            if (cache != null && cache.Capacity >= capacity)
            {
                _cache = null;
                LuaExtensionMethods.Clear(cache);
                return cache;
            }
            return new StringBuilder(capacity);
        }

        public static string GetStringAndRelease(StringBuilder sb)
        {
            string result = sb.ToString();
            Release(sb);
            return result;
        }

        public static void Release(StringBuilder sb)
        {
            if (sb.Capacity <= 512)
            {
                _cache = sb;
            }
        }
    }

}
