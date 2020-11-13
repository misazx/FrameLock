using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityFrame;

[CustomEditor(typeof(UnityFrame.UIDrawBoard), true)]
public class UIDrawBoardEditor : UnityEditor.UI.RawImageEditor
{

	public override void OnInspectorGUI ()
	{
		UIDrawBoard drawBoard = target as UIDrawBoard;

		EditorTools.DrawUpdateKeyTextField (drawBoard);

		GUILayout.Space (10);

		drawSprite (drawBoard);

	}  
	 

	public void drawSprite(UIDrawBoard drawBoard){

		GUI.color = Color.white;
		GUILayout.Space (3);
		float PenSize = EditorGUILayout.FloatField ("笔刷大小", drawBoard.penSize);
		GUILayout.Space (3);
		Color PenColor = EditorGUILayout.ColorField ("笔刷颜色", drawBoard.penColor);

//		float PenHard = EditorGUILayout.FloatField ("笔刷硬度", drawBoard.PenHard);
		GUILayout.Space (3);
		float PenPressure = EditorGUILayout.FloatField ("笔尖压感", drawBoard.penPressure);
		GUILayout.Space (3);
		float MiniPenSize = EditorGUILayout.FloatField ("最小笔触", drawBoard.miniPenSize);

		GUILayout.Space (3);
		Color BackGroundColor = EditorGUILayout.ColorField ("画布颜色", drawBoard.backgroundColor);
		GUILayout.Space (3);
		string EventPenPressDown = EditorGUILayout.TextField ("按下事件",drawBoard.ePressDown);
		GUILayout.Space (3);
		string EventPenPressUp = EditorGUILayout.TextField ("弹起事件",drawBoard.ePressUp);
		GUILayout.Space (3);
		string Param = EditorGUILayout.TextField ("参数",drawBoard.eParam);


		GUI.color = Color.green;
		if (GUILayout.Button ("清空画板",GUILayout.Height(36))) {
			if (Application.isPlaying) {
				drawBoard.Clear ();
			} else {
				Debug.LogError ("DrawingBoard Should Be Cleared In Application Runing");
			}
		}
		GUI.color = Color.white;

		if (GUI.changed) {
			EditorTools.RegisterUndo ("UIDrawBoard", drawBoard);

			drawBoard.penSize = PenSize;
			drawBoard.penColor = PenColor;
//			drawBoard.PenHard = PenHard;
			drawBoard.penPressure = PenPressure;
			drawBoard.miniPenSize = MiniPenSize;
			drawBoard.backgroundColor = BackGroundColor;

			drawBoard.ePressDown = EventPenPressDown;
			drawBoard.ePressUp = EventPenPressUp;
			drawBoard.eParam = Param;

			EditorTools.SetDirty (drawBoard);

		}

	}


}

