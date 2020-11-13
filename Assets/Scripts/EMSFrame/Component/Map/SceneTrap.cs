//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFrame
{
    public class SceneTrap : SceneElement,IOnStart,IOnAwake,IOnSyncUpdate
    {
        public bool isPlayOnStart = true;
        private bool m_IsPlay;
        //效果用于做指定动画
        private List<EffectBase> m_Effects = new List<EffectBase>();

        public void UF_OnStart() {
            if (isPlayOnStart)
                this.UF_Play();
            else
                this.UF_Stop();
        }

        public void UF_Play() {
            m_IsPlay = true;
            EffectControl.UF_Play(m_Effects);
        }

        public void UF_Stop() {
            m_IsPlay = false;
            EffectControl.UF_Stop(m_Effects);
        }

        public void UF_OnAwake() {
            this.GetComponentsInChildren<EffectBase>(false, m_Effects);
        }


        public void UF_OnSyncUpdate() {
            if (m_IsPlay) {
                EffectControl.UF_Run(m_Effects, GTime.RunDeltaTime, GTime.RunDeltaTime);
            }
        }


    }

}
