using System;
using com.unity.mgobe.src.Util.Def;
using UnityEngine;

namespace com.unity.mgobe.src.Util {
    public static class Debugger {
        public static bool Enable = false;
        public static Action Callback = null;
        public static void Log (string format, params object[] args) {
            if (!Enable)
                return;
            // Console.WriteLine(String.Format(format, args));
            var str = "[" + RequestHeader.Version + "] " + String.Format (format, args);
            Debug.Log (str);
            Callback?.Invoke ();
        }
    }
}