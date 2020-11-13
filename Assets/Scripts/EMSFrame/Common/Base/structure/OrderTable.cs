//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections.Generic;
using System.Collections;

namespace UnityFrame
{

	//范型顺序索引表
	//顺序自动递增
	public class OrderTable<V> : System.IDisposable
	{
		//顺序码

		private uint m_OrderValue = 0;

		private Dictionary<uint,V> m_DicTable = new Dictionary<uint,V>();

		public V this[uint id]{
			get{ 
				return UF_Get(id);
			}
		}

		private uint UF_GenUniqueCode(){
			return ++m_OrderValue;
		}

		public uint UF_Add(V value){
			if (!m_DicTable.ContainsValue(value)) {
				uint ret = UF_GenUniqueCode();
				m_DicTable.Add(ret, value);
				return ret;
			}
			return 0;
		}

		public bool UF_Check(uint id){
			return m_DicTable.ContainsKey (id);
		}

		public V UF_Get(uint id){
			if (m_DicTable.ContainsKey(id)) {
				return m_DicTable[id];
			} else {
				return default(V);
			}
		}

		public void UF_Remove(uint id){
			if (m_DicTable.ContainsKey(id)) {
				m_DicTable.Remove(id);
			}
		}

		public void UF_Clear(){
			m_DicTable.Clear();
		}


		public void Dispose (){
			m_DicTable.Clear();
		}

		public void Pair(DelegateType<V> method){
			if (method != null) {
				foreach (KeyValuePair<uint,V> item in m_DicTable) {
					method.Invoke (item.Value);
				}
			}
		}

	}
}

