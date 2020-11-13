//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace UnityFrame
{
    //控制物体是否激活
    public class EObjectActive : EffectBase
    {
        [SerializeField] private List<GameObject> m_Objects = new List<GameObject>();

        public List<GameObject> objects { get { return m_Objects; } }

        protected override void UF_OnPlay()
        {
            base.UF_OnPlay();
            for (int k = 0; k < m_Objects.Count; k++) {
                if (m_Objects[k] != null) {
                    m_Objects[k].SetActive(false);
                }
            }
        }


        protected override void UF_OnRun(float progress)
        {
            base.UF_OnRun(progress);
            for (int k = 0; k < m_Objects.Count; k++)
            {
                if (m_Objects[k] != null)
                {
                    m_Objects[k].SetActive(true);
                }
            }
        }

    }
}
