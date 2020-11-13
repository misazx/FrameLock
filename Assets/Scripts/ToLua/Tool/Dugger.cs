using LuaInterface;
using System;
using System.Text;
using UnityEngine;

namespace LuaInterface{


    public static class Debugger
    {
        public static bool useLog = true;

        public static string threadStack = string.Empty;

        private static string GetFormat(string str)
        {
            StringBuilder stringBuilder = StringBuilderCache.Acquire();
            DateTime now = DateTime.Now;
            stringBuilder.Append(now.Hour);
            stringBuilder.Append(":");
            stringBuilder.Append(now.Minute);
            stringBuilder.Append(":");
            stringBuilder.Append(now.Second);
            stringBuilder.Append(".");
            stringBuilder.Append(now.Millisecond);
            stringBuilder.Append("-");
            stringBuilder.Append(Time.frameCount % 999);
            stringBuilder.Append(": ");
            stringBuilder.Append(str);
            return StringBuilderCache.GetStringAndRelease(stringBuilder);
        }

        public static void Log(string str)
        {
            str = GetFormat(str);
            if (useLog)
            {
               Debug.Log((object)str);
            }
        }

        public static void Log(object message)
        {
            Log(message.ToString());
        }

        public static void Log(string str, object arg0)
        {
            Log(string.Format(str, arg0));
        }

        public static void Log(string str, object arg0, object arg1)
        {
            Log(string.Format(str, arg0, arg1));
        }

        public static void Log(string str, object arg0, object arg1, object arg2)
        {
            Log(string.Format(str, arg0, arg1, arg2));
        }

        public static void Log(string str, params object[] param)
        {
            Log(string.Format(str, param));
        }

        public static void LogWarning(string str)
        {
            str = GetFormat(str);
            if (useLog)
            {
               Debug.LogWarning((object)str);
            }
        }

        public static void LogWarning(object message)
        {
            LogWarning(message.ToString());
        }

        public static void LogWarning(string str, object arg0)
        {
            LogWarning(string.Format(str, arg0));
        }

        public static void LogWarning(string str, object arg0, object arg1)
        {
            LogWarning(string.Format(str, arg0, arg1));
        }

        public static void LogWarning(string str, object arg0, object arg1, object arg2)
        {
            LogWarning(string.Format(str, arg0, arg1, arg2));
        }

        public static void LogWarning(string str, params object[] param)
        {
            LogWarning(string.Format(str, param));
        }

        public static void LogError(string str)
        {
            str = GetFormat(str);
            if (useLog)
            {
               Debug.LogError((object)str);
            }
        }

        public static void LogError(object message)
        {
            LogError(message.ToString());
        }

        public static void LogError(string str, object arg0)
        {
            LogError(string.Format(str, arg0));
        }

        public static void LogError(string str, object arg0, object arg1)
        {
            LogError(string.Format(str, arg0, arg1));
        }

        public static void LogError(string str, object arg0, object arg1, object arg2)
        {
            LogError(string.Format(str, arg0, arg1, arg2));
        }

        public static void LogError(string str, params object[] param)
        {
            LogError(string.Format(str, param));
        }

        public static void LogException(Exception e)
        {
            threadStack = e.StackTrace;
            string LogFormat = GetFormat(e.Message);
            if (useLog)
            {
               Debug.LogError((object)LogFormat);
            }
        }

        public static void LogException(string str, Exception e)
        {
            threadStack = e.StackTrace;
            str = GetFormat(str + e.Message);
            if (useLog)
            {
               Debug.LogError((object)str);
            }
        }
    }

}