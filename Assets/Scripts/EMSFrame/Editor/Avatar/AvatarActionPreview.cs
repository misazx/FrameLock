using UnityEngine;
using UnityEditor;
using UnityFrame;


//角色行为预览
public partial class AvatarActionPreview : PreviewWindow
{
    private AvatarController m_Avatar;

    private Animator m_Animator;

    private AvatarClipMaker m_EditorWindow;

    //绘制组件常量
    public float titleWidth = 100;
    public float heightTrack = 25;

    //动画相关
    private float m_AnimateProgress = 0;
    private float m_AnimateTriggerTick = 0;
    private float m_TimeTick = 0;
    private bool m_DragAnima = false;


    private EditorClip selectClip {
        get {
            if (m_EditorWindow != null)
                return m_EditorWindow.selectClip;
            else
                return null;
        }
    } 

    private string playClipName
    {
        get
        {
            if (selectClip != null)
                return selectClip.GetParam("clipName");
            else
                return string.Empty;
        }
    }

    private float playSpeed
    {
        get
        {
            if (selectClip != null)
                return float.Parse(selectClip.GetParam("speed", "1"));
            else
                return 1.0f;
        }
    }

    public bool isPlaying { get; private set; } = false;

    public void Repaint() {
        if (m_EditorWindow != null)
            m_EditorWindow.Repaint();
    }


    public void Start(AvatarClipMaker window)
    {
        m_EditorWindow = window;
        this.InitPreview();
    }


    public void SetTargetAvatar(AvatarController avatar) {
        if (avatar == null)
            return;
        //拷贝新的Avatar
        var clone = this.AddGameObject(avatar.gameObject, true);
        if(clone != null) {
            m_Avatar = clone.GetComponentInChildren<AvatarController>();
            //var euler = m_Avatar.transform.eulerAngles;
            //euler.y = 180;
            //m_Avatar.transform.eulerAngles = euler;
            m_Animator = clone.GetComponentInChildren<Animator>();
        }
    }


    private float GetPlayClipLength()
    {
        if (m_Animator == null)
        {
            Debug.LogError("Can not find Animator in this model");
            return 0;
        }
        if (!string.IsNullOrEmpty(playClipName)) {
            AnimationClip[] animationClips = m_Animator.runtimeAnimatorController.animationClips;
            for (int k = 0; k < animationClips.Length; k++)
            {
                if (animationClips[k].name == playClipName)
                {
                    return animationClips[k].length;
                }
            }
        }

        //使用自定义长度
        return selectClip.GetParamFloat("length");
        //Debug.LogError(string.Format("Can not find animation clip[{0}] to play!!!", clipName));
    }

    //绘制动画播放进度播放
    protected void DrawAnimationBanner(Rect rect)
    {
        float height = rect.height;
        float lenBt = titleWidth;
        float lenPrg = Mathf.Max(rect.width - titleWidth, 1);
        float triggerTime = m_AnimateProgress;

        Rect rectPlay = new Rect(rect.x, rect.y, lenBt, height);
        Rect rectProgress = new Rect(rect.x + lenBt, rect.y, lenPrg, height);
        Rect rectMarkPtr = new Rect(rect.x + lenBt + triggerTime * lenPrg, 0, 2, rect.y + height);
        Rect rectMarkPtrBlock = new Rect(rect.x + lenBt + triggerTime * lenPrg - 7, rect.y, 14, height);

        //播放按钮
        if (!isPlaying && GUI.Button(rectPlay, EditorGUIUtility.IconContent("PlayButton")))
        {
            isPlaying = true;
            m_TimeTick = Time.realtimeSinceStartup;
            //从当前位置开始播放
            m_AnimateTriggerTick = GetPlayClipLength() * m_AnimateProgress;
            ResetClipEvent(m_AnimateProgress);
        }
        if (isPlaying && GUI.Button(rectPlay, EditorGUIUtility.IconContent("PauseButton On")))
        {
            isPlaying = false;
        }

        //进度条 
        EditorGUI.DrawRect(rectProgress, new Color(0.34f,0.78f,1,0.2f));

        //刻度值
        EditorGUI.DrawRect(rectMarkPtr, Color.yellow);

        //拖拽进度条
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rectMarkPtrBlock.Contains(Event.current.mousePosition))
        {
            m_DragAnima = true;
            isPlaying = false;
            Event.current.Use();
        }
        else if (Event.current.type == EventType.MouseUp)
        {
            m_DragAnima = false;
        }
        if (m_DragAnima)
        {
            float dragDetlaPos = Mathf.Clamp(Event.current.mousePosition.x, rectProgress.x, rectProgress.x + rectProgress.width) - rectProgress.x;
            dragDetlaPos = Mathf.Clamp(dragDetlaPos, 0, rectProgress.width);
            m_AnimateProgress = Mathf.Max(dragDetlaPos / rectProgress.width, 0);
            UpdateAnimationAt(playClipName, m_AnimateProgress);
            //UpdateClipEvent(m_AnimateProgress);
        }
    }


    public bool UpdateAnimationAt(string clipName, float triggerTime)
    {
        if (m_Animator == null)
        {
            Debug.LogError("Can not find Animator in this model");
            return false;
        }
        if (m_Animator.runtimeAnimatorController == null)
        {
            Debug.LogError("Can not find Animator Controller in this model");
            return false;
        }
        AnimationClip[] animationClips = m_Animator.runtimeAnimatorController.animationClips;
        for (int k = 0; k < animationClips.Length; k++)
        {
            if (animationClips[k].name == clipName)
            {
                m_Animator.Play(clipName, 0, triggerTime);
                m_Animator.Update(triggerTime);
            }
        }
        //Debug.LogError(string.Format("Can not find animation clip[{0}] to play!!!", clipName));
        return true;
    }


    //重写绘制代码，加入动画进度条
    public override void Draw(Rect rect)
    {
        float nHeight = heightTrack;
        //绘制动画播放进度条
        Rect rectAnimaPlayer = new Rect(0, rect.y + nHeight, rect.width, nHeight);
        DrawAnimationBanner(rectAnimaPlayer);

        Rect rectPreview = new Rect(0, rectAnimaPlayer.y + nHeight, rect.width, rect.height);

        base.Draw(rectPreview);

        this.Repaint();

        UF_OnUpdate();
    }

    public override void Reset()
    {
        m_AnimateTriggerTick = 0;
        m_AnimateProgress = 0;
        isPlaying = false;
        m_DragAnima = false;

        AudioUtil.Cleanup();
        EffectUtil.Cleanup();

        base.Reset();
    }


    private void UpdateClipEvent(float triggerTime) {
        if (selectClip != null)
        {
            foreach (var e in selectClip.ListClipEvent)
            {
                if (e.triggerTime <= triggerTime && e.state == 0) {
                    e.state = 1;
                    //派发事件执行对应的逻辑
                    PerformActionClipEvent(m_Avatar, e);
                }
            }
        }

    }

    private void ResetClipEvent(float startTrigger = 0) {
        if (selectClip != null) {
            foreach (var e in selectClip.ListClipEvent) {
                if(e.triggerTime >= startTrigger)
                    e.state = 0;
                else
                    e.state = 1;
            }
        }
    }

    private void UpdateAnimationClip() {
        if (!isPlaying) return;

        float deltaTime = Time.realtimeSinceStartup - m_TimeTick;
        m_TimeTick = Time.realtimeSinceStartup;

        //if (string.IsNullOrEmpty(playClipName))
        //{
        //    isPlaying = false;
        //    Debug.LogError(string.Format("Can not find animation clip[{0}] to play!!!", playClipName));
        //    return;
        //}
        float clipLength = GetPlayClipLength();
        if (clipLength < 0.0001f)
        {
            isPlaying = false;
            Debug.LogError(string.Format("Animation clip[{0}] Length is Zero!!!", playClipName));
            return;
        }

        m_AnimateTriggerTick = m_AnimateTriggerTick + deltaTime * playSpeed;

        m_AnimateProgress = Mathf.Clamp01(m_AnimateTriggerTick / clipLength);

        UpdateClipEvent(m_AnimateProgress);

        UpdateAnimationAt(playClipName, m_AnimateProgress);

        if (m_AnimateTriggerTick >= clipLength)
        {
            m_AnimateTriggerTick = 0;
            if (selectClip.GetParam("wrapMode").Equals("Loop"))
            {
                m_AnimateProgress = 1;
                ResetClipEvent();
            }
            else {
                m_AnimateProgress = 0;
                isPlaying = false;
            }
        }
        
    }



    private void UF_OnUpdate() {

        UpdateAnimationClip();

        UpdatePreviewEffect();

    }




}
