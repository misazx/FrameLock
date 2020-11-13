using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityFrame;

[CustomEditor(typeof(UIDrag), true)]
public class UIDragEditor : Editor
{

	public override void OnInspectorGUI ()
	{
		UIDrag uidrag = target as UIDrag;

		EditorTools.DrawUpdateKeyTextField (uidrag);

//		UISpriteEditor.DrawSpriteElement (uidrag.sprite);

//		base.OnInspectorGUI ();

		GUILayout.Space (10);

		drawSprite (uidrag);


	}


	public void drawSprite(UIDrag uidrag){
		GUILayout.Space (3);
		bool canDrag = EditorGUILayout.Toggle("允许拖拽",uidrag.canDrag);
		bool AutoBack = EditorGUILayout.Toggle("自动回位",uidrag.autoBack);
		float smoothBack = 0;
		if(AutoBack){
			smoothBack = EditorGUILayout.FloatField ("平滑回位",uidrag.smoothBack);
		}
		else{
			smoothBack = uidrag.smoothBack;
		}

		bool centerAligned = EditorGUILayout.Toggle("对齐中心",uidrag.centerAligned);


		string EventPressDown = EditorGUILayout.TextField ("按下事件",uidrag.ePressDown);
		string EventPressUp = EditorGUILayout.TextField ("弹起事件",uidrag.ePressUp);
		string Param = EditorGUILayout.TextField ("参数",uidrag.eParam);
		if (GUI.changed) {
			EditorTools.RegisterUndo ("UIDrag", uidrag);
			uidrag.canDrag = canDrag;
			uidrag.eParam = Param;
			uidrag.autoBack = AutoBack;
			uidrag.smoothBack = smoothBack;
			uidrag.centerAligned = centerAligned;
			uidrag.ePressUp = EventPressUp;
			uidrag.ePressDown = EventPressDown;

			EditorTools.SetDirty (uidrag);

		}

	}


}

