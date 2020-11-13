//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityFrame{
	/// <summary>
	/// 特效管理器，使用对象池去管理该单例生成的FXController
	/// </summary>
	public class FXManager : HandleSingleton<FXManager> {

		private bool m_IsPause = false;

		private GameObject m_FXRoot;

		public bool IsPause{
			get{ 
				return m_IsPause;
			}
			set{ 
				if (m_IsPause != value) {
					m_IsPause = value;
					if (value) {
                        UF_AllPause();
					} else {
                        UF_AllContinue();
					}
				}
			}
		}

		private void UF_AddToFXRoot(FXController fx){
			if (fx != null) {
				if (m_FXRoot == null) {
					m_FXRoot = new GameObject ("FX_Root");
					m_FXRoot.transform.position = Vector3.zero;
				}	
				fx.UF_SetParent(m_FXRoot.transform);
			}
		}

		private bool UF_CheckInvalidName(string name){
			return string.IsNullOrEmpty (name) || name == "efx_null" || name == "null" || name == "0";
		}

		//预加载一个特效资源，加载完成马上销毁
		public void UF_PreCreate(string name){
			if (UF_CheckInvalidName(name)) {return;}
			FXController fx = this.UF_Create(name);
			if (fx != null) {
				fx.Release ();
			}
		}

		public FXController UF_Create(string name){
			if (UF_CheckInvalidName(name)) {return null;}
			FXController fx = CEntitySystem.UF_GetInstance ().UF_Create<FXController> (name);
            if (fx != null)
            {
                UF_AddToFXRoot(fx);
                fx.UF_SetActive(true);
                //fx.FollowTo (null);	
            }
            else {
                Debugger.UF_Warn(string.Format("Can not create fx:{0}", name));
            }
			return fx;
		}


		public FXController UF_Create(string name,float size){
			FXController fx = this.UF_Create(name);
			if (fx != null) {
				fx.size = size;
			}
			return fx;
		}

		public FXController UF_Create(string name,float size,Vector3 pos,Vector3 euler){
			FXController fx = this.UF_Create(name);
			if (fx != null) {
				fx.size = size;
				fx.position = pos;
				fx.euler = euler;
			}
			return fx;
		}


		private void UF_GetAllActiveFXController(List<FXController> list){
			CEntitySystem.UF_GetInstance ().UF_Select(
				(e)=>{
					if(e is FXController){
						list.Add(e as FXController);
					}
					return false;
				}
			);
		}

		//暂停全部
		public void UF_AllPause(){
			List<FXController> list = ListCache<FXController>.Acquire ();
            UF_GetAllActiveFXController(list);
			foreach (FXController val in list) {val.UF_Pause();}
			ListCache<FXController>.Release (list);
		}

		//继续全部
		public void UF_AllContinue(){
			List<FXController> list = ListCache<FXController>.Acquire ();
            UF_GetAllActiveFXController(list);
			foreach (FXController val in list) {val.UF_Continue();}
			ListCache<FXController>.Release (list);
		}

		//停止全部
		public void UF_AllStop(){
			List<FXController> list = ListCache<FXController>.Acquire ();
            UF_GetAllActiveFXController(list);
			foreach (FXController val in list) {val.UF_Stop();}
			ListCache<FXController>.Release (list);
		}


		public void UF_Play(string name,Vector3 pos){
            UF_Play(name, 1.0f, pos, Vector3.zero);
		}

		public void UF_Play(string name,float size,Vector3 pos){
            UF_Play(name,size, pos, Vector3.zero);
		}

		public void UF_Play(string name,float size,Vector3 pos,Vector3 euler){
            UF_Play(name, size, pos, euler, 0);
        }

        public void UF_Play(string name, float size, Vector3 pos, Vector3 euler,int order)
        {
            if (string.IsNullOrEmpty(name))
                return;
            FXController fx = this.UF_Create(name, size, pos, euler);
            if (fx != null)
            {
                fx.sortingOrder = order;
                fx.UF_Play();
                if (m_IsPause)
                    fx.UF_Pause();
            }
        }

        //提供特殊接口，为Chain类型特效设置值
        public void UF_PlayChain(string name,string chainName,Vector3 form,Vector3 to,float size){
			FXController fx = this.UF_Create(name, size);
			if (fx != null) {
				EChainLine chain = fx.UF_FindEffect(chainName) as EChainLine;
				if (chain != null) {
					chain.SetChain (form, to);
				}
				fx.UF_Play(form, Vector3.zero);
				if (m_IsPause)
					fx.UF_Pause();
			}
		}


		public override string ToString ()
		{
			System.Text.StringBuilder sb = StrBuilderCache.Acquire ();
			List<FXController> list = ListCache<FXController>.Acquire ();
            UF_GetAllActiveFXController(list);
			sb.AppendLine(string.Format ("FX  Count:{0} \n",list.Count));
			foreach (FXController val in list) {
				if (val != null) {
					sb.AppendLine (string.Format("\t {0} | life: {1} | left: {2} ",val.name,val.life,val.leftLife));
				}
			}
			ListCache<FXController>.Release (list);
			return StrBuilderCache.GetStringAndRelease(sb);
		}


	}

}