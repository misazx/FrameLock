//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityFrame.Assets;

namespace UnityFrame
{
	public class UIManager : HandleSingleton<UIManager>,IOnAwake,IOnUpdate,IOnReset {

        public static int FixedWidth = 800;
        public static int FixedHeight = 1600;
        public static float FixedDeviceFactor = 0.85f;

        //所有图集集合
        private Dictionary<string,AssetUIAtlas> m_DicAtlaSets = new Dictionary<string, AssetUIAtlas>();

		//不同到画布存在不同到Z深度中
		private Dictionary<string,Canvas> m_DicCanvas = new Dictionary<string, Canvas>();

		private List<string> m_MarkViewInLoad = new List<string> ();

		//UI的显示栈，所有显示的UI都会被存放到栈中
		private TStack<UIView> m_ViewStack = new TStack<UIView> (64);

		private bool m_StackDirty = false;

        const float VIEW_ZOOM = -1000;

        private static Camera s_UICamera;

		private static GameObject s_UIRoot;

		private static RectTransform s_UIRootTransform;

		public static GameObject UIRoot{get{return s_UIRoot;}}

		public static RectTransform UIRootTransform{get{ return s_UIRootTransform;}}

		public static Camera UICamera{get{return s_UICamera;}}


		private string UF_getAtlaName(string spriteName){
			string ret = "";
			int idx = spriteName.IndexOf ('@');
			if (idx > -1) {
				ret = string.Format("ui_atlas_{0}",spriteName.Substring (0, idx));
			}
			return ret;
		}

		//获取动画序列数组
		public List<Sprite> UF_GetAnimateSqueueSprites(string prefixSpriteName){
			string atlaName = UF_getAtlaName(prefixSpriteName);
			if (!string.IsNullOrEmpty (atlaName)) {
				AssetUIAtlas atlas = UF_GetAtlas(atlaName);
				if (atlas != null) {
					List<Sprite> list = new List<Sprite> ();
					int idx = 1;
					Sprite sprite = atlas.UF_GetSpriteInMap(prefixSpriteName + idx);
					while (sprite != null) {
						list.Add (sprite);
						idx++;
						sprite = atlas.UF_GetSpriteInMap(prefixSpriteName + idx);
					}
					return list;
				}
			}
			return null;
		}



		internal AssetUIAtlas UF_GetAtlas(string atlaName){
			atlaName = atlaName.ToLower ();
			if (m_DicAtlaSets.ContainsKey (atlaName)) {
				return m_DicAtlaSets [atlaName];
			} else {
				AssetUIAtlas atlas = AssetSystem.UF_GetInstance().UF_LoadObjectImage(atlaName) as AssetUIAtlas;
				//设置AB引用
				AssetSystem.UF_GetInstance().UF_RetainRef(atlaName);
				if (atlas == null) {
					Debugger.UF_Warn (string.Format("This UIAlta[ {0} ] you want to load is Null",atlaName));	
				} else {
					m_DicAtlaSets.Add (atlaName, atlas);
				}
				return atlas;
			}
		}

		public Sprite UF_GetSprite(string spriteName){
			if (string.IsNullOrEmpty (spriteName)) {
				return null;
			}
			string atlaName = UF_getAtlaName(spriteName);
			if (!string.IsNullOrEmpty (atlaName)) {
				AssetUIAtlas atlas = UF_GetAtlas(atlaName);
				if (atlas != null) {
					return atlas.UF_GetSprite (spriteName);
				}
			}
			return null;
		}
			

		public void UF_RemoveAtla(string atlaName){
			if (m_DicAtlaSets.ContainsKey (atlaName)) {
				AssetUIAtlas atlas = m_DicAtlaSets [atlaName];
				m_DicAtlaSets.Remove (atlaName);
				UnityEngine.Object.Destroy(atlas);
				//释放AB引用
				AssetSystem.UF_GetInstance().UF_RelaseRef(atlaName);
			}
		}

		//清除全部图集
		public void UF_ClearAtla(){
			List<AssetUIAtlas> listTmp = new List<AssetUIAtlas>(m_DicAtlaSets.Values);
			m_DicAtlaSets.Clear ();
			for (int k = 0;k < listTmp.Count;k++) {
				listTmp[k].Dispose ();
				//释放AB引用
				AssetSystem.UF_GetInstance().UF_RelaseRef(listTmp[k].name);
			}
		}
			

	
		public void UF_OnAwake(){
            s_UIRoot = GameObject.Find("UI Root");
            if (s_UIRoot == null)
            {
                throw new System.Exception("UI Root Is Mssing!");
            }
            CanvasScaler scaler = s_UIRoot.GetComponent<CanvasScaler>();
            float curPer = Screen.width * FixedDeviceFactor / Screen.height;
            float resolutionX = scaler.referenceResolution.x;
            float resolutionY = scaler.referenceResolution.y;

            //Debug.Log($"{Screen.width},{Screen.height},{resolutionX},{resolutionY}==" + curPer + "==" + (resolutionX / resolutionY));

            if (curPer > (resolutionX / resolutionY))
            {
                scaler.matchWidthOrHeight = 1;
            }
            else
            {
                scaler.matchWidthOrHeight = 0;
            }

            s_UIRoot.transform.position = new Vector3(0, 1000, 0);
            s_UICamera = GameObject.Find("UI Camera").GetComponent<Camera>();
            s_UIRootTransform = s_UIRoot.GetComponent<RectTransform>();
            if (s_UICamera == null)
            {
                throw new System.Exception("UI Camera Is Mssing!");
            }

            //Add System Canvas
            this.UF_AddCanvas("UI System", 9000, 2000);
        }


		public static Vector3 UF_WorldToScreen(Vector3 worldPos){
			return UF_WorldToScreen(Camera.main,worldPos);
		}

		public static Vector3 UF_WorldToScreen(Camera camera,Vector3 worldPos){
			if (camera != null) {
				Vector3 tmpW2Screem = camera.WorldToScreenPoint (worldPos);
				Vector3 ret = s_UICamera.ScreenToWorldPoint (tmpW2Screem);
				ret.z = 0;
				return ret;
			}
			return Vector3.zero;	
		}




		//如果界面存在，则可以直接获取
		//该方法不会加载界面
		public UIView UF_GetView(string viewName){
			UIView ret = null;
			if (m_ViewStack.Count == 0) {
				return null;
			} else {
				m_ViewStack.Seek(0);
				do {
					ret = m_ViewStack.Current;
					if(ret != null && ret.name == viewName){
						return ret;
					}
				}while(m_ViewStack.MoveNext ());
			}
			return null;
		}

		//检查界面是否以及存在
		public bool UF_CheckViewExist(string viewName){
			return UF_GetView(viewName) != null;
		}


		//更具UIViewType类型，自动隐藏或显示界面
		//FULL类型覆盖WINDOW类型，ALWAYS类型不受影响
		protected void UF_ApplyDisplayViewOnStack(){
			if (m_ViewStack.Count == 0)
				return;
			m_ViewStack.Seek(0);
			bool mark = false;
			UIView view = null;
			UIView.ShowType viewType = UIView.ShowType.FULL;
            int stackCount = m_ViewStack.Count;
            do
            {
				view = m_ViewStack.Current;
				viewType = view.viewType;

                view.localPosition = new Vector3(view.localPosition.x, view.localPosition.y, stackCount * VIEW_ZOOM);

                if (mark && viewType != UIView.ShowType.ALWAYS && viewType != UIView.ShowType.CONTENT){
					view.UF_SetActive(false);
				}
				else{
					view.UF_SetActive(true);
				}
				if(viewType == UIView.ShowType.FULL){
					mark = true;
				}
                view.stackOrder = stackCount;
                
                stackCount--;
            } while(m_ViewStack.MoveNext ());
		}

		public int UF_GetTypeCountInViewStack(int vType){
			if (m_ViewStack.Count == 0)
				return 0;
			UIView.ShowType viewType = (UIView.ShowType)(vType);
			UIView view = null;
			int ret = 0;
			m_ViewStack.Seek(0);
			do {
				view = m_ViewStack.Current;
				if(view.viewType == viewType){
					ret++;
				}
			} while(m_ViewStack.MoveNext());
			return ret;
		}

        //获取顶部第一个指定类型界面
        public UIView UF_GetTypeTopInViewStack(int vType) {
            UIView view = null;
            m_ViewStack.Seek(0);
            do
            {
                view = m_ViewStack.Current;
                var st = (UIView.ShowType)(vType);
                if (view.viewType == st)
                {
                    return view;
                }
            } while (m_ViewStack.MoveNext());

            return null;
        }



        public void UF_SetStackDirty(){
			m_StackDirty = true;
		}

		private void UF_NormalizeRectTransform(RectTransform recttrans){
			if (recttrans != null) {
				recttrans.anchorMax = Vector2.one;
				recttrans.anchorMin = Vector2.zero;
				recttrans.offsetMin = Vector3.zero;
				recttrans.offsetMax = Vector3.zero;
				recttrans.localPosition = Vector3.zero;
				recttrans.localEulerAngles = Vector3.zero;
				recttrans.localScale = Vector3.one;
			}
		}

		public bool UF_AddToCanvas(string canvas,UIUpdateGroup target,bool normalize = true){
			if (target == null)
				return false;
			if (m_DicCanvas.ContainsKey (canvas)) {
				target.rectTransform.SetParent(m_DicCanvas [canvas].transform);
				target.rectTransform.SetAsLastSibling ();
				if (normalize) {
                    UF_NormalizeRectTransform(target.rectTransform);
				} else {
					target.rectTransform.localPosition = Vector3.zero;
					target.rectTransform.localEulerAngles = Vector3.zero;
					target.rectTransform.localScale = Vector3.one;
				}
				return true;
			}else{
				Debugger.UF_Error (string.Format ("Not Cantains Canvas[{0}],Add To Canvas Failed!", canvas));
				return false;
			}
		}


		/// <summary>
		/// 异步显示界面,加入到指定Canvas中
		/// </summary>
		public int UF_ShowView(string canvas,string viewName,DelegateObject callback){
			UIView value = UF_GetView(viewName);
			if (value != null) {
                UF_AddView(canvas,value);
				return 0;
			} else {
				if (m_MarkViewInLoad.Contains (viewName)) {
					Debugger.UF_Error (string.Format("View[{0}] is in Loading,can not load twince!",viewName));
					return 0;
				} else {
					m_MarkViewInLoad.Add (viewName);
				}

				return CEntitySystem.UF_GetInstance ().UF_AsyncCreate(viewName, 
					(e)=>{
						m_MarkViewInLoad.Remove(viewName);
						UIView view = e as UIView;
						if (view != null) {
                            UF_AddView(canvas,view);
						} else {
							Debugger.UF_Error (string.Format ("ASync Show View {0}] Failed !", viewName));		
						}
						if (callback != null) {
							callback (view);
						}
					}
				);
			}
		}

		/// <summary>
		/// 同步显示界面
		/// </summary>
		public UIView UF_SyncShowView(string canvas,string viewName){
			UIView view = UF_GetView(viewName); 
			if (view == null) {
				view = CEntitySystem.UF_GetInstance ().UF_Create<UIView>(viewName);
			}
			if (view != null) {
                UF_AddView(canvas, view);
			} else {
				Debugger.UF_Error (string.Format ("Sync Show View {0}] Failed !", viewName));
			}
			return view;
		}

		public void UF_AddToViewStack(UIView view){
			if (!m_ViewStack.ContainsValue (view)) {
				//压入显示栈
				m_ViewStack.Push (view);
                //根据类型自动隐藏栈中View
                UF_SetStackDirty();
			} else if(m_ViewStack.GetTop() != view){
				//调整层次
				if(m_ViewStack.SetTop(view)){
                    //根据类型自动隐藏栈中View
                    UF_SetStackDirty();
				}
			}
		}

		public bool UF_AddView(string canvas,UIView view){
			if (view == null) {
				return false;
			}
			if (!string.IsNullOrEmpty(canvas)) {
                UF_AddToCanvas(canvas,view);
			}

            UF_AddToViewStack(view);

            view.UF_OnShow();

			MessageSystem.UF_GetInstance().UF_Send(DefineEvent.E_UI_SHOW,view);

			return true;
		}




		/// <summary>
		/// 关闭UI，并从栈中移除
		/// </summary>
		public void UF_CloseView(string viewName){
			UIView view = UF_GetView(viewName);
			if (view != null) {
                UF_CloseView(view);
			}
		}

		public void UF_CloseView(UIView view){
			if (view != null) {
				//在显示栈中去除，去除栈顶中其中一个
				m_ViewStack.Remove (view);
				if (!m_ViewStack.CheckInStack(view)) {
					//这里设置UI关闭,栈中没有该UI，关闭并回收
					view.UF_OnClose();
					MessageSystem.UF_GetInstance().UF_Send(DefineEvent.E_UI_CLOSE, view.name);
				}
                UF_SetStackDirty();
			}
		}

		//关闭显示栈上的指定界面位置后的全部界面
		public void UF_CloseViewRangeTo(string viewName,bool includeSelf = false){
			UIView view = UF_GetView(viewName);
			if (view != null) {
				while (m_ViewStack.GetTop () != view && m_ViewStack.Count != 0) {
					UIView value = m_ViewStack.Pop ();
					if (!m_ViewStack.CheckInStack(value)) {
                        value.UF_OnClose();
						MessageSystem.UF_GetInstance().UF_Send(DefineEvent.E_UI_CLOSE, value.name);
					}
				}
                if (includeSelf) {
                    //是否关闭自身
                    this.UF_CloseView(view);
                }
                UF_SetStackDirty();
			} else {
				Debugger.UF_Warn (string.Format ("CloseViewRangeTo -> UIView[{0}] not Exist", viewName));
			}
		}


		//清空显示栈
		public void UF_ClearAllView(){
			while (m_ViewStack.Count > 0) {
				UIView view = m_ViewStack.Pop ();
				if (!m_ViewStack.CheckInStack(view)) {
					view.UF_OnClose();
					MessageSystem.UF_GetInstance().UF_Send(DefineEvent.E_UI_CLOSE, view.name);
				}
			}
			m_ViewStack.Clear();
		}
			
		//返回显示栈顶的界面
		public UIView UF_GetTopView(){
			return m_ViewStack.GetTop ();
		}


		/// <summary>
		/// 关闭显示栈顶界面
		/// </summary>
		public void UF_CloseTopView(){
            UF_CloseView(m_ViewStack.Pop ());
		}
			

		/// <summary>
		/// 加载一个Item
		/// </summary>
		public UIItem UF_CreateItem(string itemName){
			UIItem uiitem = CEntitySystem.UF_GetInstance ().UF_Create<UIItem> (itemName);
			if (uiitem == null) {
				Debugger.UF_Error (string.Format ("This UIItem[ {0} ] you want to create is Null", itemName));	
			}
			return uiitem;
		}


		public bool UF_CheckCanvasExist(string nameCanvas){
			return m_DicCanvas.ContainsKey (nameCanvas);
		}


		public void UF_AddCanvas(string nameCanvas,int sortingOrder,int depth){
            if (!m_DicCanvas.ContainsKey(nameCanvas))
            {
                GameObject go = new GameObject(nameCanvas, typeof(RectTransform));
                go.layer = DefineLayer.UI;
                go.transform.SetParent(s_UIRoot.transform);

                RectTransform recttrans = go.GetComponent<RectTransform>();

                UF_NormalizeRectTransform(recttrans);

                go.transform.localPosition = new Vector3(0, 0, depth);

                Canvas canvas = go.AddComponent<Canvas>();
                go.AddComponent<GraphicRaycaster>();
                canvas.overrideSorting = true;
                canvas.sortingOrder = sortingOrder;
                m_DicCanvas.Add(nameCanvas, canvas);
            }
            else {
                Debugger.UF_Warn(string.Format("UI Canvas[{0}] has already Exist", nameCanvas));
                var canvas = m_DicCanvas[nameCanvas];
                canvas.sortingOrder = sortingOrder;
                canvas.transform.localPosition = new Vector3(0, 0, depth);
            }
        }


		//世界空间的UI画布
		public void UF_AddWorldCanvas(string nameCanvas,int sortingOrder, int depth)
        {
            if (!m_DicCanvas.ContainsKey(nameCanvas))
            {
                GameObject go = new GameObject(nameCanvas, typeof(RectTransform));
                go.layer = DefineLayer.UI;

                RectTransform recttrans = go.GetComponent<RectTransform>();

                UF_NormalizeRectTransform(recttrans);

                go.transform.localPosition = new Vector3(0, 0, depth);

                recttrans.sizeDelta = s_UIRootTransform.sizeDelta;

                Canvas canvas = go.AddComponent<Canvas>();
                canvas.sortingOrder = sortingOrder;
                float slope = Camera.main.transform.eulerAngles.x;
                float CameraSizeAspt = Camera.main.orthographicSize / s_UICamera.orthographicSize;

                Vector3 localScale = s_UIRootTransform.localScale * CameraSizeAspt;
                //世界与UI X轴相反
                //localScale.x = -localScale.x;
                go.transform.localScale = localScale;

                //朝向摄像机
                recttrans.localEulerAngles = new Vector3(slope, 0,0);
                m_DicCanvas.Add(nameCanvas, canvas);
            }
            else {
                Debugger.UF_Warn(string.Format("World Canvas[{0}] has already Exist", nameCanvas));
                var canvas = m_DicCanvas[nameCanvas];
                canvas.sortingOrder = sortingOrder;
                canvas.transform.localPosition = new Vector3(0, 0, depth);
            }
		}



		//位置适配，使用该接口会改变描点等值
		//适配到指定的root框体内,默认的anchors为右下角
		public void UF_SetLocatioAdapter(RectTransform target,RectTransform root,Vector2 padding){
			if (target != null && root != null) {

				Vector2 vec = new Vector2 (0.5f, 0.5f);
				target.anchorMax = vec;
				target.anchorMin = vec;
				target.pivot = vec;

				Vector2 hlrootSize = root.sizeDelta;
				hlrootSize.x = hlrootSize.x / 2;
				hlrootSize.y = hlrootSize.y / 2;

				Vector2 tpos = target.anchoredPosition;

				Vector2 hltsize = target.sizeDelta;
				hltsize.x = hltsize.x / 2;
				hltsize.y = hltsize.y / 2;

				Vector2 fixPos = tpos;

				if(tpos.x + hltsize.x > hlrootSize.x){
					fixPos.x = hlrootSize.x - hltsize.x - padding.x;
				}
				if (tpos.x - hltsize.x < -hlrootSize.x) {
					fixPos.x = -hlrootSize.x + hltsize.x + padding.x;
				}
				if (tpos.y + hltsize.y > hlrootSize.y) {
					fixPos.y = hlrootSize.y - hltsize.y - padding.y;
				}
				if (tpos.y < -hlrootSize.y) {
					fixPos.y = -hlrootSize.y + hltsize.y + padding.y;
				}
				if (fixPos.x != tpos.x || fixPos.y != tpos.y) {
					target.anchoredPosition = fixPos;
				}
			}
		}

		public void UF_SetLocatioAdapterInRoot(RectTransform target,Vector2 padding){
            UF_SetLocatioAdapter(target, UIRootTransform, padding);
		}

        //自动适配安全区域
        public void UF_AutoFitNotchScreen() {
            if (GlobalSettings.NativeInfo != null)
            {
                float offset = GHelper.UF_ParseFloat(GlobalSettings.NativeInfo.UF_GetValue("SAFE_OFFSET", "0"));
                if (offset > 0)
                {
                    UF_SetSafeOffset(offset);
                }
            }
        }

		//设置显示安全区域
		public void UF_SetSafeOffset(float pixValue){
			Debugger.UF_Log (string.Format ("SetSafeOffset: Screem -> {0} | {1}  pixValue-> {2} ", Screen.width, Screen.height, pixValue));
            if (pixValue <= 0) return;
            //横屏左右裁剪
   //         float width = Screen.width;
			//float value = pixValue / width;
			//value = Mathf.Clamp01 (value);
			//Rect viewrect = new Rect (value, 0, Mathf.Clamp01 (1 - value * 2.0f), 1);
            //竖屏上边裁剪
            float height = Screen.height;
            float value = pixValue / height;
            value = Mathf.Clamp01(value);
            Rect viewrect = new Rect(0, 0,1, Mathf.Clamp01(1 - value));

            UICamera.rect = viewrect;
			Camera.main.rect = viewrect;

            //创建一个用于清除界面缓存的摄像机
            if (!GameObject.Find("Clear Camera")) {
                Camera clearCamera = new GameObject("Clear Camera").AddComponent<Camera>();
                clearCamera.depth = -100;
                clearCamera.clearFlags = CameraClearFlags.Skybox;
                clearCamera.cullingMask = 0;
                clearCamera.allowDynamicResolution = false;
                clearCamera.allowHDR = false;
                clearCamera.allowMSAA = false;
                clearCamera.orthographic = true;
                clearCamera.backgroundColor = Color.black;
                Debugger.UF_Log("======= Create Clear Camera Success =======");
            }
        }


		public Vector2 UF_GetUIPosInRoot(IUIUpdate iui){
			MonoBehaviour mb = iui as MonoBehaviour;
			if (mb != null) {
				return UF_GetPosInRoot(mb.transform.position);
			}
			return Vector2.zero;
		}

		//获取指定UI基于root中的真实位置
		public Vector2 UF_GetPosInRoot(Vector3 uiWorldPos){
			Vector3 screenPos = UF_GetPosInScreen(uiWorldPos);
			screenPos.x -= Screen.width / 2 ;
			screenPos.y -= Screen.height / 2;
			screenPos.x *= s_UIRootTransform.sizeDelta.x/Screen.width ;
			screenPos.y *= s_UIRootTransform.sizeDelta.y/Screen.height;
			return new Vector2 (screenPos.x, screenPos.y);
		}


		public Vector3 UF_GetPosInScreen(Vector3 uiWorldPos){
			return UICamera.WorldToScreenPoint (uiWorldPos);
		}



		public static void UF_LayoutRebuilder(RectTransform rect_trans){
			UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rect_trans);
		}
			

		public void UF_OnUpdate(){
			if (m_StackDirty) {
				m_StackDirty = false;
                UF_ApplyDisplayViewOnStack();
			}
		}


        public void UF_OnReset() {
            m_StackDirty = false;
            m_DicAtlaSets.Clear();
            m_MarkViewInLoad.Clear();
            m_ViewStack.Clear();
        }


        public override string ToString ()
		{
			System.Text.StringBuilder sb = StrBuilderCache.Acquire ();

			if (m_ViewStack != null && m_ViewStack.Count > 0) {
				sb.Append("\n========== View Stack ==========\n");
                sb.AppendLine();
                for (int k = 0; k < m_ViewStack.Count; k++) {
                    if (m_ViewStack.Elements[k] != null)
                    {
                        sb.AppendLine(string.Format("<color=green>{0} | order:{1} | tpye: <{2}></color>", m_ViewStack.Elements[k].name, m_ViewStack.Elements[k].viewOrder.ToString(), m_ViewStack.Elements[k].viewType.ToString()));
                        //检查是否有content 内容
                        List<Object> list = m_ViewStack.Elements[k].ListUpdateUI;
                        for (int t = 0; t < list.Count; t++)
                        {
                            if (list[t] is UIContent && (list[t] as UIContent).target != null)
                            {
                                //sb.AppendLine();
                                sb.AppendLine(string.Format("\t\t<color=yellow>{0}->{1}</color>", (list[t] as UIContent).updateKey, (list[t] as UIContent).target.name));
                            }
                        }
                        sb.AppendLine();
                    }
                    else
                    {
                        sb.AppendLine("<color=red> <NULL> </color>");
                    }
				}
			}

			return StrBuilderCache.GetStringAndRelease (sb);
		}
			

	}
}