//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace UnityFrame{
	/// <summary>
	/// 公共的动态库导入类
	/// </summary>
	public class DLLImport
	{

        //IOS SDK 使用
#if UNITY_IPHONE
		[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
		public static extern void __UnityCall(string methodName,string arg);

		//返回格式为IP&&Type = {ipv4,ipv6}
		[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
		public static extern string __GetIPv6(string mHost, string mPort);
#endif


        //自定义动态库导入
#if UNITY_EDITOR
        const string LLCore = "LLCore";
		#elif UNITY_IPHONE
		const string LLCore = "__Internal";
		#else
		const string LLCore = "LLCore";
		#endif

		//文件差异
		[DllImport(LLCore,CallingConvention = CallingConvention.Cdecl)]
		public static extern int LLFileDiff(string _oldfile,string _newfile,string _difffile);

        //文件打补丁
        [DllImport(LLCore,CallingConvention = CallingConvention.Cdecl)]
		public static extern int LLFilePatch(string _oldfile,string _newfile,string _patchfile);

        //解压全部文件
        //ptrprogress 进度回调函数
        //ptrerror 错误回调函数
        [DllImport(LLCore,CallingConvention = CallingConvention.Cdecl)]
		public static extern int LLFileExtract(string zipfile,string expath,IntPtr ptrprogress,IntPtr ptrerror);
		public static int LLFileExtract(string zipfile,string expath,DelegateMethodExtractProgress dprogress,DelegateMethodExtractError derror){
			IntPtr ptrprogress = Marshal.GetFunctionPointerForDelegate (dprogress);
			IntPtr ptrerror = Marshal.GetFunctionPointerForDelegate (derror);
			return LLFileExtract (zipfile, expath, ptrprogress, ptrerror);
		}

		//解压指定文件
		[DllImport(LLCore,CallingConvention = CallingConvention.Cdecl)]
		public static extern int LLSpecifiedFileExtract(string zipfile,string targetfile,string outpath);

        //解压指定文件夹
        [DllImport(LLCore, CallingConvention = CallingConvention.Cdecl)]
        public static extern int LLSpecifiedFolderExtract(string zipfile, string targetfolder, string outpath);

    }

}