using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityFrame;

[CustomEditor(typeof(UISortingObject), true)]
public class UISortingEditor : UIObjectEditor
{

    public override void OnInspectorGUI()
    {

        UISortingObject sortingObject = target as UISortingObject;

        base.OnInspectorGUI();

        int sortingOrder = sortingObject.sortingOrder;

        sortingOrder = EditorGUILayout.IntSlider("sortingOrder", sortingOrder, 0, 100);

        if (GUI.changed) {
            EditorTools.RegisterUndo("UISlider", sortingObject);
            sortingObject.sortingOrder = sortingOrder;
            EditorTools.SetDirty(sortingObject);
        }

    }
}

[CustomEditor(typeof(UISortingGroup), true)]
public class UISortingGroupEditor : Editor
{

    public override void OnInspectorGUI()
    {

        UISortingGroup sortingObject = target as UISortingGroup;

        int sortingOrder = sortingObject.sortingOrder;

        sortingOrder = EditorGUILayout.IntSlider("sortingOrder", sortingOrder, 0, 100);

        if (GUI.changed)
        {
            EditorTools.RegisterUndo("UISlider", sortingObject);
            sortingObject.sortingOrder = sortingOrder;
            EditorTools.SetDirty(sortingObject);
        }

    }
}
