//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

//分类消息打印
namespace UnityFrame{
	public class MsgLoger : System.IDisposable {
		class TagMessage{
			public StringBuilder message = new StringBuilder();
			public int count = 0;
			public void AddCount(){
				count++;
			}
			public int UF_GetLenght(){
				return message.Length;
			}

			public void UF_Clip(int length){
				message.Remove (0, length);
			}

			public void UF_Append(string msg){
				message.Append (msg);
			}

			public void UF_Clear(){
				message.Remove (0, message.Length);
				count = 0;
			}
		}

		protected bool m_Active = true;

		protected int m_BufferSize = 50000;

		private Dictionary<string,TagMessage> m_DicTagMessages = new Dictionary<string, TagMessage>();

		protected List<string> m_ListTags = new List<string> ();

		public List<string> Tags{get{ return m_ListTags;}}

		protected List<IWriter> m_ListWriters = new List<IWriter>();

		public MsgLoger(){}

		public MsgLoger(int bufferSize){
			m_BufferSize = bufferSize;
		}

		private string UF_GetLongTimeString(){
			return System.DateTime.Now.ToLongTimeString ();
		}

		public void UF_AddWriter(IWriter writer){
			if(!m_ListWriters.Contains(writer))
			m_ListWriters.Add (writer);
		}

		public bool UF_RemoveWriter(IWriter writer){
			return m_ListWriters.Remove (writer);
		}

		public void UF_Log(string tag,string message){

			string msg = string.Format ("[{0}][{1}]:{2}\n",UF_GetLongTimeString(),tag,message);

			//write to the tags message dictionary
			if (!m_DicTagMessages.ContainsKey (tag)) {
				m_DicTagMessages.Add(tag,new TagMessage());
				m_ListTags.Add (tag);
			}

			m_DicTagMessages [tag].UF_Append(msg);

			m_DicTagMessages[tag].AddCount();

			if (m_DicTagMessages [tag].UF_GetLenght() > m_BufferSize) {
				//remove half buffer
				m_DicTagMessages [tag].UF_Clip(m_BufferSize / 2);
			}

			//write to the writer
			if (m_ListWriters != null) {
				for(int k = 0;k < m_ListWriters.Count;k++){
					if(m_ListWriters[k] != null){
						m_ListWriters[k].UF_Write(msg);
					}
				}
			}

		}

		public void UF_SetActive(bool active){
			m_Active = active;
		}


		public string UF_GetTagMessage(string tag){
			if (m_DicTagMessages.ContainsKey (tag)) {
				return m_DicTagMessages[tag].message.ToString();
			}	
			else{
				return "";
			}
		}

		public int UF_GetTagCount(string tag){
			if (m_DicTagMessages.ContainsKey (tag)) {
				return m_DicTagMessages [tag].count;
			} else {
				return 0;
			}
		}
		
		public void UF_ClearTagMessage(string tag){
			if (m_DicTagMessages.ContainsKey (tag)) {
				m_DicTagMessages [tag].UF_Clear();
			}
		}

		public void UF_ClearMessage(){
			foreach (TagMessage tagm in m_DicTagMessages.Values) {
				tagm.UF_Clear();
			}
			m_DicTagMessages.Clear ();
            m_ListTags.Clear();
        }

		public void Dispose(){
			foreach (IWriter writer in m_ListWriters) {
				if (writer != null) {
					writer.UF_Close();
				}
			}
		}

	}
}