using UnityEngine;
using UnityEditor;
using UnityFrame;
using System.Collections.Generic;
using System.Collections;

public class UIEditroCommon
{

	public static void DrawUIUpdatable(IUIUpdate uiupdatable){
		if (uiupdatable == null)
			return;
		
		string ukey = EditorGUILayout.TextField ("更新健",uiupdatable.updateKey);

		if (GUI.changed) {
			uiupdatable.updateKey = ukey;
		}

	}


	public static void DrawUIEventClickable(UnityEngine.Object obj,UIEventClicker MsgClickable){
		Color source = GUI.color;
		GUILayout.Space (3);
		string upressclickkey = EditorGUILayout.TextField ("点击事件",MsgClickable.ePressClick);
        GUILayout.Space(3);
		string doubleclickkey = EditorGUILayout.TextField("双击事件", MsgClickable.ePressDClick);
        GUILayout.Space (3);
		string upressdownkey = EditorGUILayout.TextField ("按下事件",MsgClickable.ePressDown);
		GUILayout.Space (3);
		string uupdownkey = EditorGUILayout.TextField ("弹起事件",MsgClickable.ePressUp);
		GUILayout.Space (3);
		string utsound = EditorGUILayout.TextField ("触发声音",MsgClickable.eSound);
		GUILayout.Space (3);

		int count = MsgClickable.eParams.Length;
		string[] extendParam = new string[count];
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("扩展参数");
		GUI.color = new Color(0.5f,1f,0.5f,1);
		if (GUILayout.Button ("添加")) {
			List<string> cache = new List<string> ();
			for (int k = 0; k < count; k++) {
				cache.Add (MsgClickable.eParams [k]);
			}
			cache.Add ("");
			MsgClickable.eParams = cache.ToArray ();
			extendParam = cache.ToArray();
		}
		GUILayout.EndHorizontal ();
		GUILayout.Space (5);
		for (int k = 0; k < MsgClickable.eParams.Length; k++) {
			GUILayout.BeginHorizontal ();
			extendParam [k] = EditorGUILayout.TextField (MsgClickable.eParams [k]);
			GUI.color = Color.red;
			if (GUILayout.Button ("X")) {
				List<string> cache = new List<string> ();
				for (int j = 0; j < MsgClickable.eParams.Length; j++) {
					if (j != k) {
						cache.Add (MsgClickable.eParams [j]);
					}
				}
				extendParam = cache.ToArray ();
				break;
			}
			GUI.color = new Color(0.85f,1f,0.85f,1);
			GUILayout.EndHorizontal ();
		}
		GUI.color = source;
		if (GUI.changed) {
			EditorTools.RegisterUndo ("UIButton", obj);

			MsgClickable.ePressClick = upressclickkey;
			MsgClickable.ePressDClick = doubleclickkey;
			MsgClickable.ePressDown = upressdownkey;
			MsgClickable.ePressUp = uupdownkey;
			MsgClickable.eParams = extendParam;
			MsgClickable.eSound = utsound;

			EditorTools.SetDirty (obj);

		}

	}


}

