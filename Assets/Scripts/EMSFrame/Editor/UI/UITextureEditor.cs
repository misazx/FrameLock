using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.Sprites;
using UnityFrame;

[CustomEditor(typeof(UITexture), true)]
public class UITextureEditor : UnityEditor.UI.RawImageEditor
{

	public override void OnInspectorGUI ()
	{
		UITexture uitexture = target as UITexture;

		EditorTools.DrawUpdateKeyTextField (uitexture);


		DrawSpriteElement (uitexture.texture as Texture2D);

		base.OnInspectorGUI ();

		GUILayout.Space (10);

		bool autoNS = EditorGUILayout.Toggle ("Auto Native Size",uitexture.autoNativeSize);

		if (GUI.changed) {
			EditorTools.RegisterUndo ("UITexture", uitexture);
			uitexture.autoNativeSize = autoNS;
			EditorTools.SetDirty (uitexture);
		}

	}  

	public static void DrawSpriteElement(Texture2D texture){
		if (texture == null)
			return;
		Vector2 fixeSize = new Vector2 (100, 100);

		Rect rect = GUILayoutUtility.GetRect (200, fixeSize.y, GUI.skin.box);
		Rect rectImage = new Rect(rect.x,rect.y,fixeSize.x,fixeSize.y);

		Rect rectLB = new Rect (rect.x + fixeSize.x + 20, rect.y, 150, fixeSize.y);

		float aspect = (float)texture.width / (float)texture.height;
		if (aspect > 0) {
			rectImage.height = rectImage.width / aspect;
		} else {
			rectImage.height = rectImage.width * aspect;
		}

		GUI.DrawTexture (rectImage,texture);

		GUI.Label (rectLB,string.Format ("{0} x {1}\n{2}\n{3}\n{4}", 
			texture.width, 
			texture.height,
			texture.format.ToString(),
			texture.wrapMode.ToString(),
			texture.filterMode.ToString()
		)); 


		EditorTools.ClickAndPingObject (rect,texture);
	}
}

[CustomEditor(typeof(UIPreview), true)]
public class UIPreviewEditor : UnityEditor.UI.RawImageEditor
{

    public override void OnInspectorGUI()
    {
        UIPreview ui = target as UIPreview;

        EditorTools.DrawUpdateKeyTextField(ui);

        UITextureEditor.DrawSpriteElement(ui.texture as Texture2D);

        base.OnInspectorGUI();

        GUILayout.Space(10);

        Vector3 previewPos = EditorGUILayout.Vector3Field("Preview Pos", ui.previewPos);
        Vector3 previewEuler = EditorGUILayout.Vector3Field("Preview Pos", ui.previewEuler);

        float FOV = EditorGUILayout.FloatField("Filed Of View",ui.FOV);

        float fieldDistance = EditorGUILayout.FloatField("Filed Distance", ui.fieldDistance);

        

        bool useDragRotate = EditorGUILayout.Toggle("Drag Rotate", ui.useDragRotate);

        float speedRotate = EditorGUILayout.FloatField("Speed Rotate", ui.speedRotate);

        string ePressClick = EditorGUILayout.TextField("Click Event", ui.ePressClick);

        string eParam = EditorGUILayout.TextField("Click Param", ui.eParam);
        

        if (GUI.changed)
        {
            EditorTools.RegisterUndo("UITexture", ui);
            ui.previewPos = previewPos;
            ui.previewEuler = previewEuler;
            ui.FOV = FOV;
            ui.fieldDistance = fieldDistance;

            ui.useDragRotate = useDragRotate;
            ui.speedRotate = speedRotate;
            ui.ePressClick = ePressClick;
            ui.eParam = eParam;
            


            EditorTools.SetDirty(ui);
        }

    }

}

    



