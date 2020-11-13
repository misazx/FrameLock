//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

namespace UnityFrame
{
	public class Node<T>
	{
		public T refer;
		public Node<T> next;

		public void Reset(){
			refer = default(T);
			next = null;
		}
	}

}

