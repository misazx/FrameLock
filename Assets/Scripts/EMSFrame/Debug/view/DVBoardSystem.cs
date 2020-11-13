//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using LuaInterface;
using System.IO;

namespace UnityFrame
{
	public class DVBoardSystem : IDebugViewBoard
	{
		const int TAG_INFO = 0;
        const int TAG_GC = 1;
		const int TAG_GLOBAL_SETTING = 3;
		const int TAG_USER_SETTING = 4;
		const int TAG_PDATA = 5;
		const int TAG_PCACHE = 6;
		const int TAG_DWRITER = 7;
        const int TAG_TIMEFRAME = 9;
        const int TAG_CONSOLE = 10;


        private int m_CurrentTag;

		public void UF_DrawDetail (Rect rect){

			if (GUILayout.Button ("System Info",GUILayout.Height(40))) {
				m_CurrentTag = TAG_INFO;
			}
            if (GUILayout.Button("Time Frame", GUILayout.Height(40)))
            {
                m_CurrentTag = TAG_TIMEFRAME;
            }
            if (GUILayout.Button ("GC Options",GUILayout.Height(40))) {
				m_CurrentTag = TAG_GC;
			}
			if (GUILayout.Button ("GLobal Settings",GUILayout.Height(40))) {
				m_CurrentTag = TAG_GLOBAL_SETTING;
			}
			if (GUILayout.Button ("User Settings",GUILayout.Height(40))) {
				m_CurrentTag = TAG_USER_SETTING;
			}
			if (GUILayout.Button ("PData Info",GUILayout.Height(40))) {
				m_CurrentTag = TAG_PDATA;
			}
			if (GUILayout.Button ("Pool Cache",GUILayout.Height(40))) {
				m_CurrentTag = TAG_PCACHE;
			}
            if (GUILayout.Button("Debug Writer", GUILayout.Height(40)))
            {
                m_CurrentTag = TAG_DWRITER;
            }
            if (GUILayout.Button("Console Settings", GUILayout.Height(40)))
            {
                m_CurrentTag = TAG_CONSOLE;
            }

        }

		public void UF_DrawInfo(Rect rect){
            if (m_CurrentTag == TAG_INFO)
                UF_DrawTagInfo();
            else if (m_CurrentTag == TAG_GC)
                UF_DrawTagGC();
            else if (m_CurrentTag == TAG_GLOBAL_SETTING)
                UF_DrawGlobalSettings();
            else if (m_CurrentTag == TAG_USER_SETTING)
                UF_DrawUserSettings();
            else if (m_CurrentTag == TAG_PDATA)
                UF_DrawPDataInfo();
            else if (m_CurrentTag == TAG_PCACHE)
                UF_DrawPoolCache();
            else if (m_CurrentTag == TAG_DWRITER)
                UF_DrawDebugWriter();
            else if (m_CurrentTag == TAG_TIMEFRAME)
                UF_DrawTimeFrame();
            else if (m_CurrentTag == TAG_CONSOLE)
                UF_DrawConsole();
        }

		//系统相关信息
		private void UF_DrawTagInfo(){
			GUILayout.Space (4);
			System.Text.StringBuilder sb = StrBuilderCache.Acquire ();
			sb.Append(string.Format("AppPath:{0}",GlobalPath.AppPath));
			sb.AppendLine ();
			sb.Append(string.Format("StreamingAssetsPath:{0}",GlobalPath.StreamingAssetsPath));
			sb.AppendLine ();
			sb.Append(string.Format("PersistentPath:{0}",GlobalPath.PersistentPath));
			sb.AppendLine ();
			sb.Append(string.Format("DeviceModel: {0}",GlobalSettings.DeviceModel));
			sb.AppendLine ();
			sb.Append(string.Format("DeviceIMEI: {0}",GlobalSettings.DeviceIMEI));
			sb.AppendLine ();
			sb.Append(string.Format("DeviceOS: {0}",GlobalSettings.DeviceOS));
			sb.AppendLine ();
			sb.Append(string.Format("Battery: {0}",GlobalSettings.BatteryValue));
			sb.AppendLine ();
			sb.Append(string.Format("BatteryStatus: {0}",GlobalSettings.BatteryStatus));

			GUILayout.TextArea (StrBuilderCache.GetStringAndRelease(sb));

		}


        private void UF_DrawTimeFrame() {
            GUILayout.Space(4);
            GUILayout.Label("Run Frame: " + GTime.FrameRate.ToString());
            if (GUILayout.Button("120 FPS", GUILayout.Height(40), GUILayout.Width(120)))
            {
                GTime.FrameRate = 120;
            }
            if (GUILayout.Button("60 FPS", GUILayout.Height(40), GUILayout.Width(120)))
            {
                GTime.FrameRate = 60;
            }
            if (GUILayout.Button("45 FPS", GUILayout.Height(40), GUILayout.Width(120)))
            {
                GTime.FrameRate = 45;
            }
            if (GUILayout.Button("30 FPS", GUILayout.Height(40), GUILayout.Width(120)))
            {
                GTime.FrameRate = 30;
            }
            if (GUILayout.Button("15 FPS", GUILayout.Height(40), GUILayout.Width(120)))
            {
                GTime.FrameRate = 15;
            }
            GUILayout.Space(4);
            GUILayout.Label("Time Scale: " + GTime.TimeScale.ToString());
            GTime.TimeScale = GUILayout.HorizontalSlider(GTime.TimeScale, 0, 2);

        }

		private void UF_DrawTagGC(){
			GUILayout.Space (4);
			if(GUILayout.Button("GC UnusedRes",GUILayout.Height(40),GUILayout.Width(120))){
				Resources.UnloadUnusedAssets();
			}
			GUILayout.Space (4);
			if(GUILayout.Button("GC AssetBundle",GUILayout.Height(40),GUILayout.Width(120))){
				AssetSystem.UF_GetInstance ().UF_ClearAll(false);
				Resources.UnloadUnusedAssets();
			}
			GUILayout.Space (4);
			if(GUILayout.Button("GC Mono",GUILayout.Height(40),GUILayout.Width(120))){
				System.GC.Collect();           
			}
			GUILayout.Space (4);
			if(GUILayout.Button("GC Lua",GUILayout.Height(40),GUILayout.Width(120))){
				GLuaState lua = GLuaState.Get (System.IntPtr.Zero);
				lua.LuaGC (LuaGCOptions.LUA_GCCOLLECT);
			}
		}

		private void UF_DrawGlobalSettings(){
			if (GlobalSettings.GlobalConfig != null) {
				GUILayout.TextArea (GlobalSettings.GlobalConfig.UF_Serialize());
			}
		}

		private void UF_DrawUserSettings(){
            if (GUILayout.Button("Clear", GUILayout.Height(60)))
            {
                GlobalSettings.UserConfig.UF_Clean();
                GlobalSettings.UserConfig.UF_Save();
            }
            if (GlobalSettings.UserConfig != null) {
				GUILayout.TextArea (GlobalSettings.UserConfig.UF_Serialize());
			}
            
        }

		private void UF_DrawPDataInfo(){
			if (GlobalSettings.UserConfig != null) {
				GUILayout.Label (PDataManager.UF_GetInstance().ToString());
			}
		}


		private void UF_DrawDebugWriter(){
			string pwfile = PlayerPrefs.GetString ("dw_p_file","");	
			string pwidpe = PlayerPrefs.GetString ("dw_p_idpe","127.0.0.1:6789");
			GUI.color = Color.white;

			try{
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Set File Writer",GUILayout.Width(120))) {
					if (!string.IsNullOrEmpty (pwfile.Trim ())) {
						if (!File.Exists (pwfile)) {
							string abspath = Path.GetDirectoryName (pwfile);
							if (!Directory.Exists (abspath)) {
								Directory.CreateDirectory (abspath);
							}
						}
						FileWriter fw = new FileWriter(pwfile);
						Debugger.UF_GetInstance ().logger.UF_AddWriter(fw);
					}
				}
				pwfile = GUILayout.TextArea (pwfile);
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Set UDP Writer",GUILayout.Width(120))) {
					if (!string.IsNullOrEmpty (pwidpe.Trim ())) {
						string[] args = GHelper.UF_SplitStringWithCount(pwidpe,2,':');
						UdpNetWriter uw = new UdpNetWriter(args[0],int.Parse(args[1]));
						uw.UF_Write(string.Format("\n\n\n------------------Start UPD Console:[{0}]------------------\n",GTime.DateHMS));
						Debugger.UF_GetInstance ().logger.UF_AddWriter(uw);
					}
				}
				pwidpe = GUILayout.TextField (pwidpe);
				GUILayout.EndHorizontal ();

			}catch(System.Exception ex){
				Debugger.UF_Exception (ex);
			}

			if (GUI.changed) {
				PlayerPrefs.SetString ("dw_p_file", pwfile);
				PlayerPrefs.SetString ("dw_p_idpe", pwidpe);
			}

		}


		//绘制所有缓存对象数据缓冲
		private void UF_DrawPoolCache(){
            GUILayout.Label(string.Format("ProtocalData: {0}", ProtocalData.StaticBufferCount));
            GUILayout.Space(10);
            GUILayout.Label(string.Format("ListPool<TextToken>: {0}",ListPool<TextToken>.Count));
            GUILayout.Space (10);
			GUILayout.Label(string.Format("DictionaryPool<Transform,IUIUpdate>: {0}",DictionaryPool<Transform,IUIUpdate>.Count));
            GUILayout.Space(10);
            GUILayout.Label(string.Format("DictionaryPool<string,IUIUpdate>: {0}",DictionaryPool<string,IUIUpdate>.Count));
			GUILayout.Space (10);
			GUILayout.Label(string.Format("QueuePool<IUIUpdate>: {0}",QueuePool<IUIUpdate>.Count));
			GUILayout.Space (10);
			GUILayout.Label(string.Format("NCollector<IEntityHnadle>: {0}", NCollector<IEntityHnadle>.StaticBufferCount));            
        }


        private void UF_DrawConsole() {
            if (GUILayout.Button("Hide", GUILayout.Height(40), GUILayout.Width(120))) {
                Debugger.IsActive = false;
            }
            GUILayout.Space(10);
            if (GUILayout.Button("Pin", GUILayout.Height(40), GUILayout.Width(120)))
            {
                GlobalSettings.UF_SetUserValue("DEBUG","PIN_CONSOLE","1");
            }
            GUILayout.Space(10);
            if (GUILayout.Button("UnPin", GUILayout.Height(40), GUILayout.Width(120)))
            {
                GlobalSettings.UF_SetUserValue("DEBUG", "PIN_CONSOLE", "0");
            }

        }




	}
}

