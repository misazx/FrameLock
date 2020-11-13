using UnityEngine;
using UnityEditor;
using UnityFrame;

[CustomEditor(typeof(UIScrollSelection), true)]
public class UIScrollSelectionEditor : UnityEditor.UI.ScrollRectEditor
{

	public override void OnInspectorGUI ()
	{
		UIScrollSelection uiScrollView = target as UIScrollSelection;

		EditorTools.DrawUpdateKeyTextField (uiScrollView);

		base.OnInspectorGUI ();

		GUILayout.Space (10);

		draw (uiScrollView);



	}


	private void draw(UIScrollSelection ScrollView){
		
		UILabel content = EditorGUILayout.ObjectField ("内容",ScrollView.labelContent,typeof(UILabel),true) as UILabel;

		int startSpaceCount = EditorGUILayout.IntField ("起始空行数",ScrollView.startSpaceCount);

		int endSpaceCount = EditorGUILayout.IntField ("结尾空行数",ScrollView.endSpaceCount);

		GUILayout.Space (3);
		string uvaluechange = EditorGUILayout.TextField ("值改变事件",ScrollView.eValueChange);
		GUILayout.Space (3);
		string extraParam = EditorGUILayout.TextField ("事件额外参数",ScrollView.eParam);

		if (GUI.changed) {
			EditorTools.RegisterUndo ("UIScrollSelection", ScrollView);

			ScrollView.startSpaceCount = startSpaceCount;

			ScrollView.endSpaceCount = endSpaceCount;

			ScrollView.eValueChange = uvaluechange;

			ScrollView.eParam = extraParam;

			ScrollView.labelContent = content;

			EditorTools.SetDirty (ScrollView);


		}



	}



}

