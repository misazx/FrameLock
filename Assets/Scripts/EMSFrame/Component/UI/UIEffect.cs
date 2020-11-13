//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace UnityFrame
{
    //UI 特效组件
    //控制特效创建，播放，以及特效显示层次问题
    public class UIEffect : UISortingObject,IUIUpdate,IOnReset
    {
        public override void UF_SetValue(object value) {
            if (value is bool)
            {
                bool val = (bool)value;
                if (val)
                {
                    Play();
                }
                else
                {
                    Stop();
                }
            }
            else if (value is string) {
                string fxname = value as string;
                if (!string.IsNullOrEmpty(fxname)) {
                    Play(fxname);
                }
            }
        }

        public void Play() {
            List<ParticleSystem> list = ListCache<ParticleSystem>.Acquire();
            this.GetComponentsInChildren<ParticleSystem>(false, list);
            foreach(var item in list)
            {
                item.Play(false);
            }
            ListCache<ParticleSystem>.Release(list);
            this.UF_SetDirty();
        }

        public void Play(string fxname) {
            var fx = FXManager.UF_GetInstance().UF_Create(fxname);
            if (fx != null) {
                fx.UF_SetParent(this.transform);
                fx.transform.localScale = Vector3.one;
                fx.transform.localPosition= Vector3.zero;
                fx.UF_Play();
                this.UF_SetDirty();
            }
        }

        public void Stop() {
            List<ParticleSystem> list = ListCache<ParticleSystem>.Acquire();
            this.GetComponentsInChildren<ParticleSystem>(false, list);
            foreach (var item in list)
            {
                item.Stop(false);
            }
            ListCache<ParticleSystem>.Release(list);
        }

        //强制清除动态实体
        public void Clear() {
            List<FXController> list = ListCache<FXController>.Acquire();
            this.GetComponentsInChildren<FXController>(false, list);
            foreach (var item in list)
            {
                item.Release();
            }
            ListCache<FXController>.Release(list);

        }


        public void UF_OnReset() {
            this.Clear();
        }



        protected override void OnApplySortingOrder() {
            //获取正确的渲染序号
            int absOrder = rootSortingOrder + sortingOrder;

            List<Renderer> list = ListCache<Renderer>.Acquire();
            this.gameObject.GetComponentsInChildren<Renderer>(false,list);
            foreach (var render in list) {
                render.sortingOrder = absOrder;
            }
            ListCache<Renderer>.Release(list);
        }


    }


}