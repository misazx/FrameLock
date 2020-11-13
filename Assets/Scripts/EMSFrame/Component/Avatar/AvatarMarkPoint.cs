//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace UnityFrame{
	[System.Serializable]
	public class AvatarMarkPoint {
		struct WidgetPoint{
			public string name;
			public string mpName;
			public EntityObject target;
			public WidgetPoint(EntityObject w,string m,string n){
				target = w;
				mpName = m;
				name = n;
			}
			public void Dispose(){
                if (target != null) {
                    target.Release();
                    target = null;
                }
			}
		}
			
		private List<WidgetPoint> m_ListWidgetPoint = new List<WidgetPoint> ();
		private AvatarController m_Avatar;

		//记录挂载实体
		[SerializeField]private List<GameObject> m_MarkPoints = new List<GameObject> ();

		public List<GameObject> listMarkPoint{get{return m_MarkPoints;}}

		public bool Exist(string strName){
			return UF_Find(strName) != null;
		}

		public GameObject UF_Find(string strName){
			int count = m_MarkPoints.Count;
			//root is special
			if (strName == "ROOT")
				return m_Avatar.gameObject;
			
			for (int k = 0; k < count; k++) {
				if (m_MarkPoints [k].name == strName) {
					return m_MarkPoints [k];
				}
			}
			return null;
		}
			
		public void UF_OnAwake(AvatarController avatar){
			m_Avatar = avatar;
		}

		//获取局部位置坐标
		public Vector3 UF_GetLocalPosition(string strName){
			GameObject go = UF_Find(strName);
			if(go != null)
			{
				return go.transform.localPosition;
			}
			return Vector3.zero;
		}

		public Vector3 UF_GetPosition(string strName){
			GameObject go = UF_Find(strName);
			if(go != null)
			{
				return go.transform.position;
			}
			return Vector3.zero;
		}

		public Vector3 UF_GetEuler(string strName){
			GameObject go = UF_Find(strName);
			if(go != null)
			{
				return go.transform.eulerAngles;
			}
			return Vector3.zero;
		}

		public Vector3 UF_GetLocalEuler(string strName){
			GameObject go = UF_Find(strName);
			if(go != null)
			{
				return go.transform.localEulerAngles;
			}
			return Vector3.zero;
		}


        public EntityObject UF_GetWidgetObject(string markPointName, string spName) {
            for (int k = 0; k < m_ListWidgetPoint.Count; k++)
            {
                if (m_ListWidgetPoint[k].mpName == markPointName && m_ListWidgetPoint[k].name == spName)
                {
                    return m_ListWidgetPoint[k].target;
                }
            }
            return null;
        }

	
		public void UF_AddWidget(EntityObject widget,string markPointName){
			if (widget == null){return;}
            UF_AddWidget(widget,markPointName,widget.name);
		}


		public void UF_AddWidget(EntityObject widget,string markPointName,string spName){
			if (widget == null){return;}

			GameObject markPoint = UF_Find(markPointName);
            if (markPoint == null)
            {
                Debugger.UF_Error(string.Format("Can not find markPoint[{0}] in Avatar", markPointName));
                markPoint = m_Avatar.gameObject;
            }
            

            if (markPoint != null) {
				m_ListWidgetPoint.Add (new WidgetPoint (widget,markPointName,spName));
				widget.UF_SetParent(markPoint.transform,true);
				widget.localScale = Vector3.one;

			}
		}

		public void UF_RemoveWidget(string markPointName,string spName){
			for (int k = 0; k < m_ListWidgetPoint.Count; k++) {
				if (m_ListWidgetPoint [k].mpName == markPointName && m_ListWidgetPoint [k].name == spName) {
					m_ListWidgetPoint [k].Dispose ();
					m_ListWidgetPoint.RemoveAt (k);
					return;
				}
			}
		}

		public void UF_ClearWidget(string markPointName){
			for (int k = 0; k < m_ListWidgetPoint.Count; k++) {
				if (m_ListWidgetPoint [k].mpName == markPointName) {
					m_ListWidgetPoint [k].Dispose ();
					m_ListWidgetPoint.RemoveAt (k);
					k--;
				}
			}
		}
			

		public void UF_OnReset(){
			if (m_ListWidgetPoint.Count > 0) {
				for (int k = 0; k < m_ListWidgetPoint.Count; k++) {
					m_ListWidgetPoint [k].Dispose ();
				}
				m_ListWidgetPoint.Clear ();
			}
		}


	}
}