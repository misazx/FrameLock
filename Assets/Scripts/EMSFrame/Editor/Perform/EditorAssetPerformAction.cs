using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityFrame;
using UnityFrame.Assets;

[CustomEditor(typeof(AssetPerformAction), true)]
public class EditorAssetPerformAction : Editor
{

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        GUI.color = Color.green;
        if (GUILayout.Button("打开表现事件编辑器", GUILayout.Height(40))) {
            var maker = EditorWindow.GetWindow<PerformActionClipMaker>();
            if (maker.Target == this.target)
            {
                maker.Focus();
            }
            else
            {
                maker.Close();
                EditorWindow.GetWindow<PerformActionClipMaker>("编辑表现", true).Show(this.target as AssetPerformAction);
            }
        }
    }

}
