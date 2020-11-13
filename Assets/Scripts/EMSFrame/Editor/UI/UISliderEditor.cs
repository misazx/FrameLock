using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityFrame;

[CustomEditor(typeof(UISlider), true)]
public class UISliderEditor : UnityEditor.UI.SliderEditor
{


	public override void OnInspectorGUI ()
	{
		UISlider uislider = target as UISlider;

		EditorTools.DrawUpdateKeyTextField (uislider);

//		EditorTools.BeginContents (false);
		base.OnInspectorGUI ();

		GUILayout.Space (10);

		draw (uislider);

//		EditorTools.EndContents ();


	}


	private void draw(UISlider slider){
		bool uselimit = EditorGUILayout.ToggleLeft ("使用范围限制",slider.useLimitValue);
		float limitMin = 0;
		float limitMax = 0;

		if (uselimit) {
			limitMin = EditorGUILayout.FloatField ("限制最小值",slider.limitMinValue);
			limitMax = EditorGUILayout.FloatField ("限制最大值",slider.limitMaxValue);
		} else {
			limitMin = slider.limitMinValue;
			limitMax = slider.limitMaxValue;
		}


        UILabel label = (UILabel)EditorGUILayout.ObjectField("添加文本进度",slider.textInfo,typeof(UILabel), true);
        GUILayout.Space (3);
        if (label != null) {

            GUILayout.BeginHorizontal ();
            GUILayout.Label ("显示模式");
            UISlider.TextInfoStyle style = (UISlider.TextInfoStyle)EditorGUILayout.EnumPopup ("",slider.textInfoStyle, GUILayout.MinWidth (40));
            GUILayout.EndHorizontal ();

            float scale = EditorGUILayout.FloatField("缩放值", slider.scaleInfo);

            if (GUI.changed) {
                EditorTools.RegisterUndo ("UISlider", slider);
                slider.scaleInfo = scale;
                slider.textInfoStyle = style;
                EditorTools.SetDirty (slider);
            }
        }
			
		string uvaluechange = EditorGUILayout.TextField ("值改变事件",slider.eValueChange);
		GUILayout.Space (3);
		string extraParam = EditorGUILayout.TextField ("值改变参数",slider.eParam);
		GUILayout.Space (3);


		if (GUI.changed) {
			EditorTools.RegisterUndo ("UISlider", slider);

            slider.textInfo = label;

			slider.eValueChange = uvaluechange;

			slider.eParam = extraParam;

			slider.useLimitValue = uselimit;
			slider.limitMinValue = limitMin;
			slider.limitMaxValue = limitMax;


			EditorTools.SetDirty (slider);


		}



	}



}