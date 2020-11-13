//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections.Generic;
using System.Collections;


namespace UnityFrame
{
	/// <summary>
	/// 类型列表，内部设置Dictionary 
	/// 快速索引查找
	/// 遍历时为列表遍历，保证遍历顺序
	/// </summary>
	public class TypeList<T,V> 
	{
		//val 为表中点索引值
		Dictionary<T,int> m_Map = new Dictionary<T, int> ();
		List<V> m_List = new List<V>();

		public int Count{
			get{
				return m_List.Count;
			}
		}

		public V this[int idx]{
			get{
				return m_List [idx];
			}
			set{ 
				m_List [idx] = value;
			}
		}

		public V UF_GetBaseIndex(int index){
			return m_List [index];	
		}

		public V UF_GetBaseType(T key){
			if (m_Map.ContainsKey (key))
				return m_List [m_Map [key]];
			else
				return default(V);
		}
			

		public bool UF_ContainsKey(T key){
			return m_Map.ContainsKey (key);
		}

		public bool UF_ContainsValue(V val){
			return m_List.Contains (val);
		}


		/// <summary>
		/// 如果已经添加过，则替换
		/// </summary>
		public void UF_Add(T key,V val){
			if (!m_Map.ContainsKey (key)) {
				m_List.Add (val);
				m_Map.Add (key, m_List.Count - 1);
			} else {
				m_List [m_Map [key]] = val;
			}
		}

		/// <summary>
		/// 尝试添加，如果已经存在，则不操作
		/// </summary>
		public bool UF_TryAdd(T key,V val){
			if (!m_Map.ContainsKey (key)) {
				m_List.Add (val);
				m_Map.Add (key, m_List.Count - 1);
				return true;
			} 
			return false;
		}


		public void UF_Remove(V val){
			int idx = m_List.IndexOf (val);
            UF_RemoveAt(idx);
		}

		public void UF_Remove(T key){
			if (!m_Map.ContainsKey (key))
				return;
			int idx = m_Map [key];
			m_Map.Remove (key);
			m_List.RemoveAt (idx);
		}

		public void UF_RemoveAt(int idx){
			if (idx > -1) {
				m_List.RemoveAt (idx);
				T key = default(T);
				foreach (KeyValuePair<T,int> item in m_Map) {
					if (item.Value == idx) {
						key = item.Key;
						break;
					}
				}
				if(m_Map.ContainsKey(key))
					m_Map.Remove (key);
			}
		}

		public void UF_Clear(){
			m_List.Clear ();
			m_Map.Clear ();
		}


	}
}

