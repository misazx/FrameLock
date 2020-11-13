//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace UnityFrame
{
    public class EObjectTrigger : EffectBase
    {
        public string eTrigger;
        public string[] eParams = { "" };
        
        public bool useFixedTime;
        //持续触发到间隔
        public float interval;
        public string eSound = "";
        public string triggerMask = "Player";

        private bool m_IsIntervalRun = false;

        [SerializeField] private UnityEvent m_UETrigger;

        [SerializeField] private UnityEvent m_UEOnPlay;

        Dictionary<GameObject, float> m_MapTriggerCache = new Dictionary<GameObject, float>();


        protected float time { get {
                return useFixedTime ? GTime.Time : GTime.FixedTime;
            } }

        public string GetEParam(int index)
        {
            if (eParams == null || eParams.Length == 0){return "";}
            else{return eParams[0];}
        }

        public void SetEParam(int index, string param)
        {
            if (eParams == null || eParams.Length == 0)
            {
                eParams = new string[] { param };
            }
            if (index >= eParams.Length)
            {
                string[] tmp = new string[index + 1];
                System.Array.Copy(eParams, tmp, eParams.Length);
                tmp[index] = param;
                eParams = tmp;
            }
            else
            {
                eParams[index] = param;
            }
        }

        public void SetEParam(string param)
        {
            SetEParam(0, param);
        }


        private void TriggerGameObject(GameObject go)
        {
            if (go ==null || !go.activeInHierarchy) return;
            //send message
            if (!string.IsNullOrEmpty(eTrigger)) {
                MessageSystem msg = MessageSystem.UF_GetInstance();
                msg.UF_BeginSend();
                msg.UF_PushParam(eTrigger);
                msg.UF_PushParam(this.gameObject);
                msg.UF_PushParam(go);
                for (int k = 0; k < eParams.Length; k++)
                {
                    msg.UF_PushParam(eParams[k]);
                }
                msg.UF_EndSend(DefineEvent.E_TRIGGER_CONTROLLER);
            }

            if (m_UETrigger != null) {
                m_UETrigger.Invoke();
            }

            //paly sound
            if (!string.IsNullOrEmpty(eSound)) {
                AudioManager.UF_GetInstance().UF_Play(eSound);
            }
        }

        protected void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == DefineLayer.HitRaycast)
                return;

            if (!string.IsNullOrEmpty(triggerMask))
            {
                if (!GHelper.UF_CheckStringMask(triggerMask, other.gameObject.tag))
                    return;
            }

            if (m_MapTriggerCache.ContainsKey(other.gameObject))
            {
                float fixedTime = time;
                //判定是否在连续触发事件内
                float dt = fixedTime - m_MapTriggerCache[other.gameObject];
                if (dt >= interval)
                {
                    m_MapTriggerCache[other.gameObject] = fixedTime;
                    TriggerGameObject(other.gameObject);
                }
            }
            else
            {
                m_MapTriggerCache.Add(other.gameObject, time);
                if (interval <= 0 || m_IsOnRun) {
                    TriggerGameObject(other.gameObject);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (m_MapTriggerCache.ContainsKey(other.gameObject)) {
                m_MapTriggerCache.Remove(other.gameObject);
            }
        }

        private void OnDisable()
        {
            m_MapTriggerCache.Clear();
        }

        protected override void UF_UF_OnReset()
        {
            base.UF_UF_OnReset();
            m_MapTriggerCache.Clear();
        }

        protected override void UF_OnPlay()
        {
            base.UF_OnPlay();
            if (m_UEOnPlay != null)
            {
                m_UEOnPlay.Invoke();
            }
        }
    

        protected override void UF_OnRun(float progress)
        {
            if (m_MapTriggerCache.Count > 0 && interval > 0)
            {
                float t = time;
                List<GameObject> tempList = ListCache<GameObject>.Acquire();
                foreach (var key in m_MapTriggerCache.Keys) {
                    tempList.Add(key);
                }
                for (int k = 0; k < tempList.Count; k++) {
                    var key = tempList[k];
                    if (key == null) {
                         continue;
                    }
                    //if (key.activeSelf == false) {
                    //    m_MapTriggerCache.Remove(key);
                    //    continue;
                    //}
                    float dt = time - m_MapTriggerCache[key];
                    if (dt >= interval)
                    {
                        m_MapTriggerCache[key] = time;
                        TriggerGameObject(key);
                    }
                }
                ListCache<GameObject>.Release(tempList);
            }
        }


    }
}


