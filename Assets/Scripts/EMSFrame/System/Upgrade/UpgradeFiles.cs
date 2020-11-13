//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;  
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using AOT;
using System.Text;

namespace UnityFrame{
	class UpgradeThread {
		public delegate int INVOKE_VOID();
		public delegate int INVOKE_PARAM(object e);

		private INVOKE_VOID m_VoidFunc = null;
		private INVOKE_PARAM m_ParamFunc = null;
		private bool m_bIsRunning = false;
		private object m_Param = null;
		private Thread m_Thread = null;
		private int m_RetCode = 0;

		public UpgradeThread() {
			m_bIsRunning = false;
			m_VoidFunc = null;
			m_ParamFunc = null;
			m_Thread = null;
		}

		public int retcode {
			get {return m_RetCode;}
		}

		public bool UF_start(INVOKE_VOID Func) {
			if ( m_bIsRunning )
				return false;
			m_bIsRunning = true;
			m_VoidFunc = Func;
			m_Thread = new Thread (new ThreadStart(UF_thread_run));
			m_Thread.Start();
			return true;
		}

		public bool UF_start(INVOKE_PARAM Func,object e) {
			m_bIsRunning = true;
			m_ParamFunc = Func;
			m_Param = e;
			m_Thread = new Thread (new ThreadStart(UF_thread_run));
			m_Thread.Start();
			return true;
		}

		private void UF_thread_run() {
			if ( null != m_VoidFunc )
				m_RetCode = m_VoidFunc();
			else if ( null != m_ParamFunc )
				m_RetCode = m_ParamFunc (m_Param);
			m_bIsRunning = false;
			m_ParamFunc = null;
			m_VoidFunc = null;
			m_Param = null;
		}

		public IEnumerator UF_check_run_end() {
			while ( m_bIsRunning )
				yield return null;
		}
	}


	public class UpgradeFiles
	{
		enum FILE_TYPE{
			MODIFY,
			DELETE,
			ADD,
			UNKNOW,
		}

		private WebDownload downloader;

		

		public string UpgradeInfo{ get; private set;}

		public float Progress{
			get{
                if (UseDownloadProgress)
                    return Mathf.Clamp01((m_CurProgress + s_ProgresssZip + m_ProgressUpdate + DownloadProgress) / m_ProgressTotal);
                else
                    return Mathf.Clamp01((m_CurProgress + s_ProgresssZip + m_ProgressUpdate) / m_ProgressTotal);
            }
		}

        public float DownloadSpeed { get { if (downloader != null) return downloader.Speed; return -1; } }

        private float DownloadProgress{get { return downloader != null ? downloader.Progress : 0; }}

        public bool UseDownloadProgress { get { return m_UseDownloadProgress; } }

        //解压文件,需要是静态，用于取值
        private static float s_ProgresssZip = 0;

        //文件流缓冲，减少GC
        private static byte[] s_FileByteBuffer = new byte[2048];

        private bool m_UseDownloadProgress = false;
        //更新文件
        private float m_ProgressUpdate = 0;
		private float m_ProgressTotal = 1;
		private float m_CurProgress = 0;

		static DelegateMethodExtractProgress Handle_ExtractProgress;
		static DelegateMethodExtractError Handle_UnZipError;

		public UpgradeFiles()
		{
			Handle_ExtractProgress = new DelegateMethodExtractProgress(UF_Event_UnZipFileProgress);
			Handle_UnZipError = new DelegateMethodExtractError(UF_Event_UnZipError);
		}

		/// <summary>
		/// 解压文件事件进度
		/// </summary>
		[MonoPInvokeCallback(typeof(DelegateMethodExtractProgress))]
		static int UF_Event_UnZipFileProgress(uint p1,uint p2)
		{
			s_ProgresssZip = (float)p1/(float)p2;
			return 0;
		}
		/// <summary>
		/// 解压安装包事件进度
		/// </summary>
		[MonoPInvokeCallback(typeof(DelegateMethodExtractError))]
		static int UF_Event_UnZipError(System.IntPtr ptrMsg,uint p1){
			string msg = Marshal.PtrToStringAnsi (ptrMsg);
			Debugger.UF_Error (string.Format("DelegateMethodExtractError-->> retcode:{0}   msg:{1}",p1,msg));
			return 0;
		}

		private FILE_TYPE UF_stringToFTEnum(string str){
			switch(str){
			case "A":return FILE_TYPE.ADD;
			case "D":return FILE_TYPE.DELETE;
			case "M":return FILE_TYPE.MODIFY;
			default: return FILE_TYPE.UNKNOW;
			}
		}

		private void UF_logInfo(string info){
			Debugger.UF_LogTag("upgrade",info);
		}

		private void UF_logError(string error){
			Debugger.UF_Error (error);
		}

		private void UF_logWarn(string error){
			Debugger.UF_Warn (error);
		}


		private Coroutine UF_StartCoroutine(IEnumerator method){
			return GameMain.Instance.StartCoroutine(method);
		}

		/// <summary>
		/// 创建更新文件字典映射 
		/// </summary>
		private int UF_wrapdicFilePatch(string path,Dictionary<string,FILE_TYPE> dicFilePatch){
			FileInfo fi = new FileInfo(path);
			if(!fi.Exists){
				UF_logError(string.Format("unzip :version zip file [{0}] not exist! ",path)); 
				return -1;
			}

			StreamReader sr =  fi.OpenText();
			string txt = sr.ReadToEnd();
			sr.Dispose();
			sr.Close();
			if(txt == null || txt == ""){
				UF_logError(string.Format("unzip :version zip file [{0}] is null or empty! ",path));
				return -2;
			}

			StringReader strR = new StringReader(txt);
			string line = "";
			string version = strR.ReadLine();

			UF_logInfo("Patch Pacakge Version:" + version);
			try{
				//根据具体情况而定
				while((line = strR.ReadLine()) != null)
				{
					if(line != ""){
						string[] array = line.Split(new char[] { '\t' },System.StringSplitOptions.RemoveEmptyEntries);
						if (array == null || array.Length < 2) {
							UF_logError(string.Format("Line Split Error in :[{0}] ",line));
							return -3;
						}
						UF_logInfo(string.Format("{0}\t{1}",array [0].Trim (),array [1].Trim ()));
						dicFilePatch.Add (array [1].Trim (), UF_stringToFTEnum(array [0].Trim ()));
					}
				}
			}
			catch(System.Exception e){
				UF_logError(string.Format("Wrap Patch File Exception:{0}",e.Message));
				UF_logError(e.StackTrace);
				return -4;
			}

			strR.Close();
			return 0;
		}


		public void UF_SetUpgradeInfo(string info){
			UpgradeInfo = info;
		}


		/// <summary>
		/// 开始安装base
		/// </summary>
		public void UF_InstallBase(DelegateMethod callback){
            UF_StartCoroutine(UF_coroutine_start_install_game(callback));
		}

		//携程启动安装
		private IEnumerator UF_coroutine_start_install_game(DelegateMethod callback){
            //清空之前解压的代码文件，避免覆盖安装错误问题
#if !UNITY_EDITOR
			UF_deleteCacheFolder(new DirectoryInfo(GlobalPath.ScriptPath));
#endif
            if (GlobalSettings.InstallMode == "raw") {
                //无安装包方式
                UF_logInfo(">>>>>>>>>>>>>>>>>>>> Start Install Raw");
                UpgradeThread rawInstall = new UpgradeThread();
                rawInstall.UF_start(UF_threadInstallRaw, string.Empty);
                yield return UF_StartCoroutine(rawInstall.UF_check_run_end());
                UF_logInfo(">>>>>>>>>>>>>>>>>>>> End Install Raw");
                yield return null;
                if (callback != null) {
                    callback(rawInstall.retcode);
                }
            }
            else if (GlobalSettings.InstallMode == "net") {
                //安装包下载方式
                UF_logInfo(">>>>>>>>>>>>>>>>>>>> Start Install Net");
                UpgradeThread baseInstall = new UpgradeThread();
                baseInstall.UF_start(UF_threadInstallNet, "base.zip");
                yield return UF_StartCoroutine(baseInstall.UF_check_run_end());
                UF_logInfo(">>>>>>>>>>>>>>>>>>>> End Install Net");
                yield return null;
                if (callback != null)
                {
                    callback(baseInstall.retcode);
                }
            }
            else {
                //整包
                UF_logInfo(">>>>>>>>>>>>>>>>>>>> Start Install Base");
                UpgradeThread baseInstall = new UpgradeThread();
                baseInstall.UF_start(UF_threadInstallBase, "base.zip");
                yield return UF_StartCoroutine(baseInstall.UF_check_run_end());
                UF_logInfo(">>>>>>>>>>>>>>>>>>>> End Install Base");
                yield return null;
                if (callback != null) {
                    callback(baseInstall.retcode);
                }
            }
		}

		//raw 的安装方式
		//拷贝全部Raw中文件到ResPersistent
		private int UF_threadInstallRaw(object e) {
			int retcode = 0;
			m_ProgressTotal = Mathf.Max(1,GlobalSettings.ResFileCount);
			m_ProgressUpdate = 0;
			s_ProgresssZip = 0;
			m_CurProgress = 0;

			int bytekey = GlobalSettings.ResBKey;

			string runtimePath = GlobalPath.ScriptPath;
			string bundlePath = GlobalPath.BundlePath;
			string rawVersionFile = GlobalPath.StreamingAssetsPath + "/res_version.txt";
			string resVersionFile = GlobalPath.ResPersistentPath + "/res_version.txt";

			UF_SetUpgradeInfo(GlobalText.LAN_INSTALL_GAME);

			UF_logInfo (string.Format("App is Raw Asset package,Copy Raw to Persistent folder,File Count:{0}",m_ProgressTotal));

            //清除Res中的全部Runtime
            UF_deleteCacheFolder(new DirectoryInfo (runtimePath));

            //确保AssetDataBase中已经读入assetbase.asset资源文件映射信息


			//安装Asset 文件
            var listAssets = AssetDataBases.UF_GetAllAssets(AssetDataBases.AssetFileType.Runtimes | AssetDataBases.AssetFileType.Bundle);
            if (listAssets.Count > 0)
            {
                UF_logInfo(string.Format("Start to Install Raw Asset: {0} ", listAssets.Count));
                UF_installAssetFiles(listAssets, GlobalPath.ResPersistentPath, bytekey, ref m_CurProgress);
            }
            else {
                UF_logError("Asset to install if zero,Check AssetDataBase is already load asset table");
                return -1101;
            }


            //拷贝version 文件
            UF_copyfile(rawVersionFile, resVersionFile);

			m_CurProgress = m_ProgressTotal;

			return retcode;
		}



		//zip 的安装方式
		private int UF_threadInstallBase(object e) {
			UF_logInfo ("running in thread -> threadInstallzip");

			string zipName = e as string;
			if (string.IsNullOrEmpty (zipName)) {
				UF_logError("Thread Install Failed,Zip Name is null");
			}

			UF_logInfo (string.Format ("Start Install Zip file:{0}", zipName));

			string zipfilePath = GlobalPath.CachePath +  zipName;

			string cachefolderPath = GlobalPath.CachePath;

			UF_SetUpgradeInfo(GlobalText.LAN_START_INSTALL_GAME);
			int retcode = 0;

			m_ProgressTotal = 1;
			m_ProgressUpdate = 0;
			s_ProgresssZip = 0;
			m_CurProgress = 0;

			#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IPHONE
			zipfilePath = GlobalPath.StreamingAssetsPath + "/"+zipName;
#elif UNITY_ANDROID
			string apkPath =  GlobalPath.AppPath;
			string packageName = "assets/"+zipName; 
			zipfilePath = cachefolderPath + zipName;
            UF_logInfo (string.Format ("Extract Base File[{0}] Zip file Form: {1} \n To: {2}", packageName, apkPath, cachefolderPath));
			//解压apK中的安装文件到cache目录
			retcode = DLLImport.LLSpecifiedFileExtract(apkPath,packageName,cachefolderPath);
			if(retcode != 0){
			UF_logError(string.Format("DLLImport.LLSpecifiedFileExtract Failed [{0}] \n{1}\n{2}\n{3}",retcode,apkPath,packageName,cachefolderPath));
			return retcode;
			}
			UF_logInfo ("Extract Install zip to Cache");
#endif
            UF_SetUpgradeInfo(GlobalText.LAN_INSTALL_GAME);

			//解压安装 文件
			retcode = DLLImport.LLFileExtract(zipfilePath,GlobalPath.ResPersistentPath,Handle_ExtractProgress,Handle_UnZipError);

			#if !UNITY_EDITOR && UNITY_ANDROID
			//清空cache 中的安装文件
			System.IO.FileInfo zipfi = new System.IO.FileInfo(zipfilePath);
			if(zipfi.Exists){zipfi.Delete();}
			UF_logInfo ("Clean Install file in Cache");
			#endif

			if(retcode != 0){
				UF_logError(string.Format("DLLImport.FileExtract Failed [{0}] \n{1}\n{2}",retcode,zipfilePath,GlobalPath.ResPersistentPath));
			}

            s_ProgresssZip = 1;

			return retcode;
		}

        private int UF_threadInstallNet(object e)
        {
            UF_logInfo("running in thread -> threaddownLoadAndIntallzip");

            string zipName = e as string;

            if (string.IsNullOrEmpty(zipName))
            {
                UF_logError("Thread Install Failed,Zip Name is null");
            }
            string zipfilePath = GlobalPath.CachePath + zipName;
            string url_zipfile = GlobalSettings.UrlAssetsUpgrade + zipName;

            UF_logInfo(string.Format("Start Install Zip file:{0}\n{1}\n{2}\n", zipName, zipfilePath, url_zipfile));

            string cachefolderPath = GlobalSettings.UrlAssetsUpgrade;

            UF_SetUpgradeInfo(GlobalText.LAN_START_INSTALL_GAME);

            int retcode = 0;

            m_ProgressTotal = 1;
            m_ProgressUpdate = 0;
            s_ProgresssZip = 0;
            m_CurProgress = 0;

            //下载base 安装包
            UF_SetUpgradeInfo(GlobalText.LAN_DOWNLOAD_GAME);
            m_UseDownloadProgress = true;
            while (true)
            {
                if (0 != (retcode = UF_downloadzipfile(url_zipfile, cachefolderPath)))
                {
                    //断点更新失败
                    if (retcode == 416)
                    {
                        if (File.Exists(zipfilePath))
                        {
                            UF_logError(string.Format("Download breakpoint continue Failed url:[{1}],try again now", retcode, url_zipfile));
                            File.Delete(zipfilePath);
                            continue;
                        }
                    }
                    else
                    {
                        UF_logError(string.Format("Download zipfile Failed retcode:[{0}]  url:[{1}]", retcode, url_zipfile));
                        break;
                    }
                }
                break;
            }
            //下载成功，进行安装
            UF_SetUpgradeInfo(GlobalText.LAN_INSTALL_GAME);
            m_UseDownloadProgress = false;
            m_ProgressTotal = 1;
            m_ProgressUpdate = 0;
            s_ProgresssZip = 0;
            m_CurProgress = 0;

            //解压安装 文件
            retcode = DLLImport.LLFileExtract(zipfilePath, GlobalPath.ResPersistentPath, Handle_ExtractProgress, Handle_UnZipError);
            
#if !UNITY_EDITOR && UNITY_ANDROID
			//清空cache 中的安装文件
			System.IO.FileInfo zipfi = new System.IO.FileInfo(zipfilePath);
			if(zipfi.Exists){zipfi.Delete();}
			UF_logInfo ("Clean Install file in Cache");
#endif

            if (retcode != 0)
            {
                UF_logError(string.Format("AppDLLImport.FileExtract Failed [{0}] \n{1}\n{2}", retcode, zipfilePath, GlobalPath.ResPersistentPath));
            }

            m_CurProgress = 1;

            return retcode;

        }


        private void UF_WriteLocalVersion(string version){
			string ver_file = GlobalPath.ResPersistentPath + "res_version.txt";
			if (File.Exists(ver_file)) {
				File.Delete(ver_file);
			}
			StreamWriter sw = new StreamWriter(new FileStream(ver_file, FileMode.CreateNew));
			sw.Write(version);
			sw.Close();
		}


		///开始更新文件 
		public void UF_Upgrade(List<UpgradeFileInfo> lstUpgrade, DelegateMethod callback)
		{
			if (lstUpgrade.Count == 0) {
				UF_logInfo ("Upgrade List is null or empty");
				callback (true);
				return;
			} else {
				UF_logInfo ("Upgrade List :");
				for (int k = 0; k < lstUpgrade.Count; k++) {
					UF_logInfo (string.Format("\tfile: {0} | size: {1}",lstUpgrade[k].version,lstUpgrade[k].size));		
				}
			}

            UF_StartCoroutine(UF_coroutine_start_update_file(lstUpgrade,callback));
		}

		private IEnumerator UF_coroutine_start_update_file(List<UpgradeFileInfo> lstUpgrade,DelegateMethod callback) {
			UF_logInfo ("Start Upgrade File");
			UpgradeThread thread = new UpgradeThread ();
			thread.UF_start(UF_thread_updatefile, lstUpgrade);
			yield return UF_StartCoroutine(thread.UF_check_run_end());

			if (callback != null)
				callback (thread.retcode == 0);

			UF_logInfo ("End Upgrade File");
		}

		private void UF_checkCacheFolder(string url){
			DirectoryInfo di = new DirectoryInfo(url);
			if(!di.Exists){
				di.Create();
			}
		}

		private int UF_thread_updatefile(object obj) {
			List<UpgradeFileInfo> lstUpgrade = (List<UpgradeFileInfo>)obj;
			string cachefolderPath = GlobalPath.CachePath;

			int retcode = 0;
			//进度包括，下载,解压，更新
			m_ProgressTotal = lstUpgrade.Count * 3;
			m_CurProgress = 0;
            m_UseDownloadProgress = true;
            for (int k = 0;k < lstUpgrade.Count;k++){
				s_ProgresssZip = 0;
				m_ProgressUpdate = 0;

				string url_zipfile = lstUpgrade [k].url;
				string zipfilePath = cachefolderPath + lstUpgrade[k].zip;

				UF_SetUpgradeInfo (GlobalText.LAN_START_UGRADE + string.Format ("  [{0}/{1}]  ", k + 1, lstUpgrade.Count));

                //创建缓存文件路径
                UF_checkCacheFolder(cachefolderPath);

				//检查随包体的补丁文件，如果存在，则直接安装解压，否则重新下载
				//下载更新资源
				if (0 != (retcode = UF_downloadzipfile(url_zipfile, cachefolderPath))) {
					//断点更新失败
					if (retcode == 416) {
						if (File.Exists (zipfilePath)) {
							UF_logError (string.Format ("Download breakpoint continue Failed retcode:[{0}] url:[{1}],try again now", retcode, url_zipfile));
							File.Delete (zipfilePath);
							k--;
							continue;
						}
					} else {
						UF_logError (string.Format ("Download zipfile Failed retcode:[{0}]  url:[{1}]", retcode, url_zipfile));
						return retcode;
					}
				}
                //校验文件 MD5 合法性
                //string md5 = GHelper.UF_GetMD5HashFromFile(url_zipfile);
                //if(md5 != lstUpgrade [k].ucode){
                //	UF_logError (string.Format ("Upgrade ZIP File[{0}] MD5 Check Failed:[{1}] | [{2}]",url_zipfile,md5,lstUpgrade [k].ucode));
                //	//删除校验失败的更新包，并重新下载
                //	deletefile (zipfilePath);
                //	k--;
                //	continue;
                //}
                //下载完成，更新进度
                m_CurProgress++;
                //更新补丁文件
                if (0 != (retcode = UF_install_patch_zip(zipfilePath))) {
					UF_logError (string.Format ("Install zipfile Failed retcode:[{0}]  zipfile:[{1}]", retcode, zipfilePath));
					return retcode;
				}
                //更新Version 文件
                UF_WriteLocalVersion(lstUpgrade[k].version);
                //解压与安装完成，更新进度
                m_CurProgress+=2;

                s_ProgresssZip = 0;
				m_ProgressUpdate = 0;
			}

            m_UseDownloadProgress = false;

            m_CurProgress = m_ProgressTotal;

			UF_SetUpgradeInfo(GlobalText.LAN_START_INIT);

			return 0;
		}

		private int UF_downloadzipfile(string url,string savefilepath) {
			downloader = new WebDownload();
			UF_logInfo(string.Format("DownLoad Asset:{0}",url));
			downloader.UF_download(url,savefilepath,false);
			int retcode = downloader.UF_SyncDownLoad(true);
			downloader = null;
			return retcode;
		}

		//安装补丁文件文件
		private int UF_install_patch_zip(string ZipFilePath) {
			UF_logInfo (string.Format("install_patch_zip -- begin install zipfile : {0}",ZipFilePath));

			string cachefolderPath = GlobalPath.CachePath;
			string resfolderPath = GlobalPath.ResPersistentPath;
			string rawfolderPath = GlobalPath.StreamingAssetsPath+"/";
			int retcode = 0;

			//检查zip包是否存在
			FileInfo fi = new FileInfo(ZipFilePath);
			if(!fi.Exists){
				UF_logError(string.Format("install_unzip -- zip file{0} not exist! ",ZipFilePath));
				return -103;
			}

			//解压补丁文件到缓存目录
			if (0 != (retcode = DLLImport.LLFileExtract(ZipFilePath, cachefolderPath, Handle_ExtractProgress, Handle_UnZipError))) {
				UF_logError(string.Format("install_unzip -- DLLImport.LLFileExtrac Failed retcode[{0}] zipfile[{1}]",retcode,ZipFilePath));
				return retcode;
			}

			UF_logInfo ("install_patch_zip -- loading version");

			//获取补丁列表处理解压出来的补丁文件
			Dictionary<string,FILE_TYPE> dicUpdatePatch = new Dictionary<string, FILE_TYPE> ();

			if (UF_wrapdicFilePatch(cachefolderPath+"res_version.txt",dicUpdatePatch) != 0 ) {
                UF_deleteCacheFolder( new DirectoryInfo(cachefolderPath));
				return -104;
			}

			UF_logInfo("Start Patching Files");
			//处理补丁文件
			if ((UF_handlepatchfile(cachefolderPath, resfolderPath,rawfolderPath, dicUpdatePatch)) != 0) {
                UF_deleteCacheFolder( new DirectoryInfo(cachefolderPath));
				return -105;
			}

            UF_deleteCacheFolder( new DirectoryInfo(cachefolderPath));

			UF_logInfo ("install_patch_zip -- install game thread end");

			return 0;
		}


		private bool UF_deleteCacheFolder(DirectoryInfo di){
			try{
				if(di.Exists){
					FileInfo[] fis = di.GetFiles();
					for(int k = 0;k < fis.Length;k++){
						fis[k].Delete();
					}
					DirectoryInfo[] dis = di.GetDirectories();
					for(int k = 0;k < dis.Length;k++){
                        UF_deleteCacheFolder(dis[k]);
					}
					di.Delete();
				}
				return true;
			}catch(System.Exception e){
				Debugger.UF_Exception(e);
				return false;
			}
		}

		private  int m_start_mark_time = 0;
		void UF_set_duration_start(string info){
			UF_logInfo(info);
			m_start_mark_time = System.Environment.TickCount;
		}

		int UF_get_duration_over(string info){
			int duration = System.Math.Abs(System.Environment.TickCount - m_start_mark_time);
			UF_logInfo(info+"  >>>  "+duration);
			return duration;
		}


		/// 检查文件夹
		private bool UF_checkfilefolder(string filePath,bool autoCreate){
			string path = Path.GetDirectoryName(filePath);
			bool ret = Directory.Exists (path);
			if (autoCreate && !ret) {
				Directory.CreateDirectory(path);
			}
			return ret;
		}


		private void UF_deletefile(string filefullpath){
			if (File.Exists(filefullpath)) {
				File.Delete (filefullpath);
			}
		}


		private void UF_movefile(string _from,string _to){
			if (File.Exists(_from)) {
				File.Move (_from, _to);
			}
		}

		private void UF_copyfile(string _from,string _to){
			if (File.Exists(_from)) {
				File.Copy (_from, _to,true);
			}
		}


		private static bool UF_copyfolder(string SourcePath, string DestinationPath, bool overwrite)  
		{  
			bool ret = false;  
			try  
			{
				if (Directory.Exists(SourcePath))  
				{
					if (Directory.Exists(DestinationPath) == false)  
						Directory.CreateDirectory(DestinationPath);  

					string path = DestinationPath +"/";

					foreach (string fls in Directory.GetFiles(SourcePath))  
					{  
						FileInfo flinfo = new FileInfo(fls);  
						flinfo.CopyTo(path + flinfo.Name, overwrite);
					}
					foreach (string drs in Directory.GetDirectories(SourcePath))  
					{  
						DirectoryInfo drinfo = new DirectoryInfo(drs);  
						if (UF_copyfolder(drs, path + drinfo.Name, overwrite) == false)  
							ret = false;  
					}
				}  
				ret = true;  
			}
			catch (System.Exception ex)  
			{  
				ret = false;
				Debugger.UF_Exception (ex);
			}
			return ret;  
		}

		private static void UF_copyFileTo(FileInfo fi,string toPath,int bytekey){
            if (bytekey == 0)
            {
                fi.CopyTo(toPath);
            }
            else {
                FileStream fs = fi.OpenRead();
                int len = (int)fs.Length;
                byte[] buffer = s_FileByteBuffer;
                if (len > 2048)
                {
                    buffer = new byte[len];
                }
                fs.Read(buffer, 0, len);
                fs.Close();
                if (bytekey != 0)
                {
                    for (int k = 0; k < len; k++)
                    {
                        buffer[k] = (byte)(buffer[k] ^ bytekey);
                    }
                }
                FileStream toFS = new FileStream(toPath, FileMode.Create);
                toFS.Write(buffer, 0, len);
                toFS.Close();
            }
		}


        private static void UF_installAssetFiles(List<AssetDataBases.AssetFileInfo> list,string dirPath, int bytekey, ref float count) {
            foreach (var afi in list) {
                FileInfo flinfo = new FileInfo(afi.path);
                string toPath = dirPath + afi.absName;
                string toDir = Path.GetDirectoryName(toPath);
                if (!Directory.Exists(toDir)) Directory.CreateDirectory(toDir);
                //使用字节偏移,拷贝到指定目录
                UF_copyFileTo(flinfo, toPath, bytekey);
                count++;
            }
        }


		//处理更新文件
		private int UF_handlepatchfile(string cachefolderPath,string filefolderPath,string rawfolderPath,Dictionary<string,FILE_TYPE> dicUpdatePatch){
			int count = 0;
			int whole_count = dicUpdatePatch.Count;
			int retcode = 0;
			m_ProgressUpdate = 0;

			try{
				foreach(KeyValuePair<string,FILE_TYPE> item in dicUpdatePatch)
				{

					string t_file_name = item.Key.Replace(".patch","").Replace("#","/");
					string patch_file_path = string.Format("{0}{1}", cachefolderPath, item.Key);
					string old_file_path = string.Format("{0}{1}", filefolderPath, t_file_name);

					UF_logInfo(string.Format("Patching: {0} ",t_file_name));

					if (item.Value == FILE_TYPE.MODIFY) {

						string new_file_path = string.Format("{0}{1}.new", cachefolderPath, Path.GetFileName(t_file_name));

						if (!File.Exists(old_file_path)) {
							//如果 Old File中没有原始文件，会在Raw 文件夹中拷贝原始文件到Res中,安卓除外
							#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IPHONE
							string raw_file_path = string.Format("{0}{1}", rawfolderPath, t_file_name);
							UF_logInfo(string.Format("Old File not Exist,Try Copy Raw file to:{0}",old_file_path));
							if(!File.Exists(raw_file_path)){
								UF_logError(string.Format("Raw File not Exist: {0} ", raw_file_path));
								return -201;	
							}
							else{
                                UF_checkfilefolder(old_file_path,true);
                                UF_copyfile(raw_file_path,old_file_path);
							}
							#else
							UF_logError(string.Format("Old File not Exist: {0} ", old_file_path));
							return -201;	
							#endif
						}
						if (!File.Exists(patch_file_path)) {
							UF_logError(string.Format("Patch File not Exist: {0} ", patch_file_path));
							return -202;
						}
						//对文件进行补丁，生成最新版本的文件
						retcode = DLLImport.LLFilePatch(old_file_path,new_file_path,patch_file_path);
						if (retcode != 0) {
							UF_logError(string.Format("DLLImport.LLFilePatch Failed  retcode[{0}]",retcode));
							UF_logError(old_file_path);
							UF_logError(new_file_path);
							UF_logError(patch_file_path);
							return retcode;
						}
						if (!File.Exists(new_file_path)) {
							UF_logError(string.Format("New File not Exist: {0} ", new_file_path));
							return -203;
						}
                        //删除原来的文件			
                        UF_deletefile(old_file_path);

                        //移动新的文件到原文件位置
                        UF_movefile(new_file_path, old_file_path);

					} else if (item.Value == FILE_TYPE.ADD) {
						if(UF_checkfilefolder(old_file_path,true)){
                            //删除原来的文件           
                            UF_deletefile(old_file_path);
						}
                        //移动新的文件到原文件位置
                        UF_movefile(patch_file_path, old_file_path);
					} else if (item.Value == FILE_TYPE.DELETE) {
                        //删除目标文件			
                        UF_deletefile(old_file_path);
					}

					count++;
					m_ProgressUpdate = (float)count/(float)whole_count;
				}
			}
			catch(System.Exception e){
				UF_logError(string.Format("Patch File Exception:{0}", e.Message));
				UF_logError(e.StackTrace);
			}

			m_ProgressUpdate = 1;

			return 0;
		}

	}
}