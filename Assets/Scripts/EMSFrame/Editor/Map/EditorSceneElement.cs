using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

using UnityFrame;


[CustomEditor(typeof(UnityFrame.SceneElement), true)]
public class EditorSceneElement : Editor {

    private string[] tagOptions = {
        "Block",
        "Unwalk",
        "Walkable",
    };

    private int tagIndex = 0;

    public override void OnInspectorGUI()
    {
        SceneElement element = target as SceneElement;

        GUILayout.BeginHorizontal();
        GUI.color = Color.cyan;
        GUILayout.Label("Tag", GUILayout.Width(20));
        GUILayout.Space(95);
        tagIndex = GetTagIndex(element.gameObject);
        
        int index = EditorGUILayout.Popup(tagIndex, tagOptions);
        if (index != tagIndex || element.tag == DefineTag.Untagged)
        {
            tagIndex = index;
            element.gameObject.tag = tagOptions[index];
        }
        GUI.color = Color.white;
        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        var unitSize = EditorGUILayout.Vector2Field("UnitSize", element.unitSize);

        base.OnInspectorGUI();

        if (GUILayout.Button("构建元素",GUILayout.Height(40)))
        {
            BuildElement();
        }

        if (GUI.changed) {
            //element.collider.isTrigger = element.tag != DefineTag.Walkable;

            EditorTools.RegisterUndo("SceneElement", element);
            element.unitSize = unitSize;
            EditorTools.SetDirty(element);
        }


    }

    private int GetTagIndex(GameObject go){
        for (int k = 0; k < tagOptions.Length;k++) {
            if (tagOptions[k] == go.tag) {
                return k;
            }
        }
        return 0;
    }

    protected void BuildElement() {
        SceneElement element = target as SceneElement;
        if (element.tag == "Untagged") {
            element.tag = tagOptions[0];
        }
        SpriteRenderer sr = element.GetComponentInChildren<SpriteRenderer>();
        if (sr == null) {
            GameObject goPivot = new GameObject("pivot");
            goPivot.transform.parent = element.transform;
            goPivot.transform.localPosition = new Vector3(0, 0, -0.5f);
            
            GameObject go = new GameObject("sprite");
            go.transform.parent = goPivot.transform;
            go.transform.localPosition = new Vector3(0, 0, 0.5f);
            sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        }
        var sppos = sr.transform.localPosition;
        sr.transform.localPosition = new Vector3(sppos.x,0, sppos.z);
        sr.transform.localEulerAngles = new Vector3(90, 0, 0);

        BoxCollider bc = element.GetComponent<BoxCollider>();
        Vector3 size = bc.size;
        bc.size = new Vector3(size.x, 5, size.z);

        Rigidbody rig = element.GetComponent<Rigidbody>();
        if (rig == null) {
            rig = element.gameObject.AddComponent<Rigidbody>();
        }
        rig.isKinematic = true;
        rig.useGravity = false;
    }


}