//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFrame {
    public class TriggerSprite : TriggerObject,IOnAwake
    {
        public SpriteRenderer render;

        public int sortingOrder
        {
            get {
                if (render)
                    return render.sortingOrder;
                else
                    return 0;
            }
            set {
                if (render) {
                    render.sortingOrder = value;
                }
            }
        }

        public void UF_OnAwake() {
            if (render == null) {
                render = this.gameObject.GetComponentInChildren<SpriteRenderer>();
            }
        }

        public void UF_SetSprite(string spName) {
            if (render == null) return;
            render.sprite = UIManager.UF_GetInstance().UF_GetSprite(spName);

        }

    }

}
