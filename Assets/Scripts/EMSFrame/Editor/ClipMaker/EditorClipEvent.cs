using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Internal;
using System.Text;
using UnityFrame;

public class EditorClipEvent : EdSerialObject
{
	public const float Deadzoom = 0.005f;
	public const int WidthNode = 8;

	//ser pro
	[SerializeField]private float m_TriggerTime = 0;

    //记录状态字段
    public int state { get; set; }

    public float triggerTime{
		get{
			if (mDrag)
				return m_DragTriggerTime;
			else
			return m_TriggerTime;
		}
		set{ 
			m_TriggerTime = value;
		}
	}

	private float m_DragTriggerTime = 0;

	private bool mDrag = false;

	public EditorClipEvent(){}

	public Rect position{ get; private set;}

	public static EditorClipEvent CloneInstance;

    

	public EditorClipEvent Clone(){
		EditorClipEvent ret = new EditorClipEvent ();
		ret.triggerTime = this.triggerTime;
        ret.state = this.state;
        ret.name = this.name;

        CopyCustomParamsTo (ret);
		return ret;
	}

	protected void EventMeauDeleteClipEvent(EditorClip clip,EditorClipMaker maker){
		clip.BeginChange ();
		clip.ListClipEvent.Remove (this);
		clip.EndChange ();
        maker.ClearSelectClipEvent();
		maker.MarkModified();
	}

	protected void EventMeauCopyClipEvent(){
		CloneInstance = this.Clone ();
		CloneInstance.mDrag = false;
	}
		

	public void Draw(Rect rect,EditorClip clip,EditorClipMaker maker){
		float x = 0;
		if (mDrag) {
			x = rect.width * m_DragTriggerTime;
		} else {
			x = rect.width * m_TriggerTime;
		}

		position = new Rect (rect.x + x - WidthNode/2,rect.y,WidthNode,maker.heightTrack);

		EditorGUI.DrawRect (position, maker.GetColorSet(this.name));

		if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && position.Contains (Event.current.mousePosition)) {
			// Now create the menu, add items and show it
			GenericMenu menu = new GenericMenu ();
			menu.AddItem (new GUIContent ("Copy Event"), false, EventMeauCopyClipEvent);
			menu.AddSeparator ("");
			menu.AddItem (new GUIContent ("Delete Event"), false, (e) =>{EventMeauDeleteClipEvent (clip,maker);},null);
			menu.ShowAsContext ();
			Event.current.Use ();
		} else {
			if (Event.current.type == EventType.MouseDown && Event.current.button == 0) {
				if (position.Contains (Event.current.mousePosition)) {
					mDrag = true;
					m_DragTriggerTime = rect.width * triggerTime;
					maker.SetSelectClip(clip);
					maker.SetSelectClipEvent(this);
					maker.SetSelectListClipEvent (clip, this);
					maker.SetClickOpera (this);
					maker.Repaint ();
					Event.current.Use ();
				}
			}
			else  if (Event.current.type == EventType.MouseUp){
				if (mDrag == true && m_DragTriggerTime != m_TriggerTime) {
					this.BeginChange ();
					m_TriggerTime = m_DragTriggerTime;
					this.EndChange ();
					maker.MarkModified ();
				}
				mDrag = false;
			}
			if (mDrag) {
				float dragDetlaPos = Mathf.Clamp (Event.current.mousePosition.x, rect.x, rect.x + rect.width) - rect.x;
				dragDetlaPos = Mathf.Clamp (dragDetlaPos, 0,rect.width);
				float newtrigger = Mathf.Max (dragDetlaPos / rect.width, 0);
				if (Mathf.Abs(newtrigger - m_DragTriggerTime) > Deadzoom) {
					m_DragTriggerTime = (float)System.Math.Round(newtrigger,2);
					maker.SetSelectListClipEvent (clip, this);
					maker.Repaint ();
//					maker.MarkChange ();
//					colorFrame = frameColorselected;
				}
			} else {

//				colorFrame = getFrameColor(editorAAClip.Target);
			}
		}

	}



}


