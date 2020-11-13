using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityFrame;


public class EditorFXDrawBase {
	private  Dictionary<Object,Editor> MapEditor = new Dictionary<Object,Editor>();

	private Editor GetEditor(Object target){
		if (MapEditor.ContainsKey (target)) {
			return MapEditor [target];
		} else {
			Editor editor = Editor.CreateEditor (target);
			MapEditor.Add (target, editor);
		}
		return MapEditor [target];
	}

	public bool DrawFXEffects(Component fx,List<EffectBase> effects){
        bool ischange = false;
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button ("绑定效果",GUILayout.Height(30))) {
			effects.Clear ();
			foreach (var v in fx.GetComponentsInChildren<EffectBase>(true)) {
				effects.Add (v);
			}
            ischange = true;
        }
        GUI.backgroundColor = Color.white;
        for (int k = 0; k < effects.Count; k++) {
			GUILayout.Space (6);
			var effect = effects [k];
			bool click = false;
			GUI.color = Color.white;
			EditorTools.BeginContents (false);
			if (EditorTools.DrawHeaderItem (string.Format("<{0}>  {1}",effect.GetType().Name,effect.effectName), effect.GetType().Name + 1, "删除", ref click,Color.red)) {

                var effectEditor = GetEditor(effect);
                if (effectEditor != null)
                {
                    GUI.backgroundColor = new Color(0.5f, 0.8f, 1, 1);
                    effectEditor.OnInspectorGUI();
                }
            }
			EditorTools.EndContents ();
			GUI.backgroundColor = Color.white;
			if (click) {
				effects.RemoveAt (k);
				k++;
                ischange = true;
            }
		}
        return ischange;
    }

	public static bool DrawListParticles(Component fxp,List<ParticleSystem> particles){
        bool ischange = false;
        GUILayout.Space(10);
		if (GUILayout.Button ("绑定粒子",GUILayout.Height(30))) {
			particles.Clear ();
			foreach (var v in fxp.GetComponentsInChildren<ParticleSystem>(true)) {
                if(v.emission.enabled)
				    particles.Add (v);
			}
            ischange = true;
        }

		for (int k = 0; k < particles.Count; k++) {
			var particle = particles [k];
			GUILayout.Space(6);
			GUILayout.BeginHorizontal ();

			var icon = EditorTools.GetParticleRenderPreviewIcon (particle);
			if (icon != null)
			{
				GUILayout.Label(icon,GUILayout.Width(20),GUILayout.Height(20));
			}

			EditorTools.DrawPingBox (particle, particle.name,20);

			GUI.color = Color.red;
			if (GUILayout.Button("X",GUILayout.Width(20),GUILayout.Height(20))) {
				particles.RemoveAt (k);
				k--;
                ischange = true;
            }
			GUI.color = Color.white;
			GUILayout.EndHorizontal ();
		}
        return ischange;
    }


}


[CustomEditor(typeof(FXController), true)]
public class EditorFXController : Editor {

	private EditorFXDrawBase FXDrawBase = new EditorFXDrawBase();
    List<ParticleSystem> tempList = new List<ParticleSystem>();
    private int simProgress = 0;
    private bool CheckParticlePlayerExist(FXController fx) {
        foreach (var item in fx.effects) {
            if (item != null && item is EParticlePlayer) {
                return true;
            }
        }
        return false;
    }

	public override void OnInspectorGUI()
	{
		FXController fx = target as FXController;

		EditorTools.DrawEntityMark ();

		base.OnInspectorGUI ();

        //GUI.backgroundColor = Color.yellow;
        //if (!CheckParticlePlayerExist(fx) && GUILayout.Button("添加粒子组件", GUILayout.Height(30)))
        //{

        //    EParticlePlayer particlePlayer = fx.gameObject.GetComponent<EParticlePlayer>();
        //    if(particlePlayer == null)
        //        particlePlayer=fx.gameObject.AddComponent<EParticlePlayer>();
        //    fx.effects.Add(particlePlayer);
        //    particlePlayer.particles.Clear();
        //    foreach (var v in fx.GetComponentsInChildren<ParticleSystem>(true))
        //    {
        //        if (v.emission.enabled)
        //            particlePlayer.particles.Add(v);
        //    }
        //}
        //GUI.backgroundColor = Color.white;

        if (!Application.isPlaying) {
            simProgress = EditorGUILayout.IntSlider("Sample Preview",simProgress, 0,100);
            if (GUI.changed) {
                fx.GetComponentsInChildren<ParticleSystem>(false, tempList);
                foreach (var item in tempList)
                {
                    item.Simulate(simProgress / 100.0f);
                }
            }
        }

        if (GUILayout.Button("ScaleMode: Hierarchy")) {
            fx.GetComponentsInChildren<ParticleSystem>(false, tempList);
            foreach (var item in tempList) {
                item.scalingMode = ParticleSystemScalingMode.Hierarchy;
            }
        }

        GUILayout.Space(10);

        if (FXDrawBase.DrawFXEffects(fx, fx.effects)) {
			EditorTools.SetDirty (fx);
		}


	}
}


