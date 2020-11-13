//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;

namespace UnityFrame {
    public class ETweenBase : EffectBase
    {
        public bool isSelfCtrl = true;
        public bool isFixedRun = false;
        //Active时播放
        public bool playOnActive;
        //影响同层级
        public bool playSynctSibling = false;

        protected override void UF_OnPlay()
        {
            base.UF_OnPlay();
            //影响同层级
            if (playSynctSibling)
            {
                var uitweens = this.GetComponents<ETweenBase>();
                if (uitweens != null)
                {
                    foreach (var tween in uitweens)
                    {
                        if (tween != this && !tween.playSynctSibling)
                        {
                            tween.Play();
                        }
                    }
                }
            }
        }
        void Start()
        {
            if(isSelfCtrl)
                this.Init();
        }

        void Update()
        {
            if (isSelfCtrl && !isFixedRun)
                this.Run(GTime.DeltaTime, GTime.UnscaleDeltaTime);
        }

        private void FixedUpdate()
        {
            if (isSelfCtrl && isFixedRun)
                this.Run(GTime.DeltaTime, GTime.UnscaleDeltaTime);
        }

        void OnEnable()
        {
            if (isSelfCtrl)
                if (playOnActive) { this.Play(); }
        }



    }


}
