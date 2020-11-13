using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace UnityFrame
{
    public class CheckPointManager : HandleSingleton<CheckPointManager>, IOnAwake,IOnStart
    {

         /*
         *      point id 定义
         *      
         *      1   游戏启动
         *      2   SDK 初始化
         *      3   SDK 初始化完成
         *      4   游戏初始化
         *      5   检测资源版本信息
         *      6   安装资源包
         *      7   检测资源更新
         *      8   SDK登录开始
         *      9   SDK登录结束
         *      10   登陆游戏登陆服
         *      11   连接服务器
         *      12   进入游戏
         * */


        private string hostAddress { get; set; }

        private string urlCheckPoint { get {return GlobalSettings.UF_GetGlobalValue("NETWORK", "URL_CHECK_POINT");} }

        public static void UF_Send(int pointId)
        {
            CheckPointManager.UF_GetInstance().SendPoint(pointId, UF_GetInstance().UF_GetDeviceChunk(), "");
        }

        public static void UF_Send(int pointId, string userChunk)
        {
            CheckPointManager.UF_GetInstance().SendPoint(pointId, UF_GetInstance().UF_GetDeviceChunk(), userChunk);
        }

        public static void UF_Send(string urlchunk) {
            CheckPointManager.UF_GetInstance().UF_SendUrl(urlchunk);
        }

        public void UF_OnAwake() {
            hostAddress = NetworkSystem.UF_GetInstance().UF_GetHostIP();
        }

        public void UF_OnStart() {
            FrameHandle.UF_AddCoroutine(UF_ICoOnStart());
        }

        IEnumerator UF_ICoOnStart() {
            CheckPointManager.UF_Send(1);
            yield return null;
            CheckPointManager.UF_Send(4);
            yield break;
        }

        private string UF_GetDeviceChunk() {
            string deviceMAC = GlobalSettings.DeviceMac;
            string deviceModel = GlobalSettings.DeviceIMEI;
            string deviceType = GlobalSettings.DeviceModel;
            string appVersion = GlobalSettings.ResVersion;
            string networkType = GlobalSettings.NetworkType == 2 ? "wifi" : "4G";
            string platform = GlobalSettings.PlatformEnv;
            return string.Format("&ip={0}&mac={1}&device_model={2}&phone_model={3}&client_num={4}&network_type={5}&platform={6}",
                            hostAddress,
                            deviceMAC,
                            deviceModel,
                            deviceType,
                            appVersion,
                            networkType,
                            platform);
        }

        int idx = 0;
        internal void SendPoint(int pointId, string deviceChunk,string userChunk) {
            
            string param = string.Format("&reason={0}{1}{2}", pointId, deviceChunk, userChunk);
            string url = urlCheckPoint + param;
            Debugger.UF_Log("Check Point:" + url);
            //FrameHandle.UF_AddCoroutine(SendRequest(url));

            NetworkSystem.UF_GetInstance().UF_HttpGetRequest(url, "", 5000, null);

            //for (int k = 0; k < 100; k++) {
            //    NetworkSystem.UF_GetInstance().HttpGetRequest(url, "", 5000, (a,e)=> { idx++; Debug.Log(e.ToString() + "| " + idx); });
            //}
        }

        internal void UF_SendUrl(string urlChunk)
        {
            Debugger.UF_Log("Check Url:" + urlChunk);
            //FrameHandle.UF_AddCoroutine(SendRequest(urlChunk));
            NetworkSystem.UF_GetInstance().UF_HttpGetRequest(urlChunk, "", 5000, null);
        }

        //IEnumerator SendRequest(string url)
        //{
        //    UnityWebRequest request = UnityWebRequest.Get(url);
        //    yield return request.SendWebRequest();
        //    if (request.isHttpError || request.isNetworkError)
        //    {
        //        Debugger.UF_Error(request.error + "\n"+ url);
        //    }
        //    request.Dispose();
        //}

    }
    
}