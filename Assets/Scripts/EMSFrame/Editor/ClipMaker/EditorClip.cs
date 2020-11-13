using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Internal;
using System.Text;
using UnityFrame;

public class EditorClip : EdSerialObject
{
	
	[SerializeField]private List<EditorClipEvent> m_ListClipEvent = new List<EditorClipEvent>();

	// ser obj
	public List<EditorClipEvent> ListClipEvent{get{return m_ListClipEvent;}}


	public EditorClip(){}

	public EditorClip(string strName){
		m_strName = strName;
	}


	public Rect position{ get; private set;}

	public void AddClipEvent(EditorClipEvent item,bool regUndo){
		if (!ListClipEvent.Contains (item)) {
			if (regUndo) {
				this.BeginChange ();
				ListClipEvent.Add (item);
				this.EndChange();
			} else {
				ListClipEvent.Add (item);
			}
		}
	}
		
	protected void MeauEventAddClipEvent(float rate,EditorClipMaker maker){
		this.BeginChange ();
		EditorClipEvent clipEvent = new EditorClipEvent ();
		clipEvent.triggerTime = rate;
		ListClipEvent.Add (clipEvent);
		this.EndChange ();
		maker.MarkModified ();
	}

	protected void MeauEventParseClipEvent(float rate,EditorClipMaker maker){
		this.BeginChange ();
		if (EditorClipEvent.CloneInstance != null) {
			EditorClipEvent.CloneInstance.triggerTime = rate;
			ListClipEvent.Add (EditorClipEvent.CloneInstance);
		}
		EditorClipEvent.CloneInstance = null;
		this.EndChange ();
		maker.MarkModified ();
	}


	protected void MeauEventClearClipEvent(object param){
		EditorClipMaker editorMaker = param as EditorClipMaker;
		this.BeginChange ();
		ListClipEvent.Clear ();
		this.EndChange();
		editorMaker.MarkModified ();
	}

	private void switchListItem<T>(List<T> list,int currentIdx, int targetIdx){
		if (0 <= currentIdx && currentIdx < list.Count && 0 <= targetIdx && targetIdx < list.Count) {
			T currenttmp = list [currentIdx];
			list [currentIdx] = list [targetIdx];
			list [targetIdx] = currenttmp;
		}
	}

	//	上移
	private void MeauEventMoveUpClip(object param){
		EditorClipMaker editorMaker = param as EditorClipMaker;
		editorMaker.BeginChange ();
		int idx = editorMaker.ListClip.IndexOf (this);
		switchListItem (editorMaker.ListClip,idx, idx - 1);
		editorMaker.EndChange();
	}

	//	下移
	private void MeauEventMoveDownClip(object param){
		EditorClipMaker editorMaker = param as EditorClipMaker;
		editorMaker.BeginChange ();
		int idx = editorMaker.ListClip.IndexOf (this);
		switchListItem (editorMaker.ListClip,idx, idx + 1);
		editorMaker.EndChange();
	}

	protected void EventMeauDeleteClip(object param){
		EditorClipMaker editorMaker = param as EditorClipMaker;
		editorMaker.BeginChange ();
		editorMaker.ListClip.Remove (this);
		editorMaker.EndChange ();
	}
	protected void EventMeauCopyClip(object param){
		EditorClipMaker editorMaker = param as EditorClipMaker;
		editorMaker.BeginChange ();
		editorMaker.ListClip.Add (this.Clone ());
		editorMaker.EndChange ();
	}


	private float getRateBaseMousePosition(Rect rect,Vector2 mousePosition){
		return (float)(mousePosition.x - rect.x) / (float)rect.width;
	}

	public EditorClipEvent SetAtFirst(EditorClipEvent clipEvent){
		EditorClipEvent target = null;
		for (int k = 0; k < ListClipEvent.Count; k++) {
			if (ListClipEvent [k] == clipEvent) {
				target = ListClipEvent [k];
				ListClipEvent [k] = ListClipEvent [0];
				ListClipEvent [0] = target;
				break;
			}
		}
		return target;
	}

	public void RemoveClipEvent(EditorClipEvent clipEvent){
		foreach(EditorClipEvent ecevent in ListClipEvent){
			if (ecevent == clipEvent) {
				ListClipEvent.Remove (ecevent);
				break;
			}
		}
	}
	

	public void Draw(Rect rect,EditorClipMaker maker){
		GUI.color = Color.white;
		GUI.contentColor = Color.white;
		GUI.backgroundColor = Color.white;

		this.position = rect;

		Rect rectHead = new Rect (0,rect.y,maker.titleWidth,maker.heightTrack);
		Rect rectHeadText = new Rect (rectHead.x, rectHead.y, rectHead.width, rectHead.height);
		Rect rectBg = new Rect (rect.x, rect.y, rect.width + maker.widthHead/2, rect.height);

		Rect rectTrack = new Rect (maker.titleWidth,rect.y,rect.width - maker.titleWidth,maker.heightTrack);

        GUI.Box(rectBg, "");

        EditorGUI.DrawRect(rectHead, new Color(1, 1f, 1, 0.1f));

        int fontSize = GUI.skin.label.fontSize;
        GUI.skin.label.fontSize = 16;
        GUI.Label(rectHeadText, this.name);
        GUI.skin.label.fontSize = fontSize;

        //GUI.skin.textArea.alignment = TextAnchor.MiddleCenter;
        //string valname = GUI.TextArea (rectHeadText, this.name);
        //GUI.skin.textArea.alignment = TextAnchor.UpperLeft;

//		EditorGUI.DrawRect (rectBg, new Color (0, 0.4f, 1, 0.1f));
		

		

		for (int k = 0; k < ListClipEvent.Count; k++) {
			ListClipEvent [k].Draw (rectTrack,this,maker);
		}

		//if (GUI.changed) {
		//	this.name = valname;
		//}

		if (Event.current.type == EventType.MouseDown) {
			if (rectTrack.Contains (Event.current.mousePosition)) {
				if (Event.current.button == 1) {
					GenericMenu menu = new GenericMenu ();
					float rate = getRateBaseMousePosition (rectTrack, Event.current.mousePosition);
					GenericMenu.MenuFunction2 callAddNode = delegate(object userData) {
						MeauEventAddClipEvent (rate, maker);
					};
					menu.AddItem (new GUIContent ("Add Event"), false, callAddNode,maker);
					if(EditorClipEvent.CloneInstance != null){
						GenericMenu.MenuFunction2 callPareseNode = delegate(object userData) {
							MeauEventParseClipEvent (rate,maker);
						};
						menu.AddItem (new GUIContent ("Parse Event"), false, callPareseNode, maker);	
					}
					menu.AddItem (new GUIContent ("Clear Event"), false,MeauEventClearClipEvent, maker);

					if (maker.ListClip.Count >= 2) {
						menu.AddSeparator ("");
						menu.AddItem (new GUIContent ("Move Up"), false, MeauEventMoveUpClip, maker);
						menu.AddItem (new GUIContent ("Move Down"), false, MeauEventMoveDownClip, maker);
					}
					menu.AddSeparator ("");

					menu.AddItem (new GUIContent ("Copy Track"),false,EventMeauCopyClip, maker);
					menu.AddItem (new GUIContent ("Delete Track"), false, EventMeauDeleteClip, maker);

					// Now create the menu, add items and show it
					if (menu != null) {
						menu.ShowAsContext ();
					}
				} else if (Event.current.button == 0) {
					maker.SetSelectListClipEvent (this, null);
					maker.SetSelectClipEvent (null);
					maker.SetSelectClip (this);
					maker.SetClickOpera (this);
				}
				Event.current.Use ();
			} 

		}


	}

	public EditorClip Clone(){
		EditorClip ret = new EditorClip ();
		foreach (var item in this.ListClipEvent) {
			ret.ListClipEvent.Add (item.Clone ());
		}
		this.CopyCustomParamsTo (ret);
		return ret;
	}

}

