//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System;
using System.Text;
using System.Collections.Generic;

namespace UnityFrame
{
	//自带对象池的基类
	//用于自身对象内存管理
	public class PoolHandle<T> where T : new()
	{
		protected static TStack<T> Pool = new TStack<T>(256);

		public static int Count{get{ return Pool.Count;}}

		public static T Create(UILabel label){
			T ret = Pool.Pop ();
			if (ret == null) {
				ret = new T();
			}
			return ret;
		}

		public static void UF_Recover(T handle){
			Pool.Push (handle);
		}

		public static void UF_Clear(){
			Pool.Clear ();
		}

		public static String UF_ToInformation ()
		{
			return string.Format ("RichTextHandle[{0}] PoolCount:{1}",typeof(T).ToString(),Pool.Count);
		}

	}
}

