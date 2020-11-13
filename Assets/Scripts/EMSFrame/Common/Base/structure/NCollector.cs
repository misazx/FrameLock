//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------


//静态buff,共享内存,但线程不问题
#define STATIC_BUFFER

using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityFrame
{
	public class NCollector<T> : System.IDisposable
	{
		#if STATIC_BUFFER 
		protected static Node<T> m_NodeBuffer;
		private static int m_BufferCount = 0;
		public static int StaticBufferCount{get{return m_BufferCount;}}
		#else
		protected Node<T> m_NodeBuffer;
		private int m_BufferCount = 0;
		#endif

		public int BufferCount{get{return m_BufferCount;}}

		virtual protected Node<T> UF_CreateNode(T value){
			Node<T> ret = null;
			if (m_NodeBuffer == null) {
				ret = new Node<T> ();
			} else {
				ret = m_NodeBuffer;
				m_NodeBuffer = ret.next;
				ret.next = null;
				m_BufferCount--;
			}
			ret.refer = value;
			return  ret;
		}

		virtual protected int UF_RecoverNode(Node<T> node){
			int ret = 0;
			if (node == null)
				return ret;
			//如果是列表，则完整回收
			if (node.next == null) {
				node.Reset ();
				node.next = m_NodeBuffer;
				m_NodeBuffer = node;
				ret++;
			} else {
				Node<T> head = node;
				Node<T> tail = node;
				while (true) {
					tail.refer = default(T);
					ret++;
					if(tail.next == null){
						break;
					}
					tail = tail.next;
				}
				tail.next = m_NodeBuffer;
				m_NodeBuffer = head;
			}
			m_BufferCount += ret;
			return ret;
		}

		//清空缓存
		public void UF_ClearBuffer(){
			Node<T> node = m_NodeBuffer;
			Node<T> temp = null;
			while (node != null) {
				temp = node;
				node = node.next;
				temp.Reset ();
			}
			m_NodeBuffer = null;
			m_BufferCount = 0;
		}


		virtual public void Dispose (){
			UF_ClearBuffer ();
		}

	}


}