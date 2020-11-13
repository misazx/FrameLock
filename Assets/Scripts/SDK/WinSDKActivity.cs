#if UNITY_EDITOR || UNITY_STANDALONE
using System;
using UnityFrame;
using System.Reflection;

public class WinSDKActivity
{

	///////////////////////////////////////////////////////////////////////////////////////////////////// 
	//SDK接入模版
	/////////////////////////////////////////////////////////////////////////////////////////////////////




	/////////////////////////////////////////////////////////////////////////////////////////////////////
	//DLL Import
	/////////////////////////////////////////////////////////////////////////////////////////////////////




	//常量定义
	protected const string AppID = "";
	//变量定义
	protected string Token = "";

	protected string UserID = "";

	protected WinInteractivePhoto mWinInteractivePhoto = new WinInteractivePhoto();

	private void SendMsgToUnity(string eventID,MsgDataStruct dataStruct){
		this.SendMsgToUnity (eventID, dataStruct.UF_Serialize());
	}

	private void SendMsgToUnity(string eventID,string msg){
		MessageSystem.UF_GetInstance ().UF_HandleNativeMessage(string.Format ("{0};{1}", eventID, msg));
	}


	public void CallSDKMethod(string methodName,string arg){
		MethodInfo method = this.GetType ().GetMethod(methodName,BindingFlags.Instance|BindingFlags.GetField|BindingFlags.NonPublic|BindingFlags.Public);

		if (method != null) {
			MsgDataStruct dataStruct = new MsgDataStruct ();
			dataStruct.UF_SetTable(arg);

			object[] param = { dataStruct };

			method.Invoke (this, param);
		} else {
			Debugger.UF_Error ("Can not find SDK Method: " + methodName);
		}
	}



	public void Init(){
        MsgDataStruct dataStruct = new MsgDataStruct();
        //未接入SDK
        dataStruct.UF_SetValue("SDK_ON","0");
        SendMsgToUnity("SDK_INFO", dataStruct.UF_Serialize());
    }


	protected void onNativeInfo(MsgDataStruct msgData){
        SendMsgToUnity("NATIVE_INFO", "");
    }


	protected void sdkLogin(MsgDataStruct msgData){
		Debugger.UF_Log("Unity Call: " + msgData.UF_Serialize());
	}

	protected void sdkPay(MsgDataStruct msgData){

		 
	}

	protected void sdkCollectData(MsgDataStruct msgData){

	}



	protected void sdkLogout(MsgDataStruct msgData){


	}

    protected void sdkInfo(MsgDataStruct msgData)
    {


    }


    protected void sdkQuit(MsgDataStruct msgData){


	}

	protected void openCustomImage(MsgDataStruct msgData){
		mWinInteractivePhoto.OpenImageSelect ();
	}

	protected void openCustomIcon(MsgDataStruct msgData){
		mWinInteractivePhoto.OpenIconSelect ();
	}



	//返回登录信息到游戏中	
	protected void respSdkLogin(int retcode,String token,String userid,String appID){
		MsgDataStruct msgdata = new MsgDataStruct();  
		msgdata.UF_SetValue("retcode", ""+retcode);
		msgdata.UF_SetValue("token", token);
		msgdata.UF_SetValue("userid", userid);
		msgdata.UF_SetValue("appid", appID);
		this.SendMsgToUnity("E_SDK_LOGIN_RESPONSE",msgdata);
	}
	//返回登出信息到游戏中	
	protected void respSdkLogout(int retcode,String msg){
		MsgDataStruct msgdata = new MsgDataStruct();  
		msgdata.UF_SetValue("retcode", ""+retcode);
		msgdata.UF_SetValue("msg", msg);
		this.SendMsgToUnity("E_SDK_LOGOUT_RESPONSE",msgdata);
	}

	protected void respSdkPay(int retcode,String msg){
		MsgDataStruct msgdata = new MsgDataStruct();  
		msgdata.UF_SetValue("retcode", ""+retcode);
		msgdata.UF_SetValue("msg", msg);
		this.SendMsgToUnity("E_SDK_PAY_RESPONSE",msgdata);

	}


}
#endif