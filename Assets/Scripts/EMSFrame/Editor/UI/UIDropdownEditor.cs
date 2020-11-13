using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityFrame;

[CustomEditor(typeof(UIDropdown), true)]
public class UIDropdownEditor : UnityEditor.UI.DropdownEditor
{


	public override void OnInspectorGUI ()
	{
		UIDropdown dropdown = target as UIDropdown;

		EditorTools.DrawUpdateKeyTextField (dropdown);

		base.OnInspectorGUI ();

		GUILayout.Space (10);

		draw (dropdown);


	}




	private void draw(UIDropdown slider){
		string uvaluechange = EditorGUILayout.TextField ("值改变事件",slider.eValueChange);
		GUILayout.Space (3);
		string extraParam = EditorGUILayout.TextField ("事件额外参数",slider.eParam);

		if (GUI.changed) {
			EditorTools.RegisterUndo ("UIDropdown", slider);

			slider.eValueChange = uvaluechange;

			slider.eParam = extraParam;

			EditorTools.SetDirty (slider);


		}



	}



}