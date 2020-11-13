using UnityFrame;
using System;
using UnityEditor;


namespace UnityEngine.UI
{
	public static class UIDefaultControls
	{
		//
		// Static Fields
		//
		private static Color s_PanelColor = new Color (1, 1, 1, 0.392f);

		private static Color s_TextColor = new Color (0.1960784f, 0.1960784f, 0.1960784f, 1);

		private static Color s_DefaultSelectableColor = new Color (1, 1, 1, 1);

		private static Vector2 s_ImageElementSize = new Vector2 (100, 100);

		private static Vector2 s_ThinElementSize = new Vector2 (160, 20);

		private static Vector2 s_ThickElementSize = new Vector2 (160, 30);

		private const float kThinHeight = 20;

		private const float kThickHeight = 30;

		private const float kWidth = 160;

		private static Resources s_StandardResources;

		public static UIDefaultControls.Resources GetStandardResources ()
		{
			if (s_StandardResources.standard == null) {
				s_StandardResources.standard = AssetDatabase.GetBuiltinExtraResource<Sprite> ("UI/Skin/UISprite.psd");
				s_StandardResources.background = AssetDatabase.GetBuiltinExtraResource<Sprite> ("UI/Skin/Background.psd");
				s_StandardResources.inputField = AssetDatabase.GetBuiltinExtraResource<Sprite> ("UI/Skin/InputFieldBackground.psd");
				s_StandardResources.knob = AssetDatabase.GetBuiltinExtraResource<Sprite> ("UI/Skin/Knob.psd");
				s_StandardResources.checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite> ("UI/Skin/Checkmark.psd");
				s_StandardResources.dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite> ("UI/Skin/DropdownArrow.psd");
				s_StandardResources.mask = AssetDatabase.GetBuiltinExtraResource<Sprite> ("UI/Skin/UIMask.psd");
			}
			return s_StandardResources;
		}

		//
		// Static Methods
		//
		public static GameObject CreateButton (UIDefaultControls.Resources resources)
		{
			GameObject gameObject = UIDefaultControls.CreateUIElementRoot ("Button", UIDefaultControls.s_ThickElementSize);
			GameObject gameObject2 = new GameObject ("UILabel");
			UIDefaultControls.SetParentAndAlign (gameObject2, gameObject);
			UISprite image = gameObject.AddComponent<UISprite> ();
			image.sprite = resources.standard;
			image.type = UISprite.Type.Sliced;
			image.color = UIDefaultControls.s_DefaultSelectableColor;
			Button defaultColorTransitionValues = gameObject.AddComponent<Button> ();
			UIDefaultControls.SetDefaultColorTransitionValues (defaultColorTransitionValues);
			UILabel text = gameObject2.AddComponent<UILabel> ();
			text.text = "Button";
			text.alignment =  TextAnchor.MiddleCenter;
			UIDefaultControls.SetDefaultTextValues (text);
			RectTransform component = gameObject2.GetComponent<RectTransform> ();
			component.anchorMin =(Vector2.zero);
			component.anchorMax =(Vector2.one);
			component.sizeDelta =(Vector2.zero);
			return gameObject;
		}

		public static GameObject CreateDropdown (UIDefaultControls.Resources resources)
		{
			GameObject gameObject = UIDefaultControls.CreateUIElementRoot ("Dropdown", UIDefaultControls.s_ThickElementSize);
			GameObject gameObject2 = UIDefaultControls.CreateUIObject ("Label", gameObject);
			GameObject gameObject3 = UIDefaultControls.CreateUIObject ("Arrow", gameObject);
			GameObject gameObject4 = UIDefaultControls.CreateUIObject ("Template", gameObject);
			GameObject gameObject5 = UIDefaultControls.CreateUIObject ("Viewport", gameObject4);
			GameObject gameObject6 = UIDefaultControls.CreateUIObject ("Content", gameObject5);
			GameObject gameObject7 = UIDefaultControls.CreateUIObject ("Item", gameObject6);
			GameObject gameObject8 = UIDefaultControls.CreateUIObject ("Item Background", gameObject7);
			GameObject gameObject9 = UIDefaultControls.CreateUIObject ("Item Checkmark", gameObject7);
			GameObject gameObject10 = UIDefaultControls.CreateUIObject ("Item Label", gameObject7);
			GameObject gameObject11 = UIDefaultControls.CreateScrollbar (resources);
			gameObject11.name =("Scrollbar");
			UIDefaultControls.SetParentAndAlign (gameObject11, gameObject4);
			Scrollbar component = gameObject11.GetComponent<Scrollbar> ();
			component.SetDirection (Scrollbar.Direction.BottomToTop, true);
			RectTransform component2 = gameObject11.GetComponent<RectTransform> ();
			component2.anchorMin =(Vector2.right);
			component2.anchorMax =(Vector2.one);
			component2.pivot =(Vector2.one);
			component2.sizeDelta =(new Vector2 (component2.sizeDelta.x, 0));
			UILabel text = gameObject10.AddComponent<UILabel> ();
			UIDefaultControls.SetDefaultTextValues (text);
			text.alignment =  TextAnchor.MiddleCenter;
			UISprite image = gameObject8.AddComponent<UISprite> ();
			image.color = new Color32 (245, 245, 245, 255);
			UISprite image2 = gameObject9.AddComponent<UISprite> ();
			image2.sprite = resources.checkmark;
			UIToggle toggle = gameObject7.AddComponent<UIToggle> ();
			toggle.targetGraphic = image;
			toggle.graphic = image2;
			toggle.isOn = true;
			UISprite image3 = gameObject4.AddComponent<UISprite> ();
			image3.sprite = resources.standard;
			image3.type = UISprite.Type.Sliced;
			ScrollRect scrollRect = gameObject4.AddComponent<ScrollRect> ();
			scrollRect.content = (RectTransform)gameObject6.transform;
			scrollRect.viewport = (RectTransform)gameObject5.transform;
			scrollRect.horizontal = false;
			scrollRect.movementType = ScrollRect.MovementType.Clamped;
			scrollRect.verticalScrollbar = component;
			scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
			scrollRect.verticalScrollbarSpacing = -3;
			Mask mask = gameObject5.AddComponent<Mask> ();
			mask.showMaskGraphic = false;
			UISprite image4 = gameObject5.AddComponent<UISprite> ();
			image4.sprite = resources.mask;
			image4.type = UISprite.Type.Sliced;
			UILabel text2 = gameObject2.AddComponent<UILabel> ();
			UIDefaultControls.SetDefaultTextValues (text2);
			text2.alignment =  TextAnchor.MiddleCenter;
			UISprite image5 = gameObject3.AddComponent<UISprite> ();
			image5.sprite = resources.dropdown;
			UISprite image6 = gameObject.AddComponent<UISprite> ();
			image6.sprite = resources.standard;
			image6.color = UIDefaultControls.s_DefaultSelectableColor;
			image6.type = UISprite.Type.Sliced;
			UIDropdown dropdown = gameObject.AddComponent<UIDropdown> ();
			dropdown.targetGraphic = image6;
			UIDefaultControls.SetDefaultColorTransitionValues (dropdown);
			dropdown.template = gameObject4.GetComponent<RectTransform> ();
			dropdown.captionText = text2;
			dropdown.itemText = text;
			text.text = "Option A";
			dropdown.options.Add (new Dropdown.OptionData {
				text = "Option A"
			});
			dropdown.options.Add (new Dropdown.OptionData {
				text = "Option B"
			});
			dropdown.options.Add (new Dropdown.OptionData {
				text = "Option C"
			});
			dropdown.RefreshShownValue ();
			RectTransform component3 = gameObject2.GetComponent<RectTransform> ();
			component3.anchorMin =(Vector2.zero);
			component3.anchorMax =(Vector2.one);
			component3.offsetMin =(new Vector2 (10, 6));
			component3.offsetMax =(new Vector2 (-25, -7));
			RectTransform component4 = gameObject3.GetComponent<RectTransform> ();
			component4.anchorMin =(new Vector2 (1, 0.5f));
			component4.anchorMax =(new Vector2 (1, 0.5f));
			component4.sizeDelta =(new Vector2 (20, 20));
			component4.anchoredPosition =(new Vector2 (-15, 0));
			RectTransform component5 = gameObject4.GetComponent<RectTransform> ();
			component5.anchorMin =(new Vector2 (0, 0));
			component5.anchorMax =(new Vector2 (1, 0));
			component5.pivot =(new Vector2 (0.5f, 1));
			component5.anchoredPosition =(new Vector2 (0, 2));
			component5.sizeDelta =(new Vector2 (0, 150));
			RectTransform component6 = gameObject5.GetComponent<RectTransform> ();
			component6.anchorMin =(new Vector2 (0, 0));
			component6.anchorMax =(new Vector2 (1, 1));
			component6.sizeDelta =(new Vector2 (-18, 0));
			component6.pivot =(new Vector2 (0, 1));
			RectTransform component7 = gameObject6.GetComponent<RectTransform> ();
			component7.anchorMin =(new Vector2 (0, 1));
			component7.anchorMax =(new Vector2 (1, 1));
			component7.pivot= (new Vector2 (0.5f, 1));
			component7.anchoredPosition =(new Vector2 (0, 0));
			component7.sizeDelta =(new Vector2 (0, 28));
			RectTransform component8 = gameObject7.GetComponent<RectTransform> ();
			component8.anchorMin =(new Vector2 (0, 0.5f));
			component8.anchorMax =(new Vector2 (1, 0.5f));
			component8.sizeDelta =(new Vector2 (0, 20));
			RectTransform component9 = gameObject8.GetComponent<RectTransform> ();
			component9.anchorMin =(Vector2.zero);
			component9.anchorMax =(Vector2.one);
			component9.sizeDelta =(Vector2.zero);
			RectTransform component10 = gameObject9.GetComponent<RectTransform> ();
			component10.anchorMin =(new Vector2 (0, 0.5f));
			component10.anchorMax =(new Vector2 (0, 0.5f));
			component10.sizeDelta =(new Vector2 (20, 20));
			component10.anchoredPosition =(new Vector2 (10, 0));
			RectTransform component11 = gameObject10.GetComponent<RectTransform> ();
			component11.anchorMin =(Vector2.zero);
			component11.anchorMax =(Vector2.one);
			component11.offsetMin =(new Vector2 (20, 1));
			component11.offsetMax =(new Vector2 (-10, -2));
			gameObject4.SetActive (false);
			return gameObject;
		}

		public static GameObject CreateImage (UIDefaultControls.Resources resources)
		{
			GameObject gameObject = UIDefaultControls.CreateUIElementRoot ("UISprite", UIDefaultControls.s_ImageElementSize);
			gameObject.AddComponent<UISprite> ();
			return gameObject;
		}

		public static GameObject CreateInputField (UIDefaultControls.Resources resources)
		{
			GameObject gameObject = UIDefaultControls.CreateUIElementRoot ("UIInputField", UIDefaultControls.s_ThickElementSize);
//			GameObject gameObject2 = UIDefaultControls.CreateUIObject ("Placeholder", gameObject);
			GameObject gameObject3 = UIDefaultControls.CreateUIObject ("UILabel", gameObject);
			UISprite image = gameObject.AddComponent<UISprite> ();
			image.sprite = resources.inputField;
			image.type = UISprite.Type.Sliced;
			image.color = UIDefaultControls.s_DefaultSelectableColor;
			UIInputField inputField = gameObject.AddComponent<UIInputField> ();
			UIDefaultControls.SetDefaultColorTransitionValues (inputField);
			UILabel text = gameObject3.AddComponent<UILabel> ();
			text.text = string.Empty;
			text.supportRichText = false;
			UIDefaultControls.SetDefaultTextValues (text);
//			UILabel text2 = gameObject2.AddComponent<UILabel> ();
//			text2.text = "Enter text...";
//			text.UseRTSprite = false;
//			text2.fontStyle =  FontStyle.BoldAndItalic;
//			Color color = text.color;
//			color.a *= 0.5f;
//			text2.color = color;
			RectTransform component = gameObject3.GetComponent<RectTransform> ();
			component.anchorMin =(Vector2.zero);
			component.anchorMax =(Vector2.one);
			component.sizeDelta =(Vector2.zero);
			component.offsetMin =(new Vector2 (10, 6));
			component.offsetMax =(new Vector2 (-10, -7));
//			RectTransform component2 = gameObject2.GetComponent<RectTransform> ();
//			component2.anchorMin =(Vector2.zero);
//			component2.anchorMax =(Vector2.one);
//			component2.sizeDelta =(Vector2.zero);
//			component2.offsetMin =(new Vector2 (10, 6));
//			component2.offsetMax =(new Vector2 (-10, -7));
			inputField.textComponent = text;
//			inputField.placeholder = text2;
			return gameObject;
		}

		public static GameObject CreatePanel (UIDefaultControls.Resources resources)
		{
			GameObject gameObject = UIDefaultControls.CreateUIElementRoot ("Panel", UIDefaultControls.s_ThickElementSize);
			RectTransform component = gameObject.GetComponent<RectTransform> ();
			component.anchorMin =(Vector2.zero);
			component.anchorMax =(Vector2.one);
			component.anchoredPosition =(Vector2.zero);
			component.sizeDelta =(Vector2.zero);
			UISprite image = gameObject.AddComponent<UISprite> ();
			image.sprite = resources.background;
			image.type = UISprite.Type.Sliced;
			image.color = UIDefaultControls.s_PanelColor;
			return gameObject;
		}

		public static GameObject CreateRawImage (UIDefaultControls.Resources resources)
		{
			GameObject gameObject = UIDefaultControls.CreateUIElementRoot ("RawImage", UIDefaultControls.s_ImageElementSize);
			gameObject.AddComponent<RawImage> ();
			return gameObject;
		}

		public static GameObject CreateScrollbar (UIDefaultControls.Resources resources)
		{
			GameObject gameObject = UIDefaultControls.CreateUIElementRoot ("Scrollbar", UIDefaultControls.s_ThinElementSize);
			GameObject gameObject2 = UIDefaultControls.CreateUIObject ("Sliding Area", gameObject);
			GameObject gameObject3 = UIDefaultControls.CreateUIObject ("Handle", gameObject2);
			UISprite image = gameObject.AddComponent<UISprite> ();
			image.sprite = resources.background;
			image.type = UISprite.Type.Sliced;
			image.color = UIDefaultControls.s_DefaultSelectableColor;
			UISprite image2 = gameObject3.AddComponent<UISprite> ();
			image2.sprite = resources.standard;
			image2.type = UISprite.Type.Sliced;
			image2.color = UIDefaultControls.s_DefaultSelectableColor;
			RectTransform component = gameObject2.GetComponent<RectTransform> ();
			component.sizeDelta =(new Vector2 (-20, -20));
			component.anchorMin =(Vector2.zero);
			component.anchorMax =(Vector2.one);
			RectTransform component2 = gameObject3.GetComponent<RectTransform> ();
			component2.sizeDelta =(new Vector2 (20, 20));
			Scrollbar scrollbar = gameObject.AddComponent<Scrollbar> ();
			scrollbar.handleRect = component2;
			scrollbar.targetGraphic = image2;
			UIDefaultControls.SetDefaultColorTransitionValues (scrollbar);
			return gameObject;
		}

		public static GameObject CreateScrollView (UIDefaultControls.Resources resources)
		{
			GameObject gameObject = UIDefaultControls.CreateUIElementRoot ("Scroll View", new Vector2 (200, 200));
			GameObject gameObject2 = UIDefaultControls.CreateUIObject ("Viewport", gameObject);
			GameObject gameObject3 = UIDefaultControls.CreateUIObject ("Content", gameObject2);
			GameObject gameObject4 = UIDefaultControls.CreateScrollbar (resources);
			gameObject4.name =("Scrollbar Horizontal");
			UIDefaultControls.SetParentAndAlign (gameObject4, gameObject);
			RectTransform component = gameObject4.GetComponent<RectTransform> ();
			component.anchorMin =(Vector2.zero);
			component.anchorMax =(Vector2.right);
			component.pivot =(Vector2.zero);
			component.sizeDelta =(new Vector2 (0, component.sizeDelta.y));
			GameObject gameObject5 = UIDefaultControls.CreateScrollbar (resources);
			gameObject5.name =("Scrollbar Vertical");
			UIDefaultControls.SetParentAndAlign (gameObject5, gameObject);
			gameObject5.GetComponent<Scrollbar> ().SetDirection (Scrollbar.Direction.BottomToTop, true);
			RectTransform component2 = gameObject5.GetComponent<RectTransform> ();
			component2.anchorMin =(Vector2.right);
			component2.anchorMax =(Vector2.one);
			component2.pivot =(Vector2.one);
			component2.sizeDelta =(new Vector2 (component2.sizeDelta.x, 0));
			RectTransform component3 = gameObject2.GetComponent<RectTransform> ();
			component3.anchorMin =(Vector2.zero);
			component3.anchorMax =(Vector2.one);
			component3.sizeDelta =(Vector2.zero);
			component3.pivot =(Vector2.up);
			RectTransform component4 = gameObject3.GetComponent<RectTransform> ();
			component4.anchorMin= (Vector2.up);
			component4.anchorMax= (Vector2.one);
			component4.sizeDelta= (new Vector2 (0, 300));
			component4.pivot= (Vector2.up);
			ScrollRect scrollRect = gameObject.AddComponent<ScrollRect> ();
			scrollRect.content = component4;
			scrollRect.viewport = component3;
			scrollRect.horizontalScrollbar = gameObject4.GetComponent<Scrollbar> ();
			scrollRect.verticalScrollbar = gameObject5.GetComponent<Scrollbar> ();
			scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
			scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
			scrollRect.horizontalScrollbarSpacing = -3;
			scrollRect.verticalScrollbarSpacing = -3;
			UISprite image = gameObject.AddComponent<UISprite> ();
			image.sprite = resources.background;
			image.type = UISprite.Type.Sliced;
			image.color = UIDefaultControls.s_PanelColor;
			Mask mask = gameObject2.AddComponent<Mask> ();
			mask.showMaskGraphic = false;
			UISprite image2 = gameObject2.AddComponent<UISprite> ();
			image2.sprite = resources.mask;
			image2.type = UISprite.Type.Sliced;
			return gameObject;
		}

		public static GameObject CreateSlider (UIDefaultControls.Resources resources)
		{
			GameObject gameObject = UIDefaultControls.CreateUIElementRoot ("UISlider", UIDefaultControls.s_ThinElementSize);
			GameObject gameObject2 = UIDefaultControls.CreateUIObject ("Background", gameObject);
			GameObject gameObject3 = UIDefaultControls.CreateUIObject ("Fill Area", gameObject);
			GameObject gameObject4 = UIDefaultControls.CreateUIObject ("Fill", gameObject3);
			GameObject gameObject5 = UIDefaultControls.CreateUIObject ("Handle Slide Area", gameObject);
			GameObject gameObject6 = UIDefaultControls.CreateUIObject ("Handle", gameObject5);
			UISprite image = gameObject2.AddComponent<UISprite> ();
			image.sprite = resources.background;
			image.type = UISprite.Type.Sliced;
			image.color = UIDefaultControls.s_DefaultSelectableColor;
			RectTransform component = gameObject2.GetComponent<RectTransform> ();
			component.anchorMin =(new Vector2 (0, 0.25f));
			component.anchorMax =(new Vector2 (1, 0.75f));
			component.sizeDelta =(new Vector2 (0, 0));
			RectTransform component2 = gameObject3.GetComponent<RectTransform> ();
			component2.anchorMin =(new Vector2 (0, 0.25f));
			component2.anchorMax =(new Vector2 (1, 0.75f));
			component2.anchoredPosition =(new Vector2 (-5, 0));
			component2.sizeDelta =(new Vector2 (-20, 0));
			UISprite image2 = gameObject4.AddComponent<UISprite> ();
			image2.sprite = resources.standard;
			image2.type = UISprite.Type.Sliced;
			image2.color = Color.green;
			RectTransform component3 = gameObject4.GetComponent<RectTransform> ();
			component3.sizeDelta =(new Vector2 (10, 0));
			RectTransform component4 = gameObject5.GetComponent<RectTransform> ();
			component4.sizeDelta =(new Vector2 (-20, 0));
			component4.anchorMin =(new Vector2 (0, 0));
			component4.anchorMax =(new Vector2 (1, 1));
			UISprite image3 = gameObject6.AddComponent<UISprite> ();
			image3.sprite = resources.knob;
			image3.color = UIDefaultControls.s_DefaultSelectableColor;
			RectTransform component5 = gameObject6.GetComponent<RectTransform> ();
			component5.sizeDelta= (new Vector2 (20, 0));
			UISlider slider = gameObject.AddComponent<UISlider> ();
			slider.value = 0.5f;
			slider.fillRect = gameObject4.GetComponent<RectTransform> ();
			slider.handleRect = gameObject6.GetComponent<RectTransform> ();
			slider.targetGraphic = image3;
			slider.direction = UISlider.Direction.LeftToRight;
			UIDefaultControls.SetDefaultColorTransitionValues (slider);
			return gameObject;
		}

		public static GameObject CreateText (UIDefaultControls.Resources resources)
		{
			GameObject gameObject = UIDefaultControls.CreateUIElementRoot ("UILabel", UIDefaultControls.s_ThickElementSize);
			UILabel text = gameObject.AddComponent<UILabel> ();
			text.text = "New UILabel";
			UIDefaultControls.SetDefaultTextValues (text);
			return gameObject;
		}

		public static GameObject CreateToggle (UIDefaultControls.Resources resources)
		{
			GameObject gameObject = UIDefaultControls.CreateUIElementRoot ("UIToggle", UIDefaultControls.s_ThinElementSize);
			GameObject gameObject2 = UIDefaultControls.CreateUIObject ("Background", gameObject);
			GameObject gameObject3 = UIDefaultControls.CreateUIObject ("Checkmark", gameObject2);
			GameObject gameObject4 = UIDefaultControls.CreateUIObject ("Label", gameObject);
			UIToggle toggle = gameObject.AddComponent<UIToggle> ();
			toggle.isOn = true;
			UISprite image = gameObject2.AddComponent<UISprite> ();
			image.sprite = resources.standard;
			image.type = UISprite.Type.Sliced;
			image.color = UIDefaultControls.s_DefaultSelectableColor;
			UISprite image2 = gameObject3.AddComponent<UISprite> ();
			image2.sprite = resources.checkmark;
			UILabel text = gameObject4.AddComponent<UILabel> ();
			text.text = "UIToggle";
			UIDefaultControls.SetDefaultTextValues (text);
			toggle.graphic = image2;
			toggle.targetGraphic = image;
			UIDefaultControls.SetDefaultColorTransitionValues (toggle);
			RectTransform component = gameObject2.GetComponent<RectTransform> ();
			component.anchorMin =(new Vector2 (0, 1));
			component.anchorMax =(new Vector2 (0, 1));
			component.anchoredPosition= (new Vector2 (10, -10));
			component.sizeDelta =(new Vector2 (20, 20));
			RectTransform component2 = gameObject3.GetComponent<RectTransform> ();
			component2.anchorMin =(new Vector2 (0.5f, 0.5f));
			component2.anchorMax =(new Vector2 (0.5f, 0.5f));
			component2.anchoredPosition= (Vector2.zero);
			component2.sizeDelta =(new Vector2 (20, 20));
			RectTransform component3 = gameObject4.GetComponent<RectTransform> ();
			component3.anchorMin =(new Vector2 (0, 0));
			component3.anchorMax =(new Vector2 (1, 1));
			component3.offsetMin =(new Vector2 (23, 1));
			component3.offsetMax=(new Vector2 (-5, -2));
			return gameObject;
		}

		private static GameObject CreateUIElementRoot (string name, Vector2 size)
		{
			GameObject gameObject = new GameObject (name);
			RectTransform rectTransform = gameObject.AddComponent<RectTransform> ();
			rectTransform.sizeDelta = (size);
			return gameObject;
		}

		private static GameObject CreateUIObject (string name, GameObject parent)
		{
			GameObject gameObject = new GameObject (name);
			gameObject.AddComponent<RectTransform> ();
			UIDefaultControls.SetParentAndAlign (gameObject, parent);
			gameObject.layer = LayerMask.NameToLayer ("UI");
			return gameObject;
		}

		private static void SetDefaultColorTransitionValues (Selectable slider)
		{
			ColorBlock colors = slider.colors;
			colors.highlightedColor = new Color (0.882f, 0.882f, 0.882f);
			colors.pressedColor = new Color (0.698f, 0.698f, 0.698f);
			colors.disabledColor = new Color (0.521f, 0.521f, 0.521f);
		}

		private static void SetDefaultTextValues (UILabel lbl)
		{
			lbl.color = UIDefaultControls.s_TextColor;
		}

		private static void SetLayerRecursively (GameObject go, int layer)
		{
			go.layer =  (layer);
			Transform transform = go.transform;
			for (int i = 0; i < transform.childCount; i++) {
				UIDefaultControls.SetLayerRecursively (transform.GetChild (i).gameObject, layer);
			}
		}

		private static void SetParentAndAlign (GameObject child, GameObject parent)
		{
			if (parent == null) {
				return;
			}
			child.transform.SetParent (parent.transform, false);
			UIDefaultControls.SetLayerRecursively (child, parent.layer);
		}

		//
		// Nested Types
		//
		public struct Resources
		{
			public Sprite standard;

			public Sprite background;

			public Sprite inputField;

			public Sprite knob;

			public Sprite checkmark;

			public Sprite dropdown;

			public Sprite mask;
		}
	}
}
