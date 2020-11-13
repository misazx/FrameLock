using UnityEngine;
using UnityEditor;
using UnityFrame;
using System.Collections.Generic;

[CustomEditor(typeof(UIItem), true)]
public class UIItemEditor : UIUpdateGroupEditor{
	public override void OnInspectorGUI ()
	{
		UIItem uiitem = target as UIItem;
		EditorTools.DrawEntityMark ();

		EditorTools.DrawUpdateKeyTextField (uiitem);

		UIItem updateGroup = target as UIItem;

		EditorTools.DrawGreyPreView(updateGroup);

		DrawUpdateTree (uiitem);
	}
}


[CustomEditor(typeof(UIView), true)]
public class UIViewEditor : Editor{

	private SerializedProperty m_EventOnShow;
	private SerializedProperty m_EventOnClose;

    private string[] hierarchyOptions =
    {
        "Bottom",   //-2
        "StackDown",     //-1
        "Common",   //0
        "Middle",   //1
        "StackUp",       //2
        "Mask",     //3
        "Top"       //4
    };

    private Dictionary<int,KeyValuePair<int,string>> indexToViewOrder = new Dictionary<int, KeyValuePair<int, string>>{
        {-2,new KeyValuePair<int,string>(0,"Bottom")},
        {-1,new KeyValuePair<int,string>(1,"StackDown")},
        {0,new KeyValuePair<int,string>(2,"Common")},
        {1,new KeyValuePair<int,string>(3,"Middle")},
        {2,new KeyValuePair<int,string>(4,"StackUp")},
        {3,new KeyValuePair<int,string>(5,"Mask")},
        {4,new KeyValuePair<int,string>(6,"Top")}
    };


    private int IndexToViewOrder(int index) {
        foreach (var item in indexToViewOrder) {
            if (item.Value.Key == index) {
                return item.Key;
            }
        }
        return 0;
    }

    private int ViewOrderToIndex(int ViewOrder)
    {
        if (indexToViewOrder.ContainsKey(ViewOrder)) {
            return indexToViewOrder[ViewOrder].Key;
        }
        return indexToViewOrder[0].Key;
    }

    public override void OnInspectorGUI ()
	{
		UIView uiview = target as UIView;

		EditorTools.DrawEntityMark ();

        EditorTools.DrawUpdateKeyTextField (uiview);

		UIView.ShowType viewType = (UIView.ShowType)EditorGUILayout.EnumPopup ("视图类型", uiview.viewType);

        float releaseDelay = 0;
        int orderIndex = 2;
        if (uiview.viewType != UIView.ShowType.CONTENT) {
            orderIndex = ViewOrderToIndex(uiview.viewOrder);
            orderIndex = EditorGUILayout.Popup("渲染层级", orderIndex, hierarchyOptions);
            releaseDelay = EditorGUILayout.FloatField("延迟释放", uiview.releaseDelay);
        }

        if (Application.isPlaying && uiview.viewType != UIView.ShowType.CONTENT)
        {
            GUI.color = Color.yellow;
            EditorGUILayout.LabelField("渲染序号", (uiview.sortingOrder).ToString());
            GUI.color = Color.white;
        }

        if (GUI.changed) {
			EditorTools.RegisterUndo ("UIView",uiview);
			uiview.viewType = viewType;
            uiview.viewOrder = IndexToViewOrder(orderIndex);
            uiview.releaseDelay = releaseDelay;
            EditorTools.SetDirty (uiview);
		}

		if (EditorTools.DrawHeader ("逻辑绑定", false, false)) {
			this.serializedObject.Update ();
			//开始检查是否有修改
			EditorGUI.BeginChangeCheck ();

			EditorGUILayout.PropertyField (this.m_EventOnShow, new GUILayoutOption[0]);
			EditorGUILayout.PropertyField (this.m_EventOnClose, new GUILayoutOption[0]);

			if (EditorGUI.EndChangeCheck ())
				this.serializedObject.ApplyModifiedProperties ();

			GUILayout.Space (10);
		}




		UIUpdateGroupEditor.DrawUpdateTree (uiview);

	}

	protected void OnEnable(){
		m_EventOnShow = this.serializedObject.FindProperty ("m_EventOnShow");
		m_EventOnClose = this.serializedObject.FindProperty("m_EventOnClose");
	}

}


[CustomEditor(typeof(UIUpdateGroup),true)]
public class UIUpdateGroupEditor : Editor
{

	public override void OnInspectorGUI ()
	{
		UIUpdateGroup updateGroup = target as UIUpdateGroup;

		base.OnInspectorGUI ();
		EditorTools.DrawUpdateKeyTextField (updateGroup);
        EditorTools.DrawGreyPreView(updateGroup);
        DrawUpdateTree (updateGroup);
	}

	public static void DrawUpdateTree(UIUpdateGroup updateGroup){
		GUI.color = Color.yellow;



		if (GUILayout.Button ("构建更新树", GUILayout.Height (40))) {
			UIUpdateTools.UF_BuildUpdateList(updateGroup,updateGroup.ListUpdateUI);
			EditorTools.SetDirty (updateGroup);
		}

		GUI.color = Color.white;


		if (updateGroup == null)
			return;
		if (EditorTools.DrawHeader ("显示更新树", false, false)) {

			DrawUpdateGroup (updateGroup as UIUpdateGroup, 0,Color.cyan);
		}

	}

	private static void DrawUpdateGroup(UIUpdateGroup uigroup,int tagidx,Color color){
		if (uigroup == null)
			return;

		GUI.backgroundColor = color;

		GUILayout.BeginHorizontal ();
		GUILayout.Space (tagidx * 50);

//		EditorGUILayout.ObjectField (uigroup, uigroup.GetType (),false);

		GUI.contentColor = Color.white;
		EditorTools.DrawPingBox (uigroup,string.Format("{0}  <{1}>",uigroup.updateKey,uigroup.GetType().Name));

		GUILayout.EndHorizontal ();

		++tagidx;

		for (int k = 0; k < uigroup.ListUpdateUI.Count; k++) {
			if (uigroup.ListUpdateUI [k] is UIUpdateGroup) {
				DrawUpdateGroup (uigroup.ListUpdateUI [k] as UIUpdateGroup, tagidx,color);
			} else {
				GUI.backgroundColor = color;
				GUILayout.BeginHorizontal ();
				GUILayout.Space (tagidx * 50);
                IUIUpdate ui = uigroup.ListUpdateUI[k] as IUIUpdate;
                if (ui != null)
                {
                    EditorTools.DrawPingBox(uigroup.ListUpdateUI[k], string.Format("{0}  <{1}>", ui.updateKey, ui.GetType().Name));
                }
                else {
                    GUI.backgroundColor = Color.grey;
                    EditorTools.DrawPingBox(null, "<null>");
                }
				GUILayout.EndHorizontal ();
			}
		}

		if (uigroup.MapDynamicUI != null) {
			++tagidx;
			foreach (Object obj in uigroup.MapDynamicUI.Values) {
				if (obj is UIUpdateGroup) {
					DrawUpdateGroup (obj as UIUpdateGroup, tagidx,Color.red);

				} else {
					GUI.backgroundColor = color;
					GUILayout.BeginHorizontal ();
					GUILayout.Space (tagidx * 50);
                    IUIUpdate ui = obj as IUIUpdate;

                    if (ui != null)
                    {
                        EditorTools.DrawPingBox(obj, string.Format("{0}  <{1}>", ui.updateKey, ui.GetType().Name));
                    }
                    else {
                        GUI.backgroundColor = Color.grey;
                        EditorTools.DrawPingBox(null, "<null>");
                    }

					GUILayout.EndHorizontal ();
				}

			}
		}

	}
		


}


