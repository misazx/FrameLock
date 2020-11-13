using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityFrame;

public class UIToolKit{

	public static string ProjectPath{
		get{ 
			return Application.dataPath.Replace ("Assets", "");
		}
	}
//
//	[MenuItem("GameTools/UI/Build UIAtlas")]
//	public static void BuildAtla(){
//
//	}
//

	[MenuItem("GameTools/UI/Format Sprites Name")]
	public static void FormatSpriteName(){
		Object sel = Selection.activeObject;
		if (sel is DefaultAsset) {
			DefaultAsset da = sel as DefaultAsset;
			string path = ProjectPath + AssetDatabase.GetAssetPath (da);

			DirectoryInfo di = new DirectoryInfo (path);
			string prefix = di.Name;
			if (di.Exists) {
				formatSpriteFileName (di,prefix);
			}
			AssetDatabase.Refresh ();
			Debug.Log ("Format Sprites Name Finish");
		}
	}
    private static Transform SceneSelection()
    {
        return Selection.activeTransform;
    }
	private static void formatSpriteFileName(DirectoryInfo directoryInfo,string prefix){
		FileInfo[] fis = directoryInfo.GetFiles ();
		foreach (FileInfo fi in fis) {
			if(fi.Extension == ".png" || fi.Extension == ".jpeg" || fi.Extension == ".tga"){
				string fileName = fi.Name;
				int idx = fileName.IndexOf ("@");
				if (idx > -1) {
					fileName = fileName.Substring (idx+1);
				}

				string newFileFullName = directoryInfo.FullName + "/" + prefix +"@"+ fileName;
//				Debug.Log (newFileFullName);
				fi.MoveTo (newFileFullName);
			}
		}

		DirectoryInfo[] dis = directoryInfo.GetDirectories ();

		foreach(DirectoryInfo di in dis){
			formatSpriteFileName (di, prefix);
		}
	}



	static void AddToUIRoot(GameObject ui){
		if (ui == null)
			return;
        

        GameObject root = GameObject.Find ("UI Root");
		if (root == null) {
			root = new GameObject("UI Root");
			root.AddComponent < RectTransform> ();
			Canvas canvas = root.AddComponent<Canvas> ();
			canvas.pixelPerfect = true;
			canvas.planeDistance = 100;
			CanvasScaler cs = root.AddComponent<CanvasScaler> ();
			cs.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
			root.AddComponent<GraphicRaycaster> ();

			Camera cam = new GameObject ("UI Camera").AddComponent<Camera> ();
			cam.clearFlags = CameraClearFlags.Depth;
			cam.cullingMask = LayerMask.NameToLayer ("UI");
			cam.orthographicSize = 5;
			cam.orthographic = true;
			cam.farClipPlane = 100;
			cam.nearClipPlane = -100;
			cam.rect = new Rect (0, 0, 1, 1);
			cam.useOcclusionCulling = false;

			canvas.worldCamera = cam;

			cam.transform.parent = root.transform;
		}
        //如果有选择UI的时候 直接设置父物体为选择的UI
        Transform parent = SceneSelection();
        if (parent != null && parent.root != null && parent.root.transform == root.transform)
        {
            ui.transform.SetParent(parent.transform);
            ui.transform.localPosition = Vector3.zero;
            ui.transform.localScale = Vector3.one;
            ui.transform.localRotation = Quaternion.identity;
        }
        else
        {
            ui.transform.SetParent(root.transform);
            ui.transform.localScale = Vector3.one;
            ui.transform.localPosition = Vector3.zero;
            ui.transform.localRotation = Quaternion.identity;
        }

	}



	[MenuItem("GFUI/UISprite",false,10)]
    [MenuItem("GameObject/GFUI/UISprite",false,10)]
	static void Gen_GF_UISprite(){
		GameObject go = new GameObject("UISprite");
		go.layer = LayerMask.NameToLayer ("UI");
		go.AddComponent<RectTransform>().sizeDelta = new Vector2(100,100);
		UISprite uis = go.AddComponent<UISprite> ();
		uis.raycastTarget = false;
		AddToUIRoot (go);
	}


	[MenuItem("GFUI/UISpriteAnimation",false,10)]
	[MenuItem("GameObject/GFUI/UISpriteAnimation",false,10)]
	static void Gen_GF_UISpriteAnimation(){
		GameObject go = new GameObject("UISpriteAnimation");
		go.layer = LayerMask.NameToLayer ("UI");
		go.AddComponent<RectTransform>().sizeDelta = new Vector2(100,100);
		UISpriteAnimation uias = go.AddComponent<UISpriteAnimation> ();
		uias.raycastTarget = false;
		AddToUIRoot (go);
	}

    [MenuItem("GFUI/UILabel", false, 12)]
    [MenuItem("GameObject/GFUI/UILabel", false, 12)]
    static void Gen_GF_UILabel()
    {
        GameObject go = new GameObject("UILabel");

        go.layer = LayerMask.NameToLayer("UI");
        go.AddComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
        go.AddComponent<UILabel>().raycastTarget = false;
        go.GetComponent<UILabel>().supportRichText = false;
        AddToUIRoot(go);
    }


    [MenuItem("GFUI/UITexture",false,11)]
    [MenuItem("GameObject/GFUI/UITexture", false, 11)]
    static void Gen_GF_UITexture(){
		GameObject go = new GameObject("UITexture");
		go.layer = LayerMask.NameToLayer ("UI");
		go.AddComponent<RectTransform>().sizeDelta = new Vector2(100,100);
		UITexture uit = go.AddComponent<UITexture> ();
		uit.raycastTarget = false;
		AddToUIRoot (go);
	}

    [MenuItem("GFUI/UIPreview", false, 11)]
    [MenuItem("GameObject/GFUI/UIPreview", false, 11)]
    static void Gen_GF_UIPreview()
    {
        GameObject go = new GameObject("UIPreview");
        go.layer = LayerMask.NameToLayer("UI");
        go.AddComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
        UIPreview uit = go.AddComponent<UIPreview>();
        uit.raycastTarget = true;
        AddToUIRoot(go);
    }



	[MenuItem("GFUI/UIButton",false,13)]
    [MenuItem("GameObject/GFUI/UIButton", false, 13)]
    static void Gen_GF_UIButton(){
		GameObject go = new GameObject("UIButton");
		go.layer = LayerMask.NameToLayer ("UI");
		go.AddComponent<RectTransform>().sizeDelta = new Vector2(100,25);

		go.AddComponent<CanvasRenderer> ();

		UISprite uisprite = go.AddComponent<UISprite> ();
		uisprite.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite> ("UI/Skin/UISprite.psd");
		uisprite.type = Image.Type.Sliced;
		UIButton uibutton = go.AddComponent<UIButton> ();

		uibutton.targetGraphic = uisprite;


		GameObject tex = new GameObject ("Text");

		tex.transform.parent = go.transform;

		tex.layer = LayerMask.NameToLayer ("UI");
		RectTransform rectTransform = tex.AddComponent<RectTransform> ();
		rectTransform.sizeDelta = new Vector2(100,25);

		rectTransform.anchorMax = Vector2.one;
		rectTransform.anchorMin = Vector2.zero;
		rectTransform.offsetMin = Vector3.zero;
		rectTransform.offsetMax = Vector3.zero;

		UILabel uilabel = tex.AddComponent<UILabel> ();
		uilabel.text = "Button";
		uilabel.raycastTarget = false;
		uilabel.color = Color.black;
		uilabel.alignment = TextAnchor.MiddleCenter;
		AddToUIRoot (go);
	}  


	[MenuItem("GFUI/UIToggle",false,14)]
    [MenuItem("GameObject/GFUI/UIToggle", false, 14)]
    static void Gen_GF_UIToggle(){
		GameObject go = UIDefaultControls.CreateToggle (UIDefaultControls.GetStandardResources());
		go.layer = LayerMask.NameToLayer ("UI");
		AddToUIRoot (go);

	}
	[MenuItem("GFUI/UIToggleGroup",false,14)]
	[MenuItem("GameObject/GFUI/UIToggleGroup", false, 14)]
	static void Gen_GF_UIToggleGroup(){
		GameObject go = new GameObject("UIToggleGroup");
		go.AddComponent<UIDragGroup> ();
		go.layer = LayerMask.NameToLayer ("UI");
		AddToUIRoot (go);

	}

	[MenuItem("GFUI/UISlider",false,15)]
    [MenuItem("GameObject/GFUI/UISlider", false, 15)]
    static void Gen_GF_UISlider(){
		GameObject go = UIDefaultControls.CreateSlider (UIDefaultControls.GetStandardResources());
		go.layer = LayerMask.NameToLayer ("UI");
		AddToUIRoot (go);

	}

	[MenuItem("GFUI/UIInputField",false,16)]
    [MenuItem("GameObject/GFUI/UIInputField", false, 16)]
    static void Gen_GF_UIInputField(){
		GameObject go = UIDefaultControls.CreateInputField (UIDefaultControls.GetStandardResources());
		go.layer = LayerMask.NameToLayer ("UI");
		AddToUIRoot (go);

	}
	[MenuItem("GFUI/UIObject",false,17)]
    [MenuItem("GameObject/GFUI/UIObject", false, 17)]
    static void Gen_GF_UIObject(){
		GameObject gameObject = new GameObject("UIObject",typeof(RectTransform));
		gameObject.AddComponent<UIObject> ();
		AddToUIRoot (gameObject);
		gameObject.layer = LayerMask.NameToLayer ("UI");
	}

	[MenuItem("GFUI/UIModel",false,17)]
	[MenuItem("GameObject/GFUI/UIModel", false, 17)]
	static void Gen_GF_UIModel(){
		GameObject gameObject = new GameObject("UIModel",typeof(RectTransform));
		gameObject.AddComponent<UIModel> ();
		gameObject.GetComponent<BoxCollider> ().size = new Vector3 (1, 1.6f, 1);
		AddToUIRoot (gameObject);
		gameObject.transform.localScale = new Vector3 (100, 100, 100);
		gameObject.layer = LayerMask.NameToLayer ("UI");
	}


	[MenuItem("GFUI/UIModelMask",false,17)]
	[MenuItem("GameObject/GFUI/UIModelMask", false, 17)]
	static void Gen_GF_UIModelMask(){
		GameObject gameObject = new GameObject("UIModelMask",typeof(RectTransform));
		UIModelMask mask = gameObject.AddComponent<UIModelMask> ();
		mask.raycastTarget = false;
		mask.color = new Color (0, 0, 0, 0.3f);
		AddToUIRoot (gameObject);
		gameObject.transform.localPosition += new Vector3 (0, 0, -10);
		gameObject.layer = LayerMask.NameToLayer ("UI");
	}





	[MenuItem("GFUI/UIItem",false,18)]
    [MenuItem("GameObject/GFUI/UIItem", false, 18)]
    static void Gen_GF_UIItem(){
		GameObject gameObject = new GameObject("UIItem",typeof(RectTransform));
		gameObject.AddComponent<UIItem> ();
        AddToUIRoot(gameObject);
		gameObject.layer = LayerMask.NameToLayer ("UI");
	}


	[MenuItem("GFUI/UIScrollView",false,19)]
	[MenuItem("GameObject/GFUI/UIScrollView", false, 19)]
	static void Gen_GF_UIScrollView(){
		GameObject g_scrollview = new GameObject("UIScrollView",typeof(RectTransform));
		GameObject g_Grid = new GameObject ("grid",typeof(RectTransform));
		g_Grid.transform.parent = g_scrollview.transform;

		RectTransform rect_scrollview = g_scrollview.GetComponent<RectTransform>();
		RectTransform rect_grid = g_Grid.GetComponent<RectTransform>();

		rect_scrollview.anchorMax = new Vector2 (1, 1);
		rect_scrollview.anchorMin = new Vector2 (0, 0);
		rect_scrollview.pivot = new Vector2 (0.5f, 0.5f);

		rect_grid.anchorMax = new Vector2 (0, 1);
		rect_grid.anchorMin = new Vector2 (0, 1);
		rect_grid.pivot = new Vector2 (0.5f, 1f);

		g_scrollview.AddComponent<Image> ().color = new Color (1, 1, 1, 0.5f);
		g_scrollview.AddComponent<Mask> ();
		UIScrollView uisv = g_scrollview.AddComponent<UIScrollView> ();
        RectTransform uisvtf = uisv.GetComponent<RectTransform>();

        uisvtf.anchorMin = new Vector2(0.5f, 0.5f);
        uisvtf.anchorMax = new Vector2(0.5f, 0.5f);
        uisvtf.pivot = new Vector2(0.5f, 0.5f);

		uisv.content = rect_grid;
		uisv.horizontal = false;
		uisv.viewport = rect_scrollview;

		g_Grid.AddComponent<UIGrid> ();

        AddToUIRoot(g_scrollview);
		g_scrollview.layer = LayerMask.NameToLayer ("UI");    

    }

	[MenuItem("GFUI/UIScrollSelection",false,20)]
	[MenuItem("GameObject/GFUI/UIScrollSelection", false, 20)]
	static void Gen_GF_UIScrollSelection(){
		GameObject g_scrollview = new GameObject("UIScrollSelection",typeof(RectTransform));

		GameObject g_Grid = new GameObject ("content",typeof(RectTransform));

		GameObject g_select = new GameObject ("select",typeof(RectTransform));

		g_select.transform.parent = g_scrollview.transform;

		g_Grid.transform.parent = g_scrollview.transform;



		RectTransform rect_scrollview = g_scrollview.GetComponent<RectTransform>();
		RectTransform rect_select = g_select.GetComponent<RectTransform>();
		RectTransform rect_grid = g_Grid.GetComponent<RectTransform>();

		rect_select.sizeDelta = new Vector2 (100, 20);
		rect_select.anchoredPosition = new Vector2 (0, 0);
		rect_select.anchorMin = new Vector2 (0.5f, 0.5f);
		rect_select.anchorMax = new Vector2 (0.5f, 0.5f);

		Image image = g_select.AddComponent<UISprite> ();
		image.color = new Color (0, 0, 0, 0.4f);
		image.raycastTarget = false;

		rect_grid.pivot = new Vector2 (0.5f, 1.0f);

		rect_scrollview.sizeDelta = new Vector2 (100, 111);

		UILabel content = g_Grid.AddComponent<UILabel> ();
		content.raycastTarget = false;
		content.alignment = TextAnchor.UpperCenter;
		content.verticalOverflow = VerticalWrapMode.Overflow;
		content.supportRichText = false;
		content.fontSize = 20;

		content.text = "\n\n1\n2\n3\n4\n5\n6\n7\n8\n9\nA\nB\nC\nD\nE\nF\n\n";

		rect_scrollview.anchorMax = new Vector2 (1, 1);
		rect_scrollview.anchorMin = new Vector2 (0, 0);
		rect_scrollview.pivot = new Vector2 (0.5f, 0.5f);

		rect_grid.anchorMax = new Vector2 (0.5f, 1.0f);
		rect_grid.anchorMin = new Vector2 (0.5f, 1.0f);
		rect_grid.pivot = new Vector2 (0.5f, 1f);
		rect_grid.anchoredPosition = Vector2.zero;


		g_scrollview.AddComponent<Image> ().color = new Color (1, 1, 1, 0.5f);
		g_scrollview.AddComponent<Mask> ();
		UIScrollSelection uisv = g_scrollview.AddComponent<UIScrollSelection> ();
		uisv.movementType = ScrollRect.MovementType.Elastic;
		uisv.vertical = true;
		uisv.horizontal = false;
		uisv.labelContent = content;
		uisv.startSpaceCount = 2;
		uisv.endSpaceCount = 2;
		RectTransform uisvtf = uisv.GetComponent<RectTransform>();

		uisvtf.anchorMin = new Vector2(0.5f, 0.5f);
		uisvtf.anchorMax = new Vector2(0.5f, 0.5f);
		uisvtf.pivot = new Vector2(0.5f, 0.5f);

		uisv.content = rect_grid;
		uisv.horizontal = false;
		uisv.viewport = rect_scrollview;


		ContentSizeFitter csf = g_Grid.AddComponent<ContentSizeFitter> ();
		csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
		csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			

		AddToUIRoot(g_scrollview);

		g_scrollview.layer = LayerMask.NameToLayer ("UI");    

	}



	[MenuItem("GFUI/UIDropdown",false,18)]
	[MenuItem("GameObject/GFUI/UIDropdown", false, 18)]
	static void Gen_GF_UIDropdown(){
		GameObject go = UIDefaultControls.CreateDropdown (UIDefaultControls.GetStandardResources());
		go.layer = LayerMask.NameToLayer ("UI");
		AddToUIRoot (go);
	}


	static GameObject CreateViewObject(string name){
		GameObject gameObject = new GameObject (name, typeof(RectTransform));
		RectTransform component = gameObject.GetComponent<RectTransform> ();
		component.sizeDelta = Vector2.zero;
		component.anchorMin =(Vector2.zero);
		component.anchorMax =(Vector2.one);
		component.anchoredPosition =(Vector2.zero);		component.sizeDelta =(Vector2.zero);
		return gameObject;

	}


	[MenuItem("GFUI/UIView",false,18)]
	[MenuItem("GameObject/GFUI/UIView", false, 18)]
	static void Gen_GF_UIView(){
		GameObject ui_view = CreateViewObject ("ui_view");
		GameObject ui_mask = CreateViewObject ("mask");
		GameObject ui_content = CreateViewObject ("view");
		ui_mask.transform.parent = ui_view.transform;
		ui_content.transform.parent = ui_view.transform;
		ui_view.layer = LayerMask.NameToLayer ("UI");

		ui_view.AddComponent<UIView> ();

		UISprite image = ui_mask.AddComponent<UISprite> ();
		image.color = new Color (0, 0, 0, 0.4f);

		AddToUIRoot (ui_view);

		ui_view.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
	}


	[MenuItem("GFUI/UIDrag",false,21)]
	[MenuItem("GameObject/GFUI/UIDrag", false, 14)]
	static void Gen_GF_UIDrag(){
		GameObject go = new GameObject("UIDrag");
		go.AddComponent<UIDrag> ();
		go.layer = LayerMask.NameToLayer ("UI");
		AddToUIRoot (go);
	}
	[MenuItem("GFUI/UIDragGroup",false,21)]
	[MenuItem("GameObject/GFUI/UIDragGroup", false, 14)]
	static void Gen_GF_UIDragGroup(){
		GameObject go = new GameObject("UIDragGroup");
		go.AddComponent<UIDragGroup> ();
		go.layer = LayerMask.NameToLayer ("UI");
		AddToUIRoot (go);
	}


	[MenuItem("GFUI/UIDrawingBoard",false,21)]
	[MenuItem("GameObject/GFUI/UIDrawingBoard", false, 14)]
	static void Gen_GF_UIDrawingBoard(){
		GameObject go = new GameObject("UIDrawingBoard");
		go.AddComponent<UIDrawBoard> ();
		RectTransform component = go.GetComponent<RectTransform> ();
		component.sizeDelta = new Vector2 (400, 300);
		go.layer = LayerMask.NameToLayer ("UI");
		AddToUIRoot (go);
	}



}
