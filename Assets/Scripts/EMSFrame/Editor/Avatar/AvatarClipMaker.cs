using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Internal;
using System.Text;
using UnityFrame;
using UnityFrame.Assets;


public class AvatarClipMaker : EditorClipMaker
{
	public AvatarController Target;

    private AvatarActionPreview m_AvatarActionPreview = new AvatarActionPreview();
    private Animator m_PreviewModelAnimator = null;

    private int m_ClipClickTick = 0;
	private bool m_IsPreviewStyle = false;

    public void Show(AvatarController target){
		Target = target;
		if (Target == null)
			return;
		//初始化数据
		InitData();
		base.Show ();
	}

	protected void InitData(){
		if (Target == null) {return;}
		this.ListClip.Clear ();
        var animatorListClips = Target.animator.listClips;

        foreach (var clip in animatorListClips)
        {
			EditorClip eclip = new EditorClip ();
			eclip.name = clip.name;
			eclip.SetParam ("clipName", clip.clipName);
			eclip.SetParam ("param", clip.param);
			eclip.SetParam ("length", clip.length.ToString());
			eclip.SetParam ("speed", clip.speed.ToString());
			eclip.SetParam ("fadeFactor", clip.fadeFactor.ToString());
			eclip.SetParam ("wrapMode", clip.wrapMode.ToString());
			eclip.SetParam ("crossMode", clip.crossMode.ToString());
			foreach (var clipevent in clip.clipEvents) {
				EditorClipEvent ece = new EditorClipEvent ();
				ece.name = clipevent.name;
				ece.triggerTime = clipevent.trigger;
				ece.SetParam ("param", clipevent.param);
				eclip.ListClipEvent.Add (ece);
			}
			this.ListClip.Add (eclip);
		}
	}


	//保存是更新值
	protected override void OnSave ()
	{
		if (Target == null) {return;}
        var animatorListClips = Target.animator.listClips;

        animatorListClips.Clear ();
        
        foreach (var clip in this.ListClip) {
			AnimatorClip aclip = new AnimatorClip ();
			aclip.name = clip.name;
			aclip.clipName = clip.GetParam ("clipName");
			aclip.param = clip.GetParam ("param");
			aclip.length = clip.GetParamFloat ("length");
			aclip.speed = clip.GetParamFloat ("speed");
			aclip.fadeFactor = clip.GetParamFloat("fadeFactor");

			aclip.UF_SetWrapMode(clip.GetParam ("wrapMode",WrapMode.Once.ToString()));
			aclip.UF_SetCrossMode(clip.GetParam ("crossMode",AnimatorClip.CrossMode.Direct.ToString()));

			foreach (var clipevent in clip.ListClipEvent) {
				ClipEvent ace = new ClipEvent ();
				ace.name = string.IsNullOrEmpty(clipevent.name) ? AvatarClipEventType.Null.ToString() : clipevent.name;
				ace.param = clipevent.GetParam ("param");
				ace.trigger = clipevent.triggerTime;
				aclip.clipEvents.Add (ace);
			}
			// 必须按照trigger 时间进行列表排序
			aclip.clipEvents.Sort((a,b)=>{return a.trigger < b.trigger ? -1:1;});

            animatorListClips.Add (aclip);
		}

		// 保存Avatar
        EditorTools.SetDirty(Target);
        EditorTools.RegisterUndo ("AvatarController Action Change", Target);
	}
		


	protected override void OnOpen ()
	{
		colorSets.Clear ();
        //映射颜色
        foreach (var item in EditorActionClipTool.ListActionClipDefine) {
            this.UF_SetColorSet(item.eventType, item.color);
        }
	}

	protected override void OnDrawClipInfo (Rect rectArea)
	{
		base.OnDrawClipInfo (rectArea);
		GUILayout.Space (5);
		DrawClipDetial ();
		GUILayout.Space (5);
		DrawSelectedNode ();
	}
		

	protected override void OnClickClip (EditorClip target)
	{
		int interval = Mathf.Abs (System.Environment.TickCount - m_ClipClickTick);
		m_ClipClickTick = System.Environment.TickCount;
		if (interval < 200) {
			ChangePreviewDetialStyle ();
		}
	}

	//打开预览面板模式
	protected void ChangePreviewDetialStyle(){

        m_AvatarActionPreview.Start(this);

        m_IsPreviewStyle = !m_IsPreviewStyle;

        if (m_IsPreviewStyle) {
            m_AvatarActionPreview.SetTargetAvatar(Target);

            if (selectClip != null) {
                m_AvatarActionPreview.UpdateAnimationAt(selectClip.GetParam ("clipName"), 0);
			}

			this.ScrollTop ();
		} else {
            m_AvatarActionPreview.Reset();
		}
		this.Repaint ();
	}



	//重写 tracks 只需要绘制选择的指定轨道即可
	//底部加入预览界面
	protected override void OnDrawClipTracks (Rect rectArea)
	{
		if (m_IsPreviewStyle && selectClip != null) {
            Rect rectClip = new Rect(0, 0, WinWidthTrack, heightTrack);
            selectClip.Draw(rectClip, this);
            
            DrawSelectClip ();
			DrawSelectClipEvent ();

            //绘制预览窗口
            Rect prevewRect = new Rect(0, 0, WinWidthTrack + widthHead / 2, this.position.height);
            m_AvatarActionPreview.Draw (prevewRect);

		} else {
			base.OnDrawClipTracks (rectArea);
		}
	}
		

	protected void DrawClipDetial(){
		if (selectClip == null)
			return;
		var clip = selectClip;

		GUILayout.BeginHorizontal ();

		GUI.backgroundColor = m_IsPreviewStyle ? Color.red : Color.green;
		if (GUILayout.Button (m_IsPreviewStyle ? "关闭预览编辑" : "打开预览编辑",GUILayout.Height(30))) {
			ChangePreviewDetialStyle ();
		}
		GUI.backgroundColor = Color.white;

		GUILayout.EndHorizontal ();

			
		if (EditorTools.DrawHeader ("轨道属性", false, false)) {
			EditorTools.BeginContents(false);
			GUI.skin.label.alignment = TextAnchor.MiddleLeft;
			GUI.contentColor = Color.white;
			GUILayout.Space (5);
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("节点名:", GUILayout.Width (50));
			string node_name = EditorGUILayout.TextField (clip.name, GUILayout.MinWidth (30));
			GUILayout.EndHorizontal ();

			GUILayout.Space (5);
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("动作名:", GUILayout.Width (50));
			string clipName = EditorGUILayout.TextField (clip.GetParam("clipName"), GUILayout.MinWidth (30));
			GUILayout.EndHorizontal ();

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("默认长度");
            float length = EditorGUILayout.FloatField(clip.GetParamFloat("length"));
            GUILayout.EndHorizontal();

            GUILayout.Space (5);
			GUILayout.Label ("播放速度:");
			float speed = EditorGUILayout.Slider ("", clip.GetParamFloat("speed"), 0.0f, 5.0f);

            GUILayout.Space (5);
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("播放模式");
			WrapMode wrapMode = (WrapMode)System.Enum.Parse (typeof(WrapMode), clip.GetParam ("wrapMode", WrapMode.Once.ToString ()));
			wrapMode = (WrapMode)EditorGUILayout.EnumPopup ("",wrapMode, GUILayout.MinWidth (40));
			GUILayout.EndHorizontal ();

			GUILayout.Space (5);
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("混合模式");
			var evcrossMode = clip.GetParam("crossMode",AnimatorClip.CrossMode.Direct.ToString ());
			AnimatorClip.CrossMode crossMode = (AnimatorClip.CrossMode )System.Enum.Parse(typeof(AnimatorClip.CrossMode),evcrossMode);
			crossMode = (AnimatorClip.CrossMode )EditorGUILayout.EnumPopup ("",crossMode, GUILayout.MinWidth (40));
			float fadeFactor = EditorGUILayout.FloatField ("", clip.GetParamFloat("fadeFactor"), GUILayout.Width (40));
			GUILayout.EndHorizontal ();

			GUILayout.Space (5);

			EditorGUILayout.LabelField (EditorActionClipTool.ActionExtraParam.desc);
			GUILayout.BeginHorizontal ();
			GUI.backgroundColor = Color.clear;
			string descParam = EditorActionClipTool.ActionExtraParam.descParam;
			GUILayout.TextArea (descParam,GUILayout.Width(60));
			GUI.backgroundColor = Color.white;
			string param = GUILayout.TextArea (clip.GetParam("param"));
			GUILayout.EndHorizontal (); 

			EditorTools.EndContents ();

			if (GUI.changed) {
				clip.name = node_name;
				clip["clipName"] = clipName;
				clip["speed"] = speed.ToString();
                clip["length"] = length.ToString();
                clip["param"] = param;
				clip["fadeFactor"] = fadeFactor.ToString();
				clip["crossMode"] = crossMode.ToString();
				clip["wrapMode"] = wrapMode.ToString();
                
                this.MarkModified();
            }
				
		}
		
	}


	protected void DrawSelectedNode(){
		if (this.selectListClipEvent.Count > 1) {
            for (int k = 0; k < this.selectListClipEvent.Count; k++) {
                if (this.selectListClipEvent[k] != null) {
                    DrawClipEventDetial(this.selectListClipEvent[k]);
                }
            }

		}
		else if(this.selectClipEvent != null){
			DrawClipEventDetial (selectClipEvent);
		}
	}

	private void DrawClipEventDetial(EditorClipEvent node){
		EditorTools.BeginContents(false);

		var tempval = string.IsNullOrEmpty(node.name) ? AvatarClipEventType.Null.ToString() : node.name;
		AvatarClipEventType eventType = (AvatarClipEventType)System.Enum.Parse (typeof(AvatarClipEventType),tempval);


		//		GUI.backgroundColor = Color.cyan;
		GUILayout.BeginHorizontal();
		if (GUILayout.Button (EditorGUIUtility.IconContent("ViewToolOrbit"), GUILayout.Width (25),GUILayout.Height(14))) {
			if (selectClip != null) {
				EditorClipEvent target = selectClip.SetAtFirst (node);
				if (target != null) {
					SetSelectClipEvent (target);
				}
				this.Repaint ();
			}
		}
		GUI.backgroundColor = this.GetColorSet(eventType.ToString());
		eventType = (AvatarClipEventType)EditorGUILayout.EnumPopup ("", eventType,GUILayout.Height(16));
        var scolor = GUI.backgroundColor;
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("x", GUILayout.Height(14), GUILayout.Width(20)))
        {
            if (selectClip != null)
            {
                selectClip.BeginChange();
                selectClip.ListClipEvent.Remove(selectClipEvent);
                selectClip.EndChange();
                this.ClearSelectClipEvent();
                this.MarkModified();
                if (selectListClipEvent.Count > 0)
                    mSelectClipEvent = selectListClipEvent[0];
            }
        }
        GUI.backgroundColor = scolor;
        GUILayout.EndHorizontal ();

		GUI.contentColor = Color.yellow;
		GUILayout.Label ("事件描述: "+ EditorActionClipTool.GetDeafultDesc (eventType.ToString ()));
		GUI.contentColor = Color.white;
		GUILayout.BeginHorizontal ();
		GUI.backgroundColor = Color.white;

		string param = node.GetParam("param");
		if (param == "") {
			param = EditorActionClipTool.GetDefaultParams (eventType.ToString ());
		}
		if (GUILayout.Button ("默认值")) {
			param =  (EditorActionClipTool.GetDefaultParams (node.name));
		}

		GUILayout.EndHorizontal ();

		GUILayout.Space (2);

		GUILayout.BeginHorizontal ();
		//		GUI.backgroundColor = Color.clear;
		GUILayout.TextArea(EditorActionClipTool.GetDefaultDescParam(eventType.ToString ()),GUILayout.Width(60));
		//		GUI.backgroundColor = Color.cyan;


		param = EditorGUILayout.TextArea(param);

		GUILayout.EndHorizontal ();

		EditorTools.EndContents ();

		if (GUI.changed) {
            node.name = eventType.ToString();
			node["param"] = param;
            this.MarkModified();
        }
	}

	protected override void OnDestroy(){
		if (this.IsModified && EditorUtility.DisplayDialog ("警告", "当前编辑数据尚未保存", "保存并退出", "退出")) {
			OnSave ();
		}
        m_PreviewModelAnimator = null;
		base.OnDestroy ();
		m_AvatarActionPreview.Dispose ();
	}


}


