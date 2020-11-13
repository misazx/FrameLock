//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Runtime.InteropServices;


namespace UnityFrame
{
	public delegate void DelegateVoid();

	public delegate void DelegateType<T>(T param);

	public delegate void DelegateObject(UnityEngine.Object param);

	public delegate void DelegateEntity(EntityObject param);

	public delegate void DelegateTexture(UnityEngine.Texture2D texture);

	public delegate void DelegateResponse(int retcode,object param);

	public delegate void DelegateConnectState(string ip,int port,int state);

	public delegate void DelegateIntMethod(int param);

	public delegate void DelegateFloatMethod(float param);

	public delegate void DelegateBoolMethod(bool param);

    public delegate void DelegateStringMethod(string param);

    public delegate void DelegateMethod(object param);

	public delegate void DelegateVectorMethod(UnityEngine.Vector3 value);

	public delegate void DelegateMessage(object[] args);

	public delegate bool DelegateNForeach<T>(T value);

	public delegate string DelegateMsgToStrForeach(string info,int stamp);

	public delegate void DelegateUIResposition(IUIUpdate item,int curIndex,int lastIndex);

	//解压进度事件
	#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	#endif
	public delegate int DelegateMethodExtractProgress(uint index,uint totalcount);
	//解压错误
	#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	#endif
	public delegate int DelegateMethodExtractError(System.IntPtr ptrLogInfo,uint retcode);


}

