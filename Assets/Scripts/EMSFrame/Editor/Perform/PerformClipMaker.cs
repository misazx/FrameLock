using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Internal;
using System.Text;
using UnityFrame;
using UnityFrame.Assets;


public class PerformActionClipMaker : EditorClipMaker
{

    public AssetPerformAction Target;


    public void Show(AssetPerformAction target)
    {
        Target = target;
        if (Target == null)
            return;
        //初始化数据
        this.InitData();
        base.Show();
    }



    private void InitData() {
        if (Target == null) { return; }
        this.ListClip.Clear();
        foreach (var clip in Target.listClips)
        {
            EditorClip eclip = new EditorClip();
            eclip.name = clip.name;
            eclip.SetParam("detialName", clip.detialName);
            eclip.SetParam("param", clip.param);
            eclip.SetParam("loop", clip.loop.ToString());
            eclip.SetParam("length", clip.length.ToString());
            foreach (var clipevent in clip.clipEvents)
            {
                EditorClipEvent ece = new EditorClipEvent();
                ece.name = clipevent.name;
                ece.triggerTime = clipevent.trigger;
                ece.SetParam("rate", clipevent.rate.ToString());
                ece.SetParam("param", clipevent.param);
                eclip.ListClipEvent.Add(ece);
            }
            this.ListClip.Add(eclip);
        }
    }



    //保存是更新值
    protected override void OnSave()
    {
        if (Target == null) { return; }
        Target.listClips.Clear();
        foreach (var clip in this.ListClip)
        {
            PerformActionClip aclip = new PerformActionClip();
            aclip.name = clip.name;
            aclip.detialName = clip.GetParam("detialName");
            aclip.param = clip.GetParam("param");
            aclip.length = clip.GetParamFloat("length");
            aclip.loop = clip.GetParamBool("loop");

            foreach (var clipevent in clip.ListClipEvent)
            {
                ClipEvent ace = new ClipEvent();
                ace.name = string.IsNullOrEmpty(clipevent.name) ? PerformActionClipEventType.Null.ToString() : clipevent.name;
                ace.param = clipevent.GetParam("param");
                ace.rate = clipevent.GetParamInt("rate");
                ace.trigger = clipevent.triggerTime;
                aclip.clipEvents.Add(ace);
            }
            // 必须按照trigger 时间进行列表排序
            aclip.clipEvents.Sort((a, b) => { return a.trigger < b.trigger ? -1 : 1; });
            Target.listClips.Add(aclip);
        }

        // 保存Avatar
        
        EditorTools.SetDirty(Target);
        EditorTools.RegisterUndo("Perform Package Change", Target);
        AssetDatabase.SaveAssets();
    }


    //映射颜色
    protected override void OnOpen()
    {
        colorSets.Clear();
        //映射颜色
        foreach (var item in EditorActionClipTool.ListActionClipDefine)
        {
            this.UF_SetColorSet(item.eventType, item.color);
        }
    }



    protected override void OnDrawClipInfo(Rect rectArea)
    {
        
        base.OnDrawClipInfo(rectArea);
        GUILayout.Space(5);
        DrawClipDetial();
        GUILayout.Space(5);
        DrawClipEventDetial();
    }


    private void DrawClipDetial() {
        if (selectClip == null) return;
        var clip = selectClip;

        EditorTools.BeginContents(false);
        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
        GUI.contentColor = Color.white;
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.Label("表现名:", GUILayout.Width(50));
        string node_name = EditorGUILayout.TextField(clip.name, GUILayout.MinWidth(30));
        GUILayout.EndHorizontal();

        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.Label("备注名:", GUILayout.Width(50));
        string detialName = EditorGUILayout.TextField(clip.GetParam("detialName"), GUILayout.MinWidth(30));
        GUILayout.EndHorizontal();

        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.Label("循环播放:", GUILayout.Width(50));
        bool loop = EditorGUILayout.Toggle(clip.GetParamBool("loop"), GUILayout.MinWidth(30));
        GUILayout.EndHorizontal();

        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.Label("播放时长:", GUILayout.Width(50));
        float length = EditorGUILayout.FloatField(clip.GetParamFloat("length"), GUILayout.MinWidth(30));
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        GUI.backgroundColor = Color.clear;
        GUILayout.Label("参数:", GUILayout.Width(50));
        GUI.backgroundColor = Color.white;
        string param = GUILayout.TextArea(clip.GetParam("param"));
        GUILayout.EndHorizontal();


        EditorTools.EndContents();

        if (GUI.changed)
        {
            clip.name = node_name;
            clip["detialName"] = detialName;
            clip["length"] = length.ToString();
            clip["param"] = param;
            clip["loop"] = loop.ToString();

            this.MarkModified();
        }
    }

    private void DrawClipEventDetial()
    {
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
     
    private void DrawClipEventDetial(EditorClipEvent node)
    {
        EditorTools.BeginContents(false);

        var tempval = string.IsNullOrEmpty(node.name) ? PerformActionClipEventType.Null.ToString() : node.name;
        PerformActionClipEventType eventType = (PerformActionClipEventType)System.Enum.Parse(typeof(PerformActionClipEventType), tempval);


        //		GUI.backgroundColor = Color.cyan;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(EditorGUIUtility.IconContent("ViewToolOrbit"), GUILayout.Width(25), GUILayout.Height(14)))
        {
            if (selectClip != null)
            {
                EditorClipEvent target = selectClip.SetAtFirst(node);
                if (target != null)
                {
                    SetSelectClipEvent(target);
                }
                this.Repaint();
            }
        }
        GUI.backgroundColor = this.GetColorSet(eventType.ToString());
        eventType = (PerformActionClipEventType)EditorGUILayout.EnumPopup("", eventType, GUILayout.Height(16));
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
        GUILayout.EndHorizontal();

        GUI.contentColor = Color.yellow;
        GUILayout.Label("事件描述: " + EditorActionClipTool.GetDeafultDesc(eventType.ToString()));
        GUI.contentColor = Color.white;

        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.Label("触发概率:", GUILayout.Width(50));
        int rate = EditorGUILayout.IntField(node.GetParamInt("rate"), GUILayout.MinWidth(30));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUI.backgroundColor = Color.white;


        string param = node.GetParam("param");
        if (param == "")
        {
            param = EditorActionClipTool.GetDefaultParams(eventType.ToString());
        }
        if (GUILayout.Button("默认值"))
        {
            param = (EditorActionClipTool.GetDefaultParams(node.name));
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(2);

        GUILayout.BeginHorizontal();
        //		GUI.backgroundColor = Color.clear;
        GUILayout.TextArea(EditorActionClipTool.GetDefaultDescParam(eventType.ToString()), GUILayout.Width(60));
        //		GUI.backgroundColor = Color.cyan;


        param = EditorGUILayout.TextArea(param);

        GUILayout.EndHorizontal();

        EditorTools.EndContents();

        if (GUI.changed)
        {
            node.name = eventType.ToString();
            node["param"] = param;
            node["rate"] = rate.ToString();
            this.MarkModified();
        }
    }


}
