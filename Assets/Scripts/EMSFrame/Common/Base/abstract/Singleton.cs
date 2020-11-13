//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections;

namespace UnityFrame
{	
	//框架范型单例
	public class HandleSingleton<T> : IFrameHandle where T : new() {
		private static T s_Instance;
		private static readonly object s_LockHelper = new object();

		public static T UF_GetInstance(){
			if (Equals(s_Instance, null)) {
				lock(s_LockHelper){
					if(Equals(s_Instance, null))
                    {
						s_Instance = new T();
					}
				}
			}
			return s_Instance;
		}

        public HandleSingleton()
        {
            this.OnInstanced();
        }

        protected virtual void OnInstanced(){}
	}

    //框架范型单例
    public class Singleton<T> where T : new()
    {
        private static T s_Instance;
        private static readonly object s_LockHelper = new object();
        public static T UF_GetInstance()
        {
            if (Equals(s_Instance, null))
            {
                lock (s_LockHelper)
                {
                    if (Equals(s_Instance, null))
                    {
                        s_Instance = new T();
                    }
                }
            }
            return s_Instance;
        }
    }

}

