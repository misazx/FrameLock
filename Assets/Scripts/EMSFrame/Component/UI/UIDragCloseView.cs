using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityFrame;

namespace UnityFrame{
	[RequireComponent(typeof(RectTransform))]
	public class UIDragCloseView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		public enum DragDirectionType
		{
			VerticalUp = 1,
			VerticalDown = 2,
			HorizontalLeft = 3,
			HorizontalRight = 4
		}

		public float scaleSpeed = 2f;
		//public float closeScale = 0.6f;
		public DragDirectionType dir = DragDirectionType.VerticalUp;		
		public string eventName = "E_CLICK_CLOSE";
		public string[] eParams;
		public UnityEngine.Events.UnityEvent invoke;
        public EffectBase[] effects;

		private RectTransform m_RectTransform;
		private Vector3 m_sourcePosition;
		//private Vector3 m_sourceScale;
		private Vector3 m_pointerSourcePosition;
        //private float m_tempScale = 1;

        private float m_progress = 0;

		void Start()
		{
			m_RectTransform = gameObject.GetComponent<RectTransform>();
		}
		private void UF_OnReset()
        {
            //this.m_RectTransform.localScale = this.m_sourceScale;
            this.m_RectTransform.position = this.m_sourcePosition;
            //this.m_tempScale = 1;            
            UF_SetEffectProgress(0);
        }

        private void UF_SetEffectProgress(float progress)
        {
            this.m_progress = progress;
            foreach (var item in this.effects)
            {
                item.SetTo(progress);
            }
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
		{			
			//this.m_sourceScale = this.m_RectTransform.localScale;
			//this.m_tempScale = 1;
            this.m_sourcePosition = this.m_RectTransform.position;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(this.m_RectTransform, eventData.position, eventData.pressEventCamera, out this.m_pointerSourcePosition);
            UF_SetEffectProgress(0);
        }

		void IDragHandler.OnDrag(PointerEventData eventData)
		{
            Vector3 vecPos = eventData.position;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(this.m_RectTransform, eventData.position, eventData.pressEventCamera, out vecPos);

            var offset = (vecPos - this.m_pointerSourcePosition);
            this.m_RectTransform.position = this.m_sourcePosition + offset;    //将最终的pos 位置值持续赋值给当前游戏物体的position

            if(null == this.effects || this.effects.Length == 0)
            {
                return;
            }

            switch (dir)
			{
				case DragDirectionType.VerticalUp:
                case DragDirectionType.VerticalDown:
                    UF_Vertical(offset);
					break;
				case DragDirectionType.HorizontalLeft:
                case DragDirectionType.HorizontalRight:
                    UF_Horizontal(offset);
					break;
				default:
					break;
			}
            
        }
		void IEndDragHandler.OnEndDrag(PointerEventData eventData)
		{
            //bool isClose = this.m_tempScale <= this.closeScale;
            bool isClose = this.m_progress >= 1;
			UF_OnReset();
			if (isClose)
			{
				if(!string.IsNullOrEmpty(this.eventName))
				{
					MessageSystem msg = MessageSystem.UF_GetInstance ();
					msg.UF_BeginSend();
					msg.UF_PushParam(eventName);
					for (int k = 0; k < eParams.Length; k++) {
						msg.UF_PushParam(eParams [k]);
					}
					msg.UF_EndSend(DefineEvent.E_UI_OPERA);
				}
				invoke?.Invoke();
			}
		}

		private void UF_Horizontal(Vector3 offset)
		{
            float per = scaleSpeed <= 0 ? 0 : Mathf.Min(1, Mathf.Abs(offset.x) / scaleSpeed);
            UF_SetEffectProgress(per);
        }

		private void UF_Vertical(Vector3 offset)
		{
            float per = scaleSpeed <= 0 ? 0 : Mathf.Min(1, Mathf.Abs(offset.y) / scaleSpeed);
            UF_SetEffectProgress(per);
        }    

	}
}