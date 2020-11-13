using System;
using System.Text;

namespace LuaInterface
{
    public static class LuaExtensionMethods
    {
        public static void Clear(this StringBuilder sb)
        {
            sb.Length = 0;
        }

        public static void AppendLineEx(this StringBuilder sb, string str = "")
        {
            sb.Append(str + "\r\n");
        }
    }
}