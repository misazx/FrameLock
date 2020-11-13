using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityFrame;

[CustomEditor(typeof(AvatarController), true)]  
public class EditorAvatarController : Editor {

	private string GetDefaultParams(string pType){
		return EditorActionClipTool.GetDefaultParams (pType);
	}

	private int GetParamCount(string pType){
		return EditorActionClipTool.GetParamCount (pType);
	}

	private string linkAnimParamset(string[] paramSet){
		if(paramSet == null){
			return "";
		}
		else{
			string str = "";
			for(int k = 0;k < paramSet.Length;k++){
				if(k == paramSet.Length - 1){
					str += paramSet[k];
				}
				else{
					str += paramSet[k] + ";";
				}
			}
			return str;
		}
	}

	private static AvatarController mTargetEditor;

	public static AvatarController TargetEditor{get{return mTargetEditor;}}

	public override void OnInspectorGUI ()
	{
		mTargetEditor = target as AvatarController;
		GUILayout.Space(6);
		//base.OnInspectorGUI();
		EditorGUIUtility.labelWidth = 110f;
        
        Draw_AvatarCapsule();
		Draw_MarkPoint();
		Draw_MoveMotion();

		Draw_Renderer ();
        Draw_AnimationMotion();

    }

	//
	void OnSceneGUI(){
		AvatarController avatar = target as AvatarController;
		GUI.color = Color.yellow;
		GUI.skin.label.alignment = TextAnchor.MiddleCenter;
		for (int k = 0; k < avatar.markPoint.listMarkPoint.Count; k++) {
			if (avatar.markPoint.listMarkPoint[k] != null) {
				Handles.Label(avatar.markPoint.listMarkPoint[k].transform.position, avatar.markPoint.listMarkPoint[k].name);
				Handles.DrawWireCube (avatar.markPoint.listMarkPoint[k].transform.position,Vector3.one * 0.1f);
            }
		}
		GUI.color = Color.white;
	}


	private string getTextureName(AvatarController avatar){
		if(avatar != null){
			Renderer[] renders = avatar.GetComponentsInChildren<Renderer>();
			if(renders != null){
				for(int k = 0;k < renders.Length;k++){
					Texture tex = renders[k].sharedMaterial.GetTexture("_MainTex");
					if(tex != null){
						return tex.name;
					}
				}
			}
		}
		return avatar.name;
	}

//    private static string[] DeafultMarkPoint = {"GD_Left", "GD_Right","GD_Back"};


    public static bool AddToMarkPoint(AvatarController controller){
//        List<string> fitter = new List<string>(DeafultMarkPoint);
        bool ret = false;

        Transform[] trans = controller.gameObject.GetComponentsInChildren<Transform>(); 
        for (int k = 0; k < trans.Length; k++) {
            GameObject go = trans[k].gameObject;
			List<GameObject> list = controller.markPoint.listMarkPoint;
//            if (!list.Contains(go) && fitter.Contains(go.name)) {
			if (!list.Contains(go) && go.name.IndexOf("MP_") > -1) {
                list.Add(go);
                ret = true;
            }
        }

        return ret;
    }




	protected void Draw_MarkPoint(){
		AvatarController avatar = target as AvatarController;
		if (EditorTools.DrawHeader("角色挂点",false,false))
		{
			EditorTools.BeginContents(false);
            if (GUILayout.Button("自动添加")) {

                if (AddToMarkPoint(avatar)) {
                    EditorTools.SetDirty(avatar);
                    return;
                }
            }
			GameObject Obj_markPoint_buffer = null;
			Obj_markPoint_buffer = (GameObject)EditorGUILayout.ObjectField("添加挂点",Obj_markPoint_buffer,typeof(GameObject),true);
			bool isExist = false;
			for(int k = 0;k < avatar.markPoint.listMarkPoint.Count;k++){
                if (avatar.markPoint.listMarkPoint[k] == null)
                {
                    avatar.markPoint.listMarkPoint.RemoveAt(k);
                    k--;
                    continue;
                }
                if (Obj_markPoint_buffer != null && avatar.markPoint.listMarkPoint[k] == Obj_markPoint_buffer) {
                    isExist = true;
                }
                GUILayout.BeginHorizontal();
                GUILayout.Space(6);
                GUI.backgroundColor = new Color(0.8f, 0.5f, 0);
                
				if (GUILayout.Button (avatar.markPoint.listMarkPoint [k].name, GUILayout.Width (150))) {
					EditorTools.PingObject (avatar.markPoint.listMarkPoint [k]);
				}
				GUI.backgroundColor = Color.red;
                if (GUILayout.Button("X", GUILayout.Width(20))) {
                    //删除挂点
                    if (GUI.changed) {
                        EditorTools.RegisterUndo("AvatarController Change", avatar);
                        GameObject markPoint = avatar.markPoint.listMarkPoint[k];
                        avatar.markPoint.listMarkPoint.Remove(markPoint);
                        EditorTools.SetDirty(avatar);
                        k--;
                    }
                }
				GUI.backgroundColor = Color.white;
                GUILayout.EndHorizontal();

			}

			if(!isExist){
				if(Obj_markPoint_buffer != null){
					if(GUI.changed){
						EditorTools.RegisterUndo("AvatarController Change", avatar);
						avatar.markPoint.listMarkPoint.Add((GameObject)Obj_markPoint_buffer);
						EditorTools.SetDirty(avatar);
					}
				}
			}
				


			GUI.backgroundColor = Color.white;
			EditorTools.EndContents();
		}
	}


	protected void Draw_AvatarCapsule(){
		
		AvatarController avatar = target as AvatarController;

		if (EditorTools.DrawHeader("角色胶囊体",false,false))
		{
			EditorTools.BeginContents(false);

			Vector3 center = EditorGUILayout.Vector3Field("偏移中心",avatar.capsule.center);
			float rad = EditorGUILayout.FloatField("半径", avatar.capsule.radius, GUILayout.Width(170));
			float hei = EditorGUILayout.FloatField("高度", avatar.capsule.height, GUILayout.Width(170));
			float bound_rad = EditorGUILayout.FloatField("约束半径", avatar.capsule.boundRadius, GUILayout.Width(170));

			//GUILayout.BeginHorizontal();
			//GUILayout.EndHorizontal();

			if (GUI.changed)
			{	
				EditorTools.RegisterUndo("AvatarController Change", avatar);
				avatar.capsule.center = center;
				avatar.capsule.radius = rad;
				avatar.capsule.height = hei;
				avatar.capsule.boundRadius = bound_rad;
				EditorTools.SetDirty(avatar);
			}
			EditorTools.EndContents();
		}

	}

	protected void Draw_MoveMotion(){
		GUI.backgroundColor = Color.white;
		if (EditorTools.DrawHeader("角色运动",false,false))
		{
			EditorTools.BeginContents(false);
			AvatarController avatar = target as AvatarController;
			float speed = EditorGUILayout.FloatField("移动速度",avatar.motion.speedMove,GUILayout.Width(170));
            float speedTurn = EditorGUILayout.FloatField("转向速度", avatar.motion.speedTurn, GUILayout.Width(170));
            float bounds = EditorGUILayout.FloatField("距离偏差",avatar.motion.distanceBounds, GUILayout.Width(170));


			if (GUI.changed)
			{	
				EditorTools.RegisterUndo("AvatarController Change", avatar);
				avatar.motion.speedMove = speed;
                avatar.motion.speedTurn = speedTurn;
                avatar.motion.distanceBounds = bounds;

				EditorTools.SetDirty(avatar);
			}
			EditorTools.EndContents();
		}
	}



	private void init_animationNode(AvatarController avatar){
		Animation animation = avatar.GetComponentInChildren<Animation> ();
		if(animation != null){
			foreach(AnimationState aState in animation){
				bool exist = false;
				foreach(var tNode in avatar.animator.listClips )
				{
					if(aState.clip.name == tNode.name){
						exist = true;
						break;
					}
				}
				if(!exist){
					EditorTools.RegisterUndo("AvatarController Change", avatar);
					string nodeName = aState.clip.name.Substring (aState.clip.name.LastIndexOf ('_')+1);
					var newClip = new AnimatorClip (nodeName);
					newClip.length = aState.clip.length;
					newClip.clipName = aState.clip.name;
					avatar.animator.listClips.Insert(0,newClip);
					EditorTools.SetDirty(avatar);
				}
			}
		}
	}


	private void updateAvatarAnimationParam(AvatarController avatar){
		var m_curAnimation = avatar.animator.currentClip;

		Animation animation = avatar.GetComponent<Animation> ();

		if(m_curAnimation == null || animation == null){
			return;
		} 
		AnimationState animaState = animation[m_curAnimation.clipName];
		if(animaState == null){return;}
		//set lenght
		m_curAnimation.length = animaState.clip.length;
		//set speed 
		m_curAnimation.speed = m_curAnimation.speed;
		//set speed
		animaState.speed = m_curAnimation.speed;

	}


	protected float floatParse(string param,float deafult){
		try{
			return float.Parse(param);
		}
		catch{
			return deafult;
		}
	}



	protected void Draw_Renderer(){
		//AvatarController avatar = target as AvatarController;
		//if (EditorTools.DrawHeader("角色渲染",false,false))
		//{
		//	EditorTools.BeginContents(false);
		//	GUILayout.Label ("偏向颜色",GUILayout.Width(100));
		//	Color color = EditorGUILayout.ColorField (avatar.render.preferColor);
		//	if (GUI.changed) {
		//		EditorTools.RegisterUndo("AvatarController Change", avatar);
		//		avatar.render.preferColor = color;
		//		EditorTools.SetDirty(avatar);
		//		return;
		//	}


		//	GUI.backgroundColor = Color.white;
		//	EditorTools.EndContents();
		//}

	}

		
//    private static EditorAAEventMaker mEditorAAEventMaker;
	protected void Draw_AnimationMotion(){
		AvatarController avatar = target as AvatarController;

        if (EditorTools.DrawHeader("角色动画", false, false)) {
            EditorTools.BeginContents(false);

            GUI.color = Color.green;

            if (GUILayout.Button("打开角色动作事件编辑器", GUILayout.Height(40)))
            {
                var maker = EditorWindow.GetWindow<AvatarClipMaker>();

                if (maker.Target == avatar)
                {
                    maker.Focus();
                }
                else
                {
                    maker.Close();
                    EditorWindow.GetWindow<AvatarClipMaker>(string.Format("编辑动作事件({0})", avatar.name), true).Show(avatar);
                }
            }
            GUI.color = Color.white;

            EditorTools.EndContents();
        }



	}



















}
