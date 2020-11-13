//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace UnityFrame{
	/// <summary>
	/// 本地配置文件数据
	/// </summary>
	public class ConfigFile {
		private Dictionary<string,Dictionary<string,string>> m_Sections = new Dictionary<string, Dictionary<string, string>>();
		private string m_FileName;

		public ConfigFile(){}

		public ConfigFile(string filepath){
			this.UF_OpenPath(filepath);
		}

		public void UF_SetString(string SectionName,string Key,string Value){
			if(!m_Sections.ContainsKey(SectionName))
			{
				m_Sections.Add(SectionName,new Dictionary<string, string>());
			}
			Dictionary<string,string> dic_keyvalue = m_Sections[SectionName];
			if(dic_keyvalue.ContainsKey(Key)){
				dic_keyvalue[Key] = Value;
			}
			else{
				dic_keyvalue.Add(Key,Value);
			}
		}

		public string UF_GetString(string SectionName,string Key,string DefaultValue = ""){
			if (m_Sections.ContainsKey(SectionName)) {
				if(m_Sections[SectionName].ContainsKey(Key)){
					return m_Sections[SectionName][Key];
				}
			}
			return DefaultValue;
		}

		public void UF_SetInt(string SectionName,string Key,int Value){
            UF_SetString(SectionName,Key,Value.ToString());
		}

		public int UF_GetInt(string SectionName,string Key,int Deafult_Value = 0){
			int outInt = 0;
			int.TryParse(UF_GetString(SectionName,Key,Deafult_Value.ToString()),out outInt);
			return outInt;
		}


		public void UF_RemoveSection(string section){
			if (m_Sections.ContainsKey (section)) {
				m_Sections.Remove (section);
			}
		}

		public void UF_RemoveValue(string section,string key){
			if (m_Sections.ContainsKey (section)) {
				if (m_Sections [section].ContainsKey (key)) {
					m_Sections [section].Remove (key);
				}
			}
		}

		public void UF_OpenString(string sections){
			if(sections == ""){return;}
            UF_OpenReader(new StreamReader(new MemoryStream(System.Text.Encoding.Default.GetBytes (sections))));
		}

		public void UF_OpenPath(string FileName){
			try{
                m_FileName = FileName;
                FileInfo fi = new FileInfo(FileName);
                if (fi.Exists) {
                    UF_OpenStream(fi.Open(FileMode.Open, FileAccess.Read));
                }
                else {
                    //创建文件
                    fi.Create().Close();
                }
			}
			catch(System.Exception e){
				Debugger.UF_Exception (e);
			}
		}


		public void UF_OpenStream(FileStream fileStream){
			try{
                UF_OpenReader(new StreamReader(fileStream));
			}
			catch(System.Exception e){
				Debugger.UF_Error(e.Message);
			}
		}

		public void UF_OpenReader(StreamReader streamReader){
			if (streamReader == null)
				return;
			try{
				string line = "";
				string headName = "";
				while( null != (line = streamReader.ReadLine())){
                    //int idx = line.IndexOf("#");
                    //if(idx >= 0){line = line.Substring(idx);}
                    int idx = -1;
                    line = line.Trim();
					if(line.StartsWith("#")){continue;}

					if(line.StartsWith("[")){
						idx = line.LastIndexOf("]");
						if(idx < 0){continue;}
						headName = line.Substring(1,idx-1).Trim();
					}
					if(headName == ""){continue;}

					idx = line.IndexOf("=");
					if(idx < 0) {continue;}

					string value = line.Substring(idx+1).Trim();
					string key = line.Substring(0,idx).Trim();
                    UF_SetString(headName,key,value);
				}
			}
			catch(System.Exception e){
				Debugger.UF_Error(e.Message);
			}
            if (streamReader != null) {
                streamReader.BaseStream.Close();
                streamReader.Close();
                streamReader.Dispose();
            }
		}
			

		public bool UF_CheckSection(string SectionName){
			return m_Sections.ContainsKey (SectionName);
		}

		public Dictionary<string,string> UF_GetSection(string SectionName){
			Dictionary<string, string> section = new Dictionary<string, string>();
			if(m_Sections.ContainsKey(SectionName))
			{
				//copy
				foreach(KeyValuePair<string, string> item in m_Sections[SectionName]){
					section.Add(item.Key,item.Value);
				}
			}
			return section;
		}

		public void UF_SetSection(string SectionName,Dictionary<string,string> Section){
			if(m_Sections.ContainsKey(SectionName))
			{
				m_Sections[SectionName] = Section;
			}
			else{
				m_Sections.Add(SectionName,Section);
			}
		}

		public string UF_Serialize(){
			string ser_data = "";
			foreach(KeyValuePair<string,Dictionary<string,string>> section in m_Sections){
				ser_data += "["+section.Key + "]" + "\n";
				foreach(KeyValuePair<string,string> item in section.Value){
					ser_data += item.Key + "=" +item.Value + "\n";
				}
			}
			return ser_data;
		}


		public void UF_Clean(){
			m_Sections = new Dictionary<string, Dictionary<string, string>>();
		}

		public void UF_SetFileName(string fileName){
			m_FileName = fileName;
		}

		public void UF_Save(){
            UF_Save(m_FileName);
		}

		public void UF_Save(string FileName){
			if (FileName == "") {
				Debugger.UF_Error ("FileName is null,save prefile failed");
                return;
			}
			try{
				StreamWriter sw = new StreamWriter(FileName);
				sw.Write(UF_Serialize());
				sw.Close();
			}
			catch(System.Exception e){
				Debugger.UF_Error(e.Message);
			}
		}

	}


}