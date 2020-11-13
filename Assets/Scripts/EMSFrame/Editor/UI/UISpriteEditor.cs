using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.Sprites;
using UnityFrame;

[CustomEditor(typeof(UISprite), true)]
public class UISpriteEditor : UnityEditor.UI.ImageEditor
{

	public override void OnInspectorGUI ()
	{
		UISprite uisprite = target as UISprite;

		EditorTools.DrawUpdateKeyTextField (uisprite);

		DrawSpriteElement (uisprite.sprite);

		base.OnInspectorGUI ();

		GUILayout.Space (10);



		bool autoNS = EditorGUILayout.Toggle ("Auto Native Size",uisprite.autoNativeSize);

		bool autoRevert = EditorGUILayout.Toggle ("Auto Revert",uisprite.autoRevert);

        bool hideGraphic = EditorGUILayout.Toggle("Hide Graphic", uisprite.hideGraphic);


        if (GUI.changed) {
			EditorTools.RegisterUndo ("UISprite", uisprite);

			uisprite.autoNativeSize = autoNS;
			uisprite.autoRevert = autoRevert;
            uisprite.hideGraphic = hideGraphic;

            EditorTools.SetDirty (uisprite);

		}

	}
		

	public static void DrawSpriteElement(Sprite sprite){
		if (sprite == null)
			return;
		Vector2 fixeSize = new Vector2 (64, 64);
		Rect rect = GUILayoutUtility.GetRect (200, 64, GUI.skin.box);
		Rect rectImage = new Rect(rect.x,rect.y,fixeSize.x,fixeSize.y);

		Rect rectLB = new Rect (rect.x + fixeSize.x, rect.y, 150, fixeSize.y);

		Vector4 outerUV = ((sprite == null) ? Vector4.zero : DataUtility.GetOuterUV (sprite));
		//转化一层，outerUV为（左下角点 + 右上角点） rectUV 为 （位置起始点 + 尺寸大小）
		Rect rectUV = new Rect(outerUV.x,outerUV.y,outerUV.z - outerUV.x,outerUV.w - outerUV.y);

		float aspect = sprite.rect.width / sprite.rect.height;
		if (aspect > 0) {
			rectImage.height = rectImage.width / aspect;
		} else {
			rectImage.height = rectImage.width * aspect;
		}
		if (rectImage.height > fixeSize.y) {
			rectImage.width =   fixeSize.x * fixeSize.y/rectImage.height;
			rectImage.height = fixeSize.y;
		}

		GUI.DrawTextureWithTexCoords (rectImage, sprite.texture,rectUV);
		if (sprite.border.Equals (default(Vector4))) {
			GUI.Label (rectLB,string.Format (" {0}\n {1} x {2}", sprite.name, sprite.rect.width, sprite.rect.height)); 
		} else {
			GUI.Label (rectLB,string.Format (" {0}\n {1} x {2}\n [九宫格]", sprite.name, sprite.rect.width, sprite.rect.height)); 
		}


		EditorTools.ClickAndPingObject (rect,sprite);
	}


}

