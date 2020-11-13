using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityFrame;

[CustomEditor(typeof(UIClickOpera), true)]
public class UIClickOperaEditor : Editor
{
	private SerializedProperty m_UEClick;
	private SerializedProperty m_UEPressDown;
	private SerializedProperty m_UEPressUp;
	private SerializedProperty m_UEDoubleClick;


	public override void OnInspectorGUI ()
	{
		UIClickOpera clickOpeara = target as UIClickOpera;

		EditorTools.DrawUpdateKeyTextField (clickOpeara);

		this.serializedObject.Update ();

		EditorGUI.BeginChangeCheck ();

		EditorGUILayout.PropertyField (this.m_UEClick, new GUILayoutOption[0]);
		EditorGUILayout.PropertyField (this.m_UEPressDown, new GUILayoutOption[0]);
		EditorGUILayout.PropertyField (this.m_UEPressUp, new GUILayoutOption[0]);
		EditorGUILayout.PropertyField (this.m_UEDoubleClick, new GUILayoutOption[0]);

		if (EditorGUI.EndChangeCheck ())
			this.serializedObject.ApplyModifiedProperties ();


	}

	protected void OnEnable ()
	{
		m_UEClick = this.serializedObject.FindProperty ("m_UEClick");
		m_UEPressDown = this.serializedObject.FindProperty("m_UEPressDown");
		m_UEPressUp = this.serializedObject.FindProperty ("m_UEPressUp");
		m_UEDoubleClick = this.serializedObject.FindProperty("m_UEDoubleClick");
	}




}
