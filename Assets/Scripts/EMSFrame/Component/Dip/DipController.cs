//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace UnityFrame {
    //子弹控制器,控制实体子弹移动方式，击中效果等
    [RequireComponent(typeof(Collider))]
    public class DipController : EntityObject, IOnStart, IOnSyncUpdate, IOnReset
    {
        //子弹定义的type ID
        public virtual int type { get { return 0; } }

        //子弹定义的userID
        public string uptr { get; set; }
        //子弹定义的拥有者
        public GameObject ower { get; set; }
        
        //碰撞蒙版
        public string triggerMask = "Ground;Wall;Block;Player;Monster";
        //触发特效
        public string triggerEffect = string.Empty;
        //触发弹道
        public string triggerDip = string.Empty;

        //播放音效
        public string playSound = string.Empty;

        //最大存活时间
        public float maxLifeTime = 10;

        protected float m_LifeTick;

        protected bool m_IsPlaying;

        public bool IsPlaying { get { return m_IsPlaying;}}

        private Dictionary<string, string> m_Parameters;

        private List<GameObject> m_MaskTriggerObjects;

        protected Collider m_Collider;

        //计算法线方向
        private Vector3 m_T_Forward;
        private Vector3 m_T_HitPos;
        private Vector3 m_T_SorObjPos;
        private Vector3 m_T_TarObjPos;

        //记录播放参数
        protected GameObject m_Target;
        protected Vector3 m_TarPos;

        protected Vector3 m_Forward;
        public Vector3 moveForward {
            get {
                return m_Forward;
            }
        }

        //附带的参数
        protected Dictionary<string, string> parameters {
            get {
                if (m_Parameters == null) {
                    m_Parameters = DictionaryPool<string, string>.Get();
                }
                return m_Parameters;
            }
            set
            {
                if (value == null)
                {
                    DictionaryPool<string, string>.Release(m_Parameters);
                }
                m_Parameters = value;
            }
        }

        protected List<GameObject> maskTriggerObejcts
        {
            get
            {
                if (m_MaskTriggerObjects == null) {
                    m_MaskTriggerObjects = ListPool<GameObject>.Get();
                }
                return m_MaskTriggerObjects;
            }
            set
            {
                if (value == null)
                {
                    ListPool<GameObject>.Release(m_MaskTriggerObjects);
                }
                m_MaskTriggerObjects = value;
            }
        }



        public new Collider collider {
            get{
                if (m_Collider == null) {
                    m_Collider = this.GetComponent<Collider>();
                }
                return m_Collider;
            }
        }

        //激活碰撞
        public bool activeTrigger
        {
            get{return collider.enabled;}
            set{collider.enabled = value;}
        }

        //播放
        public void UF_Play(GameObject tar, Vector3 tarPos,Vector3 vecforward) {
            m_IsPlaying = true;
            m_LifeTick = 0;
            m_Target = tar;
            m_TarPos = tarPos;
            vecforward.y = 0;
            m_Forward = vecforward.normalized;
            this.UF_OnPlay(tar, tarPos, vecforward);
            if (!string.IsNullOrEmpty(playSound)) {
                AudioManager.UF_GetInstance().UF_Play(playSound);
            }
        }


        //弹道停止
        public void UF_Stop() {
            m_IsPlaying = false;
            this.UF_OnStop();
            this.Release();
        }

        //添加附带参数
        public void UF_SetParam(string key, string value) {
            if (parameters.ContainsKey(key))
            {
                parameters[key] = value;
            }
            else {
                parameters.Add(key, value);
            }
        }

        public void UF_SetParam(string key, int value) {
            this.UF_SetParam(key, value.ToString());
        }

        public void UF_SetParam(string key,bool value)
        {
            this.UF_SetParam(key, value?"1":"0");
        }

        //获取附带参数
        public string UF_GetParam(string key)
        {
            if (parameters.ContainsKey(key))
            {
                return parameters[key];
            }
            else
            {
                return string.Empty;
            }
        }

        public int UF_GetIntParam(string key) {
            return GHelper.UF_ParseInt(UF_GetParam(key));
        }
        public bool UF_GetBoolParam(string key) {
            return GHelper.UF_ParseBool(UF_GetParam(key));
        }

        public void UF_ClearParam()
        {
            parameters.Clear();
        }

        //添加屏蔽触发对象物体
        public void UF_AddMaskTriggerObject(GameObject tar) {
            maskTriggerObejcts.Add(tar);
        }

        public bool UF_CheckMaskTriggerObject(GameObject tar) {
            if (m_MaskTriggerObjects == null)
                return false;
            if (m_MaskTriggerObjects.Count == 0)
                return false;
            for (int k = 0; k < m_MaskTriggerObjects.Count; k++) {
                if (m_MaskTriggerObjects[k] == tar)
                    return true;
            }
            return false;
        }


        protected void UF_PlayTriggerEffect(Vector3 pos)
        {
            FXManager.UF_GetInstance().UF_Play(this.triggerEffect, pos);
        }


        protected bool UF_PlayTriggerDip()
        {
            if (!m_IsPlaying)
                return false;
            DipController tDip = null;
            if (!string.IsNullOrEmpty(triggerDip))
            {
                tDip = CEntitySystem.UF_GetInstance().UF_Create<DipController>(triggerDip);
            }
            if (tDip != null)
            {
                //碰撞事件由触发弹道处理,触发弹道继承原弹道所有参数属性
                tDip.uptr = this.uptr;
                tDip.ower = this.ower;
                tDip.position = this.position;
                tDip.euler = this.euler;
                tDip.eLayer = this.eLayer;
                //拷贝参数 
                if (m_Parameters != null)
                {
                    foreach (var item in parameters)
                    {
                        tDip.UF_SetParam(item.Key, item.Value);
                    }
                }
                //设置触发弹道父类
                tDip.UF_SetParam("parent", this.name);
                tDip.UF_SetParam("parent_id", this.id.ToString());

                tDip.UF_Play(m_Target, m_TarPos, m_Forward);
                return true;
            }
            return false;
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(m_T_HitPos, m_T_HitPos + m_T_Forward);
            Gizmos.color = Color.black;
            Gizmos.DrawLine(m_T_SorObjPos, m_T_TarObjPos);
            //Gizmos.DrawRay(m_T_SorObjPos, m_T_TarObjPos)
        }


        //获取碰撞方形法线
        protected Vector3 UF_GetNormalFormBlock(Vector3 hitTarPos,Vector3 sorPoint, Vector3 vForward)
        {
            RaycastHit hit;
            int layerMask = 1 << DefineLayer.HitRaycast;
            m_T_SorObjPos = sorPoint;
            m_T_TarObjPos = sorPoint + vForward * 2;
            //通过射线方向计算反射位置
            if (Physics.Raycast(sorPoint, vForward, out hit, 2,layerMask))
            {
                m_T_HitPos = hit.point;
                m_T_Forward = hit.normal;
                return hit.normal;
            }

            var ret = hitTarPos - sorPoint;
            ret.y = 0;
            return ret.normalized;
        }


        protected void UF_UpdateLife(float dtime) {
            m_LifeTick += dtime;
            if (m_LifeTick > maxLifeTime) {
                m_IsPlaying = false;
                this.Release();
            }
        }

        public void UF_OnStart()
        {
            collider.isTrigger = true;
            this.gameObject.layer = DefineLayer.IgoreRaycast;
            this.UF_OnStarted();
        }



        public void UF_OnSyncUpdate() {
            if (!isActive || isReleased || !IsPlaying)
                return;
            float dtime = GTime.RunDeltaTime;
            UF_UpdateLife(dtime);
            UF_OnRun(dtime);
        }


        protected bool UF_CheckIsMemberTag(string val) {
            if (val == DefineTag.Player)
                return true;
            if (val == DefineTag.Monster)
                return true;
            if (val.IndexOf(DefineTag.Player, System.StringComparison.Ordinal) > -1)
                return true;
            else
                return false;
        }


        //unity monobehavior event
        private void OnTriggerEnter(Collider hitObject)
        {
            Vector3 hitPoint = this.transform.position;

            if (!string.IsNullOrEmpty(triggerMask)) {
                if (!GHelper.UF_CheckStringMask(triggerMask, hitObject.tag)) {
                    return;
                }
            }

            //自身不检测碰撞
            if (ower == hitObject.gameObject || ower == null)
            {
                return;
            }
            //同类型Member不检测
            if(ower.tag == hitObject.tag)
            {
                return;
            }

            UF_OnColliderEnter(hitObject.gameObject, hitPoint);
        }
        //unity monobehavior event
        private void OnTriggerExit(Collider hitObject) {
            Vector3 hitPoint = this.transform.position;
            UF_OnColliderExit(hitObject.gameObject, hitPoint);
        }

        protected virtual void UF_OnStarted() { }

        protected virtual void UF_OnPlay(GameObject tar, Vector3 tarPos, Vector3 vecforward) {
            //设置角度指向 
            this.euler = new Vector3(0, MathX.UF_EulerAngle(vecforward).y, 0);
        }

        protected virtual void UF_OnRun(float dtime) { }

        protected virtual void UF_OnColliderEnter(GameObject hitObject, Vector3 hitPoint)
        {
            if (!m_IsPlaying)
                return;
            //检查是有配置触发弹道，触发弹道继承当前弹道特性，触发消息通过触发弹道碰撞派发
            if (!UF_PlayTriggerDip()) {
                //发送消息到Lua中处理
                MessageSystem.UF_GetInstance().UF_Send(DefineEvent.E_DIP_COLLISION, this, hitObject.gameObject, hitPoint);
            }
            //播放触发特效
            UF_PlayTriggerEffect(hitPoint);
        }

        protected virtual void UF_OnColliderExit(GameObject hitObject, Vector3 hitPoint) { }

        protected virtual void UF_OnStop() { }

        public virtual void UF_OnReset() {
            uptr = string.Empty;
            m_LifeTick = 0;
            parameters = null;
            maskTriggerObejcts = null;
            ower = null;
            m_Target = null;
        }




    }
}

