//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace UnityFrame{
	//本地数据管理类 
	public class PDataManager : HandleSingleton<PDataManager>,IOnStart,IOnApplicationPause,IOnApplicationQuit,IOnReset
	{
		private static Dictionary<string,ConfigFile> m_DicPersistentData = new Dictionary<string, ConfigFile>();

		private ConfigFile UF_ReadConfigFile(string filename){
			string filepath = GlobalPath.LocalPath + filename;
			return new ConfigFile (filepath);
		}
		 
		public void UF_SetValue(string filename,string section,string key,string value){
			ConfigFile cfg = null;
			if (m_DicPersistentData.ContainsKey (filename)) {
				cfg = m_DicPersistentData [filename];
			} else {
				cfg = UF_ReadConfigFile(filename);
				m_DicPersistentData.Add (filename,cfg);
			}
			cfg.UF_SetString(section,key,value);
		}


		public string UF_GetValue(string filename,string section,string key){
			if (m_DicPersistentData.ContainsKey (filename)) {
				return m_DicPersistentData [filename].UF_GetString(section,key,"");
			} else {
				ConfigFile cfg = UF_ReadConfigFile(filename);
				m_DicPersistentData.Add (filename,cfg);
				return cfg.UF_GetString(section,key);
			}
		}


		public void UF_RemoveValue(string filename,string section,string key){
			if (m_DicPersistentData.ContainsKey (filename)) {
				m_DicPersistentData [filename].UF_RemoveValue(section, key);
			}
		}

		public void UF_RemoveSection(string filename,string section){
			if (m_DicPersistentData.ContainsKey (filename)) {
				m_DicPersistentData [filename].UF_RemoveSection(section);
			}
		}

		public void UF_Save(string filename){
			if (m_DicPersistentData.ContainsKey (filename)) {
				m_DicPersistentData [filename].UF_Save();
			}
		}

		public void UF_SaveAll(){
			foreach (ConfigFile cfg in m_DicPersistentData.Values) {
				if(cfg != null)	{
					cfg.UF_Save();
				}
			}
		}

		public void UF_DeleteFile(string filename){
			string filepath = GlobalPath.LocalPath + filename;
			if (System.IO.File.Exists (filepath)) {
				System.IO.File.Delete (filepath);
			}
		}


		public void UF_OnStart(){
			if (!System.IO.Directory.Exists (GlobalPath.LocalPath)) {
				System.IO.Directory.CreateDirectory (GlobalPath.LocalPath);
			}
		}

		public void OnApplicationPause(bool state){
			if (state) {
                UF_SaveAll();
			}
		}

		public void OnApplicationQuit(){
#if UNITY_EDITOR
            UF_SaveAll();
			#endif
		}


        public void UF_OnReset() {
            m_DicPersistentData.Clear();
        }


        public override string ToString ()
		{
			System.Text.StringBuilder sb = StrBuilderCache.Acquire ();
			sb.Append(string.Format ("PDataManager  count:{0} \n",m_DicPersistentData.Count));
			foreach (KeyValuePair<string,ConfigFile> item in m_DicPersistentData) {
				sb.Append(string.Format ("\t {0} \n", item.Key));
			}
			return StrBuilderCache.GetStringAndRelease(sb);
		}

	}
}

