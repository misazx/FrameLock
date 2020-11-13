using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityFrame;


[CustomEditor(typeof(EParticlePlayer), true)]
public class EditorEParticlePlayer : Editor
{
	public override void OnInspectorGUI ()
	{
		EParticlePlayer particlePlayer = target as EParticlePlayer;
		base.OnInspectorGUI ();

		EditorFXDrawBase.DrawListParticles (particlePlayer, particlePlayer.particles);

		if (GUI.changed) {
			EditorTools.SetDirty(particlePlayer);
		}
	}
}



[CustomEditor(typeof(UITweenAlpha), true)]
public class EditorETweenUIAlpha : Editor
{

	public override void OnInspectorGUI ()
	{
        UITweenAlpha tweenalpha = target as UITweenAlpha;
		base.OnInspectorGUI ();

        if(!tweenalpha.dynamicBind)
		    EditorETweenUITools.OnDrawGUI<Graphic>(tweenalpha.gameObject, tweenalpha.listGraphic);

		if (GUI.changed) {
            if (tweenalpha.dynamicBind)
                tweenalpha.listGraphic.Clear();
            EditorTools.SetDirty(tweenalpha);
		}
	}
}

[CustomEditor(typeof(UITweenColor), true)]
public class EditorETweenUIColor : Editor
{

	public override void OnInspectorGUI ()
	{
        UITweenColor tweencolor = target as UITweenColor;
		base.OnInspectorGUI ();

		EditorETweenUITools.OnDrawGUI<Graphic> (tweencolor.gameObject, tweencolor.listGraphic);

		if (GUI.changed) {
			EditorTools.SetDirty(tweencolor);
		}
	}
}

[CustomEditor(typeof(UITweenUV), true)]
public class EditorUITweenUV : Editor
{

    public override void OnInspectorGUI()
    {
        UITweenUV tweenuv = target as UITweenUV;
        base.OnInspectorGUI();

        
        EditorETweenUITools.OnDrawGUI<RawImage>(tweenuv.gameObject, tweenuv.listGraphic);


        if (GUI.changed)
        {
            EditorTools.SetDirty(tweenuv);
        }
    }
}



public class EditorETweenUITools
{
	private static bool effectChild = false;
	private static bool includeUnActive = false;

	public static void OnDrawGUI<T>(GameObject tarobj,List<T> listGraphic) where T : Graphic
	{
		GUILayout.Space (16);

		GUILayout.BeginHorizontal ();

		GUILayout.Label ("包含子层");
		effectChild = EditorGUILayout.Toggle (effectChild);
		GUILayout.Label ("包含未激活");
		includeUnActive = EditorGUILayout.Toggle (includeUnActive);

		GUILayout.EndHorizontal ();

		if (GUILayout.Button ("绑定图像",GUILayout.Height(30))) {
            T[] mListGraphic = null;
            if (effectChild)
            {
                mListGraphic = tarobj.GetComponentsInChildren<T>(includeUnActive);
            }
            else
            {
                mListGraphic = tarobj.GetComponents<T>();
            }
            listGraphic.Clear ();
            if (mListGraphic != null) {
                listGraphic.AddRange(mListGraphic);
            }
		}
			
		for (int k = 0; k < listGraphic.Count; k++) {
			T graphic = listGraphic [k];
			GUILayout.BeginHorizontal ();
            
            GUI.color = Color.cyan;
            EditorTools.DrawPingBox (graphic,string.Format("{0} <{1}>",graphic.name,graphic.GetType().Name),20);
			GUI.color = Color.red;
			if (GUILayout.Button("X",GUILayout.Width(20),GUILayout.Height(20))) {
				listGraphic.RemoveAt (k);
				k--;
			}
			GUI.color = Color.white;
			GUILayout.EndHorizontal ();
		}

	}




}