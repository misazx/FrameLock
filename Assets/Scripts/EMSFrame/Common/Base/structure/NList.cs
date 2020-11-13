//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityFrame
{
	//list struct
	/*
		head : node1 -> node2 -> node3 -> node4 ...
	*/

	public class NList<T> : NCollector<T>,IDisposable
	{
		private int m_Count;
		private Node<T> m_Head;

		public int Count {get{return m_Count;}} 

		public T this[int i]
		{
			get 
			{
				return UF_Find (i);
			}
		}

		public T UF_Find(int i){
			T ret = default(T);
			if (i < m_Count) {
				Node<T> node = m_Head;
				int k = 0;
				while (k < i && node != null) {
					k++;
					node = node.next;
				}
				if (node != null)
					ret = node.refer;
			}
			return ret;
		}

		public bool UF_Exist(T value){
			Node<T> node = m_Head;
			while (node != null) {
				if (node.refer.Equals (value)) {
					return true;	
				}
				node = node.next;
			}
			return false;

		}

		public void UF_Add(T value){
			Node<T> node = UF_CreateNode (value);
			if (m_Head == null) {
				m_Head = node;
			}
			else {
				node.next = m_Head;
				m_Head = node;
			}
		}
			
		public void UF_Remove(T value){
			Node<T> node = m_Head;
			Node<T> last = null;
			if (node.next == null && node.refer.Equals(value)) {
				UF_RecoverNode (node);
				m_Head = null;
				return;
			}
			last = node;
			node = node.next;

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

		public T UF_Pop(){
			Node<T> node = m_Head;
			T ret = default(T);
			if (node != null){
				ret = node.refer;
				m_Head = node.next;
				node.next = null;
				this.UF_RecoverNode (node);
			}
			return ret;
		}

		//根据条件筛选
		public T UF_Select(DelegateNForeach<T> method){
			if (method != null && m_Head != null) {
				Node<T> node = m_Head;
				while (node != null) {
					if (method (node.refer))
						return node.refer;
					node = node.next;
				}
			}
			return default(T);
		}


		/// <summary>
		///循环每个节点
		///如果mehod 返回false,则删除该节点
		/// </summary>
		public void UF_NForeach(DelegateNForeach<T> method){
			if (method == null || m_Head == null) {
				return;
			}
			Node<T> node = m_Head;
			Node<T> last = null;
			if (node.next == null) {
				if (!method (node.refer)) {
					this.UF_RecoverNode (node);
					m_Head = null;
				}
				return;
			}

			last = node;
			while (node != null) {
				if (!method (node.refer)) {
					if (node == m_Head) {
						m_Head = node.next;
						node.next = null;
						UF_RecoverNode (node);
						node = m_Head;
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
		/// 清空全部节点
		/// </summary>
		public void UF_NForeachClear(DelegateNForeach<T> method){
			Node<T> node = m_Head;
			while (node != null) {
				if (method != null) {
					method (node.refer);
				}
				node = node.next;
			}
			UF_Clear ();
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
			UF_RecoverNode (m_Head);
			m_Head = null;
			m_Count = 0;
		}

		public override void Dispose ()
		{
			UF_Clear ();
			base.Dispose ();
		}

	}
}

