//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections.Generic;
using System.Collections;

namespace UnityFrame
{
	//自定义的简单范型对象栈
	//可用于缓存可重复使用的对象
	//可以直接移除栈中指定元素
	//可以直接移除栈中指定索引元素
	public class TStack<T> : IEnumerable,System.IDisposable
	{
		private int m_TotalCount;
		private int m_MaxCount;
		/// 栈元素索引指向针
		private int m_StackIdxPtr = 0;

		private T[] m_Objects;

		public T[] Elements{get{return m_Objects;}}

		public int Count{get{return m_TotalCount;}}

		public int MaxCount{get{return m_MaxCount;}set{m_MaxCount = value;}}

		/// <summary>
		/// 获取当前元素指向针指向的元素
		/// </summary>
		public T Current{
			get{ 
				if (m_StackIdxPtr >= 0) {
					return m_Objects [m_StackIdxPtr];
				} else {
					Debugger.UF_Error("StackIdxPrr is out of Index");
					return default(T);
				}
			}
		}

		public IEnumerator GetEnumerator (){
			return m_Objects.GetEnumerator ();
		}

		public TStack (int maxCount)
		{
			m_Objects = new T[maxCount];
			m_MaxCount = maxCount;
		}

		public bool Push(T param){
			if (m_TotalCount >= m_MaxCount) {
                //Debugger.UF_Error (string.Format ("TStack<{0}> is Full[{1}]", typeof(T).ToString(),m_MaxCount));
                Debugger.UF_Warn(string.Format("TStack<{0}> is Full[{1}]", typeof(T).ToString(), m_MaxCount));
                return false;
			} else {
				m_Objects [m_TotalCount] = param;
				m_TotalCount++;
				return true;
			}
		}

		//检查元素是否存在于栈中
		public bool ContainsValue(T param){
			if (m_TotalCount != 0) {
				for (int k = 0; k < m_TotalCount; k++) {
					if (param.Equals (m_Objects [k])) {
						return true;
					}
				}
			}
			return false;
		}



		/// <summary>
		/// 把栈内的指定值设置到栈顶中
		/// </summary>
		public bool SetTop(T value){
			for (int k = 0; k < m_TotalCount; k++) {
				if (value.Equals (m_Objects [k])) {
					T temp = m_Objects [k];
					RemoveAt (k + 1);
					Push (temp);
					return true;
				}
			}
			return false;
		}


		/// <summary>
		/// 索引栈顶的值
		/// </summary>
		public T GetTop(){
			if (m_TotalCount == 0)
				return default(T);
			return m_Objects [m_TotalCount - 1];
		}

		public T Peek(){
			return GetTop ();
		}

		/// <summary>
		/// 重置栈元素索引针到指定位置,pos = 0 标示栈顶
		/// </summary>
		public void Seek(int pos = 0){
			m_StackIdxPtr = System.Math.Max(0,m_TotalCount - 1 - pos);
			m_StackIdxPtr = System.Math.Min (m_TotalCount - 1, m_StackIdxPtr);
		}


		/// <summary>
		/// 下移栈元素索引针,如果指向有值，则返回true
		/// </summary>
		public bool MoveNext(){
			return --m_StackIdxPtr >= 0;
		}
			
		/// <summary>
		/// 把指定索引设为栈顶
		/// 如果并把超过栈顶的元素全部出栈,例如SetTopAt(0),则全部元素出栈
		/// </summary>
		public bool SetTopAt(int index){
			if (System.Math.Abs (index) > m_TotalCount) {
				//超出索引
				Debugger.UF_Warn(string.Format("TStack.SetTopAt({0}) out of range, TotalCount[{1}]",index,m_TotalCount));
				return false;
			}

			if (index == -1 || index == m_TotalCount - 1) {
				//do not need to do anything
				return true;
			}

			int realIndex = 0;
			if (index > 0) {
				realIndex = index - 1;
			} else if (index < 0) {
				realIndex = m_TotalCount + index;
			}

			for (int k = realIndex+1; k < m_TotalCount; k++) {
				m_Objects [k] = default(T);
			}

			m_TotalCount = realIndex + 1;

			return true;
		}

		/// <summary>
		/// 把指定元素设为栈顶
		/// 如果并把超过栈顶的元素全部出栈,例如SetTopAt(0),则全部元素出栈
		/// </summary>
		public bool SetTopAt(T param){
			if (param.Equals(default(T)) || m_TotalCount == 0) {
				return false;
			}
			bool mark = false;
			int realIndex = 0;
			for(int k = 0;k < m_TotalCount;k++){
				if (mark) {
					m_Objects [k] = default(T);
				} else {
					if (m_Objects [k].Equals (param)) {
						mark = true;	
						realIndex = k;
					}
				}
			}
			if (mark) {
				m_TotalCount = realIndex + 1;
				return true;
			} else
				return false;

		}

		public bool CheckInStack(T param){
			for (int k = 0; k < m_TotalCount; k++) {
				if (m_Objects[k].Equals (param)) {
					return true;
				}
			}
			return false;
		}

		//移除指对象对象
		//从栈顶开始
		public bool Remove(T param){
			int idx = -1;
			for (int k = 0; k < m_TotalCount; k++) {
				if (m_Objects[k].Equals (param)) {
					idx = k;
					break;
				}
			}
			if (idx != -1) {
				for (int k = idx + 1; k < m_TotalCount; k++) {
					m_Objects [k - 1] = m_Objects [k];
				}
				m_Objects [--m_TotalCount] = default(T);
				return true;
			} else {
				return false;
			}
		}

		/// <summary>
		/// 移除指定索引元素
		/// 如果索引为负数，则从栈顶开始索引,起始数为-1
		/// 如果为正数，则从栈底开始索引，起始数为1
		/// </summary>
		public bool RemoveAt(int index){
			if (System.Math.Abs (index) > m_TotalCount) {
				//超出索引
				Debugger.UF_Warn(string.Format("TStack.RemoveAt({0}) out of range,TotalCount[{1}]",index,m_TotalCount));
				return false;
			}

			int realIndex = 0;

			if (index > 0) {
				realIndex = index - 1;
			} else if (index < 0) {
				realIndex = m_TotalCount + index;
			} else {
				Debugger.UF_Warn("TStack.RemoveAt(0) is Invalid");
				return false;
			}

			m_Objects [realIndex] = default(T);

			int tmpCount = m_TotalCount-1;
			for (int k = realIndex; k < tmpCount; k++) {
				m_Objects [k] = m_Objects [k + 1];
			}

			m_Objects [--m_TotalCount] = default(T);

			return true;
		}


		public T Pop(){
			if (m_TotalCount > 0) {
				int index = m_TotalCount - 1;
				T ret = m_Objects [index];
				m_Objects [index] = default(T);
				--m_TotalCount;
				return ret;
			}
			else
				return default(T);
		}


		public void Clear(){
			for (int k = 0; k < m_MaxCount; k++) {
				m_Objects [k] = default(T);
			}
			m_TotalCount = 0;
		}

		public void Dispose(){
			Clear ();
		}

		~TStack(){
			Dispose ();	
		}

	}
}

