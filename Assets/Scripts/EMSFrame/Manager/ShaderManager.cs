//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityFrame{
	public class ShaderManager : HandleSingleton<ShaderManager>,IOnStart,IOnReset
	{
		private Dictionary<string,Material> m_DicShareMaterial = new Dictionary<string, Material> ();

		//记录已经加载编译的Shader
		private Dictionary<string,Shader> m_DicShareShader = new Dictionary<string, Shader> ();

		private Dictionary<string,Dictionary<string,string>> m_DicMapShaderType = new Dictionary<string, Dictionary<string, string>> ();

		//Shader资源包
		private AssetBundleData m_ShaderPackage;

		public void UF_OnStart ()
		{
#if !UNITY_EDITOR
			//if(m_ShaderPackage == null){
   //             string pakname = "shader_package";
			//	m_ShaderPackage = AssetSystem.UF_GetInstance ().UF_LoadAssetBundleData (pakname , LoadAssetBundleOptions.DO_NOT_UNLOAD);
   //             //强制引用
   //             AssetSystem.UF_GetInstance ().UF_RetainRef(pakname);
			//}
#endif
        }

        public bool UF_PreLoad(string shaderName){
			return UF_Find(shaderName) != null;
		}

		public Shader UF_Find(string shaderName){
			Shader value = null;
			if (m_DicShareShader.ContainsKey (shaderName)) {
				return m_DicShareShader [shaderName];
			}

			if (m_ShaderPackage != null) {
				string fileName = shaderName.Substring (shaderName.LastIndexOf ('/') + 1);
				value = m_ShaderPackage.UF_LoadAsset<Shader> (fileName);
			}
			if (value == null) {
				value = Shader.Find (shaderName);
			}

			if (value != null) {
				m_DicShareShader.Add (shaderName, value);
			}
			return value;
		}

		public Shader UF_FindWithType(string shaderName,string typeName){
			if (m_DicMapShaderType.ContainsKey (shaderName)) {
				if (m_DicMapShaderType [shaderName].ContainsKey (typeName)) {
					return this.UF_Find(m_DicMapShaderType [shaderName][typeName]);
				}
			}
			return null;
		}


		public void UF_ResetShader(GameObject go){
			if(go == null) return;
			Renderer[] renders = go.GetComponentsInChildren<Renderer>();
			if(renders == null) return;
			for(int k = 0;k < renders.Length;k++){
				Material[] materials = renders[k].sharedMaterials;
				if(materials != null){
					for(int z = 0;z < materials.Length;z++){
						if(materials[z] != null){
							materials[z].shader = UF_Find(materials[z].shader.name); 
						}
					}
				}
			}
		}

		public void UF_SetShaderLOD(string shaderName,int Lod){
			Shader shader = UF_Find(shaderName);
			if (shader != null) {
				shader.maximumLOD = Lod;
			}
		}

		public bool UF_CheckShader(string shaderName){
			return UF_Find(shaderName) != null;
		}

		public Shader UF_GetShader(string shaderName){
			return UF_Find(shaderName);
		}

		public Material UF_GetShareMaterial(string shaderName){
			if (m_DicShareMaterial.ContainsKey (shaderName)) {
				return 	m_DicShareMaterial [shaderName];
			} else {
				Shader shader = UF_Find(shaderName);
				if (shader != null) {
					Material mat = new Material (shader);
					m_DicShareMaterial.Add (shaderName, mat);
					return mat;
				} else {
					Debugger.UF_Error (string.Format("GetShareMaterial Failed, Shader[{0}] is null",shaderName));
				}
			}
			return null;
		}

		public Material UF_GetMaterial(string shaderName){
			Shader shader = UF_Find(shaderName);
			if (shader != null) {
				Material mat = new Material (shader);
				return mat;
			} else {
				Debugger.UF_Error (string.Format("GetShareMaterial Failed, Shader[{0}] is null",shaderName));
			}
			return null;
		}

		public Material UF_GetRayMaterial(bool share = true){
			if (share) {
				return UF_GetShareMaterial("Game/Transparent/RayTransparent");
			} else {
				return UF_GetMaterial("Game/Transparent/RayTransparent");
			}
		}

        public Material UF_GetUIZWMaterial(bool share = true)
        {
            if (share)
            {
                return UF_GetShareMaterial("Game/GUI/UIColorZW");
            }
            else
            {
                return UF_GetMaterial("Game/GUI/UIColorZW");
            }
        }

        public Material UF_GetUIModelMaterial(bool share = true){
			if (share) {
				return UF_GetShareMaterial("Game/GUI/UIOpauqueMask");
			} else {
				return UF_GetMaterial("Game/GUI/UIOpauqueMask");
			}
		}

		public Material UF_GetUIGreyMaterial(bool share = true){
			if (share) {
				return UF_GetShareMaterial("Game/GUI/UIDefaultGrey");
			} else {
				return UF_GetMaterial("Game/GUI/UIDefaultGrey");
			}
		}

		public Material UF_GetUIGreyMaterialText(bool share = true){
			if (share) {
				return UF_GetShareMaterial("Game/GUI/UIDefaultGreyText");
			} else{
				return UF_GetMaterial("Game/GUI/UIDefaultGreyText");
			}
		}

        public Material UF_GetUIPreviewMaterial(bool share = true) {
            if (share)
            {
                return UF_GetShareMaterial("Game/GUI/UIPreview");
            }
            else
            {
                return UF_GetMaterial("Game/GUI/UIPreview");
            }
        }

        public void UF_OnReset()
        {
            m_DicShareMaterial.Clear();
            m_DicShareShader.Clear();
            m_DicMapShaderType.Clear();
            if (m_ShaderPackage != null)
            {
                m_ShaderPackage.UF_Dispose(true);
            }
            m_ShaderPackage = null;
        }



        public string UF_GetShadersInfo(){
			string ret = "Shader Info: \n";
			foreach (Shader value in m_DicShareShader.Values) {
				ret += string.Format("\t{0} | {1} | {2}\n",value.name,value.isSupported,value.maximumLOD);
			}
			return ret;
		}

	}


}
