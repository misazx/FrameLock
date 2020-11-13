//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------
using UnityEngine;
using System.Collections.Generic;

namespace UnityFrame
{
	public static class EffectControl
	{

		public static EffectBase UF_FindEffect(List<EffectBase>  effects,string effectName){
			if (!string.IsNullOrEmpty (effectName) && effects != null) {
				for (int k = 0; k < effects.Count; k++) {
					if (effects [k] != null && effects [k].effectName == effectName) {
						return effects [k];
					}
				}
			}
			return null;
		}

        public static void UF_Init(List<EffectBase> effects)
        {
            if (effects == null || effects.Count == 0) { return; }
            for (int k = 0; k < effects.Count; k++)
            {
                if (effects[k] != null)
                {
                    effects[k].Init();
                }
            }
        }


        public static void UF_Play(List<EffectBase>  effects){
			if (effects == null || effects.Count == 0){return;}
			for (int k = 0; k < effects.Count; k++) {
				if (effects [k] != null) {
                    effects [k].Play ();
				}
			}
		}

		public static void UF_Reverse(List<EffectBase>  effects){
			if (effects == null || effects.Count == 0){return;}
			for (int k = 0; k < effects.Count; k++) {   
				if (effects [k] != null) {
					effects [k].Play ();
				}
			}
		}


		public static void UF_Run(List<EffectBase>  effects,float deltaTime,float unscaleDeltaTime){
			if (effects == null || effects.Count == 0){return;}
			for (int k = 0; k < effects.Count; k++) {
				if (effects [k] != null) {
					effects [k].Run (deltaTime, unscaleDeltaTime);
				}
			}
		}
			
		public static void UF_Reset(List<EffectBase>  effects){
			if (effects == null || effects.Count == 0){return;}
			for (int k = 0; k < effects.Count; k++) {
				if (effects [k] != null) {
					effects [k].Reset ();
				}
			}
		}

		public static void UF_Stop(List<EffectBase>  effects)
		{
			if (effects == null || effects.Count == 0){return;}
			for (int k = 0; k < effects.Count; k++) {
				if (effects [k] != null) {
					effects [k].Stop ();
				}
			}
		}

		public static void UF_Pause(List<EffectBase>  effects){
			if (effects == null || effects.Count == 0){return;}
			for (int k = 0; k < effects.Count; k++) {
				if (effects [k] != null) {
					effects [k].Pause ();
				}
			}
		}

		public static void UF_Continue(List<EffectBase>  effects){
			if (effects == null || effects.Count == 0){return;}
			for (int k = 0; k < effects.Count; k++) {
				if (effects [k] != null) {
					effects [k].Continue ();
				}
			}
		}

        public static void UF_ResetTailRender(UnityEngine.GameObject go) {
            List<TrailRenderer> tempList = ListCache<TrailRenderer>.Acquire();
            go.GetComponentsInChildren<TrailRenderer>(false, tempList);
            foreach (var tr in tempList)
            {
                tr.Clear();
            }
            ListCache<TrailRenderer>.Release(tempList);
        }
			


	}
}

