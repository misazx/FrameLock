//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UnityFrame
{
	//最大显示固定item数量的Grid,如果数量超过最大数量则会循环第一个item 内容到最后
	public class UIFixedGrid : UIGrid
	{
		[SerializeField]public int m_FixedCount = 20;

		public int fixedCount{get{return m_FixedCount;}set{m_FixedCount = value;this.UF_SetDirty();}}

		private Queue<IUIUpdate> m_QueueUI = null;

        public new IUIUpdate UF_GenUI(bool firstSibling) {
            return UF_GenUI(null, firstSibling);
        }

		public override IUIUpdate UF_GenUI(string spUpdateKey,bool firstSibling)
		{
			if (m_QueueUI.Count > m_FixedCount) {
				IUIUpdate ui = m_QueueUI.Dequeue ();
				if (ui != null) {
                    if (ui is IOnReset) {
                        (ui as IOnReset).UF_OnReset();
                    }
                    if (!string.IsNullOrEmpty(spUpdateKey)) {
                        this.UF_ChangeUIUpdateKey(ui.updateKey, spUpdateKey);
                    }
                    //this.AddUI(ui, firstSibling);
                    m_QueueUI.Enqueue(ui);
                    if (firstSibling)
                        (ui as MonoBehaviour).transform.SetAsFirstSibling();
                    else
                        (ui as MonoBehaviour).transform.SetAsLastSibling();

                    UF_SetDirty();
                    return ui;
				}
				else{
					Debugger.UF_Error (string.Format("The UI[{0}] Can not Access in this Grid,Check if has been Destroyed!",m_PrefabUI));
				}
			}
			return base.UF_GenUI(spUpdateKey,firstSibling);
		}

		public override void UF_AddUI(IUIUpdate ui, bool firstSibling)
		{
			base.UF_AddUI(ui, firstSibling);
			m_QueueUI.Enqueue (ui);
		}

		protected override void Awake ()
		{
			base.Awake ();
			if (m_QueueUI == null) {
				m_QueueUI = QueuePool<IUIUpdate>.Get ();
			}
		}

        public override void UF_Clear()
        {
            if (m_QueueUI != null)
                m_QueueUI.Clear();
            base.UF_Clear();
        }



        public override void UF_OnReset()
        {
            base.UF_OnReset();
            if (m_QueueUI != null)
                m_QueueUI.Clear();
        }

        protected override void OnDestroy ()
		{
			base.OnDestroy ();
			QueuePool<IUIUpdate>.Release (m_QueueUI);
		}
			
	}
}

