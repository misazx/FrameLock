//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;

namespace UnityFrame
{
	//实体控制器基类
	[DisallowMultipleComponent]
	public class EntityObject : MonoBehaviour, IEntityHnadle,IReleasable
    {
        public int id { get { return this.GetInstanceID(); } }
        public int eLayer { get; set; }
        public bool isReleased{ get;set;} 
		public float timeStamp{get;set;}
        public bool isActive { get { return this.gameObject.activeSelf; } set { this.gameObject.SetActive(value); } }

        public Vector3 position{get{ return this.transform.position;}set{this.transform.position = value; }}
		public Vector3 euler{get{ return this.transform.eulerAngles;}set{this.transform.eulerAngles = value; }}
        public Quaternion rotation { get { return this.transform.rotation; } set { this.transform.rotation = value; } }
        public Vector3 localPosition{get{ return this.transform.localPosition;}set{this.transform.localPosition = value; }}
		public Vector3 localEuler{get{ return this.transform.localEulerAngles;}set{this.transform.localEulerAngles = value; }}
		public Vector3 localScale{get{ return this.transform.localScale;}set{this.transform.localScale = value; }}
		public Vector3 forward{get{ return this.transform.forward;}}
		public Vector3 backward{get{return -forward;}}

        private int m_TimeReleaseHandle;

		public void UF_SetActive(bool value){
            this.gameObject.SetActive (value);
		}

		public void UF_SetParent(Transform parent){
            UF_SetParent(parent, false);
		}

		public void UF_SetParent(Transform parent,bool normalized){
			this.transform.parent = parent;
			if (normalized) {
				this.transform.localPosition = Vector3.zero;
				this.transform.localEulerAngles = Vector3.zero;
			}
		}

		//设置层
		public void UF_SetLayer(int value,bool withChild = false){
            if (withChild)
            {
                GHelper.UF_BatchSetLayer(this.gameObject, value);
            }
            else {
                this.gameObject.layer = value;
            }
		}

        //设置标签
        public void UF_SetTag(string value) {
            this.tag = value;
        }

		public void Release(){
            //if (!isReleased)
            //    this.isActive = false;
            isReleased = true;
            FrameHandle.UF_RemoveCouroutine(m_TimeReleaseHandle);
            m_TimeReleaseHandle = 0;
        }


        //通过Child路径获取Component
        public Object UF_GetComponent(string path, string componentName) {
            if (string.IsNullOrEmpty(componentName)) {
                Debugger.UF_Warn(string.Format("GetChildComponent Failed With path[{0}] Component[{1}]", path, componentName));
                return null;
            }
            Transform trans = this.transform;
            if (!string.IsNullOrEmpty(path)) {
                trans = this.transform.Find(path);
            }

            if (trans != null)
            {
                return trans.GetComponent(componentName);
            }
            else
            {
                Debugger.UF_Warn(string.Format("Can not get Component[{0}] in Child[{0}]", componentName, path));
                return null;
            }
        }

        //定时释放
        public void UF_TimedRelease(float time) {
            m_TimeReleaseHandle  = FrameHandle.UF_AddCoroutine(UF_ITimedRelease(time));
        }
        System.Collections.IEnumerator UF_ITimedRelease(float time) {
            yield return new WaitForSeconds(time);
            isReleased = true;
            m_TimeReleaseHandle = 0;
        }

        void OnDestroy() {
            if (!isReleased && !GlobalSettings.IsApplicationQuit) {
                Debug.LogError(string.Format("Entity Object[{0}] has been destory with no release state",this.name));
            }
        }

    }
}

