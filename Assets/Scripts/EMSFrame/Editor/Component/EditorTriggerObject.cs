using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityFrame;


[CustomEditor(typeof(TriggerObject), true)]
public class EditorTriggerObject : Editor
{

    public override void OnInspectorGUI()
    {
        TriggerObject obj = target as TriggerObject;
        string eTrigger = EditorGUILayout.TextField("触发事件",obj.eTrigger);

        int count = obj.eParams.Length;
        string[] extendParam = new string[count];
        GUILayout.BeginHorizontal();
        GUILayout.Label("事件参数");
        GUI.color = new Color(0.5f, 1f, 0.5f, 1);
        if (GUILayout.Button("添加"))
        {
            List<string> cache = new List<string>();
            for (int k = 0; k < count; k++)
            {
                cache.Add(obj.eParams[k]);
            }
            cache.Add("");
            obj.eParams = cache.ToArray();
            extendParam = cache.ToArray();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        for (int k = 0; k < obj.eParams.Length; k++)
        {
            GUILayout.BeginHorizontal();
            extendParam[k] = EditorGUILayout.TextField(obj.eParams[k]);
            GUI.color = Color.red;
            if (GUILayout.Button("X",GUILayout.Width(20)))
            {
                List<string> cache = new List<string>();
                for (int j = 0; j < obj.eParams.Length; j++)
                {
                    if (j != k)
                    {
                        cache.Add(obj.eParams[j]);
                    }
                }
                extendParam = cache.ToArray();
                break;
            }
            GUI.color = new Color(0.85f, 1f, 0.85f, 1);
            GUILayout.EndHorizontal();
        }
        GUI.color = Color.white;

        string triggerMask = EditorGUILayout.TextField("触发蒙板", obj.triggerMask);
        string eSound = EditorGUILayout.TextField("触发声音", obj.eSound);
        bool autoRelese = EditorGUILayout.Toggle("自动释放", obj.autoRelese);

        if (GUI.changed) {
            EditorTools.RegisterUndo("TriggerObject", obj);
            obj.eTrigger = eTrigger;
            obj.eParams = extendParam;
            obj.eSound = eSound;
            obj.autoRelese = autoRelese;
            obj.triggerMask = triggerMask;
            EditorTools.SetDirty(obj);
        }
    }
}


[CustomEditor(typeof(TriggerSprite), true)]
public class EditorTriggerSprite : EditorTriggerObject
{

    public override void OnInspectorGUI()
    {
        TriggerSprite obj = target as TriggerSprite;
        SpriteRenderer sr = EditorGUILayout.ObjectField("SpriteRenderer", obj.render,typeof(SpriteRenderer),true) as SpriteRenderer;

        base.OnInspectorGUI();

        if (GUI.changed)
        {
            EditorTools.RegisterUndo("TriggerSprite", obj);
            obj.render = sr;
            EditorTools.SetDirty(obj);

        }

    }


}
