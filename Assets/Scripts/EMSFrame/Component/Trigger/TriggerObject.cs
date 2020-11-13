//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UnityFrame {
    //用于处理触发碰撞物体事件
    [RequireComponent(typeof(Collider))]
    public class TriggerObject : EntityObject,IOnAwake,IOnSyncUpdate,IOnReset
    {
        public string eTrigger;
        public string[] eParams = { "" };
        public string eSound = "";
        public string triggerMask = "Player";
        public bool autoRelese;
        [SerializeField] private UnityEvent m_UETrigger;


        //附加绑定目标
        private GameObject m_AttachTarget;

        public string UF_GetParam(int index)
        {
            if (eParams == null || eParams.Length == 0)
            {
                return "";
            }
            else
            {
                index = Mathf.Clamp(index, 0, eParams.Length - 1);
                return eParams[index];
            }
        }

        public void UF_SetEParam(int index, string param)
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

        public void UF_SetEParam(string param)
        {
            UF_SetEParam(0, param);
        }

        public void UF_SetAttachTarget(GameObject go) {
            m_AttachTarget = go;
        }


        public void UF_OnAwake() {
            this.gameObject.layer = DefineLayer.IgoreRaycast;
        }

        public virtual void UF_OnSyncUpdate()
        {
            if (m_AttachTarget != null)
            {
                this.transform.position = m_AttachTarget.transform.position;
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == DefineLayer.HitRaycast)
                return;
            if (string.IsNullOrEmpty(eTrigger))
                return;
            if (!string.IsNullOrEmpty(triggerMask)) {
                if (!GHelper.UF_CheckStringMask(triggerMask, other.tag))
                    return;
            }
            if (m_AttachTarget != null && m_AttachTarget == other.gameObject)
                return;

                //send message
            MessageSystem msg = MessageSystem.UF_GetInstance();
            msg.UF_BeginSend();
            msg.UF_PushParam(eTrigger);
            msg.UF_PushParam(this.gameObject);
            msg.UF_PushParam(other.gameObject);
            for (int k = 0; k < eParams.Length; k++)
            {
                msg.UF_PushParam(eParams[k]);
            }
            msg.UF_EndSend(DefineEvent.E_TRIGGER_CONTROLLER);

            if (m_UETrigger != null) {
                m_UETrigger.Invoke();
            }

            //paly sound
            AudioManager.UF_GetInstance().UF_Play(eSound);
            //release?
            if (autoRelese)
                this.Release();
        }

        public virtual void UF_OnReset() {
            m_AttachTarget = null;
        }

    }


}
