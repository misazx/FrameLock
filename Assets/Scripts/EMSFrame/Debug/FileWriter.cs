//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.IO;


//写入本地文件
namespace UnityFrame{

	public class FileWriter : IWriter {
		protected StreamWriter m_OutFileStream;

		public FileWriter(StreamWriter streamWriter){
            UF_Close();
			m_OutFileStream = streamWriter;
		}


		public FileWriter(string path){
			try{
                UF_Close();
				m_OutFileStream = new StreamWriter (path,true,System.Text.Encoding.UTF8);
			}
			catch(System.Exception e){
				m_OutFileStream = null;
				Debugger.UF_Exception(e);
			}
		}

		public void UF_Write(object param){
			if (param == null) 
				return;
			try{
				if (m_OutFileStream != null) {
					m_OutFileStream.WriteLine(param.ToString());
					m_OutFileStream.Flush();
				}
			}
			catch(System.Exception e){
				Debugger.UF_Exception(e);
			}
		}
		
		public void UF_Close(){
			if (m_OutFileStream != null) {
				m_OutFileStream.Close();
				m_OutFileStream = null;
			}
		}

		~FileWriter(){
			this.UF_Close();
		}

	}
}