//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections;
using UnityEngine;

namespace UnityFrame {
    public class UITextureAutoLoader : MonoBehaviour
    {
        public enum FolderType
        {
            StreamingAssets,
            Persistent,
            AssetsBundle,
        }

        public UITexture target;
        public FolderType folderType;
        public string fileName;
        public bool enableOnAccessable;

        private void Awake()
        {
            if (target == null)
                target = this.GetComponent<UITexture>();
            if (target != null && enableOnAccessable) {
                target.enabled = false;
            }
        }


        void Start()
        {
            if (target == null)
                return;
            if (string.IsNullOrEmpty(fileName))
                return;

            var array = GHelper.UF_SplitString(fileName, ';');
            foreach (var v in array) {
                if (UF_LoadTexture(v)) {
                    Debugger.UF_Log(string.Format("Auto Load Texture[{0}] Success", v));
                    break;
                }
            }
        }

        private bool UF_LoadTexture(string fName) {
            string path = fName;

            Texture2D tex = null;
            if (folderType == FolderType.Persistent)
            {
                path = GlobalPath.PersistentPath + "/" + path;
                tex = TextureManager.UF_GetInstance().UF_LoadTextureLocal(path);
            }
            else if (folderType == FolderType.StreamingAssets)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                var bytes =  GHelper.UF_LoadAndroidAssetFile(path);
                if (bytes != null && bytes.Length > 0) {
                    tex = TextureManager.UF_GetInstance().UF_LoadTextureBytes(bytes, System.IO.Path.GetFileNameWithoutExtension(path));
                }
#else
                path = GlobalPath.StreamingAssetsPath + "/" + path;
                tex = TextureManager.UF_GetInstance().UF_LoadTextureLocal(path);
#endif
            }
            else if (folderType == FolderType.AssetsBundle)
            {
                tex = TextureManager.UF_GetInstance().UF_LoadTexture(fName);
            }

            if (tex != null)
            {
                target.texture = tex;
                if (enableOnAccessable)
                {
                    target.enabled = true;
                }
                return true;
            }
            else
                return false;
        }



    }
}

