//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

#if UNITY_EDITOR || UNITY_STANDALONE

using System;  
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityFrame;


[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]  
	public class OpenFileName  
	{  
		public int structSize = 0;  
		public IntPtr dlgOwner = IntPtr.Zero;  
		public IntPtr instance = IntPtr.Zero;  
		public String filter = null;  
		public String customFilter = null;  
		public int maxCustFilter = 0;  
		public int filterIndex = 0;  
		public String file = null;  
		public int maxFile = 0;  
		public String fileTitle = null;  
		public int maxFileTitle = 0;  
		public String initialDir = null;  
		public String title = null;  
		public int flags = 0;  
		public short fileOffset = 0;  
		public short fileExtension = 0;  
		public String defExt = null;  
		public IntPtr custData = IntPtr.Zero;  
		public IntPtr hook = IntPtr.Zero;  
		public String templateName = null;  
		public IntPtr reservedPtr = IntPtr.Zero;  
		public int reservedInt = 0;  
		public int flagsEx = 0;  
	}

	public class WinInteractivePhoto
	{
		[DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]  
		public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);  

		public const int MAX_ICON_SIZE = 256;
		public const int MAX_IMAGE_WIDHT = 800;
		public const int MAX_IMAGE_HEIGHT = 600;


		private string OpenTextureFileSelect(){

#if UNITY_EDITOR_OSX
			return UnityEditor.EditorUtility.OpenFilePanel ("Image",UnityEngine.Application.dataPath,"png,jpg,jpeg,PNG,JPG");
#else
			OpenFileName ofn = new OpenFileName();  
			try{
			ofn.structSize = Marshal.SizeOf(ofn);  
			}
			catch(Exception e){
				Debugger.UF_Exception (e);
			}

			ofn.filter = "Image(*.jpg*.png)\0*.jpg;*.png";

			ofn.file = new string(new char[256]);  

			ofn.maxFile = ofn.file.Length;  

			ofn.fileTitle = new string(new char[64]);  

			ofn.maxFileTitle = ofn.fileTitle.Length;  

			ofn.initialDir = UnityEngine.Application.dataPath;//默认路径  

			ofn.title = "Open Project";  

			ofn.defExt = "JPG";//显示文件的类型  

			ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;//OFN_EXPLORER|OFN_FILEMUSTEXIST|OFN_PATHMUSTEXIST| OFN_ALLOWMULTISELECT|OFN_NOCHANGEDIR  
			if (GetOpenFileName (ofn)) {  
				Debug.Log ("Selected file with full path: {0}" + ofn.file);  
				return ofn.file;
			} else {
				return string.Empty;
			}
#endif
		}

		private Texture2D ReadTextureFromFile(string filePath){
			Texture2D texture = null;
			if (!File.Exists (filePath)) {
				Debugger.UF_Error (string.Format ("ReadTextureFromFile Error: File Not Exist :{0}",filePath));
				return null;
			}
			try{
				FileStream fs = new FileStream (filePath, FileMode.Open);
				byte[] buffer = new byte[fs.Length];
				fs.Read (buffer,0,(int)fs.Length);
				fs.Close ();
				texture = new Texture2D (512, 512, TextureFormat.RGB24, false, false);
				texture.LoadImage (buffer);

			}
			catch(System.Exception e){
				Debugger.UF_Exception (e);
			}
			return texture;
		}


		private string GetSaveFilePath(string fileName){
			if (!Directory.Exists (GlobalPath.TexturePath)) {
				Directory.CreateDirectory (GlobalPath.TexturePath);
			}
			return GlobalPath.TexturePath + fileName;
		}

		private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
		{
			Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, false);

//			float incX = (1.0f / (float)targetWidth);
//			float incY = (1.0f / (float)targetHeight);

			for (int i = 0; i < result.height; ++i)
			{
				for (int j = 0; j < result.width; ++j)
				{
					Color newColor = source.GetPixelBilinear((float)j / (float)result.width, (float)i / (float)result.height);
					result.SetPixel(j, i, newColor);
				}
			}

			result.Apply();
			return result;
		}

		private bool SaveTexture(Texture2D texture,string filepath){
			if (texture != null) {
				try {
					if(File.Exists(filepath)){
						File.Delete(filepath);
					}
					byte[] buffer = texture.EncodeToJPG(75);
					FileStream fs = new FileStream (filepath, FileMode.Create);
					fs.Write (buffer, 0, buffer.Length);
					fs.Close ();
					return true;
				} catch (Exception e) {
					Debugger.UF_Exception (e);
				}
			}
			return false;
		}


		public void OpenImageSelect(){
			string filePath = OpenTextureFileSelect ();
			if (!string.IsNullOrEmpty (filePath)) {
				Texture2D texture = ReadTextureFromFile (filePath);
				//缩放大小
				int width = texture.width;
				int height = texture.height;
				float scaleWidth = width;
				float scaleHeight = height;
				float factor = Mathf.Clamp01((float)MAX_IMAGE_HEIGHT/(float)height);
//				float factor = (float)MAX_IMAGE_HEIGHT/(float)height;
				scaleWidth = (float)width * factor;
				scaleHeight = (float)height * factor;
				if (scaleWidth > MAX_IMAGE_WIDHT) {
					float wfactor = MAX_IMAGE_WIDHT / scaleWidth;
					scaleWidth = MAX_IMAGE_WIDHT;
					scaleHeight *= wfactor;
				}

				texture = ScaleTexture(texture,(int)scaleWidth, (int)scaleHeight);
				string savepath = GetSaveFilePath ("temp.jpg");
				//保存到本地
				if (SaveTexture (texture, savepath)) {
					//派发返回事件
					MessageSystem.UF_GetInstance ().UF_Send(DefineEvent.E_LUA, DefineLuaEvent.E_CUSTOM_IMAGE_RESPONSE, savepath);
				}
			}
		}


		public void OpenIconSelect(){
			string filePath = OpenTextureFileSelect ();
			if (!string.IsNullOrEmpty (filePath)) {
				Texture2D texture = ReadTextureFromFile (filePath);
				texture = ScaleTexture(texture,MAX_ICON_SIZE, MAX_ICON_SIZE);

				string savepath = GetSaveFilePath ("temp.jpg");
				//保存到本地
				if (SaveTexture (texture, savepath)) {
					//派发返回事件
					MessageSystem.UF_GetInstance ().UF_Send(DefineEvent.E_LUA, DefineLuaEvent.E_CUSTOM_ICON_RESPONSE, savepath);
				}
			}
		}


	}

#endif