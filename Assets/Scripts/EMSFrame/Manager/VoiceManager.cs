//#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE)
//#define __VOICE_ACTIVE
//#endif
//#define __VOICE_ACTIVE

using UnityEngine;
using System.Collections.Generic;

namespace UnityFrame
{
    public class VoiceManager : HandleSingleton<VoiceManager>, IOnStart, IOnApplicationPause, IOnApplicationQuit, IOnReset
    {
        //private VoiceSpeechSystem mVoiceSystem = new VoiceSpeechSystem();

        //登录语音服务器，以playerID 
        public void UF_Login(string playerID)
        {
#if __VOICE_ACTIVE
            mVoiceSystem.Login(playerID);
#endif
        }

        public void UF_Logout()
        {
#if __VOICE_ACTIVE
            mVoiceSystem.Logout();
#endif
        }

        public bool UF_CheckIsPlaying()
        {
#if __VOICE_ACTIVE
            return mVoiceSystem.IsPlaying;
#else
			return false;
#endif
        }

        public bool UF_CheckIsLogin()
        {
#if __VOICE_ACTIVE
            return mVoiceSystem.IsLogin;
#else
			return false;
#endif
        }

        public bool UF_CheckIsInit()
        {
#if __VOICE_ACTIVE
            return mVoiceSystem.IsInit;
#else
			return false;
#endif
        }

        public void UF_JoinVoiceChannel(string channelID)
        {
#if __VOICE_ACTIVE
            mVoiceSystem.JoinChannel(channelID);
#endif

        }

        public void UF_LevelVoiceChannel(string channelID)
        {
#if __VOICE_ACTIVE
            mVoiceSystem.LevelChannel(channelID);
#endif
        }


        public void UF_SendVoiceToChannel(string channelID)
        {
#if __VOICE_ACTIVE
            mVoiceSystem.SendVoiceToChannel(channelID);
#endif
        }

        public void UF_SendVoiceToPlayer(string playerID)
        {
#if __VOICE_ACTIVE
            mVoiceSystem.SendVoiceToPlayer(playerID);
#endif
        }

        public void UF_StopSendVoice()
        {
#if __VOICE_ACTIVE
            mVoiceSystem.StopSendVoice();
#endif
        }

        public void UF_CancelSendVoice()
        {
#if __VOICE_ACTIVE
            mVoiceSystem.CancelSendVoice();
#endif
        }


        //下载并播放语音
        public void UF_PlayVoice(string request)
        {
#if __VOICE_ACTIVE
            mVoiceSystem.PlayVoice(request);
#endif
        }

        public void UF_SetVoiceVolume(float volume)
        {
#if __VOICE_ACTIVE
            mVoiceSystem.SetVolume(volume);
#endif
        }

        //清除语音缓存
        public void UF_ClearVoiceCache()
        {
#if __VOICE_ACTIVE
            mVoiceSystem.ClearVoiceCache();
#endif
        }

        public void UF_OnStart()
        {
#if __VOICE_ACTIVE
            mVoiceSystem.Init();
#endif
        }

        public void OnApplicationPause(bool state)
        {
#if __VOICE_ACTIVE
            if (state)
            {
                mVoiceSystem.OnPause();
            }
            else
            {
                mVoiceSystem.OnResume();
            }
#endif
        }

        public void OnApplicationQuit()
        {
#if __VOICE_ACTIVE
            mVoiceSystem.OnQuit();
#endif
        }


        public void UF_OnReset()
        {
            if (UF_CheckIsLogin())
            {
                this.UF_Logout();
            }
        }

    }





    ////////////////////// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



    //public class VoiceSpeechSystem :
    //YIMEngine.LoginListen,
    //YIMEngine.MessageListen,
    //YIMEngine.AudioPlayListen,
    //YIMEngine.DownloadListen
    //{

    //    struct OperaChat
    //    {
    //        public YIMEngine.ChatType ChatType;
    //        public string RevID;
    //        public OperaChat(YIMEngine.ChatType _ChatType, string _RevID)
    //        {
    //            ChatType = _ChatType; RevID = _RevID;
    //        }
    //    }

    //    private string mAppKey = @"YOUME1C65AFCF9165A07E8C2A7886D7B3833DD5465A28";

    //    private string mAppSecrect = @"wmZeyIxP/pZhwkfYkte6EuzsRVvGbsNf/xFBIx07t2gwqIUrHrXsg8+jvBvCbg7ZRGWdC29iCxBardkUX2VUk9dekyUjRmwBn/MEeHtIEiIii92ooUaqhI1Q3POOkHBnTjLQwwjmmovPJ5jjQZgoITS8XZ+mA8biKNQ9f+vL0RsBAAE=";

    //    private Dictionary<ulong, OperaChat> mMapOperaChat = new Dictionary<ulong, OperaChat>();


    //    public string PlayerID { get { return mPlayerID; } }
    //    private string mPlayerID = "";

    //    public string ChanelID { get { return mChanelID; } }
    //    private string mChanelID = "";

    //    public bool IsInit { get { return mIsInit; } }

    //    public bool IsLogin { get { return mIsLogin; } }

    //    private string mPassword = "123456";

    //    private bool mIsInit = false;
    //    private bool mIsLogin = false;

    //    public bool IsPlaying { get { return mIsPlaying; } }
    //    private bool mIsPlaying = false;

    //    private float volumeInPlay = 0.15f;

    //    private float volumeInRecord = 0.0f;

    //    private float volumeNormal = 1.0f;

    //    private bool mMarkMicrophonePressiom = false;

    //    private string mCacheVoiceUrl = string.Empty;

    //    private DelegateBoolMethod mCheckWorkableCallback;

    //    //消息状态码
    //    const int STATE_ERROR = -1; //错误
    //    const int STATE_RECORD = 1; //开始录音
    //    const int STATE_SENDED = 2; //发送完成
    //    const int STATE_PLAYED = 3; //播放完成



    //    //客户端生产唯一码
    //    private string GenUniqeID()
    //    {
    //        return mPlayerID + System.Environment.TickCount;
    //    }

    //    private void ReturnVoiceError(int errorcode)
    //    {
    //        MessageSystem.UF_GetInstance().Send(DefineEvent.E_LUA, DefineLuaEvent.E_VOICE_STATE, STATE_ERROR, errorcode);
    //    }

    //    private void ReturnVoiceError(YIMEngine.ErrorCode errorcode)
    //    {
    //        Debugger.UF_Error("ReturnVoiceError:" + errorcode.ToString());
    //        if (errorcode == YIMEngine.ErrorCode.NotLogin)
    //        {
    //            mIsLogin = false;
    //        }
    //        else if (errorcode == YIMEngine.ErrorCode.EngineNotInit)
    //        {
    //            mIsInit = false;
    //        }
    //        ReturnVoiceError((int)errorcode);
    //    }

    //    private bool AutoFixVoiceError(bool fix = true)
    //    {
    //        if (!mIsInit)
    //        {
    //            ReturnVoiceError(YIMEngine.ErrorCode.EngineNotInit);
    //            if (fix)
    //                this.Init();
    //            return true;
    //        }
    //        if (!mIsLogin)
    //        {
    //            ReturnVoiceError(YIMEngine.ErrorCode.NotLogin);
    //            if (fix)
    //                this.Login(mPlayerID);
    //            return true;
    //        }
    //        return false;
    //    }


    //    private void AddToOperaChatType(ulong id, YIMEngine.ChatType chatType, string revID)
    //    {
    //        if (mMapOperaChat.ContainsKey(id))
    //        {
    //            mMapOperaChat[id] = new OperaChat(chatType, revID);
    //        }
    //        else
    //        {
    //            mMapOperaChat.Add(id, new OperaChat(chatType, revID));
    //        }
    //    }

    //    private void StopGameVolume()
    //    {
    //        AudioManager.UF_GetInstance().SetGlobalVolume(0);
    //    }

    //    private void SetGameVolume(float volume)
    //    {
    //        AudioManager.UF_GetInstance().SmoothGlobalVolume(volume, 0.3f);
    //    }


    //    public void SetVolume(float volume)
    //    {
    //        YIMEngine.IMAPI.Instance().SetVolume(volume);
    //    }

    //    //登录语音服务器，以playerID 
    //    public bool Login(string playerID)
    //    {
    //        mPlayerID = playerID;
    //        YIMEngine.ErrorCode errCode = YIMEngine.IMAPI.Instance().Login(playerID, mPassword, "");
    //        return errCode == YIMEngine.ErrorCode.Success;
    //    }

    //    public void Logout()
    //    {

    //        YIMEngine.IMAPI.Instance().Logout();

    //    }

    //    public void JoinChannel(string channelID)
    //    {

    //    }

    //    public void LevelChannel(string channelID)
    //    {

    //    }

    //    //      最小限制时长
    //    public void SendVoice(YIMEngine.ChatType chattype, string revID)
    //    {
    //        if (AutoFixVoiceError(false)) { return; }

    //        ulong iRequestID = 0;
    //        YIMEngine.ErrorCode errorcode = YIMEngine.IMAPI.Instance().StartAudioSpeech(ref iRequestID, true);
    //        Debugger.UF_Log(string.Format("Start SendVoice:{0} | {1} | {2}", errorcode.ToString(), iRequestID, revID));

    //        if (errorcode == YIMEngine.ErrorCode.Success)
    //        {
    //            //				SetGameVolume (volumeInRecord);
    //            StopGameVolume();
    //            AddToOperaChatType(iRequestID, chattype, revID);
    //            //发送正式开始录制
    //            MessageSystem.UF_GetInstance().Send(DefineEvent.E_LUA, DefineLuaEvent.E_VOICE_STATE, STATE_RECORD, 4);
    //        }
    //        else
    //        {
    //            if (errorcode == YIMEngine.ErrorCode.NotLogin)
    //            {
    //                //重新登录
    //                bool opera = this.Login(mPlayerID);
    //                Debugger.UF_Log("ReLogin Voice Server and Send Again!:" + opera.ToString());
    //                if (opera)
    //                {
    //                    mCheckWorkableCallback = delegate (bool value)
    //                    {
    //                        if (value)
    //                        {
    //                            this.SendVoice(chattype, revID);
    //                        }
    //                    };
    //                }
    //            }
    //            else
    //            {
    //                //录音失败
    //                ReturnVoiceError(errorcode);
    //            }
    //        }
    //    }

    //    public void SendVoiceToChannel(string channelID)
    //    {
    //        SendVoice(YIMEngine.ChatType.RoomChat, channelID);
    //    }

    //    public void SendVoiceToPlayer(string playerID)
    //    {
    //        SendVoice(YIMEngine.ChatType.PrivateChat, playerID);
    //    }

    //    public void StopSendVoice()
    //    {
    //        //            if (AutoFixVoiceError()){return;}
    //        Debugger.UF_Log("Stop SendVoice");
    //        YIMEngine.IMAPI.Instance().StopAudioSpeech();
    //        SetGameVolume(volumeNormal);
    //        mCheckWorkableCallback = null;
    //    }

    //    public void CancelSendVoice()
    //    {
    //        //            if (AutoFixVoiceError()){return;}
    //        Debugger.UF_Log("Cancel SendVoice");
    //        YIMEngine.IMAPI.Instance().CancleAudioMessage();
    //        SetGameVolume(volumeNormal);
    //        mCheckWorkableCallback = null;
    //    }

    //    private string GetURLToUniqueName(string url)
    //    {

    //        return GHelper.UF_GetMD5HashFromString(url);
    //    }

    //    private void startPlayVoice(string path)
    //    {
    //        if (mIsPlaying)
    //            YIMEngine.IMAPI.Instance().StopPlayAudio();
    //        mIsPlaying = YIMEngine.IMAPI.Instance().StartPlayAudio(path) == YIMEngine.ErrorCode.Success;
    //        if (mIsPlaying)
    //        {
    //            SetGameVolume(volumeInPlay);
    //        }
    //    }

    //    //下载并播放语音
    //    public void PlayVoice(string url)
    //    {
    //        string UniqeID = GetURLToUniqueName(url);
    //        //判断本地是否已经下载
    //        string voicePathWav = string.Format("{0}{1}{2}", GlobalPath.VoicePath, UniqeID, ".wav");
    //        //            string voicePathArm = string.Format("{0}{1}{2}",GlobalSettings.VoicePath , UniqeID , ".arm");
    //        if (System.IO.File.Exists(voicePathWav))
    //        {
    //            startPlayVoice(voicePathWav);
    //            mCacheVoiceUrl = url;
    //            return;
    //        }
    //        else
    //        {
    //            //下载文件
    //            YIMEngine.IMAPI.Instance().DownloadFileByUrl(url, voicePathWav);
    //        }
    //    }

    //    //清除语音缓存
    //    public void ClearVoiceCache()
    //    {
    //        try
    //        {
    //            string[] paths = System.IO.Directory.GetFiles(GlobalPath.VoicePath);
    //            if (paths != null)
    //            {
    //                for (int k = 0; k < paths.Length; k++)
    //                {
    //                    System.IO.Directory.Delete(paths[k]);
    //                }
    //            }
    //        }
    //        catch (System.Exception e)
    //        {
    //            Debug.LogException(e);
    //        }
    //    }



    //    public void Init()
    //    {
    //        if (mIsInit)
    //            return;
    //        YIMEngine.ErrorCode retcode = YIMEngine.IMAPI.Instance().Init(mAppKey, mAppSecrect, YIMEngine.ServerZone.China);
    //        if (retcode == YIMEngine.ErrorCode.Success)
    //        {
    //            mIsInit = true;
    //            YIMEngine.IMAPI.Instance().SetLoginListen(this);
    //            YIMEngine.IMAPI.Instance().SetMessageListen(this);
    //            YIMEngine.IMAPI.Instance().SetAudioPlayListen(this);
    //            YIMEngine.IMAPI.Instance().SetDownloadListen(this);

    //            Debugger.UF_Log("VoiceSpeechSystem Init Successed!");
    //        }
    //        else
    //        {
    //            Debugger.UF_Error("VoiceSpeechSystem Init Failed!  " + retcode.ToString());
    //        }
    //    }

    //    public void OnPause()
    //    {
    //        YIMEngine.IMAPI.Instance().OnPause(true);
    //    }

    //    public void OnResume()
    //    {
    //        YIMEngine.IMAPI.Instance().OnResume();
    //    }


    //    public void OnQuit()
    //    {

    //    }


    //    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //    ///Interface
    //    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //    #region YIMEngine.Download
    //    //下载完成并播放
    //    public void OnDownload(YIMEngine.ErrorCode errorcode, YIMEngine.MessageInfoBase message, string strSavePath)
    //    {
    //        if (errorcode == YIMEngine.ErrorCode.Success)
    //        {
    //            //                if (!string.IsNullOrEmpty(strSavePath)) {
    //            //                    YIMEngine.IMAPI.Instance().StartPlayAudio(strSavePath);
    //            //                }
    //        }
    //    }
    //    public void OnDownloadByUrl(YIMEngine.ErrorCode errorcode, string strFromUrl, string strSavePath)
    //    {
    //        if (errorcode == YIMEngine.ErrorCode.Success)
    //        {
    //            //转化为WAV
    //            //                string voicePathWav = strSavePath.Replace(".arm",".wav");
    //            //                if (YIMEngine.IMAPI.Instance().ConvertAMRToWav(strSavePath, voicePathWav) == YIMEngine.ErrorCode.Success) {
    //            //                    YIMEngine.IMAPI.Instance().StartPlayAudio(voicePathWav);
    //            //                } else {
    //            //                    Debugger.UF_Error(string.Format("ConvertAMRToWav Failed:{0}", strSavePath));
    //            //                }
    //            //下载完成播放
    //            startPlayVoice(strSavePath);
    //            mCacheVoiceUrl = strFromUrl;
    //        }
    //        else
    //        {
    //            ReturnVoiceError(errorcode);
    //        }
    //    }
    //    #endregion

    //    #region YIMEngine.Login

    //    public void OnLogin(YIMEngine.ErrorCode errorcode, string strYouMeID)
    //    {
    //        if (errorcode == YIMEngine.ErrorCode.Success || errorcode == YIMEngine.ErrorCode.AlreadyLogin)
    //        {
    //            Debugger.UF_Log(string.Format("VoiceSpeechSystem Login Successed:{0}", strYouMeID));
    //            mIsLogin = true;
    //        }
    //        else
    //        {
    //            ReturnVoiceError(errorcode);
    //            mIsLogin = false;
    //            Debugger.UF_Error(string.Format("VoiceSpeechSystem Login Failed:{0}", errorcode.ToString()));
    //        }
    //        if (mCheckWorkableCallback != null)
    //        {
    //            mCheckWorkableCallback(mIsLogin);
    //            mCheckWorkableCallback = null;
    //        }
    //        mIsPlaying = false;
    //    }

    //    public void OnLogout()
    //    {
    //        Debugger.UF_Log("Voice Server Has Logout!");
    //        mIsLogin = false;
    //        mIsPlaying = false;
    //    }

    //    public void OnKickOff() { }

    //    #endregion

    //    #region YIMEngine.MessageListen implementation
    //    //获取消息历史纪录回调
    //    public void OnQueryHistoryMessage(YIMEngine.ErrorCode errorcode, string targetID, int remain, List<YIMEngine.HistoryMsg> messageList) { }

    //    public void OnQueryRoomHistoryMessageFromServer(YIMEngine.ErrorCode errorcode, string roomID, int remain, List<YIMEngine.MessageInfoBase> messageList) { }

    //    public void OnSendMessageStatus(ulong iRequestID, YIMEngine.ErrorCode errorcode, uint sendTime, bool isForbidRoom, int reasonType, ulong forbidEndTime) { }

    //    public void OnStartSendAudioMessage(ulong iRequestID, YIMEngine.ErrorCode errorcode, string strText, string strAudioPath, int iDuration) { }

    //    public void OnSendAudioMessageStatus(ulong iRequestID, YIMEngine.ErrorCode errorcode, string strText, string strAudioPath, int iDuration, uint sendTime, bool isForbidRoom, int reasonType, ulong forbidEndTime) { }

    //    public void OnAccusationResultNotify(YIMEngine.AccusationDealResult result, string userID, uint accusationTime) { }

    //    //接受信息
    //    public void OnRecvMessage(YIMEngine.MessageInfoBase message) { }

    //    public void OnStopAudioSpeechStatus(YIMEngine.ErrorCode errorcode, ulong iRequestID, string strDownloadURL, int iDuration, int iFileSize, string strLocalPath, string strText)
    //    {
    //        Debugger.UF_Log(string.Format("CallBack Stop SendVoice:{0} | {1} | {2}", (int)errorcode, iDuration, strText));
    //        if (errorcode == YIMEngine.ErrorCode.Success || errorcode == YIMEngine.ErrorCode.PTT_ReachMaxDuration)
    //        {
    //            bool throwout = false;
    //            try
    //            {
    //                if (System.IO.File.Exists(strLocalPath))
    //                {
    //                    if (!System.IO.Directory.Exists(GlobalPath.VoicePath))
    //                        System.IO.Directory.CreateDirectory(GlobalPath.VoicePath);

    //                    string uniqueID = GetURLToUniqueName(strDownloadURL);
    //                    string voicePathWav = string.Format("{0}{1}{2}", GlobalPath.VoicePath, uniqueID, ".wav");
    //                    System.IO.File.Move(strLocalPath, voicePathWav);
    //                }
    //            }
    //            catch (System.Exception e)
    //            {
    //                Debugger.UF_Exception(e);
    //                throwout = true;
    //            }

    //            //发送消息到管理器，告诉语音发送成功
    //            //chatType  聊天类型 私聊 = 1,频道聊天 = 2,
    //            //revID     频道号或者是玩家ID
    //            //url 唯一下载地址
    //            //strText 翻译文本  
    //            //iDuration 持续时间／秒
    //            if (!throwout)
    //            {
    //                int chatType = (int)mMapOperaChat[iRequestID].ChatType;
    //                string revID = mMapOperaChat[iRequestID].RevID;
    //                //    Debugger.UF_Log(string.Format("SendVoice:{0} | {1} | {2} | {3} | {4}", chatType.ToString(), revID, strDownloadURL, strText, iDuration));
    //                MessageSystem.UF_GetInstance().Send(DefineEvent.E_LUA, DefineLuaEvent.E_VOICE_STATE, STATE_SENDED, chatType, revID, strDownloadURL, strText, iDuration);
    //            }
    //        }
    //        else
    //        {
    //            ReturnVoiceError(errorcode);
    //        }
    //    }

    //    public void OnRecvNewMessage(YIMEngine.ChatType chatType, string targetID) { }

    //    public void OnTranslateTextComplete(YIMEngine.ErrorCode errorcode, uint requestID, string text, YIMEngine.LanguageCode destLangCode) { }

    //    public void OnGetForbiddenSpeakInfo(YIMEngine.ErrorCode errorcode, List<YIMEngine.ForbiddenSpeakInfo> forbiddenSpeakList) { }

    //    public void OnGetRecognizeSpeechText(ulong iRequestID, YIMEngine.ErrorCode errorcode, string text) { }

    //    public void OnBlockUser(YIMEngine.ErrorCode errorcode, string userID, bool block) { }

    //    public void OnUnBlockAllUser(YIMEngine.ErrorCode errorcode) { }

    //    public void OnGetBlockUsers(YIMEngine.ErrorCode errorcode, List<string> userList) { }

    //    public void OnRecordVolumeChange(float volume) { }

    //    #endregion


    //    #region YIMEngine.AudioPlayListen implementation

    //    public void OnPlayCompletion(YIMEngine.ErrorCode errorcode, string path)
    //    {
    //        SetGameVolume(volumeNormal);
    //        mIsPlaying = false;
    //        MessageSystem.UF_GetInstance().Send(DefineEvent.E_LUA, DefineLuaEvent.E_VOICE_STATE, STATE_PLAYED, mCacheVoiceUrl);
    //    }

    //    public void OnGetMicrophoneStatus(YIMEngine.AudioDeviceStatus status)
    //    {
    //        Debugger.UF_Log(string.Format("Microphone Status:{0}", (int)status));
    //        //无权限
    //        if (status != YIMEngine.AudioDeviceStatus.STATUS_AVAILABLE)
    //        {
    //            ReturnVoiceError(-100 - (int)status);
    //            mMarkMicrophonePressiom = false;
    //        }
    //        else
    //        {
    //            mMarkMicrophonePressiom = true;
    //        }
    //    }
    //    #endregion

    //}


}
//#endif
