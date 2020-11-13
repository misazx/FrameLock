using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Internal;
using System.Text;
using UnityFrame;


public class EditorClipMaker : EditorWindow {
    public List<EditorClip> ListClip = new List<EditorClip>();

    //param define
    public int heightHead = 20;
    public int widthHead = 10;
    public int heightTrack = 25;

    public int lineWidth = 1;
    public int spaceRow = 6;
    public int minTrackHeight = 600;

    public int splitCount = 10;
    public float titleWidth = 100;

    public float WinWidth { get { return this.position.width; } }
    public float WinHeight { get { return this.position.height; } }
    public float WinWidthInfo = 300;
    public float WinWidthTrack { get { return this.position.width - this.WinWidthInfo; } }
    public float WinTotalTrackHeight = 1000;

    private Rect rectSelectClipEventRate;

    //color define
    public Color colorBoard = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    public Color colorDefault = new Color(1f, 1f, 1f, 0.5f);
    public Color colorBoardHead = new Color(0.5f, 0.5f, 1f, 0.5f);
    public Color colorBoardHeadModify = new Color(1f, 0.5f, 0.5f, 0.5f);
    public Color colorBoardLine = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    public Color colorClipSelect = new Color(0.5f, 1f, 0.5f, 0.618f);
    public Color colorClipEventSelect = new Color(0, 1, 0, 0.4f);
    public Color colorClipEventSelectBoard = new Color(1, 1, 1, 0.618f);
    public Color colorClipEventSelectLine = new Color(1, 1, 0, 0.618f);

    //标识变更
    private bool mMarkPChange = false;
    //标识修改
    protected bool mMarkModified = false;

    public bool IsModified { get { return mMarkModified; } protected set { mMarkModified = value; } }

    private string mTempSearch = "";

    protected Dictionary<string, Color> colorSets = new Dictionary<string, Color>();

    //member
    bool splitMarkDrag = false;
    bool mIsHook = true;

    protected bool mIsShowing = false;
    //scorll view
    protected Vector2 scrollMaker = Vector2.zero;

    protected EditorClip mSelectClip;

    protected EditorClipEvent mSelectClipEvent;

    protected List<EditorClipEvent> selectListClipEvent = new List<EditorClipEvent>();

    public EditorClip selectClip { get { return mSelectClip; } }

    public EditorClipEvent selectClipEvent{get{return mSelectClipEvent;}}

    public Color GetColorSet(string key){
		if (string.IsNullOrEmpty(key))
			return colorDefault;
		if (colorSets.ContainsKey (key)) {
			return colorSets[key];
		} else {
			return colorDefault;
		}
	}
	public void UF_SetColorSet(string key,Color value){
		if (colorSets.ContainsKey (key)) {
			colorSets [key] = value;
		} else {
			colorSets.Add (key, value);
		}
	}


	protected void OnGUI(){

		WinTotalTrackHeight = (ListClip.Count + 1) * (heightTrack + spaceRow) - spaceRow * 2;

		if (WinTotalTrackHeight < this.position.height) WinTotalTrackHeight = this.position.height;

		Rect rect = new Rect (0, heightHead, WinWidth, WinHeight);

		//draw head
		OnDrawClipHead ();

		GUILayout.BeginArea (rect);

		scrollMaker = GUILayout.BeginScrollView (scrollMaker,false,true);

		//draw track on right
		DrawAreaClipTracks ();

		//draw info on right
		DrawAreaClipInfo ();

		GUILayout.EndScrollView ();

		GUILayout.EndArea ();
	}

    public void ClearSelectClipEvent()
    {
        if (mSelectClipEvent != null) {
            selectListClipEvent.Remove(mSelectClipEvent);
        }
        mSelectClipEvent = null;
    }

    public void ClearSelectClip() {
        mSelectClip = null;
    }


	protected void FixSelecter(){
		EditorClip newSelectClip = null;
		EditorClipEvent newSelectClipEvent = null;
		if (mSelectClip != null) {
			foreach (EditorClip eclip in ListClip) {
				if (eclip == mSelectClip) {
					newSelectClip = eclip;
					if (mSelectClipEvent != null) {
						foreach (EditorClipEvent eClipEvent in eclip.ListClipEvent) {
							if (mSelectClipEvent == eClipEvent) {
								newSelectClipEvent = eClipEvent;
							}
						}
					}
				}
			}
		}
		mSelectClip = newSelectClip;
		mSelectClipEvent = newSelectClipEvent;
		if (mSelectClip != null && mSelectClipEvent != null) {
			SetSelectListClipEvent (mSelectClip, mSelectClipEvent);
		}
	}
		
	//tracker on left
	private void DrawAreaClipInfo(){
		GUI.color = Color.white;
		GUI.contentColor = Color.white;
		GUI.backgroundColor = Color.white;
		float y = mIsHook ? 0 : scrollMaker.y;
		Rect rectArea = new Rect (WinWidthTrack + 10,y,WinWidthInfo - widthHead * 2,WinTotalTrackHeight);
		GUILayout.BeginArea (rectArea);

		OnDrawClipInfo (rectArea);
		GUILayout.EndArea ();
	}

	//info on right
	private void DrawAreaClipTracks(){
		GUILayout.Label ("",GUILayout.Width(1),GUILayout.Height(WinTotalTrackHeight));
		Rect rectArea = new Rect (0,0,WinWidthTrack + widthHead/2,WinTotalTrackHeight - heightHead);
		GUILayout.BeginArea (rectArea);
		OnDrawClipTracks (rectArea);
		GUILayout.EndArea ();

	}


	virtual protected void OnDrawClipHead(){
		GUI.color = Color.white;
		GUI.skin.label.alignment = TextAnchor.MiddleCenter;
		int oldSize = GUI.skin.label.fontSize;
		GUI.skin.label.fontSize = 14;
		float linesp = (WinWidthTrack - titleWidth) / (float)splitCount;

		Rect rectHead = new Rect (0,0,WinWidthTrack+ widthHead/2,heightHead);

		var colorhead = mMarkModified ? colorBoardHeadModify:colorBoardHead;
		EditorGUI.DrawRect (rectHead,colorhead);

		GUI.color = mMarkModified ? Color.green:Color.white;
		Rect rectBtSave = new Rect (WinWidthTrack + widthHead,0, 60,heightHead - 1);
		if (GUI.Button (rectBtSave, "Save")) {OnSave ();mMarkModified = false;this.Repaint(); GUI.changed = false; return;}
		GUI.color = Color.white;

		Rect rectBthook = new Rect (rectBtSave.x + rectBtSave.width + 5,rectBtSave.y,rectBtSave.width,rectBtSave.height);
		GUILayout.Space (5);
		GUI.color = mIsHook ? Color.white : Color.yellow;
		if (GUI.Button (rectBthook,"hook")) {mIsHook = !mIsHook;}
		GUI.color = Color.white;

		for (int k = 0; k < splitCount+1; k++) {
			Rect glrect = new Rect (titleWidth + k * linesp - 1, 0, 0, 0);
			GUI.Label (new Rect(glrect.x-20,0,40,heightHead),string.Format("{0}", System.Math.Round(k / (float)splitCount, 2) * 100));
		}

		Rect rect = new Rect (0, 0, WinWidth, 1);
		Rect dragWRect = new Rect (rect.width - WinWidthInfo-5, rect.y, 10, heightHead);
		if (Event.current.type == EventType.MouseDown && Event.current.button == 0) {
			if (dragWRect.Contains (Event.current.mousePosition)) {
				splitMarkDrag = true;

				Event.current.Use ();
			}
		}
		else  if (Event.current.type == EventType.MouseUp){
			splitMarkDrag = false;
		}
		if (splitMarkDrag) {
			float dragDetlaPos = rect.width - (Mathf.Clamp (Event.current.mousePosition.x, rect.x, rect.x + rect.width) - rect.x);
			dragDetlaPos = Mathf.Clamp (dragDetlaPos, 100, rect.width-100);
			WinWidthInfo = (int)dragDetlaPos;
			this.Repaint ();
		} 

		DrawSelectClipeEventRate ();

		GUI.skin.label.fontSize = oldSize;
	}

    //绘制右上面板显示的信息
	virtual protected void OnDrawClipInfo(Rect rectArea){
		DrawSearchBand ();
	}

    //绘制左测轨道面板
	virtual protected void OnDrawClipTracks(Rect rectArea){
		DrawGuideLine (new Rect(0,0,rectArea.width,rectArea.height),colorBoard);
		DrawTracks ();
		DrawCtlOptMeau (rectArea);
		DrawSelectClip ();
		DrawSelectClipEvent ();
	}


	protected void ScrollTop(){
		scrollMaker.y = 0;
	}


	protected bool DrawSearchBand(){
		GUILayout.Space (5);
		GUILayout.BeginHorizontal ();


		if (GUILayout.Button (EditorGUIUtility.IconContent("ViewToolZoom"), GUILayout.Width (40),GUILayout.Height(22))) {
			for (int k = 0; k < ListClip.Count; k++) {
				var item = ListClip[k];
				if (item != null && item.name == mTempSearch) {
					SetSelectClip (item);
					SetSelectClipEvent (null);
					//set Index to 
					scrollMaker.y = k *(spaceRow + spaceRow);
					this.Repaint ();
					return true;
				}
			}
		}
		mTempSearch = GUILayout.TextField (mTempSearch,GUILayout.Height(22));
		GUILayout.EndHorizontal ();
		return false;
	}


	//绘制控制操作下来列表
	protected void DrawCtlOptMeau(Rect rectArea){
		if (Event.current.type == EventType.ContextClick && rectArea.Contains(Event.current.mousePosition))
		{
			GenericMenu menu = new GenericMenu ();
			GenericMenu.MenuFunction2 callAddTrack = delegate(object userData) {
				this.BeginChange();
				EditorClip clip = new EditorClip ();
				clip.name = "new clip";
				this.ListClip.Add (clip);
				this.EndChange();
			};

			menu.AddItem (new GUIContent ("Add Track"), false, callAddTrack, null);
			// Now create the menu, add items and show it
			if (menu != null) {
				menu.ShowAsContext ();
			}
			Event.current.Use();
		}
	}


	protected void DrawTracks(){
		for(int k = 0;k < ListClip.Count;k++){
			Rect rect = new Rect (0,k * (heightTrack+spaceRow),WinWidthTrack,heightTrack);
			if(ListClip [k] != null)
			ListClip [k].Draw (rect,this);
		}
	}
		
	protected void DrawGuideLine(Rect rect,Color color){

		float linesp = (rect.width - titleWidth) / (float)splitCount;

		GUI.skin.label.fontSize = 10;
		GUI.color = new Color (1, 1, 1, 0.5f);
		for (int k = 0; k < splitCount+1; k++) {
			Rect glrect = new Rect (rect.x + titleWidth + k * linesp - 1, rect.y, 1, rect.height);

			EditorGUI.DrawRect (glrect, color);

		}

		EditorGUI.DrawRect (new Rect (0, rect.y + rect.height-1, rect.width,1), color);

		EditorGUI.DrawRect (new Rect (0, rect.y, rect.width,1), color);

	}

	protected void DrawSelectClip(){
		if (mSelectClip == null)
			return;
		
		Rect rect = mSelectClip.position;

		Rect rect1 = new Rect (rect.x+lineWidth,rect.y,lineWidth,rect.height);
		Rect rect2 = new Rect (rect.x,rect.y+rect.height - lineWidth,rect.width + widthHead/2,lineWidth);
		Rect rect3 = new Rect (rect.x+rect.width  - lineWidth + widthHead/2,rect.y,lineWidth,rect.height);
		Rect rect4 = new Rect (rect.x,rect.y+lineWidth,rect.width + widthHead/2,lineWidth);

		Color boardColor = colorClipSelect;
		EditorGUI.DrawRect (rect1,boardColor);
		EditorGUI.DrawRect (rect2,boardColor);
		EditorGUI.DrawRect (rect3,boardColor);
		EditorGUI.DrawRect (rect4,boardColor);

	}

	protected void DrawSelectClipEvent(){
		if (mSelectClipEvent == null)
			return;
		
		Rect rect = mSelectClipEvent.position;
		Rect rect1 = new Rect (rect.x-lineWidth,rect.y,lineWidth,rect.height);
		Rect rect2 = new Rect (rect.x,rect.y+rect.height,rect.width,lineWidth);
		Rect rect3 = new Rect (rect.x+rect.width,rect.y,lineWidth,rect.height);
		Rect rect4 = new Rect (rect.x,rect.y-lineWidth,rect.width,lineWidth);

		Color boardColor = colorClipEventSelectLine;
		EditorGUI.DrawRect (rect1,boardColor);
		EditorGUI.DrawRect (rect2,boardColor);
		EditorGUI.DrawRect (rect3,boardColor);
		EditorGUI.DrawRect (rect4,boardColor);

//		EditorGUI.DrawRect (rect,colorClipEventSelect);
		//中线
		EditorGUI.DrawRect (new Rect (rect.x + EditorClipEvent.WidthNode / 2, 0, 1,WinTotalTrackHeight), colorClipEventSelectLine);

		int lw = 40;
		rectSelectClipEventRate = new Rect (rect.x -  lw/2 + 1, 0, lw, 20);

//		GUI.skin.label.alignment = TextAnchor.MiddleCenter;

	}

	protected void DrawSelectClipeEventRate(){
		if (mSelectClipEvent == null)
			return;
		GUI.contentColor = Color.yellow;

		GUI.Label (rectSelectClipEventRate,"" +System.Math.Round (mSelectClipEvent.triggerTime, 2) * 100);

		GUI.contentColor = Color.white;
	}

	public void SetSelectListClipEvent(EditorClip clip,EditorClipEvent clipEvent){
		selectListClipEvent.Clear ();
		if (clip == null || clipEvent == null)
			return;
		foreach (EditorClipEvent item in clip.ListClipEvent) {
			if (item.triggerTime == clipEvent.triggerTime) {
				selectListClipEvent.Add (item);
			}
		}
	}

	public void SetSelectClip(EditorClip clip){
		mSelectClip = clip;
		//为了失去焦点，更新显示值
		GUI.FocusControl("None");
	}

	public void SetSelectClipEvent(EditorClipEvent clipEvent){
		mSelectClipEvent = clipEvent;
		GUI.FocusControl("None");
	}
		
	public void BeginChange(){
		if (!mMarkPChange) {
			mMarkPChange = true;
			EditorTools.RegisterUndo(this.GetType().ToString() + " Clip Maker", this);
		}
	}

	public void EndChange(){
		if (mMarkPChange) {
			mMarkPChange = false;
			EditorTools.SetDirty (this);
			MarkModified ();
		}
	}

	public void MarkModified(){
		mMarkModified = true;
		this.Repaint ();
	}


    public new void Show(){
		base.Show ();
		if (!mIsShowing) {
			UnityEditor.Undo.undoRedoPerformed += Undo;
		}
		mIsShowing = true;
		mSelectClip = null;
		mSelectClipEvent = null;
		selectListClipEvent.Clear ();
		OnOpen ();
	}

	public void SetClickOpera(EdSerialObject target){
		if (target is EditorClip) {
			OnClickClip (target as EditorClip);
		} else if (target is EditorClipEvent) {
			OnClickClipEvent (target as EditorClipEvent);
		}
	}

	virtual protected void OnOpen(){}
	//点击轨道
	virtual protected void OnClickClip(EditorClip target){}
	//点击事件节点
	virtual protected void OnClickClipEvent(EditorClipEvent target){}
    //更新保存数据
    virtual protected void OnSave(){}
	//回滚操作
	virtual protected void OnUndo(){}
	virtual protected void OnClose(){}

	virtual protected void OnDestroy(){
		UnityEditor.Undo.undoRedoPerformed -= Undo;
		Reset ();
		OnClose ();
	}


	private void Undo(){
		OnUndo ();
		this.Repaint ();
	} 

	protected void Reset(){
		mSelectClip = null;
		mSelectClipEvent = null;
		selectListClipEvent.Clear ();
	}

}
