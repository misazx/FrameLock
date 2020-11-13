//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

//Win测试
#if UNITY_EDITOR
#define __IGNORE_UPGRADE__
#endif


namespace UnityFrame
{

	using UnityEngine;
	using System.Collections;
	using System.IO;
	using System.Collections.Generic;

	public struct UpgradeFileInfo{
		public int size;
		public string version;
		public string zip;
		public string url;
		public string ucode;
	}
 
	public class UpgradeSystem : HandleSingleton<UpgradeSystem>,IOnAwake,IOnReset,IOnUpdate {

		const int STATE_NORMAL = 0;
		const int STATE_ERROR = -1;
		const int STATE_SUCCESS = 1;

		public static string LocalAssetVersion{get{return localAssetVersion;}}
		public static string LocalBaseVersion{get{ return appAssetVersion;}}

		public static string LocalAPPVersion{get{ return appVersion;}}

		public static string WebBaseVersion{get{ return webBaseResVersion;}}
		public static string WebAPPVersion{get{ return webAppVersion;}}

		public static string WebUpgradeInfo{get{ return webUpgradInfo;}}

		public static string VersionChunk{get{return string.Format ("{0}.{1}.{2}", appVersion, GlobalSettings.ResVersion, localAssetVersion);}}

		//本地资源版本
		private static string localAssetVersion = "0";
		//当前APP资源版本
		private static string appAssetVersion = "0";
		//服务器上的资源版本
		private static string webBaseResVersion = "0";

		//当前APP版本
		private static string appVersion = "0";
		//服务器上的APP版本
		private static string webAppVersion = "0";

		private static string webUpgradInfo = "";

		private UpgradeFiles m_UpgradeFiles = new UpgradeFiles();

		private UpgradeView m_UpgradeView = new UpgradeView();

		public static bool IngoreResVersionCheck{ get; set;}

		private List<UpgradeFileInfo> m_LstUpdateFileInfo = new List<UpgradeFileInfo>();

		public string VendorName{get{ return GlobalSettings.UF_GetGlobalValue ("APP", "VENDOR_NAME");}}

		public int SizeUpgradeAdvice{get{ return 2;}}

		public float UpgradeProgress{get{ return m_UpgradeFiles.Progress;}}

		public string UpgradeInfo{get{ return m_UpgradeFiles.UpgradeInfo;}}

		private UnityEngine.Coroutine UF_StartCoroutine(IEnumerator method){
			return GameMain.Instance.StartCoroutine(method);
		}

		private string UF_GetLocalResVersion(){
			FileInfo fi = new FileInfo(GlobalPath.ResPersistentPath + "res_version.txt");
			if(fi.Exists){
				StreamReader sr = fi.OpenText();
				localAssetVersion = sr.ReadToEnd().Trim();
				sr.Close();
				return localAssetVersion;
			}
			else{
				return "0";
			}
		} 

		private void UF_LogInfo(string info){
			Debugger.UF_Log (info);
		}

		private void UF_LogError(string error){
			Debugger.UF_Error (error);
		}

		private void UF_ShowMessageBox(string info,string btnInfo,DelegateMethod onClick = null){
			AlertDialog.UF_Show(GlobalText.LAN_WARNING,btnInfo,info,onClick);
		}



		internal IEnumerator UF_CheckUpgrade(){
            bool upgradeFinished = false;
			while (!upgradeFinished) {
				yield return UF_StartCoroutine(this.UF_Check((e) => {upgradeFinished = e;}));
			}
			yield return null;
		}


		private IEnumerator UF_Check(DelegateBoolMethod checkFinish){
#if __IGNORE_UPGRADE__
            m_UpgradeView.UF_Show();
            m_UpgradeView.UF_DisplayProgress(false);
            yield return new WaitForSeconds(0.5f);
            m_UpgradeView.UF_DisplayProgress(true);
            checkFinish (true);
#else
			/// 1.检查获取本地版本号
			int state = STATE_NORMAL;
            UF_StartCoroutine(UF_Start((e)=>{state = (int)e;}));
			while (state == STATE_NORMAL) {yield return null;}
			if (state == STATE_ERROR) {checkFinish (false);yield break;}
			

            //审核状态跳过安装与更新
            if (GlobalSettings.IsAppCheck)
            {
                checkFinish(true);
                Debugger.UF_Warn("App Check Mode Skip Install and Update");
                yield break;
            }
            //显示更新图
            m_UpgradeView.UF_Show();
            m_UpgradeView.UF_DisplayProgress(true);

            /// 2.检查是否已经初始化安装
            state = STATE_NORMAL;
            UF_StartCoroutine(UF_Install((e)=>{state = (int)e;}));
			while (state == STATE_NORMAL) {yield return null;}
			if (state == STATE_ERROR) {checkFinish (false);yield break;}

            UF_ResfreshVersion();
			yield return null;
			m_UpgradeView.UF_SetInfo(GlobalText.LAN_START_INIT);
			if (GlobalSettings.UF_GetGlobalValue("DEBUG","NO_HOTFIX").Equals("1")){
			    checkFinish (true);
                Debugger.UF_Warn("NO_HOTFIX Tag Skip File Update");
                yield break;
			}

            /// 3.检查是否有版本更新

            state = STATE_NORMAL;
            UF_StartCoroutine(UF_Upgrade((e)=>{state = (int)e;}));
			while (state == STATE_NORMAL) {yield return null;}
			if (state == STATE_ERROR) {checkFinish (false);yield break;}

			yield return null;
			m_UpgradeView.UF_SetInfo(GlobalText.LAN_START_INIT);
			yield return null;
            UF_ResfreshVersion();
			yield return null;
			m_UpgradeView.UF_DisplayProgress(false);
			yield return null;
			m_UpgradeView.UF_SetInfo(GlobalText.LAN_TAG_MAINWORK_LOADING);
			checkFinish (true);
#endif
            yield return null;
		}


		public void UF_ResfreshVersion(){
			localAssetVersion = UF_GetLocalResVersion();
			appAssetVersion = GlobalSettings.ResVersion;
			appVersion = GlobalSettings.AppVersion;
			m_UpgradeView.UF_SetVersion("version:" + VersionChunk);
		}


		private IEnumerator UF_Start(DelegateMethod callback){
            //获取当前资源版本号

            UF_ResfreshVersion();

			UF_LogInfo("localAssetVersion: "+localAssetVersion);
			UF_LogInfo("appAssetVersion: "+appAssetVersion);
			UF_LogInfo("appVersion: "+appVersion);

			callback (STATE_SUCCESS);

            //发送打点
            CheckPointManager.UF_Send(5);

            yield break;
		}


		//比较base包判断是否需要覆盖安装
		private IEnumerator UF_Install(DelegateMethod callback){
			int int_version_current = 0;
			int int_local_base_version = 1;
			try{
				int_version_current = int.Parse(localAssetVersion);
				int_local_base_version = int.Parse(appAssetVersion);
			}catch(System.Exception e){
				UF_LogError(e.Message);
				//UF_LogError(e.StackTrace);
			}

			if(int_version_current < int_local_base_version){
				DelegateMethod install_callback = delegate(object param) {
					int retcode = (int)param;
					if(retcode == 0){
						callback (STATE_SUCCESS);
					}
					else{
                        UF_ShowMessageBox(string.Format("{0}:{1}",GlobalText.LAN_INSTALL_ERROR,retcode),GlobalText.LAN_CONFIRM,(e)=>{callback (STATE_ERROR);});
					}
				};
				//多重闭包解决，如果有问题，用while(true){yield return null} 方式
				m_UpgradeFiles.UF_InstallBase(install_callback);
                CheckPointManager.UF_Send(6);
            }
			else{
				//包体已经安装
				callback (STATE_SUCCESS);
			}
			yield return null;
		}

        private string UF_GetUpgradeFileURL(string head) {
            return string.Format("{0}/res_version.txt?v={1}", head, Random.Range(0, short.MaxValue));
        }

        //请求是否需要更新
        internal void UF_RequestCheckUpgrade(DelegateBoolMethod callback) {
#if UNITY_EDITOR
            callback(false);
#else
            UF_StartCoroutine(UF_IRequestCheckUpgrade(callback));
#endif
        }
        private IEnumerator UF_IRequestCheckUpgrade(DelegateBoolMethod callback)
        {
            //先检查源站文件是否能正常下载，否则去CDN获取文件
            string urlVersionFile = UF_GetUpgradeFileURL(GlobalSettings.UrlRawAssetsUpgrade);
            WWW wwwResVersionFile = new WWW(urlVersionFile);
            yield return wwwResVersionFile;
            if (!string.IsNullOrEmpty(wwwResVersionFile.error)) {
                UF_LogError(string.Format("CheckUpgrade -> Raw wwwResVersionFile error[{0}]  \n errorinfo:[{1}]", wwwResVersionFile.url, wwwResVersionFile.error));
                wwwResVersionFile.Dispose();
                UF_LogInfo("CheckUpgrade -> Try to download version file from CDN");
                urlVersionFile = UF_GetUpgradeFileURL(GlobalSettings.UrlAssetsUpgrade);
                wwwResVersionFile = new WWW(urlVersionFile);
                yield return wwwResVersionFile;
            }

            List<UpgradeFileInfo> outFiles = new List<UpgradeFileInfo>();
            string localVersion = UF_GetLocalResVersion();
            if (!string.IsNullOrEmpty(wwwResVersionFile.error))
            {
                UF_LogError(string.Format("IRequestUpgradeInfo error[{0}]  \n errorinfo:[{1}]", wwwResVersionFile.url, wwwResVersionFile.error));
            }
            else {
                string txtVersionFile = wwwResVersionFile.text;
                Debugger.UF_LogTag("version", string.Format("CheckUpgrade:{0} \n {1}", urlVersionFile, txtVersionFile));
                UF_WrapUpdateList(txtVersionFile, localVersion, outFiles);
            }
            wwwResVersionFile.Dispose();
            callback(outFiles.Count > 0);
        }


        //根据base包比较判断是否需要更新
        private IEnumerator UF_Upgrade(DelegateMethod callback){
            //忽略版本更新检查 
            if (IngoreResVersionCheck){callback(STATE_SUCCESS); yield break;}

            //先检查源站文件是否能正常下载，否则去CDN获取文件
            string urlVersionFile = UF_GetUpgradeFileURL(GlobalSettings.UrlRawAssetsUpgrade);
            UF_LogInfo("URL Raw Res Version:"+urlVersionFile);
			//获取资源版本列表
			//检查是否与更新包
			WWW wwwResVersionFile = new WWW(urlVersionFile);
			yield return wwwResVersionFile;
            if (!string.IsNullOrEmpty(wwwResVersionFile.error))
            {
                UF_LogError(string.Format("Upgrade -> Raw wwwResVersionFile error[{0}]  \n errorinfo:[{1}]", wwwResVersionFile.url, wwwResVersionFile.error));
                wwwResVersionFile.Dispose();
                UF_LogInfo("Upgrade -> Try to download version file from CDN");
                urlVersionFile = UF_GetUpgradeFileURL(GlobalSettings.UrlAssetsUpgrade);
                UF_LogInfo("URL CDN Res Version:" + urlVersionFile);
                wwwResVersionFile = new WWW(urlVersionFile);
                yield return wwwResVersionFile;
            }

            if (!string.IsNullOrEmpty(wwwResVersionFile.error)){
				UF_LogError(string.Format("wwwResVersionFile error[{0}]  \n errorinfo:[{1}]",wwwResVersionFile.url,wwwResVersionFile.error));
                //无法获取更新文件，是否跳过更新
                AlertDialog.UF_ShowOkCancel(GlobalText.LAN_WARNING, GlobalText.LAN_CANCLE, GlobalText.LAN_CONFIRM, GlobalText.LAN_ERROR_CHECK_UPGRADE,
                    (e) => { callback(STATE_ERROR); }, (e) => { callback(STATE_SUCCESS); });
				yield break;
			}
			string txtVersionFile = wwwResVersionFile.text;
			wwwResVersionFile.Dispose ();
			//判断更新包大小情况
			m_LstUpdateFileInfo.Clear();

			webUpgradInfo = txtVersionFile;

            Debugger.UF_LogTag("version", string.Format("Upgrade:{0} \n {1}", urlVersionFile, txtVersionFile));

            UF_WrapUpdateList(txtVersionFile,localAssetVersion,m_LstUpdateFileInfo);

            CheckPointManager.UF_Send(7);

            //没有需要更新的更新包则跟新完成
            if (m_LstUpdateFileInfo.Count == 0) {
				callback (STATE_SUCCESS);
				yield break;
			}
			else
			{
				//更新包大小比较建议
				float size = 0;
				for (int k = 0; k < m_LstUpdateFileInfo.Count; k++) {
					size += m_LstUpdateFileInfo[k].size;
				}
				size = (int)(size / (float)(1024 * 1024));

				DelegateMethod DelegateExctureUpdate = delegate(object param) {
					m_UpgradeFiles.UF_Upgrade(m_LstUpdateFileInfo, (e) => {
						if (!(bool)e) {
                            UF_ShowMessageBox(GlobalText.LAN_ERROR_CHECK_UPGRADE, GlobalText.LAN_CONFIRM, (k) => {
								//更新失败
								callback(STATE_ERROR);
							});
						} else {
							callback(STATE_SUCCESS);
						}
					});
				};

//wifi 下载提示
//                if (size > SizeUpgradeAdvice) {
//                    ShowMessageBox(string.Format(AppLanguages.LValue("LAN_UPDATE_TIPS"), size), AppLanguages.LValue("LAN_START_UPDATE"), DelegateExctureUpdate);
//                }
//                else {
				DelegateExctureUpdate(null);
//                }
			}
		}


		public void UF_Uninstall(){
			FileInfo fi = new FileInfo(GlobalPath.ResPersistentPath + "res_version.txt");
			if (fi.Exists) {
				fi.Delete ();
			}
			if (Directory.Exists (GlobalPath.ScriptPath)) {
				Directory.Delete (GlobalPath.ScriptPath);
			}
		}


		//修复客户端
		public void UF_FixClient(){
			DelegateMethod _callback = delegate(object e) {
				try{
					string file_res_version = GlobalPath.ResPersistentPath + "res_version.txt";
					//删除全部代码文件
#if !UNITY_EDITOR
					string dir_runtime = GlobalPath.ScriptPath;
					if(Directory.Exists(dir_runtime)){
						Directory.Delete(dir_runtime,true);
					}
#endif
					//删除version 
					if (File.Exists(file_res_version)) {
						File.Delete(file_res_version);
					}
                    UF_ShowMessageBox(GlobalText.LAN_ERROR_MANUAL_FIX_CONFURE,GlobalText.LAN_CONFIRM);

				}catch(System.Exception ex){
					Debugger.UF_Exception(ex);
				}
			};
			AlertDialog.UF_ShowOkCancel(GlobalText.LAN_WARNING,GlobalText.LAN_CANCLE,GlobalText.LAN_CONFIRM,GlobalText.LAN_ERROR_MANUAL_FIX_VERSION,_callback,null);
		}

		private bool UF_checkVersionIfInList(string current_version,List<UpgradeFileInfo> lstUpdateFileinfo){
			if (lstUpdateFileinfo == null) {
				return true;
			}

			for (int k = 0; k < lstUpdateFileinfo.Count; k++) {
				if (lstUpdateFileinfo[k].version == current_version || lstUpdateFileinfo[k].zip.IndexOf(current_version) > -1) {
					return true;
				}
			}
			return false;
		}


		private UpgradeFileInfo UF_CreateUpgradeFileInfo(string chunk){
			UpgradeFileInfo ret = new UpgradeFileInfo();
			string[] value = GHelper.UF_SplitStringWithCount (chunk, 3);
			ret.version = value [0].Substring (value [0].IndexOf ('-') + 1);
			ret.size = int.Parse (value [1].Trim ());
			ret.zip = value [0].Trim () + ".zip";
            //取用CDN资源
			ret.url = string.Format("{0}/{1}", GlobalSettings.UrlAssetsUpgrade.Trim(),ret.zip);
			ret.ucode = value [2];
			return ret;
		}


//格式：
//		base=61598
//			61598-61685;1685;3504492d24d3e43f8eb174e60241fb21
//			61685-61917;31190;3504492d24d3e43f8eb174e60241fb22
//			61917-61982;2893;3504492d24d3e43f8eb174e60241fb23
//			61982-62029;4503;3504492d24d3e43f8eb174e60241fb24
//			62029-62057;1711;3504492d24d3e43f8eb174e60241fb25
//			*61598-62057;345672;3504492d24d3e43f8eb174e60241fb26
//			62057-62097;2721;3504492d24d3e43f8eb174e60241fb27
//			*61598-62097;6725;3504492d24d3e43f8eb174e60241fb28
//			62097-62104;13725;3504492d24d3e43f8eb172453241fb28

//注：* 为整合包，也就是从原始base版本包与最新的版本包所打的集合更新包
//获取更新列表只取最新的 整合包数据，并且按照版本排列更新包队列
		private void UF_WrapUpdateList(string filechunk,string curResVersion,List<UpgradeFileInfo> outUpdateFileinfos){
			//全部都的更新信息
			List<UpgradeFileInfo> lstTotalUpdateInfo = new List<UpgradeFileInfo>();

			if(string.IsNullOrEmpty(filechunk)){return;}

			StringReader sr = new StringReader(filechunk);
			string line = "";
			string assembleChunk = "";
			line = sr.ReadLine ();

			//获取base版本
			webBaseResVersion = line.Substring(line.IndexOf("=")+1);

			UF_LogInfo("WebBaseAssetVersion: "+webBaseResVersion);

			while((line = sr.ReadLine()) != null){
				if( line != "")
				{
					//只取最后版本的整合包作为最新整合包
					if (line.StartsWith ("*")) {
						assembleChunk = line.Trim().Substring(1);
					} else {
                        var upgradeinfo = UF_CreateUpgradeFileInfo(line.Trim());
                        lstTotalUpdateInfo.Add (upgradeinfo);
                    }
				}
			}

			//当前版本等于web base版本，即为随包版本
			//检查是否有整合包，并设置为首个更新包
			if (curResVersion == webBaseResVersion && !string.IsNullOrEmpty(assembleChunk)) {
				UpgradeFileInfo assemblePack = UF_CreateUpgradeFileInfo(assembleChunk);
				outUpdateFileinfos.Add (assemblePack);
				//以最新版本继续往下检测更新包
				curResVersion = assemblePack.version;
                UF_LogInfo("Add Upgrade Assemble Info: " + assemblePack.version);
            }

			bool hook_mark = false;
			for(int k = 0;k < lstTotalUpdateInfo.Count;k++){
                if (curResVersion == webBaseResVersion) {
                    hook_mark = true;
                }
				else if(curResVersion == lstTotalUpdateInfo[k].version)
                {
					hook_mark = true;
					continue;
				}
				if(hook_mark){
					outUpdateFileinfos.Add(lstTotalUpdateInfo[k]);
                    UF_LogInfo("Add Upgrade File Info: " + lstTotalUpdateInfo[k].version);
                }   
			}
		}


		private string UF_WrapResVersionCode(string version_file){
			if (!string.IsNullOrEmpty (version_file)) {
				int idx_start = version_file.LastIndexOf ('-');
				int idx_end = version_file.IndexOf (';');
				if (idx_start > -1 && idx_end > -1 && idx_end > idx_start) {
					return version_file.Substring (idx_start + 1,idx_end - idx_start - 1);
				}
			}
			return localAssetVersion;
		}


		void UF_EventFixClient(object[] args){
			this.UF_FixClient();
		}


        void UF_EventGameMainPreStart(object[] args)
        {
            m_UpgradeView.UF_ShowPreStart();
        }

        void UF_EventGameMainStart(object[] args){
            m_UpgradeView.UF_Close();
        }


        public void UF_OnAwake(){
            
            MessageSystem.UF_GetInstance().UF_AddListener(DefineEvent.E_MAIN_PRE_START, UF_EventGameMainPreStart);
            MessageSystem.UF_GetInstance().UF_AddListener(DefineEvent.E_MAIN_START, UF_EventGameMainStart);
            MessageSystem.UF_GetInstance().UF_AddListener(DefineEvent.E_CLIENT_FIX, UF_EventFixClient);
        }


        public void UF_OnUpdate() {
            m_UpgradeView.UF_Update();
        }


        public void UF_OnReset() {
            m_LstUpdateFileInfo.Clear();
        }

	}

}