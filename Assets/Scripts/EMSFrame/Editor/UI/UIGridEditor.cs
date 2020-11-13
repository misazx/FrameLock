using UnityEngine;
using UnityEditor;
using UnityFrame;
using UnityEngine.UI;

[CustomEditor(typeof(UIRecycleGrid), true)]
public class UIRecycleGridEditor : UIGridEditor{   
    protected override void DrawFields()
    {
        base.DrawFields();
        UIRecycleGrid grid = target as UIRecycleGrid;
        int maxCount = EditorGUILayout.IntField("maxShowCount", grid.maxShowCount);
        GridLayoutGroup.Axis layoutAxis = (GridLayoutGroup.Axis)EditorGUILayout.EnumPopup("Axis", grid.layoutAxis);

        if (GUI.changed)
        {
            EditorTools.RegisterUndo("UIRecycleGrid", grid);
            grid.maxShowCount = maxCount;
            grid.layoutAxis = layoutAxis;
            EditorTools.SetDirty(grid);
        }
    }
}

[CustomEditor(typeof(UIFixedGrid), true)]
public class UIFixedGridEditor : UIGridEditor
{
    protected override void DrawFields()
    {
        base.DrawFields();
        UIFixedGrid grid = target as UIFixedGrid;
        var fixedCount = EditorGUILayout.IntField("Fixed Count", grid.fixedCount);

        if (GUI.changed)
        {
            EditorTools.RegisterUndo("UIRollGridEditor", grid);
            grid.fixedCount = fixedCount;
            EditorTools.SetDirty(grid);
        }
    }
}

[CustomEditor(typeof(UIGrid), true)]
public class UIGridEditor : Editor{
	protected Object m_PrefabUI = null;

	void Start(){
		m_PrefabUI = null;
	}

	public override void OnInspectorGUI ()
    {
        UIGrid grid = target as UIGrid;

        EditorTools.DrawUpdateKeyTextField(grid);

        DrawFields();

        GUILayout.Space(10);

        UIUpdateGroupEditor.DrawUpdateTree(grid);


    }

    protected virtual void DrawFields()
    {
        UIGrid grid = target as UIGrid;
        int constraint = EditorGUILayout.IntField("Constraint", grid.constraint);

        Vector2 padding = EditorGUILayout.Vector2Field("Padding", grid.padding);

        Vector2 space = EditorGUILayout.Vector2Field("Space", grid.space);

        Vector2 cellSize = EditorGUILayout.Vector2Field("CellSize", grid.cellSize);

        LayoutCorner alignement = (LayoutCorner)EditorGUILayout.EnumPopup("Alignement", grid.alignement);

        SizeFitterType fitterType = (SizeFitterType)EditorGUILayout.EnumPopup("SizeFitter", grid.fitterType);

        

        bool fixedCellSize = EditorGUILayout.Toggle("FixedCellSize", grid.fixedCellSize);


        GUILayout.BeginHorizontal();
        string prefabUIName = EditorGUILayout.TextField("Prefab UI", grid.prefabUI, GUILayout.Height(20));

        if (m_PrefabUI == null || m_PrefabUI.name != prefabUIName)
        {
            m_PrefabUI = GetTargetUIPrefab(prefabUIName);
        }

        Rect rectbox = GUILayoutUtility.GetRect(16, 16, GUI.skin.box);

        if (m_PrefabUI != null)
        {
            GUI.Box(rectbox, EditorGUIUtility.IconContent("lightMeter/greenLight"));
            EditorTools.ClickAndPingObject(rectbox, m_PrefabUI);
            GUI.backgroundColor = Color.blue;
            if (GUILayout.Button("G",GUILayout.Width(18), GUILayout.Height(18)))
            {
                var temp = AssetDatabase.LoadAssetAtPath<GameObject>(string.Format("Assets/AssetBases/PrefabAssets/ui/{0}.prefab", prefabUIName));
                if (temp != null)
                {
                    var item = Object.Instantiate(temp);
                    item.name = temp.name;
                    item.transform.parent = grid.transform;
                    item.transform.localScale = Vector3.one;
                }
            }
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("C", GUILayout.Width(18), GUILayout.Height(18))) {
                EditorTools.DestroyChild(grid.gameObject);
            }
            GUI.backgroundColor = Color.white;
        }

        GUILayout.EndHorizontal();


        if (GUI.changed)
        {
            EditorTools.RegisterUndo("UIGrid", grid);
            grid.prefabUI = prefabUIName;

            grid.constraint = constraint;
            grid.padding = padding;
            grid.space = space;
            grid.cellSize = cellSize;
            grid.alignement = alignement;
            grid.fixedCellSize = fixedCellSize;
            grid.fitterType = fitterType;

            EditorTools.SetDirty(grid);
        }
    }


    private Object GetTargetUIPrefab(string prefabName){
		if (!string.IsNullOrEmpty (prefabName) && prefabName.IndexOf ("ui_item_") > -1)
			return AssetDatabase.LoadAssetAtPath<Object> (string.Format("Assets/AssetBases/PrefabAssets/ui/{0}.prefab",prefabName));
		else
			return null;
	}
}


