//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------   


using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;

namespace UnityFrame {
    public class UIPreview : UnityEngine.UI.RawImage, IPointerDownHandler, IPointerUpHandler, IUIUpdate, IOnReset
    {
        [SerializeField] private string m_UpdateKey;

        [SerializeField] protected Vector3 m_PreviewPos = new Vector3(0, 1.5f, 3);
        [SerializeField] protected Vector3 m_PreviewEuler = new Vector3(10, 180f, 0);
        [SerializeField] protected float m_FOV = 60;
        [SerializeField] protected float m_FieldDistance = 10;


        [SerializeField]protected bool m_UseDragRotate = false;
        [SerializeField]protected float m_SpeedRotate = 10.0f;
        [SerializeField]protected string m_EPressClick = string.Empty;
        [SerializeField]protected string m_EParam = string.Empty;

        protected bool m_IsDrag = false;
        protected float m_RotateAngle = 0;

        private RenderPreview m_Preview;

        public string updateKey { get { return m_UpdateKey; } set { m_UpdateKey = value; } }
        public Vector3 previewPos { get { return m_PreviewPos; }set { m_PreviewPos = value; UF_UpdateProperty(); } }
        public Vector3 previewEuler { get { return m_PreviewEuler; } set { m_PreviewEuler = value; UF_UpdateProperty(); } }
        public float FOV { get { return m_FOV; } set { m_FOV = value; UF_UpdateProperty(); } }
        public float fieldDistance { get { return m_FieldDistance; } set { m_FieldDistance = value; UF_UpdateProperty(); } }

        public bool useDragRotate { get { return m_UseDragRotate; } set { m_UseDragRotate = value; } }
        public float speedRotate { get { return m_SpeedRotate; } set { m_SpeedRotate = value; } }

        public string ePressClick { get { return m_EPressClick; } set { m_EPressClick = value; } }
        public string eParam { get { return m_EParam; } set { m_EParam = value; } }


        public Vector3 anchoredPosition { get { return rectTransform.anchoredPosition; } set { rectTransform.anchoredPosition = value; } }
        public Vector3 sizeDelta { get { return rectTransform.sizeDelta; } set { rectTransform.sizeDelta = value; } }


        private RenderPreview preview {
            get {
                if (m_Preview == null && Application.isPlaying)
                {
                    m_Preview = RenderPreviewManager.UF_GetInstance().UF_AcquirePreview();
                    m_Preview.camera.transform.localPosition = previewPos;
                    m_Preview.camera.fieldOfView = FOV;
                    //bind texture
                    this.texture = m_Preview.texture;
                    this.material = ShaderManager.UF_GetInstance().UF_GetUIPreviewMaterial();
                }
                return m_Preview;
            }
            set {
                if (value == null) {
                    RenderPreviewManager.UF_GetInstance().UF_ReleasePreview(m_Preview);
                    m_Preview = null;
                }
            }
        }

        public void UF_SetActive(bool active)
        {
            this.gameObject.SetActive(active);
        }

        public void UF_SetValue(object value)
        {
            if (value is GameObject)
            {
                UF_AddObject(value as GameObject);
            }
            else if (value is IEntityHnadle) {
                UF_AddObject(value as IEntityHnadle);
            }
        }


        public void UF_AddObject(IEntityHnadle obj) {
            this.UF_AddInternal(obj as MonoBehaviour, obj.name);
        }

        public void UF_AddObject(IEntityHnadle obj, string alias)
        {
            this.UF_AddInternal(obj as MonoBehaviour, alias);
        }

        public void UF_AddObject(GameObject obj)
        {
            this.UF_AddInternal(obj, obj.name);
        }

        public void UF_AddObject(GameObject obj, string alias)
        {
            this.UF_AddInternal(obj, alias);
        }

        public void UF_AddObject(Transform obj)
        {
            this.UF_AddInternal(obj, obj.name);
        }

        public void UF_AddObject(Transform obj,string alias)
        {
            this.UF_AddInternal(obj,alias);
        }

        protected void UF_AddInternal(Object obj, string alias) {
            preview.UF_Add(obj, true, alias);
            UF_UpdateProperty();
        }

        public Object UF_GetObject(string strName) {
            if (m_Preview != null)
                return m_Preview.UF_Get(strName);
            return null;
        }

        public void UF_RemoveObject(string strName) {
            if (m_Preview != null)
                m_Preview.UF_Remove(strName);
        }

        public void UF_RemoveObject(Object obj)
        {
            if (m_Preview != null)
                m_Preview.UF_Remove(obj);
        }


        protected void UF_UpdateProperty()
        {
            if (m_Preview == null) return;
            if (m_Preview.camera != null)
            {
                m_Preview.camera.transform.localPosition = previewPos;
                m_Preview.camera.transform.localEulerAngles= previewEuler;
                m_Preview.camera.fieldOfView = FOV;
                m_Preview.camera.farClipPlane = fieldDistance;
            }
        }

        public void UF_Clear() {
            preview.UF_Clear();
        }

        public void UF_OnReset() {
            preview = null;
            this.texture = null;
            m_IsDrag = false;
            m_RotateAngle = 0;
        }


        public void OnPointerDown(PointerEventData eventData) {
            m_IsDrag = true;
        }


        public void OnPointerUp(PointerEventData eventData) {
            if (m_IsDrag)
            {
                if (!string.IsNullOrEmpty(ePressClick))
                {
                    MessageSystem.UF_GetInstance().UF_Send(DefineEvent.E_UI_OPERA, ePressClick, eParam, this);
                }
            }
            m_IsDrag = false;
        }


        void Update() {
            if (m_UseDragRotate && m_IsDrag) {
                m_RotateAngle -= DeviceInput.UF_HorizontalDelta(0) * m_SpeedRotate;
                if (m_Preview != null) {
                    m_Preview.UF_SetEuler(new Vector3(0, m_RotateAngle, 0));
                }
            }
        }

        //protected override void OnDestroy()
        //{
        //    UF_OnReset();
        //    base.OnDestroy();
        //}

#if UNITY_EDITOR
        //[ContextMenu("Test")]
        //protected void Test() {
        //    this.AddObject("ava_fukaka");
        //}


        protected override void OnValidate()
        {
            base.OnValidate();

        }
#endif

    }
}

