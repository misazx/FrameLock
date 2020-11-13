using UnityEngine;
using UnityEditor;
using UnityFrame;

[CustomEditor(typeof(UIToggle), true)]
public class UIToggleEditor : UnityEditor.UI.ToggleEditor
{

	private SerializedProperty m_UEValueTrue;
	private SerializedProperty m_UEValueFalse;

    public override void OnInspectorGUI ()
	{
		UIToggle uitoggle = target as UIToggle;

		EditorTools.DrawUpdateKeyTextField (uitoggle);

		base.OnInspectorGUI ();

		if (EditorTools.DrawHeader ("逻辑绑定", false, false)) {
			this.serializedObject.Update ();
			EditorGUI.BeginChangeCheck ();
			EditorGUILayout.PropertyField (this.m_UEValueTrue, new GUILayoutOption[0]);
			EditorGUILayout.PropertyField (this.m_UEValueFalse, new GUILayoutOption[0]);
			if (EditorGUI.EndChangeCheck ())
				this.serializedObject.ApplyModifiedProperties ();
			GUILayout.Space (10);
		}

		if (EditorTools.DrawHeader ("事件/参数", false, false)) {
			UIEditroCommon.DrawUIEventClickable (uitoggle, uitoggle.eventClicker);
		}

	}

	protected override void OnEnable ()
	{
		base.OnEnable ();
		m_UEValueTrue = this.serializedObject.FindProperty ("m_UEValueTrue");

		m_UEValueFalse = this.serializedObject.FindProperty("m_UEValueFalse");
    }
		

}

