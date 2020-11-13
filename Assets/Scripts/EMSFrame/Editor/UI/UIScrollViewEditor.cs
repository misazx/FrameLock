using UnityEngine;
using UnityEditor;
using UnityFrame;

[CustomEditor(typeof(UIScrollView), true)]
public class UIScrollViewEditor : UnityEditor.UI.ScrollRectEditor
{

	public override void OnInspectorGUI ()
	{
		UIScrollView uiScrollView = target as UIScrollView;
		EditorTools.DrawUpdateKeyTextField (uiScrollView);
		base.OnInspectorGUI ();
	}
		

}

