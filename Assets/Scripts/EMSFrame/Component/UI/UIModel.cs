//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace UnityFrame{
	
	[RequireComponent(typeof(BoxCollider))]
	public class UIModel : UISortingObject,IOnReset,IUIColorable
    {
		public bool alignedCenter = false;
		public bool useDragRotate = false;
		public float speedRotate = 10.0f;
		public float deadZoomClick = 200;

		public bool useClickAnimate = false;
		public string presetAnimation = "attack;idle";

		public string ePressClick = "";
        public string eParam= "";

        private bool m_IsDrag = false;
		private EntityObject m_Target;
		private float m_LastClickTime = 0;
		private float m_SoundAngle;
		private RaycastHit m_RayHit;

		public override void UF_SetValue (object value){
			if (value == null) {return;}
            UF_SetModel(value as EntityObject);
		}


		public EntityObject UF_GetModel(){
			return m_Target;
		}

		public void UF_SetModel(EntityObject model){
			if (model == null) {
                UF_Clear();
			}
			if (m_Target != null && m_Target != model) {
                UF_Clear();
			}
			m_Target = model;
			if(model)
			{
				model.transform.SetParent(transform);
				model.localPosition = Vector3.zero;
				model.localScale = Vector3.one;
				model.localEuler = Vector3.zero;
				if (model is AvatarController)
				{
					AvatarController mAvatar = model as AvatarController;
					mAvatar = model as AvatarController;
					mAvatar.capsule.isActive = false;
					mAvatar.motion.TurnForward(Vector3.forward);
					if (alignedCenter) {
						float offset = Mathf.Abs (mAvatar.focusPosition.y - mAvatar.position.y);
						model.transform.localPosition = new Vector3 (0, -offset, 0);
					}
				}
                this.UF_SetDirty();
//				Helper.UF_BatchSetLayer(model.gameObject, DefineLayer.UI);
			}

		}

		public void UF_Clear(){
			this.UF_OnReset();
		}


		public void UF_PlayAnimation(string action){
			if (m_Target != null && m_Target is AvatarController) {
				(m_Target as AvatarController).animator.UF_Play(action);
			}
		}

		public void UF_SetGrey(bool value){
			if (m_Target != null && m_Target is AvatarController) {
				(m_Target as AvatarController).render.UF_UF_SetGrey(value);
			}
		}
		public void UF_SetAlpha (float value){}
		public void UF_SetColor (UnityEngine.Color value){}


		public void UF_OnReset(){
			m_IsDrag = false;
			if (m_Target != null) {
//				Helper.UF_BatchSetLayer (m_Target.gameObject, DefineLayer.Default);
				m_Target.UF_SetLayer(DefineLayer.Default,true);
				m_Target.Release ();
				m_Target = null;
			}
		}

		public void UF_SetEuler(Vector3 value){
			gameObject.transform.localEulerAngles = value;
		}

        protected override void OnEnable()
        {
            base.OnEnable();
        }


        protected override void OnDisable()
        {
            base.OnDisable();
            UF_OnInputUp();
        }

        protected new void Update(){
            base.Update();
            UF_UpdateRotate();
		}

		private void UF_OnInputDown(){
			Ray ray = UIManager.UICamera.ScreenPointToRay(DeviceInput.UF_DownPosition(0));
			if (Physics.Raycast(ray, out m_RayHit, 10000))
			{
				if(m_RayHit.collider.gameObject == this.gameObject)
				{
					m_IsDrag = true;
					m_LastClickTime = System.Environment.TickCount;
					m_SoundAngle = gameObject.transform.localEulerAngles.y;

					if (useClickAnimate) {
                        UF_PlayAnimation(presetAnimation);
					}
				}
			}
		}

		private void UF_OnInputUp(){
			m_IsDrag = false;
			if(Mathf.Abs(m_LastClickTime - System.Environment.TickCount) < deadZoomClick){
				//send click msg
				if (!string.IsNullOrEmpty (ePressClick)) {
					MessageSystem.UF_GetInstance ().UF_Send(DefineEvent.E_UI_OPERA,ePressClick, eParam, this);
				}
			}
		}

        protected override void OnApplySortingOrder()
        {
            //获取正确的渲染序号
            int absOrder = rootSortingOrder + sortingOrder;
            List<Renderer> list = ListCache<Renderer>.Acquire();
            this.gameObject.GetComponentsInChildren<Renderer>(false, list);
            foreach (var render in list)
            {
                render.sortingOrder = absOrder;
            }
            ListCache<Renderer>.Release(list);
        }

        private void UF_UpdateRotate(){
			if(useDragRotate){
				if(!m_IsDrag){
					if (DeviceInput.UF_Down(0))
					{
                        UF_OnInputDown();
					}
				}
				else{
					if(DeviceInput.UF_Up(0)){
                        UF_OnInputUp();
						return;
					}

					m_SoundAngle -= DeviceInput.UF_HorizontalDelta(0)*speedRotate;
					gameObject.transform.localEulerAngles = new Vector3(gameObject.transform.localEulerAngles.x,m_SoundAngle,gameObject.transform.eulerAngles.z);

				}
			}
		}

	}

}