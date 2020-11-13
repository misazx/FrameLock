//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System;
using System.Collections.Generic;

namespace UnityFrame
{

	//pool struct
	/*
	key1 : node1 -> node2 -> node3 -> node4
	|
	key2 : node1 -> node2 -> node3
	|
	key3 : node1 -> node2 -> node3 -> ...
	|
	key4 : node1 -> node2 -> node3 -> node4
	|	
	...
	*/
	public class NPool<K,T> : NCollector<T>
	{
		private int m_Count = 0;

		private Dictionary<K,Node<T>> m_MapNodes = new Dictionary<K, Node<T>>();

		public int Count{get{ return m_Count;}}

		public int KeyCount{get{ return m_MapNodes.Count;}}
	
		public bool KeepHashKey{ get; set;}

		public NPool(){}

		public NPool(bool keeyhashKey){
			KeepHashKey = keeyhashKey;
		}

		private void UF_DeleteKeys(K key){
			if (m_MapNodes.ContainsKey (key)) {
				if (KeepHashKey) {
					m_MapNodes [key] = null;
				} else {
					m_MapNodes.Remove (key);
				}
			}
		}

		public int UF_GetCount(K key){
			if (m_MapNodes.ContainsKey (key) && m_MapNodes [key] != null) {
				Node<T> head = m_MapNodes [key];
				int count = 0;
				while (head != null) {
					count++;
					head = head.next;
				}
				return count;
			}
			return 0;
		}
			
		//返回第一个值
		public T UF_Get(K key){
			if (m_MapNodes.ContainsKey (key) && m_MapNodes [key] != null) {
				return m_MapNodes [key].refer;
			} else {
				return default(T);
			}
		}
			
		public void UF_Add(K key,T value){
			Node<T> node = UF_CreateNode (value);
			if (!m_MapNodes.ContainsKey (key)) {
				m_MapNodes.Add (key, node);
			} else {
				Node<T> head = m_MapNodes [key];
				node.next = head;
				m_MapNodes [key] = node;
			}
		}
			
		public void UF_Remove(K key){
			if (m_MapNodes.ContainsKey (key)  && m_MapNodes[key] !=  null) {
				UF_RecoverNode (m_MapNodes [key]);
				UF_DeleteKeys (key);
			}
		}

		public void UF_Remove(K key,T value){
			if (m_MapNodes.ContainsKey (key) && m_MapNodes[key] !=  null) {
				Node<T> node = m_MapNodes [key];
				Node<T> last = null;
				if (node.next == null && node.refer.Equals(value)) {
					UF_RecoverNode (node);
					UF_DeleteKeys (key);
					return;
				} else {
					last = node;
					node = node.next;
				}
				while (node != null) {
					if (node.refer.Equals (value)) {
						last.next = node.next;
						node.next = null;
						UF_RecoverNode (node);
						node = last.next;
					} else {
						last = node;
						node = node.next;
					}
				}
			}
		}

		public T UF_Pop(K key){
			T ret = default(T);
			if (m_MapNodes.ContainsKey (key) && m_MapNodes[key] !=  null) {
				Node<T> node = m_MapNodes [key];
				ret = node.refer;
				if (node.next == null) {
					UF_RecoverNode (node);
					UF_DeleteKeys (key);
				} else {
					m_MapNodes [key] = node.next;
					node.next = null;
					UF_RecoverNode (node);
				}
			}
			return ret;
		}


		public T UF_Select(K key,DelegateNForeach<T> method){
			if (method != null && m_MapNodes.ContainsKey (key) && m_MapNodes [key] != null) {
				Node<T> node = m_MapNodes [key];
				while (node != null) {
					if (method (node.refer)) {
						return node.refer;
					}
				}
			}
			return default(T);
		}
			
		private void UF_DoForeach(K key,DelegateNForeach<T> method){
			if (!m_MapNodes.ContainsKey (key) || m_MapNodes[key] == null)
				return;
			if (method == null) {
				return;
			}

			Node<T> head = m_MapNodes[key];
			Node<T> node = head;
			Node<T> last = null;

			if (node.next == null) {
				if (!method (node.refer)) {
					this.UF_RecoverNode (node);
					UF_DeleteKeys (key);
				}
				return;
			}
			last = node;

			while (node != null) {
				if (!method (node.refer)) {
					if (node == head) {
						m_MapNodes[key] = node.next;
						head = node.next;
						node.next = null;
						UF_RecoverNode (node);
						node = head;
						last = node;
					} else {
						last.next = node.next;	
						node.next = null;
						UF_RecoverNode (node);
						node = last.next;
					}
				} else {
					last = node;
					node = node.next;
				}
			}
		}

		/// <summary>
		///循环每个节点
		///如果mehod 返回false,则删除该节点
		/// </summary>
		public void UF_NForeach(DelegateNForeach<T> method){
			if (method == null) {
				return;
			}
			List<K> tempList = ListCache<K>.Acquire ();
			foreach (var item in m_MapNodes) {
				if (item.Value != null) {
					tempList.Add (item.Key);
				}
			}
			for (int k = 0; k < tempList.Count; k++) {
                UF_DoForeach(tempList[k], method);
			}
			ListCache<K>.Release (tempList);
		}


		public void NForeach(K key,DelegateNForeach<T> method){
            UF_DoForeach(key,method);
		}

		/// <summary>
		/// 清空全部节点
		/// </summary>
		public void UF_NForeachClear(DelegateNForeach<T> method){
			foreach (K key in m_MapNodes.Keys) {
				Node<T> node = m_MapNodes[key];
				while (node != null) {
					if (method != null) {
						method (node.refer);
					}
					node = node.next;
				}
			}
            UF_Clear();
		}

		protected override Node<T> UF_CreateNode (T value)
		{
			m_Count++;
			return base.UF_CreateNode (value);
		}

		protected override int UF_RecoverNode (Node<T> node)
		{
			int num = base.UF_RecoverNode (node);
			m_Count -= num;
			return num;
		}

		public void UF_Clear(){
			foreach (Node<T> value in m_MapNodes.Values) {
				UF_RecoverNode (value);
			}
			m_MapNodes.Clear ();
			m_Count = 0;
		}

		public override void Dispose ()
		{
			this.UF_Clear();
			m_MapNodes = null;
			base.Dispose ();
		}
	}
}

