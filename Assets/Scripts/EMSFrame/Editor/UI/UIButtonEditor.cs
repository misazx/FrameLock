using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityFrame;

[CustomEditor(typeof(UIButton), true)]
public class UIButtonEditor : UnityEditor.UI.ButtonEditor
{


	public override void OnInspectorGUI ()
	{
		UIButton ub = target as UIButton;

		EditorTools.DrawUpdateKeyTextField (ub);

		base.OnInspectorGUI ();

		GUILayout.Space (10);

		drawSprite (ub);

	}



	public void drawSprite(UIButton uibutton){

		if(EditorTools.DrawHeader ("事件/参数", false, false)){
			GUILayout.Space (3);
			bool UseIngoreRaycastOnClick = EditorGUILayout.Toggle ("点击时使用射线穿透", uibutton.ingoreMask);
			UIEditroCommon.DrawUIEventClickable (uibutton, uibutton.eventClicker);

            if (GUI.changed) {
				EditorTools.RegisterUndo ("UIButton", uibutton);
				uibutton.ingoreMask = UseIngoreRaycastOnClick;
				EditorTools.SetDirty (uibutton);

			}
		}
	}


}

