using UnityEngine;
using UnityEditor;
using UnityFrame;

[CustomEditor(typeof(UILabel), true)]
public class UILabelEditor : UnityEditor.UI.TextEditor
{

	public override void OnInspectorGUI ()
	{
		UILabel uilabel = target as UILabel;

		EditorTools.DrawUpdateKeyTextField (uilabel);

		base.OnInspectorGUI ();

		draw (uilabel);

	}


	public void draw(UILabel uilabel){

		if (uilabel.raycastTarget) {
			string eventTrigger = EditorGUILayout.TextField("点击事件", uilabel.ePressClick);
			string EventClickTriggerParam = EditorGUILayout.TextField("事件参数", uilabel.eParam);
			if (GUI.changed) {
				uilabel.ePressClick = eventTrigger;
				uilabel.eParam = EventClickTriggerParam;
			}
		}

		//GUILayout.Space (10);

		EditorTools.BeginContents (false);

		//Vector2 mPreferredOffset = uilabel.preferredOffset;

		//bool mUsePreferredWidth = EditorGUILayout.Toggle ("自适应宽度",uilabel.usePreferredWidth);
		//bool mUsePreferredHeight = EditorGUILayout.Toggle ("自适应高度",uilabel.usePreferredHeight);
		//if (mUsePreferredHeight || mUsePreferredWidth) {
		//	mPreferredOffset = EditorGUILayout.Vector2Field ("自适应偏移",uilabel.preferredOffset);
		//}

		bool outline = EditorGUILayout.Toggle ("Outline",uilabel.outline);
        Vector4 outlineParam = uilabel.outlineParam;
        Color outlineColor = GHelper.UF_IntToColor(uilabel.outlineColor);
        if (outline) {
            EditorGUILayout.BeginHorizontal();
            outlineParam = EditorGUILayout.Vector4Field("", uilabel.outlineParam);
            outlineColor = EditorGUILayout.ColorField(outlineColor);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
        }
		if (outline && !uilabel.supportRichText) {
			EditorGUILayout.HelpBox ("Global Outline Need To Support Rich Text",MessageType.Warning);
		}
        
        bool shadow = EditorGUILayout.Toggle("Shadow", uilabel.shadow);
        Vector2 shadowParam = uilabel.shadowParam;
        Color shadowColor = GHelper.UF_IntToColor(uilabel.shadowColor);
        if (shadow)
        {
            EditorGUILayout.BeginHorizontal();
            shadowParam = EditorGUILayout.Vector2Field("", uilabel.shadowParam);
            shadowColor = EditorGUILayout.ColorField(shadowColor);
            EditorGUILayout.EndHorizontal();
        }
        if (shadow && !uilabel.supportRichText)
        {
            EditorGUILayout.HelpBox("Global Shadow Need To Support Rich Text", MessageType.Warning);
        }

        EditorTools.EndContents ();


		if (GUI.changed) {
			EditorTools.RegisterUndo ("UILabel", uilabel);
			//uilabel.usePreferredWidth = mUsePreferredWidth;
			//uilabel.usePreferredHeight = mUsePreferredHeight;

			uilabel.outline = outline;
			uilabel.outlineParam = outlineParam;
            uilabel.outlineColor = GHelper.UF_ColorToInt(outlineColor);

            uilabel.shadow = shadow;
            uilabel.shadowParam = shadowParam;
            uilabel.shadowColor = GHelper.UF_ColorToInt(shadowColor);

            EditorTools.SetDirty (uilabel);
            uilabel.SetVerticesDirty();
            //			uilabel.Refresh ();
            return;
		}


	}
}


[CustomEditor(typeof(UIInputField), true)]
public class UIInputFieldEditor : UnityEditor.UI.InputFieldEditor
{

	public override void OnInspectorGUI ()
	{
		UIInputField inputField = target as UIInputField;

		EditorTools.DrawUpdateKeyTextField (inputField);

		base.OnInspectorGUI ();


	}


}

[CustomEditor(typeof(UIClock), true)]
public class UIClockEditor : Editor
{
	public override void OnInspectorGUI ()
	{
		UIClock clock = target as UIClock;
		EditorTools.DrawUpdateKeyTextField (clock);
		base.OnInspectorGUI ();
	}
}


[CustomEditor(typeof(UIFollow), true)]
public class UIFollowEditor : Editor
{
	public override void OnInspectorGUI ()
	{
		UIFollow follow = target as UIFollow;
		EditorTools.DrawUpdateKeyTextField (follow);
		base.OnInspectorGUI ();
	}
}

[CustomEditor(typeof(UIObject), true)]
public class UIObjectEditor : Editor
{
	public override void OnInspectorGUI ()
	{
		UIObject uiobj = target as UIObject;
		EditorTools.DrawUpdateKeyTextField (uiobj);
		base.OnInspectorGUI ();
	}
}