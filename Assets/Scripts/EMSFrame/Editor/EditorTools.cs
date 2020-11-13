using UnityEngine;
using UnityEditor;
using UnityFrame;
using System.Collections.Generic;
using System.IO;

public class EditorTools
{
	static public bool DrawHeader (string text, bool forceOn, bool minimalistic)
	{
		string key = text;

		bool state = EditorPrefs.GetBool(key, false);

		if (!minimalistic) GUILayout.Space(3f);
		if (!forceOn && !state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
		GUILayout.BeginHorizontal();
		GUI.changed = false;

		if (minimalistic)
		{
			if (state) text = "\u25BC" + (char)0x200a + text;
			else text = "\u25BA" + (char)0x200a + text;

			GUILayout.BeginHorizontal();
			GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
			if (!GUILayout.Toggle(true, text, "PreToolbar2", GUILayout.MinWidth(20f))) state = !state;
			GUI.contentColor = Color.white;
			GUILayout.EndHorizontal();
		}
		else
		{
			text = "<b><size=11>" + text + "</size></b>";
			if (state) text = "\u25BC " + text;
			else text = "\u25BA " + text;
			if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;
		}

		if (GUI.changed) EditorPrefs.SetBool(key, state);

		if (!minimalistic) GUILayout.Space(2f);
		GUILayout.EndHorizontal();
		GUI.backgroundColor = Color.white;
		if (!forceOn && !state) GUILayout.Space(3f);
		return state;
	}

	static public bool DrawHeaderItem (string text,string key,string lbtn,ref bool lbclick)
	{
		return DrawHeaderItem (text, key, lbtn, ref lbclick, new Color (0.8f, 0.8f, 0.8f));
	}

	static public bool DrawHeaderItem (string text,string key,string lbtn,ref bool lbclick,Color btColor)
	{
		bool state = EditorPrefs.GetBool(key, true);

		if (!state) GUI.backgroundColor = new Color (0.8f, 0.8f, 0.8f);

		GUILayout.BeginHorizontal();
		GUI.changed = false;

		if (state) text = "\u25BC " + (char)0x200a + text;
		else text = "\u25BA " + (char)0x200a + text;

		GUILayout.BeginHorizontal();
		GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
		if (!GUILayout.Toggle(true, text, "PreToolbar2")) state = !state;
		GUI.contentColor = Color.white;

		lbclick = false;
		GUI.backgroundColor = btColor;
		if (GUILayout.Button (lbtn, GUILayout.MaxWidth(60f))) {
			lbclick = true;
		}

		GUILayout.EndHorizontal();

		if (GUI.changed) EditorPrefs.SetBool(key, state);

		GUILayout.EndHorizontal();
		GUI.backgroundColor = Color.white;
		GUILayout.Space(6);
		return state;
	}


	static bool mEndHorizontal = false;

	static public void BeginContents (bool minimalistic)
	{
		if (!minimalistic)
		{
			mEndHorizontal = true;
			GUILayout.BeginHorizontal();
			EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(10f));
		}
		else
		{
			mEndHorizontal = false;
			EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(10f));
			GUILayout.Space(10f);
		}
		GUILayout.BeginVertical();
		GUILayout.Space(2f);
	}

	static public void EndContents ()
	{
		GUILayout.Space(3f);
		GUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();

		if (mEndHorizontal)
		{
			GUILayout.Space(3f);
			GUILayout.EndHorizontal();
		}

		GUILayout.Space(3f);
	}


	static public void RegisterUndo (string name, params Object[] objects)
	{
		if (objects != null && objects.Length > 0)
		{
			UnityEditor.Undo.RecordObjects(objects, name);
			foreach (Object obj in objects)
			{
				if (obj == null) continue;
				EditorUtility.SetDirty(obj);
			}
		}
	}


	static public void SetDirty (UnityEngine.Object obj)
	{
		if (obj)
		{
            UnityEditor.EditorUtility.SetDirty(obj);

            //bool ips = PrefabUtility.IsPartOfPrefabAsset(obj);

            //var ins = PrefabUtility.GetOutermostPrefabInstanceRoot(obj);

            //Debug.Log(ins);

            //if (obj is GameObject)
            //    PrefabUtility.ApplyPrefabInstance(obj as GameObject, InteractionMode.AutomatedAction);
            //else if(obj is MonoBehaviour)
            //PrefabUtility.ApplyObjectOverride(obj, AssetDatabase.GetAssetPath(obj),InteractionMode.AutomatedAction);

            //PrefabUtility.ReplacePrefab(obj as GameObject, PrefabUtility.GetPrefabParent(obj));
            //var prefabType = PrefabUtility.GetPrefabInstanceStatus(obj);
            //if (prefabType == PrefabInstanceStatus.NotAPrefab) {

            //}

        }
	}

    static public GUIContent GetGUIContent(string name) {
        return EditorGUIUtility.IconContent(name);
    }


    //灰度预览
    static public void DrawGreyPreView(UIUpdateGroup view){
        if (view == null)
            return;
        GUILayout.Space(2);
        GUILayout.BeginHorizontal();
        GUILayout.Label("灰度预览",GUILayout.Width(115));
        GUILayout.Space(2);
        if (GUILayout.Button("色彩", GUILayout.Width(60))) {
            view.UF_SetGrey(false);
            view.UF_SetActive(false);
            view.UF_SetActive(true);
        }
        if (GUILayout.Button("灰度", GUILayout.Width(60))) {
            view.UF_SetGrey(true);
            view.UF_SetActive(false);
            view.UF_SetActive(true);
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(2);
    }



	public static string HandleCopyPaste(int controlID)
	{
		if (controlID == GUIUtility.keyboardControl)
		{
			if (Event.current.type == UnityEngine.EventType.KeyUp && (Event.current.modifiers == EventModifiers.Control || Event.current.modifiers == EventModifiers.Command))
			{
				if (Event.current.keyCode == KeyCode.C)
				{
					Event.current.Use();
					TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
					editor.Copy();
				}
				else if (Event.current.keyCode == KeyCode.V)
				{
					Event.current.Use();
					TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
					editor.Paste();
					#if UNITY_5_3_OR_NEWER || UNITY_5_3
					return editor.text; //以及更高的unity版本中editor.content.text已经被废弃，需使用editor.text代替
					#else
					return editor.content.text;
					#endif
				}
				else if (Event.current.keyCode == KeyCode.A)
				{
					Event.current.Use();
					TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
					editor.SelectAll();
				}
			}
		}
		return null;
	}

	public static string TextField(string value, params GUILayoutOption[] options)
	{
		int textFieldID = GUIUtility.GetControlID("TextField".GetHashCode(), FocusType.Keyboard) + 1;
		if (textFieldID == 0)
			return value;

		//处理复制粘贴的操作
		value = HandleCopyPaste(textFieldID) ?? value;

		return GUILayout.TextField(value, options);
	}

	public static string TextArea(string value, params GUILayoutOption[] options)
	{
		int textFieldID = GUIUtility.GetControlID("TextArea".GetHashCode(), FocusType.Keyboard) + 1;
		if (textFieldID == 0)
			return value;

		//处理复制粘贴的操作
		value = HandleCopyPaste(textFieldID) ?? value;

		return GUILayout.TextArea(value, options);
	}


	public static void DrawEntityMark(){
		GUILayout.BeginHorizontal ();
		//		GUILayout.Label(EditorGUIUtility.IconContent("lightMeter/greenLight"),GUILayout.Width(10),GUILayout.Height(15));
		GUI.color = Color.green;
		EditorGUILayout.HelpBox ("Game Entity",MessageType.None);
		GUI.color = Color.white;
		GUILayout.EndHorizontal ();
		GUILayout.Space (5);
	}


	public static void DrawUpdateKeyTextField(IUIUpdate ui){
		if (!(ui is Object))
			return;
		Color oldColor = GUI.color;
		GUI.color = new Color(0.8f,1f,0.8f,1);
		GUILayout.Space (5);
		string updateKey = EditorGUILayout.TextField ("更新健",ui.updateKey);
		GUILayout.Space (5);
		GUI.color = oldColor;
		if (GUI.changed) {
			EditorTools.RegisterUndo (ui.GetType().ToString(), ui as Object);
			ui.updateKey = updateKey;
			EditorTools.SetDirty (ui as Object);
		}
	}

	//快捷方式导航到资源位置，非常实用
	public static void PingObject(Object target){
		EditorGUIUtility.PingObject (target);
	}

	//点击区域导航到资源位置
	public static void ClickAndPingObject(Rect rect,Object target){
		if (Event.current.type == EventType.MouseDown && Event.current.clickCount==1 && rect.Contains(Event.current.mousePosition)) {
			EditorGUIUtility.PingObject (target);
		}
	}

	public static GUIContent tempGUIContent{get{ return s_TempGUIContent;}}
	static GUIContent s_TempGUIContent = new GUIContent();

	public static void DrawPingBox(Object target,string text,params GUILayoutOption[] options){
		Rect rectbox = EditorGUILayout.GetControlRect (false, 16, options);
		GUI.Label (rectbox,text,EditorStyles.textArea);
		ClickAndPingObject (rectbox, target);
	}

	public static void DrawPingBox(Object target,string text,float height,params GUILayoutOption[] options){
		Rect rectbox = EditorGUILayout.GetControlRect (false, height, options);
		GUI.Label (rectbox,text,EditorStyles.textArea);
		ClickAndPingObject (rectbox, target);
	}


	//获取预览材质图片
	public static Texture2D GetRenderPreviewIcon(Component target){
		Renderer renderer = target.GetComponent<Renderer>();
		Object instancePrev = null;
		Texture2D icon = null;
		if (renderer.sharedMaterial != null)
		{
			instancePrev = renderer.sharedMaterial;
		}

		if (instancePrev != null)
		{

			icon = AssetPreview.GetAssetPreview(instancePrev);
		}
		return icon;
	}
	//获取粒子预览图片
	public static Texture2D GetParticleRenderPreviewIcon(ParticleSystem target){
		var renderer = target.GetComponent<ParticleSystemRenderer>();
		Object instancePrev = null;
		Texture2D icon = null;
		if (renderer.renderMode == ParticleSystemRenderMode.Mesh && renderer.mesh != null) {
			instancePrev = renderer.mesh;
		}
		else if (renderer.sharedMaterial != null)
		{
			instancePrev = renderer.sharedMaterial;
		}

		if (instancePrev != null)
		{

			icon = AssetPreview.GetAssetPreview(instancePrev);
		}
		return icon;
	}


    public static void DestroyChild(GameObject root)
    {
        List<Transform> tempList = new List<Transform>();
        for (int k = 0; k < root.transform.childCount; k++)
        {
            tempList.Add(root.transform.GetChild(k));
        }
        foreach (Transform v in tempList)
        {
            Object.DestroyImmediate(v.gameObject);
        }
    }

    public static Object[] LoadAllAssetAtPath(string relpath, string extend = "")
    {

        string abspath = Application.dataPath.Replace("Assets", "") + relpath;

        List<Object> ret = new List<Object>();
        string[] filesPath = null;
        filesPath = Directory.GetFiles(abspath);

        foreach (string v in filesPath)
        {
            if (string.IsNullOrEmpty(extend) || extend.Contains(Path.GetExtension(v))) {
                string p = "Assets" + v.Replace(Application.dataPath,"");
                var asset = AssetDatabase.LoadAssetAtPath(p,typeof(Object));
                if (asset != null)
                {
                    ret.Add(asset);
                }
            }
        }
        return ret.ToArray();
    }

    public static T FindAssetAtPath<T>(string relpath, string extend = "") where T : Object {
        foreach (var v in LoadAllAssetAtPath(relpath, extend)) {
            if (v is T) {
                return v as T;
            }
        }
        return null;
    }


}

