//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFrame
{
    [RequireComponent(typeof(Canvas))]
    public class UISortingGroup : UISortingObject
	{
        private Canvas m_Canvas;

        private Canvas canvas
        {
            get
            {
                if (m_Canvas == null)
                {
                    m_Canvas = this.GetComponent<Canvas>();
                }
                if(!m_Canvas.overrideSorting)
                    m_Canvas.overrideSorting = true;
                return m_Canvas;
            }
        }

        protected override void OnApplySortingOrder()
        {
            canvas.sortingOrder = sortingOrder + rootSortingOrder;
        }


    }

}