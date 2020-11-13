//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityFrame
{
	[RequireComponent(typeof(RectTransform))]
	public class UIObject : UIBehaviour,IUIUpdate
	{
		[HideInInspector][SerializeField]protected string m_UpdateKey;

		protected RectTransform m_RectTransform;

		public string updateKey{
			get{ return m_UpdateKey;}
			set{ m_UpdateKey = value;}
		}

        public Vector3 position { get { return this.transform.position; } set { this.transform.position = value; } }
        public Vector3 euler { get { return this.transform.eulerAngles; } set { this.transform.eulerAngles = value; } }

		public Vector3 localPosition { get { return this.transform.localPosition; } set { this.transform.localPosition = value; } }
        public Vector3 localEuler { get { return this.transform.localEulerAngles; } set { this.transform.localEulerAngles = value; } }
        public Vector3 localScale { get { return this.transform.localScale; } set { this.transform.localScale = value; } }
        
        public Vector3 anchoredPosition{get{ return rectTransform.anchoredPosition;}set{rectTransform.anchoredPosition = value;}}
		public Vector3 sizeDelta{get{ return rectTransform.sizeDelta;}set{rectTransform.sizeDelta = value;}}

		public RectTransform rectTransform{
			get{
				if (m_RectTransform == null && this != null) {
                    this.m_RectTransform = base.GetComponent<RectTransform>();
                }
				return m_RectTransform;
			}
		}

        public virtual void UF_SetActive(bool active){this.gameObject.SetActive (active);}

		public virtual void UF_SetValue (object value){}

	}


}

