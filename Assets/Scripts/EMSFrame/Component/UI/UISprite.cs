//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace UnityFrame{
    public class UISprite : UnityEngine.UI.Image, IUIUpdate, IUIColorable, IOnReset
    {

        private int m_HandleFillAmount = 0;

        private Sprite m_RevertSprite = null;

        [SerializeField] private string m_UpdateKey;
        [SerializeField] protected bool m_AutoNativeSize;
        [SerializeField] protected bool m_AutoRevert;
        [SerializeField] protected bool m_HideGraphic;

        public string updateKey { get { return m_UpdateKey; } set { m_UpdateKey = value; } }

        public Vector3 anchoredPosition { get { return rectTransform.anchoredPosition; } set { rectTransform.anchoredPosition = value; } }

        public Vector3 sizeDelta { get { return rectTransform.sizeDelta; } set { rectTransform.sizeDelta = value; } }


        public bool hideGraphic {
            get { return m_HideGraphic; }
            set { m_HideGraphic = value; this.SetVerticesDirty(); }
        }


        //在更新sprite	的时候自动SetNativeSize
        public bool autoNativeSize
        {
            get { return m_AutoNativeSize; }
            set
            {
                m_AutoNativeSize = value;
                if (m_AutoNativeSize)
                {
                    SetNativeSize();
                }
            }
        }

        public bool autoRevert { get { return m_AutoRevert; } set { m_AutoRevert = value; } }

        public bool grey { set { UF_SetGrey(value); } }

        public void UF_SetActive(bool active)
        {
            this.gameObject.SetActive(active);
        }

        public void UF_SetValue(object value)
        {
            if (value == null) { return; }
            UF_SetSprite(value as string);
        }

        public void UF_SetGrey(bool opera)
        {
            this.material = opera ? ShaderManager.UF_GetInstance().UF_GetUIGreyMaterial() : null;
        }

        public void UF_SetAlpha(float value)
        {
            this.color = new Color(this.color.r, this.color.g, this.color.b, value);
        }

        public void UF_SetColor(UnityEngine.Color value)
        {
            this.color = value;
        }

        protected override void Awake()
        {
            base.Awake();
            if (autoRevert)
            {
                m_RevertSprite = this.sprite;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (autoNativeSize)
            {
                SetNativeSize();
            }
        }


        public virtual void UF_SetSprite(string spriteName)
        {
            this.sprite = UIManager.UF_GetInstance().UF_GetSprite(spriteName);
            if (m_AutoNativeSize)
            {
                SetNativeSize();
            }
        }

        #if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (autoNativeSize)
            {
                SetNativeSize();
            }
        }
        #endif


        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            if (m_HideGraphic)
            {
                toFill.Clear();
                return;
            }
            base.OnPopulateMesh(toFill);
        }

        public int UF_SmoothFillAmount(float _to, float duration)
        {
            return UF_SmoothFillAmount(this.fillAmount, _to, duration);
        }

        public int UF_SmoothFillAmount(float _from, float _to, float duration)
        {
            return UF_SmoothFillAmount(_from, _to, duration, false);
        }

        public int UF_SmoothFillAmount(float _from, float _to, float duration, bool ingoreTimeScale)
        {
            UF_StopFillAmount();
            if (this.gameObject.activeInHierarchy)
            {
                m_HandleFillAmount = FrameHandle.UF_AddCoroutine(UF_ISmoothToValue(_from, _to, duration, ingoreTimeScale));
            }
            return m_HandleFillAmount;
        }

        public void UF_StopFillAmount()
        {
            if (m_HandleFillAmount != 0)
            {
                FrameHandle.UF_RemoveCouroutine(m_HandleFillAmount);
                m_HandleFillAmount = 0;
            }
        }


        IEnumerator UF_ISmoothToValue(float _from, float _to, float duration, bool ingoreTimeScale)
        {
            float progress = 0;
            float tickBuff = 0;
            while (progress < 1)
            {
                float delta = ingoreTimeScale ? GTime.DeltaTime : GTime.UnscaleDeltaTime;
                tickBuff += delta;
                progress = Mathf.Clamp01(tickBuff / duration);
                this.fillAmount = Mathf.Lerp(_from, _to, progress);
                yield return null;
            }
        }

        public void UF_OnReset()
        {
            if (autoRevert)
            {
                this.sprite = m_RevertSprite;
            }
        }

    }

}