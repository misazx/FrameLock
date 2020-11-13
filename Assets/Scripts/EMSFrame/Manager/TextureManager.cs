//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace UnityFrame{
	public class TextureManager : HandleSingleton<TextureManager>,IOnStart{

		//图片请求队列
		private Dictionary<string,DelegateTexture> m_DicWebTextureRequeset = new Dictionary<string, DelegateTexture>();

		private Dictionary<string,string> m_DicMapWebTextureToLocal = new Dictionary<string, string> ();

		private Texture2D UF_SerializeImageFormBytes(byte[] bytes,string texName){
			Texture2D t2d = new Texture2D (512,512,TextureFormat.RGB24,false);
			t2d.name = texName;
			t2d.LoadImage (bytes);
			t2d.filterMode = FilterMode.Point;
            //添加引用管理
            RefObjectManager.UF_GetInstance().UF_RetainRef(t2d);
			return t2d;
		}

		public void UF_AddMapWebToLocal(string key,string value){
			if (m_DicMapWebTextureToLocal.ContainsKey (key)) {
				m_DicMapWebTextureToLocal [key] = value;
			} else {
				m_DicMapWebTextureToLocal.Add (key, value);
			}
		}

		public void UF_RemoveMapWebToLocal(string key){
			if (m_DicMapWebTextureToLocal.ContainsKey (key)) {
				m_DicMapWebTextureToLocal.Remove (key);
			}
		}

		public Texture2D UF_LoadTexture(string textureName){
            return RefObjectManager.UF_GetInstance().UF_LoadRefObject<Texture2D>(textureName, false) as Texture2D;
        }

		public int UF_LoadTextureAsync(string textureName,DelegateTexture callback){
            var tex = RefObjectManager.UF_GetInstance().UF_GetRefObject<Texture2D>(textureName, false) as Texture2D;
            if (tex != null) {
				if (callback != null) {
					callback (tex);
				}
			} else {
				DelegateObject _loadfinish = delegate(UnityEngine.Object image) {
					Texture2D texture = image as Texture2D;
					if(texture != null){
                        RefObjectManager.UF_GetInstance().UF_RetainRef(texture);
                    }
					if (callback != null) {
						callback (texture);
					}
				};
				return AssetSystem.UF_GetInstance ().UF_AsyncLoadObjectImage(textureName,_loadfinish);
			}
			return 0;
		}


		public Texture2D UF_LoadTextureLocal(string filePath){
			Texture2D ret = null;
			if (File.Exists (filePath)) {
				try{
					FileStream fs = new FileStream (filePath, FileMode.Open, FileAccess.Read);
					byte[] temp = new byte[fs.Length];
					fs.Read (temp,0,(int)fs.Length);
					fs.Close ();
					ret = UF_SerializeImageFormBytes(temp,Path.GetFileNameWithoutExtension(filePath));
				}
				catch(System.Exception e){
					Debugger.UF_Exception (e);
				}
			}
			return ret;
		}

        public Texture2D UF_LoadTextureBytes(byte[] bytes, string alitsName = "")
        {
            Texture2D ret = null;
            try
            {
                if (string.IsNullOrEmpty(alitsName)) {
                    alitsName = "tex_byte_" + bytes.Length;
                }
                ret = UF_SerializeImageFormBytes(bytes, alitsName);
            }
            catch (System.Exception e)
            {
                Debugger.UF_Exception(e);
            }
            return ret;
        }

        public int UF_LoadTextureFromCacheOrDownload(string url,DelegateTexture methodCallback){
			//解析出名字
			string fileName = Path.GetFileNameWithoutExtension (url);
			string localfile = GlobalPath.TexturePath + fileName;
			//指定的url图片名字转化为本地图片名字
			bool isWebToLocal = false;

			if (string.IsNullOrEmpty (fileName))
				return 0;

			if (methodCallback == null) {
				Debug.LogWarning ("LoadTextureFromCacheOrDownload with Not Callback, Invoke Method Failed");
				return 0;
			}

			if (m_DicMapWebTextureToLocal.ContainsKey (fileName)) {
				fileName = m_DicMapWebTextureToLocal [fileName];
				isWebToLocal = true;
			}

            var tex = RefObjectManager.UF_GetInstance().UF_LoadRefObject<Texture2D>(fileName, false) as Texture2D;

            if (tex != null) {
				if(methodCallback != null)
					methodCallback (tex);
				return 0;
			}

			if (isWebToLocal) {
				Texture2D image = UF_LoadTexture(fileName);
				if (image == null) {
					image = UF_LoadTextureLocal(GlobalPath.TexturePath + fileName);
				}
				if(methodCallback != null)
					methodCallback (image);
				return 0;
			}

			//检查本地是否存在缓存图片 
			if (File.Exists (localfile)) {
				//				Debug.Log ("load form Cache:" + localfile);
				Texture2D t2d = UF_LoadTextureLocal(localfile);
				if(methodCallback != null)
					methodCallback (t2d);
				return 0;
			}
			else{
				//				Debug.Log ("load form Web:" + url);
				if (!m_DicWebTextureRequeset.ContainsKey (fileName)) {
					m_DicWebTextureRequeset.Add (fileName, methodCallback);
				} else {
					m_DicWebTextureRequeset[fileName] += methodCallback;
				}
				return FrameHandle.UF_AddCoroutine (UF_ILoadTextureFormWeb(url));
			}

		}

		private void UF_InvokeWebTextureRequestCallback(string fileName,Texture2D texture){
			if (m_DicWebTextureRequeset.ContainsKey (fileName)) {
				m_DicWebTextureRequeset [fileName] (texture);
				m_DicWebTextureRequeset.Remove (fileName);
			}
		}

		IEnumerator UF_ILoadTextureFormWeb(string url){
			Debugger.UF_Log (string.Format("Download Texture: {0}",url));
			WWW www = new WWW (url);
			yield return www;

			string fileName = Path.GetFileNameWithoutExtension (url);
			string localfile = GlobalPath.TexturePath + fileName;
			byte[] temp = null;
			Texture2D t2d = null;

			if (!string.IsNullOrEmpty (www.error)) {
				Debugger.UF_Error ("ILoadTextureFormWeb Error:" + www.error);
                UF_InvokeWebTextureRequestCallback(fileName,null);
				yield break;
			}

			temp = www.bytes;

			try{
				if (File.Exists (localfile)) {
					File.Delete(localfile);
				}
				FileStream fs = new FileStream (localfile,FileMode.CreateNew);
				fs.Write (temp, 0, (int)temp.Length);
				fs.Close ();
				t2d = UF_SerializeImageFormBytes(temp,fileName);
			}
			catch(System.Exception e){
				Debugger.UF_Exception (e);
			}

			www.Dispose ();

            UF_InvokeWebTextureRequestCallback(fileName,t2d);

		}


		public void UF_OnStart(){
			if (!System.IO.Directory.Exists (GlobalPath.TexturePath)) {
				System.IO.Directory.CreateDirectory (GlobalPath.TexturePath);
			}
		}

	}

}
