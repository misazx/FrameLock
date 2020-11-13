//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
namespace UnityFrame
{
    public static class UILayoutTools {
        internal class LayoutBuilder : ICanvasElement
        {
            public Transform transform{ get { return null; } }

            List<IUILayout> m_ListLayouts = new List<IUILayout>();
            HashSet<int> m_MapLayouts = new HashSet<int>();

            public bool AddToRebuild(IUILayout layout) {
                int hashcode = layout.GetHashCode();
                if (!m_MapLayouts.Contains(hashcode))
                {
                    m_ListLayouts.Add(layout);
                    m_MapLayouts.Add(hashcode);
                    //for register ui canvas rebuild system
                    CanvasUpdateRegistry.TryRegisterCanvasElementForLayoutRebuild(this);
                    return true;
                }
                else {
                    return false;
                }
            }

            public void Rebuild(CanvasUpdate executing) {
                for (int k = 0; k < m_ListLayouts.Count; k++) {
                    //if (m_ListLayouts[k] != null)
                    if (!object.ReferenceEquals(null, m_ListLayouts[k]) && !m_ListLayouts[k].IsDestroyed())
                    {
                        m_ListLayouts[k].UF_RebuildLoyout();
                    }
                }
            }

            public void LayoutComplete() {
                m_ListLayouts.Clear();
                m_MapLayouts.Clear();
            }

            public void GraphicUpdateComplete() {}

            public bool IsDestroyed() {
                return false;
            }
        }

        static LayoutBuilder s_LayoutBuilder = new LayoutBuilder();


        //标记为需要重新构建的布局
        public static void UF_MarkLayoutForRebuild(IUILayout layout) {
            if (object.ReferenceEquals(null, layout))
                return;
            //LayoutRebuilder.MarkLayoutForRebuild(this.rectTransform);
            s_LayoutBuilder.AddToRebuild(layout);
        }


        public static void UF_ContentSizeFitter(RectTransform rectTransform, SizeFitterType fitterType, Vector2 size) {
            UF_ContentSizeFitter(rectTransform, fitterType, size, Vector2.zero);
        }
        public static void UF_ContentSizeFitter(RectTransform rectTransform, SizeFitterType fitterType,Vector2 size,Vector2 padding)
        {
            if (fitterType == SizeFitterType.Aspect)
            {
                rectTransform.sizeDelta = new Vector2(size.x + padding.x, size.y + padding.y);
            }
            else if (fitterType == SizeFitterType.Horizontal)
            {
                rectTransform.sizeDelta = new Vector2(size.x + padding.x, rectTransform.sizeDelta.y);
            }
            else if (fitterType == SizeFitterType.Vertical)
            {
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, size.y + padding.y);
            }
        }


        public static Vector2 UF_GetContentSize(RectTransform rectTransform) {
            RectTransform rectTrans = null;
            int childCount = rectTransform.childCount;
            float minX = 0;
            float maxX = 0;
            float minY = 0;
            float maxY = 0;
            float temp = 0;
            for (int k = 0; k < childCount;)
            {
                rectTrans = rectTransform.GetChild(k++) as RectTransform;
                if (rectTrans != null && rectTrans.gameObject.activeSelf)
                {
                    temp = rectTrans.rect.x + rectTrans.anchoredPosition.x;
                    if (temp < minX)
                        minX = temp;
                    temp = rectTrans.rect.x + rectTrans.rect.width + rectTrans.anchoredPosition.x;
                    if (temp > maxX)
                        maxX = temp;

                    temp = rectTrans.rect.y + rectTrans.anchoredPosition.y;
                    if (temp < minY)
                        minY = temp;
                    temp = rectTrans.rect.y + rectTrans.rect.height + rectTrans.anchoredPosition.y;
                    if (temp > maxY)
                        maxY = temp;

                }
            }
            return new Vector2(maxX - minX, maxY - minY);
        }


        static Vector2 UF_GetAnchorSide(LayoutCorner alignement)
        {
            switch (alignement)
            {
                case LayoutCorner.UpperLeft: return new Vector2(1, 1);
                case LayoutCorner.UpperRight: return new Vector2(-1, 1);
                case LayoutCorner.LowerLeft: return new Vector2(1, -1);
                case LayoutCorner.LowerRight: return new Vector2(-1, -1);
            }
            return new Vector2(1, 1);
        }

        static Vector2 UF_GetAnchor(LayoutCorner alignement)
        {
            switch (alignement)
            {
                case LayoutCorner.UpperLeft: return new Vector2(0, 1);
                case LayoutCorner.UpperRight: return new Vector2(1, 1);
                case LayoutCorner.LowerLeft: return new Vector2(0, 0);
                case LayoutCorner.LowerRight: return new Vector2(1, 0);
            }
            return new Vector2(0, 1);
        }

        //构建Grid布局并返回布局大小
        public static Vector2 UF_BuildLayoutGrid(RectTransform rectTransform, LayoutCorner alignement, Vector2 padding, Vector2 space, int constraint) {
            int curConstraint = 0;
            int childNum = rectTransform.childCount;
            float curWidth = padding.x;
            float curHeight = 0;
            float maxWidth = padding.x;
            float maxHeight = padding.y;
            bool rawH = false;
            Vector2 anchor = UF_GetAnchor(alignement);
            Vector2 side = UF_GetAnchorSide(alignement);
            int ctVal = constraint - 1;
            RectTransform rectTrans = null;
            for (int k = 0; k < childNum;)
            {
                rectTrans = rectTransform.GetChild(k++) as RectTransform;
                if (rectTrans != null && rectTrans.gameObject.activeSelf && rectTrans.tag != DefineTag.IngoreLayout)
                {
                    rectTrans.anchorMax = anchor;
                    rectTrans.anchorMin = anchor;
                    rectTrans.pivot = anchor;
                    if (rectTrans.rect.height > curHeight)
                    {
                        curHeight = rectTrans.rect.height;
                    }
                    rectTrans.anchoredPosition = new Vector2(curWidth * side.x, -maxHeight * side.y);
                    curWidth += rectTrans.rect.width + space.x;
                    if (maxWidth < curWidth)
                    {
                        maxWidth = curWidth;
                    }
                    rawH = false;
                    curConstraint++;
                    if (curConstraint > ctVal)
                    {
                        rawH = true;
                        maxHeight += curHeight + space.y;
                        curConstraint = 0;
                        curWidth = padding.x;
                        curHeight = 0;
                    }
                }
            }

            maxWidth += padding.x - space.x;

            if (rawH) { maxHeight += padding.y - space.y; }

            else { maxHeight += padding.y + curHeight; }

            return new Vector2(maxWidth, maxHeight);
        }


        public static void UF_RebuildParentLayout(IUILayout layout)
        {
            var parent = (layout as MonoBehaviour).transform.parent;
            if (parent != null)
            {
                var parentLayout = parent.GetComponentInParent<IUILayout>();
                //忽略标签布局
                if (parentLayout != null)
                {
                    parentLayout.UF_RebuildLoyout();
                }
            }
        }

        public static void UF_RebuildSiblingLayout(IUILayout layout)
        {
            List<IUILayout> list = ListCache<IUILayout>.Acquire();
            (layout as MonoBehaviour).gameObject.GetComponents<IUILayout>(list);
            foreach (var item in list)
            {
                //忽略标签布局
                if (item != layout)
                {
                    item.UF_RebuildLoyout();
                }
            }
            ListCache<IUILayout>.Release(list);
        }

    }



	public static class UIUpdateTools{
		//构建更新列表，用于UIUpdateGroup 自身构建
		public static void UF_BuildUpdateList(UIUpdateGroup headGroup,List<UnityEngine.Object> ret){
			ret.Clear ();

			List<UIUpdateGroup> m_TmpGroups = new List<UIUpdateGroup>();

			List<UnityEngine.Object> uiupdates = new List<UnityEngine.Object> ();

            UF_searchSubUIUpdatables(headGroup.gameObject,uiupdates);

			m_TmpGroups.Clear();

			//耗时较长，待优化,1K个物体耗时越为10毫秒
			for (int k = 0; k < uiupdates.Count; k++) {
				if (uiupdates[k] != headGroup) {
					if (uiupdates [k] is UIUpdateGroup) {
						UIUpdateGroup tmp = uiupdates [k] as UIUpdateGroup;
						m_TmpGroups.Add (tmp);
                        UF_BuildUpdateList(tmp, tmp.ListUpdateUI);
					}
					UIUpdateGroup targetParentGroup = UF_findParentGroup((uiupdates [k] as MonoBehaviour).transform, m_TmpGroups,headGroup);
					if (targetParentGroup == headGroup) {
						ret.Add (uiupdates [k]);
					}

				}
			}

			m_TmpGroups.Clear();
		}



		private static void UF_searchSubUIUpdatables(GameObject go,List<UnityEngine.Object> ret){
			MonoBehaviour[] uictrls = go.GetComponentsInChildren<MonoBehaviour> (true);

			for (int k = 0; k < uictrls.Length; k++) {
				if (uictrls[k] != null && uictrls[k] is IUIUpdate && !string.IsNullOrEmpty(((IUIUpdate)uictrls[k]).updateKey)) {
					ret.Add (uictrls[k]);
				}
			}

		}

		private static UIUpdateGroup UF_findParentGroup(Transform current,List<UIUpdateGroup> uigroups,UIUpdateGroup rootGroup){
			current = current.parent;
			while (current != null && current != rootGroup.transform) {
				for (int k = uigroups.Count - 1; k >= 0; --k) {
					if (current == uigroups [k].transform) {
						return uigroups [k];
					}
				}
				current = current.parent;
			}
			return rootGroup;
		}





	}


	public static class UIColorTools{

		public static void UF_SetGrey(GameObject target,bool opera){
            List<IUIColorable> array = ListCache<IUIColorable>.Acquire();
			target.GetComponentsInChildren<IUIColorable>(true, array);
			if (array != null && array.Count> 0) {
				for (int k = 0; k < array.Count; k++) {
					array[k].UF_SetGrey(opera);
				}
			}
            ListCache<IUIColorable>.Release(array);
        }

		public static void UF_SetAlpha(GameObject target,float value){
            List<IUIColorable> array = ListCache<IUIColorable>.Acquire();
            target.GetComponentsInChildren<IUIColorable>(true, array);
            value = Mathf.Clamp01 (value);
			if (array != null && array.Count> 0) {
				for (int k = 0; k < array.Count; k++) {
                    array[k].UF_SetAlpha(value);
                }
			}
            ListCache<IUIColorable>.Release(array);
        }

        public static void UF_SetRenderAlpha(GameObject target, float value)
        {
            List<CanvasRenderer> array = ListCache<CanvasRenderer>.Acquire();
            target.GetComponentsInChildren<CanvasRenderer>(true, array);
            value = Mathf.Clamp01(value);
            if (array != null && array.Count > 0)
            {
                for (int k = 0; k < array.Count; k++)
                {
                    array[k].SetAlpha(value);
                }
            }
            ListCache<CanvasRenderer>.Release(array);
        }

        public static void UF_SetColor(GameObject target,Color value){
            List<IUIColorable> array = ListCache<IUIColorable>.Acquire();
            target.GetComponentsInChildren<IUIColorable>(true, array);
			if (array != null && array.Count > 0) {
				for (int k = 0; k < array.Count; k++) {
                    array[k].UF_SetColor(value);
                }
			}
            ListCache<IUIColorable>.Release(array);
        }

        public static void UF_SetRenderColor(GameObject target, Color value)
        {
            List<CanvasRenderer> array = ListCache<CanvasRenderer>.Acquire();
            target.GetComponentsInChildren<CanvasRenderer>(true, array);
            if (array != null && array.Count > 0)
            {
                for (int k = 0; k < array.Count; k++)
                {
                    array[k].SetColor(value);
                }
            }
            ListCache<CanvasRenderer>.Release(array);
        }

        public static int UF_CrossColor(GameObject target,Color vfrom,Color vto,float duration,bool ingoreTimeScale){
			return FrameHandle.UF_AddCoroutine(UF_ICrossColor(target,vfrom,vto,duration,ingoreTimeScale));
		}
        static IEnumerator UF_ICrossColor(GameObject target, Color vfrom, Color vto, float duration, bool ingoreTimeScale)
        {
            List<IUIColorable> array = ListCache<IUIColorable>.Acquire();
            target.GetComponentsInChildren<IUIColorable>(true, array);
            IUIColorable coloreffect = null;
            if (array != null && array.Count > 0)
            {
                float progress = 0;
                float tickBuff = 0;
                while (progress < 1)
                {
                    float delta = ingoreTimeScale ? GTime.DeltaTime : GTime.UnscaleDeltaTime;
                    tickBuff += delta;
                    progress = Mathf.Clamp01(tickBuff / duration);
                    Color current = progress * vto + (1 - progress) * vfrom;
                    for (int k = 0; k < array.Count; k++)
                    {
                        coloreffect = array[k];
                        if (coloreffect != null)
                        {
                            coloreffect.UF_SetColor(current);
                        }
                    }
                    yield return null;
                }
            }
            ListCache<IUIColorable>.Release(array);
        }

        public static int UF_CrossAlpha(GameObject target,float vfrom,float vto,float duration,bool ingoreTimeScale){
			return FrameHandle.UF_AddCoroutine(UF_ICrossAlpha(target,vfrom,vto,duration,ingoreTimeScale));
		}
        static IEnumerator UF_ICrossAlpha(GameObject target, float vfrom, float vto, float duration, bool ingoreTimeScale)
        {
            List<IUIColorable> array = ListCache<IUIColorable>.Acquire();
            target.GetComponentsInChildren<IUIColorable>(true, array);
            IUIColorable coloreffect = null;
            if (array != null && array.Count > 0)
            {
                float progress = 0;
                float tickBuff = 0;
                while (progress < 1)
                {
                    float delta = ingoreTimeScale ? GTime.DeltaTime : GTime.UnscaleDeltaTime;
                    tickBuff += delta;
                    progress = Mathf.Clamp01(tickBuff / duration);
                    float current = progress * vto + (1 - progress) * vfrom;
                    for (int k = 0; k < array.Count; k++)
                    {
                        coloreffect = array[k];
                        if (coloreffect != null)
                        {
                            coloreffect.UF_SetAlpha(current);
                        }
                    }
                    yield return null;
                }
            }
            ListCache<IUIColorable>.Release(array);
        }


        public static int UF_CrossRenderColor(GameObject target, Color vfrom, Color vto, float duration, bool ingoreTimeScale)
        {
            return FrameHandle.UF_AddCoroutine(UF_ICrossRenderColor(target, vfrom, vto, duration, ingoreTimeScale));
        }
        static IEnumerator UF_ICrossRenderColor(GameObject target, Color vfrom, Color vto, float duration, bool ingoreTimeScale)
        {
            List<CanvasRenderer> array = ListCache<CanvasRenderer>.Acquire();
            target.GetComponentsInChildren<CanvasRenderer>(true, array);
            CanvasRenderer coloreffect = null;
            if (array != null && array.Count > 0)
            {
                float progress = 0;
                float tickBuff = 0;
                while (progress < 1)
                {
                    float delta = ingoreTimeScale ? GTime.DeltaTime : GTime.UnscaleDeltaTime;
                    tickBuff += delta;
                    progress = Mathf.Clamp01(tickBuff / duration);
                    Color current = progress * vto + (1 - progress) * vfrom;
                    for (int k = 0; k < array.Count; k++)
                    {
                        coloreffect = array[k];
                        if (coloreffect != null)
                        {
                            coloreffect.SetColor(current);
                        }
                    }
                    yield return null;
                }
            }
            ListCache<CanvasRenderer>.Release(array);
        }


        public static int UF_CrossRenderAlpha(GameObject target, float vfrom, float vto, float duration, bool ingoreTimeScale)
        {
            return FrameHandle.UF_AddCoroutine(UF_ICrossRenderAlpha(target, vfrom, vto, duration, ingoreTimeScale));
        }
        static IEnumerator UF_ICrossRenderAlpha(GameObject target, float vfrom, float vto, float duration, bool ingoreTimeScale)
        {
            List<CanvasRenderer> array = ListCache<CanvasRenderer>.Acquire();
            target.GetComponentsInChildren<CanvasRenderer>(true, array);
            CanvasRenderer coloreffect = null;
            if (array != null && array.Count > 0)
            {
                float progress = 0;
                float tickBuff = 0;
                while (progress < 1)
                {
                    float delta = ingoreTimeScale ? GTime.DeltaTime : GTime.UnscaleDeltaTime;
                    tickBuff += delta;
                    progress = Mathf.Clamp01(tickBuff / duration);
                    float current = progress * vto + (1 - progress) * vfrom;
                    for (int k = 0; k < array.Count; k++)
                    {
                        coloreffect = array[k];
                        if (coloreffect != null)
                        {
                            coloreffect.SetAlpha(current);
                        }
                    }
                    yield return null;
                }
            }
            ListCache<CanvasRenderer>.Release(array);
        }


    }


}