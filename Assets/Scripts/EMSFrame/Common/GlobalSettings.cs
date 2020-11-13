//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFrame
{
	public static class GlobalSettings
	{
        //全局设置
        internal static ConfigFile GlobalConfig{ get; private set; }
        //用户设置
        internal static ConfigFile UserConfig{ get; private set; }

        //Native 相关信息
        internal static MsgDataStruct NativeInfo { get; private set; }
        //SDK 相关信息
        internal static MsgDataStruct SDKInfo { get; private set; }

        internal static bool IsApplicationQuit { get; set; }

        //平台环境
        public static string PlatformEnv{
			get{ 
				#if UNITY_EDITOR || UNITY_STANDALONE
				return "pc";
				#elif UNITY_ANDROID
				return "android";
				#elif UNITY_IOS
				return "ios";
				#else
				return "pc";
				#endif
			}
		}

		public static bool DebugMode{
			get{
				#if UNITY_EDITOR
					return true;
				#else
					return false;
				#endif
			}
		}


		//电池值0 - 1
		public static float BatteryValue {
			get{ 
				return SystemInfo.batteryLevel;
			}
		}
		//电池状态，Charging NotCharging
		public static string BatteryStatus {
			get{ 
				return SystemInfo.batteryStatus.ToString();
			}
		}
			
//		设备型号,机型信息
		public static string DeviceModel{
			get{ 
				return SystemInfo.deviceModel;
			}
		}

		//设备IMEI码
		public static string DeviceIMEI{
			get{ 
				return SystemInfo.deviceUniqueIdentifier;
			}
		}
			
//		设备系统
		public static string DeviceOS{
			get{ 
				return SystemInfo.operatingSystem;
			}
		}

        public static string DeviceMac
        {
            get
            {
                string physicalAddress = "";
                try
                {
                    System.Net.NetworkInformation.NetworkInterface[] nice = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
                    foreach (System.Net.NetworkInformation.NetworkInterface adaper in nice)
                    {
                        //Debug.Log(adaper.Description);
                        if (adaper.Description == "en0")
                        {
                            physicalAddress = adaper.GetPhysicalAddress().ToString();
                            break;
                        }
                        else
                        {
                            physicalAddress = adaper.GetPhysicalAddress().ToString();
                            if (physicalAddress != "")
                            {
                                break;
                            }
                        }
                    }

                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
                var strbuilder = StrBuilderCache.Acquire();
                //插入冒号
                for (int k = 0; k < physicalAddress.Length;k++) {
                    strbuilder.Append(physicalAddress[k]);
                    if (k % 2 != 0 && k < physicalAddress.Length - 1) {
                        strbuilder.Append(':');
                    }        
                }
                return StrBuilderCache.GetStringAndRelease(strbuilder);
            }
        }



        //当前网络类型
        public static int NetworkType{
			get{ 
				switch (Application.internetReachability) {
				case NetworkReachability.ReachableViaCarrierDataNetwork:
					return 1;//3G/4G
				case NetworkReachability.ReachableViaLocalAreaNetwork:
					return 2;//WIFI
				}
				return 0;//None
			}
		}
		
		//应用版本
		public static string AppVersion{
			get{
				return GlobalSettings.UF_GetGlobalValue ("APP", "APP_VERSION");
			}
		}

        public static string AppID { get; private set; }

        public static bool IsSdkOn{ get; private set; }

        //资源版本
        public static string ResVersion{
			get{
				return GlobalSettings.UF_GetGlobalValue ("APP", "ASSET_VERSION");
			}
		}

        //当前资源版本
        public static string CurResVersion
        {
            get
            {
                return UpgradeSystem.LocalAssetVersion;
            }
        }

        //当前chunk号
        public static string VersionChunk
        {
            get
            {
                return UpgradeSystem.VersionChunk;
            }
        }

        public static string HostIP {
            get {
                return NetworkSystem.UF_GetInstance().UF_GetHostIP();
            }
        }


        //资源前缀
        public static string ResPrefix {
			get;
			private set;
		}
        //资源后缀
        public static string ResSuffix
        {
            get;
            private set;
        }

        public static bool IsHasResPrefix
        {
            get { return !string.IsNullOrEmpty(ResPrefix); }
        }

        //资源文件数
        public static int ResFileCount{
				get{ 
					int count = 0;
					int.TryParse(GlobalSettings.UF_GetGlobalValue ("APP", "RES_COUNT"),out count);
					return count;
				}
		}

		//无压缩资源包，所有资源在Raw中，决定应用安装方式
		public static bool IsRawAsset{
            get;
            private set;
		}

        //安装包方式
        public static string InstallMode {
            get {
                var ret = GlobalSettings.UF_GetGlobalValue("APP", "INS_MODE");
                if (string.IsNullOrEmpty(ret))
                    ret = "base";
                return ret;
            }
        }

        //原站下载连接地址
        public static string UrlRawAssetsUpgrade
        {
            get
            {

                string url = GlobalSettings.UF_GetGlobalValue("NETWORK", "URL_RAW_ASSET_UPGRADE");
                //如果没有配置，则取CND下载链接
                if (string.IsNullOrEmpty(url)) {
                    url = UrlAssetsUpgrade;
                }
                return url;
            }
        }


        //资源下载连接
        public static string UrlAssetsUpgrade {
            get {
                return GlobalSettings.UF_GetGlobalValue("NETWORK", "URL_ASSET_UPGRADE");
            }
        }



        //资源字节键，用于读取使用BKey偏移资源字节
        public static int ResBKey {
            get; private set;
        }

        //资源字节加密键
        public static int EncBKey {
            get; private set;
        }

        //配置文件加密键，固定
        internal static int ConfigEncBKey {get { return 520;}}

        //是否审核状态
        public static bool IsAppCheck {
			get;
			private set;
		}


        //请求是否有最新更新
        public static void UF_RequestCheckUpgrade(DelegateBoolMethod callback) {
            UpgradeSystem.UF_GetInstance().UF_RequestCheckUpgrade(callback);
        }

        //应用重启
        public static void UF_AppReboot()
        {
            GameMain.Instance.UF_GameReboot();
        }


        //获取全局配置值
        public static string UF_GetGlobalValue(string setion,string key){
            if (GlobalConfig != null)
            {
                return GlobalConfig.UF_GetString(setion, key, "");
            }
            else {
                Debugger.UF_Warn("GlobalConfig has not been init");
                return string.Empty;
            }
		}
		//获取用户配置值
		public static string UF_GetUserValue(string setion,string key){
            if (GlobalConfig != null)
            {
                return UserConfig.UF_GetString(setion, key, "");
            }
            else
            {
                Debugger.UF_Warn("UserConfig has not been init");
                return string.Empty;
            }
        }
		//设置用户配置值
		public static void UF_SetUserValue(string setion,string key,string value){
            UserConfig.UF_SetString(setion, key, value);
            UserConfig.UF_Save();
		}


        public static string UF_GetNativeValue(string key) {
            if (NativeInfo != null)
                return NativeInfo.UF_GetValue(key);
            else
                return string.Empty;
        }

        public static string UF_GetSDKValue(string key) {
            if (SDKInfo != null)
                return SDKInfo.UF_GetValue(key);
            else
                return string.Empty;
        }


        internal static void UF_SetNativeInfo(MsgDataStruct config) {
            NativeInfo = config;
        }

        //获取native 的配置信息
        static IEnumerator UF_IGetNativeInfo() {
            Debugger.UF_Log("Start Get Native info");
            float starttick = System.Environment.TickCount;
            float interval = 1000;
            VendorSDK.UF_Call("onNativeInfo","");
            while (NativeInfo == null) {
                if (NativeInfo != null || Mathf.Abs(System.Environment.TickCount - starttick) > interval) {
                    Debugger.UF_Error("Get Native Info Timeout");
                    break;
                }
                yield return null;
            }
            if (NativeInfo != null)
            {
                Debugger.UF_Log(string.Format("Native Info:\n{0}", NativeInfo.UF_Serialize()));
                //自动适配刘海屏幕
                UIManager.UF_GetInstance().UF_AutoFitNotchScreen();
            }
            else {
                Debugger.UF_Warn("Can not Get Native Info !");
            }
        }

        internal static void UF_SetSDKInfo(MsgDataStruct config)
        {
            SDKInfo = config;
        }

        //获取SDK信息
        static IEnumerator UF_IGetSDKInfo()
        {
            Debugger.UF_Log("==== Waiting For SDK info ====");
            IsAppCheck = false;
            
            //等待返回,无限期等待
            while (SDKInfo == null)
            {
                if (SDKInfo != null)
                {
                    break;
                }
                yield return null;
            }
            if (SDKInfo != null)
            {
                Debugger.UF_Log(string.Format("SDK Info:\n{0}", SDKInfo.UF_Serialize()));
                //是否接入了SDK
                IsSdkOn = SDKInfo.UF_GetValue("SDK_ON", "0") == "1";
                //当前的APPID
                AppID = SDKInfo.UF_GetValue("APP_ID","0");
                //查询获取审核状态
                IsAppCheck = SDKInfo.UF_GetValue("APP_CHECK", "0") == "1";
            }
            yield break;
        }

        

        static IEnumerator UF_IELoadSettingsFile(ConfigFile config, string settingFileName)
        {
            string filepath = GlobalPath.StreamingAssetsPath + "/" + settingFileName;
#if UNITY_EDITOR || UNITY_IPHONE || UNITY_STANDALONE
            WWW www = new WWW("file://" + filepath);
#elif UNITY_ANDROID
			WWW www = new WWW (filepath);
#endif
            yield return www;
            if (string.IsNullOrEmpty(www.error))
            {
                //检查配置是否加密
                var bytedata = GHelper.UF_BytesCopy(www.bytes);
                string chunk = System.Text.Encoding.UTF8.GetString(bytedata, 0, Mathf.Min(bytedata.Length, 8)).Trim();
                if (chunk.StartsWith("[", System.StringComparison.Ordinal) || chunk.StartsWith("#", System.StringComparison.Ordinal))
                {
                    Debugger.UF_Log("No need to encryp setting file:" + settingFileName);
                }
                else {
                    Debugger.UF_Log(string.Format("Encryp setting file[{0}] with key[{1}]", settingFileName, ConfigEncBKey));
                    GHelper.UF_BytesKey(bytedata, ConfigEncBKey);
                }
                System.IO.MemoryStream ms = new System.IO.MemoryStream(bytedata);
                config.UF_OpenReader(new System.IO.StreamReader(ms));
            }
            else
            {
                Debugger.UF_Warn(string.Format("IELoadSettingsFile:[{0}]  error:［{1}］ ", filepath, www.error));
            }
            www.Dispose();
        }


        //获取扩展信息
        internal static IEnumerator UF_InitExternInfo() {
            //获取SDK 信息
            yield return GameMain.Instance.StartCoroutine(UF_IGetSDKInfo());
            //获取native 信息
            yield return GameMain.Instance.StartCoroutine(UF_IGetNativeInfo());
        }


        //载入全局配置
        internal static IEnumerator UF_InitGameConfigs(){
            GlobalConfig = new ConfigFile();
            UserConfig = new ConfigFile(GlobalPath.ResPersistentPath + "user_setting.ini");

            yield return GameMain.Instance.StartCoroutine(UF_IELoadSettingsFile(GlobalConfig, "settings.ini"));
            //app打包配置
            yield return GameMain.Instance.StartCoroutine(UF_IELoadSettingsFile(GlobalConfig, "appsettings.ini"));
            //渠道配置
            yield return GameMain.Instance.StartCoroutine(UF_IELoadSettingsFile(GlobalConfig, "vdsettings.ini"));

            //for test
#if UNITY_EDITOR
            IsAppCheck = GlobalSettings.UF_GetGlobalValue("DEBUG", "REVIEW") == "1";
#endif

            if (IsAppCheck) {
                //审核配置覆盖
                yield return GameMain.Instance.StartCoroutine(UF_IELoadSettingsFile(GlobalConfig, "rwsettings.ini"));
            }

            CheckPointManager.UF_Send(2);

            ResPrefix = GlobalSettings.UF_GetGlobalValue ("APP", "RES_PREFIX");
            ResSuffix = GlobalSettings.UF_GetGlobalValue("APP", "RES_SUFFIX");

            ResBKey = GHelper.UF_ParseInt(GlobalSettings.UF_GetGlobalValue("APP", "RES_BKEY"));
            EncBKey = GHelper.UF_ParseInt(GlobalSettings.UF_GetGlobalValue("APP", "ENC_KEY"));
            IsRawAsset = InstallMode == "raw";
            Debugger.IsActive = GlobalSettings.UF_GetGlobalValue("DEBUG", "CONSOLE") == "1" ||
                                GlobalSettings.UF_GetUserValue("DEBUG", "PIN_CONSOLE") == "1";
            yield return null;
            CheckPointManager.UF_Send(3);
        }




	}
}

