using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityFrame;

[CustomEditor(typeof(UnityFrame.UISpriteAnimation), true)]
public class UISpriteAnimationEditor : UISpriteEditor
{


	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();

		UISpriteAnimation uisprite = target as UISpriteAnimation;

		GUILayout.Space (10);
		EditorTools.BeginContents (false);

		drawSprite (uisprite);

		EditorTools.EndContents ();

	}



	public void drawSprite(UISpriteAnimation uisprite){



		bool active = EditorGUILayout.Toggle ("激活",uisprite.active);

		bool IsPlayOnActive = EditorGUILayout.Toggle ("激活时播放",uisprite.isPlayOnActive);

		bool IgnoreTimeScale =  EditorGUILayout.Toggle ("忽略时间缩放",uisprite.ignoreTimeScale);

		UISpriteAnimation.AnimatedModeType type = (UISpriteAnimation.AnimatedModeType)EditorGUILayout.EnumPopup ("播放模式",uisprite.animatedModeType);

		float Interval = EditorGUILayout.FloatField ("速率", uisprite.interval);

		float delay = EditorGUILayout.FloatField ("延迟播放", uisprite.delay);


		if (GUI.changed) {
			EditorTools.RegisterUndo ("UISpriteAnimation", uisprite);

			uisprite.active = active;
			uisprite.ignoreTimeScale = IgnoreTimeScale;
			uisprite.animatedModeType = type;
			uisprite.interval = Interval;
			uisprite.delay = delay;
			uisprite.isPlayOnActive = IsPlayOnActive;

			EditorTools.SetDirty (uisprite);

		}

	}


}

