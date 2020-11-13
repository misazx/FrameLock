using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityFrame;


[CustomEditor(typeof(DipRayController), true)]
public class EditorDipController : Editor
{
    public override void OnInspectorGUI()
    {
        DipRayController dip = target as DipRayController;
        base.OnInspectorGUI();
        GUI.color = Color.green;
        if (GUILayout.Button("绑定射线",GUILayout.Height(40))) {
            dip.chains.Clear();
            dip.GetComponentsInChildren<ULineRenderer>(false, dip.chains);

        }
        GUI.color = Color.white;

    }

}
