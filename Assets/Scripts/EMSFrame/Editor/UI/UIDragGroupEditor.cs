using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityFrame;

[CustomEditor(typeof(UIDragGroup), true)]
public class UIDragGroupEditor : Editor
{

	public override void OnInspectorGUI ()
	{
		UIDragGroup dgroup = target as UIDragGroup;
	
		EditorTools.DrawUpdateKeyTextField (dgroup);

		GUILayout.Space (10);

		drawSprite (dgroup);


	}


	public void drawSprite(UIDragGroup dgroup){

		GUILayout.Space (3);
		bool allowMultiCollision = EditorGUILayout.Toggle ("允许多个碰撞", dgroup.allowMultiCollision);
		string upressclickkey = EditorGUILayout.TextField ("触发事件",dgroup.eDragDown);
		GUILayout.Space (3);
		UIDrag buffer = null;
		buffer = (UIDrag)EditorGUILayout.ObjectField("绑定UIDrag",buffer,typeof(UIDrag),true);
		for (int k = 0; k < dgroup.drags.Count; k++) {
			GUILayout.BeginHorizontal();
			GUILayout.Space (20);
			if (GUILayout.Button ("X", GUILayout.Width (20))) {
				EditorTools.RegisterUndo ("UIDragGroup", dgroup);
				dgroup.drags.RemoveAt (k);
				EditorTools.SetDirty (dgroup);
				return;
			}
			EditorGUILayout.ObjectField ("", dgroup.drags [k], typeof(UIDrag), true);
			GUILayout.Label (dgroup.drags [k].eParam);
			GUILayout.EndHorizontal ();
		}

		GUILayout.Space (15);
		if (GUILayout.Button ("自动绑定")) {
			UIDrag[] array = dgroup.gameObject.GetComponentsInChildren<UIDrag> (true);

			if (array != null && array.Length > 0) {
				EditorTools.RegisterUndo ("UIDragGroup", dgroup);
				foreach (UIDrag drag in array) {
					if (!dgroup.drags.Contains (drag)) {
						dgroup.drags.Add (drag);
					}
				}
				EditorTools.SetDirty (dgroup);
				return;
			}
		}


		if (GUI.changed) {
			EditorTools.RegisterUndo ("UIDragGroup", dgroup);

			dgroup.eDragDown = upressclickkey;
			dgroup.allowMultiCollision = allowMultiCollision;

			if (buffer != null) {
				if (!dgroup.drags.Contains (buffer)) {
					dgroup.drags.Add (buffer);
				}
			}
			EditorTools.SetDirty (dgroup);

		}

	}


}

