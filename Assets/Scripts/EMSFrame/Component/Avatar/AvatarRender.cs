//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace UnityFrame{
	internal struct ReMaterialData {
		public int reQueue;
		public Color reColor;
		public Shader reShader;
	}

	[System.Serializable]
	public class AvatarRender
	{

		private Dictionary<Material, ReMaterialData> m_ListMats = new Dictionary<Material, ReMaterialData>();

		private AvatarController m_Avatar;

		private bool m_IsChanged = false;

        private GhostImageRender m_GhostImage;

        private SkinnedMeshRenderer m_BodySkinnedMesh;

        protected GhostImageRender ghostImage {
            get {
                if (m_GhostImage == null) {
                    m_GhostImage = new GhostImageRender();
                }
                return m_GhostImage;
            }
        }



        public void UF_OnAwake (AvatarController entity){
			m_Avatar = entity;
			if (entity == null) {
				Debugger.UF_Error ("AvatarRender need AvatarController Component Instance");
				return;
			}
            m_ListMats.Clear();
            Renderer[] renders = entity.GetComponentsInChildren<Renderer> ();
			foreach (Renderer render in renders) {
                ////忽略粒子效果
                //if (render is ParticleSystemRenderer)
                //    continue;
                if (render is SkinnedMeshRenderer) {
                    if (m_BodySkinnedMesh == null) {
                        m_BodySkinnedMesh = render as SkinnedMeshRenderer;
                    }
                }
                if (render is SkinnedMeshRenderer || render is MeshRenderer)
                {
                    Material[] materials = render.materials;
                    for (int k = 0; k < materials.Length; k++)
                    {
                        if (materials[k] != null)
                        {
                            var rdata = new ReMaterialData();
                            rdata.reQueue = materials[k].renderQueue;
                            rdata.reColor = materials[k].color;
                            rdata.reShader = materials[k].shader;
                            m_ListMats.Add(materials[k], rdata);
                        }
                    }
                }
			}
		}
		private void UF_SetMaterialTexture(Material mat,string key,Texture texture){
			try{
				mat.SetTexture(key,texture);
			}
			catch(System.Exception e){
				Debugger.UF_Exception(e);
			}
		}


		public void UF_SetTexture(string textureName){
			Texture2D texture = TextureManager.UF_GetInstance().UF_LoadTexture(textureName);
			if (texture != null) {
				foreach (Material mat in m_ListMats.Keys) {
					if (mat != null) {
                        UF_SetMaterialTexture(mat, "_MainTex", texture);	
					}
				}
			}
		}


		public void UF_SetShaderType(string typeName){
			if (m_ListMats == null) {return;}
			m_IsChanged = true;
            foreach (Material mat in m_ListMats.Keys)
            {
                if (mat.shader != null)
                {
                    mat.shader = ShaderManager.UF_GetInstance().UF_FindWithType(mat.shader.name, typeName);
                }
            }
        }

        public void UF_SetShader(string shaderName)
        {
            if (m_ListMats == null) { return; }
            m_IsChanged = true;
            foreach (Material mat in m_ListMats.Keys)
            {
                if (mat.shader != null)
                {
                    mat.shader = ShaderManager.UF_GetInstance().UF_Find(shaderName);
                }
            }
        }

        public void UF_SetQueue(int value){
			if (m_ListMats == null) {return;}
			m_IsChanged = true;
            foreach (Material mat in m_ListMats.Keys)
            {
                mat.renderQueue = value;
			}
		}

		public void UF_SetColor(string name,Color color){
			if (m_ListMats == null) {return;}
			m_IsChanged = true;
            foreach (Material mat in m_ListMats.Keys)
            {
                mat.SetColor (name, color);
			}
		}

		public void UF_SetValue(string name,float value){
			if (m_ListMats == null) {return;}
            foreach (Material mat in m_ListMats.Keys)
            {
				mat.SetFloat (name,value);
			}
		}

		public void UF_SetVector(string name,Vector3 vector){
			if (m_ListMats == null) {return;}
            foreach (Material mat in m_ListMats.Keys)
            {
				mat.SetVector (name,vector);
			}
		}


        public void UF_SetGhostImageAffter(float duration,float pval,Color color) {
            ghostImage.UF_SetColor(color);
            ghostImage.UF_Show(duration, pval);
        }

        public void UF_StopGhostImageAffter() {
            if (m_GhostImage != null)
            {
                m_GhostImage.UF_Close();
            }
        }


        public void UF_CreateChostImage(float life,Color color)
        {
            if (m_Avatar == null) return;
            UF_CreateChostImage(life, m_Avatar.position, color);
        }

        public void UF_CreateChostImage(float life,Vector3 pos,Color color) {
            if (m_Avatar == null) return;
            ghostImage.UF_Add(m_BodySkinnedMesh, pos, m_Avatar.transModel.rotation, life, color);
        }


		public void UF_RevertShader(){
			if (m_ListMats == null) {return;}
            foreach (KeyValuePair<Material, ReMaterialData> item in m_ListMats)
            {
                item.Key.shader = item.Value.reShader;
			}
		}

		public void UF_Revert(){
			if (m_ListMats == null) {return;}
			if (m_IsChanged == false) {return;}
            foreach (KeyValuePair<Material, ReMaterialData> item in m_ListMats)
            {
                item.Key.shader = item.Value.reShader;
                item.Key.renderQueue = item.Value.reQueue;
                item.Key.color = item.Value.reColor;
			}
		}

        public void UF_OnUpdate() {
            if (m_Avatar == null) { return;}
            if (m_GhostImage != null)
            {
                m_GhostImage.UF_OnUpdate(m_BodySkinnedMesh,m_Avatar.position, m_Avatar.transModel.rotation);
            }
        }

		public void UF_OnReset(){
            UF_Revert();
			m_IsChanged = false;
            if (m_GhostImage != null)
            {
                m_GhostImage.UF_OnReset();
            }
        }
			
		public int UF_BounceColor(Color target,Color source,float duration,string colorName){
			if (m_Avatar != null && m_Avatar.isActive) {
				return FrameHandle.UF_AddCoroutine(UF_IBounceColor(target,source, duration, colorName));
			}
			return 0;
		}

		System.Collections.IEnumerator UF_IBounceColor(Color target,Color source,float duration,string colorName){
			float buffer = 0;
			do {
				buffer += GTime.RunDeltaTime;
				float rate = Mathf.Clamp01(buffer / duration);
                float t = Mathf.Sin(Mathf.Deg2Rad * 180 * rate);

                UF_SetColor(colorName,Color.Lerp(source,target,t));
				yield return null;
			} while(buffer < duration);
			UF_SetColor (colorName,source);
		}


		public int UF_BounceValue(float target,float source,float duration,string valueName){
			if (m_Avatar != null && m_Avatar.isActive) {
				return FrameHandle.UF_AddCoroutine(UF_IBounceValue(target,source, duration, valueName));
			}
			return 0;
		}

		System.Collections.IEnumerator UF_IBounceValue(float target,float source,float duration,string valueName){
			float buffer = 0;
			do {
				buffer += GTime.RunDeltaTime;
				float rate = Mathf.Clamp01(buffer / duration);
				UF_SetValue(valueName,Mathf.Lerp(source,target,Mathf.Cos(Mathf.Deg2Rad * 180 * rate)));
				yield return null;
			} while(buffer < duration);
			UF_SetValue (valueName,source);
		}


		/// <summary>
		/// 设置明暗
		/// </summary>
		public void UF_SetDarken(bool value){
			if (value) {
				UF_SetColor ( "_Color",new Color (0.4f, 0.4f, 0.4f, 1));
			} else {
				UF_SetColor ("_Color",Color.white);
			}
		}

		/// <summary>
		/// 设置灰度
		/// </summary>
		public void UF_UF_SetGrey(bool value){
			if (value) {
                UF_SetShaderType("Grey");
			} else {
                UF_RevertShader();
			}
		}


		/// <summary>
		/// 设置半透明
		/// </summary>
		public void UF_SetTransparent(bool value){
			if (value) {
                UF_SetShaderType("Transparent");
			} else {
                UF_RevertShader();
			}
		}


	}
}

