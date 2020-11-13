using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AssetImportProcessor : AssetPostprocessor {

	private string[] looptimemark = { "run", "idle","walk"};

	private bool NeedLoopTime(string name){
		foreach(string value in looptimemark){
			if (name.IndexOf (value) > -1) {
				return true;
			}
		}
		return false;
	}

	//导入模型的时候进行动画压缩优化
	void OnPostprocessModel(GameObject g)
	{
		List<AnimationClip> animationClipList = new List<AnimationClip>(AnimationUtility.GetAnimationClips(g));
		if (animationClipList.Count == 0) {
			AnimationClip[] objectList = UnityEngine.Object.FindObjectsOfType (typeof(AnimationClip)) as AnimationClip[];
			animationClipList.AddRange(objectList);
		}

		foreach (AnimationClip theAnimation in animationClipList)
		{

			try 
			{
				//去除scale曲线
				//				foreach (EditorCurveBinding theCurveBinding in AnimationUtility.GetCurveBindings(theAnimation))
				//				{
				//					string name = theCurveBinding.propertyName.ToLower();
				//					if (name.Contains("scale"))
				//					{
				//						AnimationUtility.SetEditorCurve(theAnimation, theCurveBinding, null);
				//					}
				//				} 

				//浮点数精度压缩到f3
				AnimationClipCurveData[] curves = null;
				curves = AnimationUtility.GetAllCurves(theAnimation);
				Keyframe key;
				Keyframe[] keyFrames;
				for (int ii = 0; ii < curves.Length; ++ii)
				{
					AnimationClipCurveData curveDate = curves[ii];
					if (curveDate.curve == null || curveDate.curve.keys == null)
					{
						//Debug.LogWarning(string.Format("AnimationClipCurveData {0} don't have curve; Animation name {1} ", curveDate, animationPath));
						continue;
					}
					keyFrames = curveDate.curve.keys;
					for (int i = 0; i < keyFrames.Length; i++)
					{
						key = keyFrames[i];
						key.value = float.Parse(key.value.ToString("f3"));
						key.inTangent = float.Parse(key.inTangent.ToString("f3"));
						key.outTangent = float.Parse(key.outTangent.ToString("f3"));
						keyFrames[i] = key;
					}
					curveDate.curve.keys = keyFrames;
					theAnimation.SetCurve(curveDate.path, curveDate.type, curveDate.propertyName, curveDate.curve);
				}
			}
			catch (System.Exception e)
			{
				Debug.LogError(string.Format("CompressAnimationClip Failed !!! animationPath : {0} error: {1}", assetPath, e));
			}
		}

	}

	//导入模型的时候设置固定参数
	public void OnPreprocessModel()
	{
		ModelImporter modelImporter = (ModelImporter) assetImporter;     
		//		modelImporter.animationType = ModelImporterAnimationType.Generic;
		ModelImporterClipAnimation[] animations = modelImporter.clipAnimations;
		modelImporter.animationCompression = ModelImporterAnimationCompression.Optimal;
		for (int k = 0; k < animations.Length; k++) {
			if (NeedLoopTime (animations [k].name)) {
				animations [k].loopTime = true;

			}
		}
		AssetDatabase.SaveAssets ();
	}


	private TextureImporterPlatformSettings getStandaloneTIPS(){
		TextureImporterPlatformSettings stdtps = new TextureImporterPlatformSettings ();
		stdtps.name = "Standalone";
		stdtps.overridden = true;
		stdtps.format = TextureImporterFormat.RGBA32;
		return stdtps;
	}

	private TextureImporterPlatformSettings getIPhoneTIPS(){
		TextureImporterPlatformSettings iostps = new TextureImporterPlatformSettings ();
		iostps.name = "iPhone";
		iostps.overridden = true;
		//		iostps.compressionQuality = 0;
		iostps.format = TextureImporterFormat.RGBA32;
		return iostps;
	}

	private TextureImporterPlatformSettings getIPhoneAtlasTIPS(string path){
		string name = System.IO.Path.GetFileNameWithoutExtension (path);
		TextureImporterPlatformSettings iostps = new TextureImporterPlatformSettings ();
		iostps.name = "iPhone";
		iostps.overridden = true;
		//		iostps.compressionQuality = 0;
		if (name == "ui_atlas_icon_0" || name == "ui_atlas_pic_1" || name == "ui_atlas_pic_2") {
			iostps.format = TextureImporterFormat.RGBA32;
		} else {
			iostps.compressionQuality = (int)UnityEditor.TextureCompressionQuality.Fast;
			iostps.format = TextureImporterFormat.RGBA32;
		}
		return iostps;
	}

	private TextureImporterPlatformSettings getAndroidTIPS(){
		TextureImporterPlatformSettings androidtps = new TextureImporterPlatformSettings ();
		androidtps.name = "Android";
		androidtps.overridden = true;
		//		androidtps.compressionQuality = 0;
		androidtps.format = TextureImporterFormat.RGBA32;
		return androidtps;
	}

	private int getAvatarMaxTextureSize(string name,int curSize){
		if (name.IndexOf ("_mask") > -1) {
			return 128;
		}
		if (curSize > 512) {
			return 512;
		} else {
			return curSize;
		}
	}

	public void OnPreprocessTexture(){
		TextureImporter textureImporter = (TextureImporter) assetImporter; 

		textureImporter.mipmapEnabled = false;
		// textureImporter.wrapMode = TextureWrapMode.Clamp;
//
//		string filename = System.IO.Path.GetFileName (textureImporter.assetPath);
//		if (filename.IndexOf ("ava_") > -1) {
//			//			textureImporter.maxTextureSize = 512;
//			textureImporter.wrapMode = TextureWrapMode.Clamp;
//
//			TextureImporterPlatformSettings tips = default(TextureImporterPlatformSettings);
//			//win端 设置
//			tips = getStandaloneTIPS();
//			tips.maxTextureSize = 512;
//			textureImporter.SetPlatformTextureSettings (tips);
//			//iOS 设置
//			tips = getIPhoneTIPS();
//			var defaultSetting = textureImporter.GetPlatformTextureSettings ("iPhone");
//			tips.maxTextureSize = getAvatarMaxTextureSize(filename,defaultSetting.maxTextureSize);
//			tips.compressionQuality = defaultSetting.compressionQuality;
//			textureImporter.SetPlatformTextureSettings (tips);
//
//			//安卓设置
//			tips = getAndroidTIPS();
//			tips.maxTextureSize = getAvatarMaxTextureSize(filename,textureImporter.GetPlatformTextureSettings ("Android").maxTextureSize);
//			tips.compressionQuality = (int)TextureCompressionQuality.Normal;
//			textureImporter.SetPlatformTextureSettings (tips);
//		}
//
//		if (textureImporter.assetPath.IndexOf ("ui_atlas_") > -1) {
//			textureImporter.wrapMode = TextureWrapMode.Clamp;
//			TextureImporterPlatformSettings tips = default(TextureImporterPlatformSettings);
//			//win端 设置
//			textureImporter.SetPlatformTextureSettings (getStandaloneTIPS());
//			//iOS 设置
//			tips = getIPhoneAtlasTIPS(textureImporter.assetPath);
//			textureImporter.SetPlatformTextureSettings (tips);
//			//安卓设置
//			tips = getAndroidTIPS();
//			tips.compressionQuality = (int)TextureCompressionQuality.Normal;
//			textureImporter.SetPlatformTextureSettings (getAndroidTIPS());
//		}
	}

	public void OnPreprocessAudio(){
		AudioImporter audioImporter = (AudioImporter) assetImporter;   
		AudioImporterSampleSettings settings = audioImporter.defaultSampleSettings;
		if (audioImporter.assetPath.IndexOf ("bg") > -1) {
			//			audioImporter.forceToMono = true;
			audioImporter.loadInBackground = true;
			settings.loadType = AudioClipLoadType.CompressedInMemory;
			settings.compressionFormat = AudioCompressionFormat.ADPCM;
		} else {
			audioImporter.forceToMono = true;
			audioImporter.loadInBackground = false;
			settings.loadType = AudioClipLoadType.DecompressOnLoad;
			settings.compressionFormat = AudioCompressionFormat.ADPCM;
		}
		audioImporter.defaultSampleSettings = settings;


	}




}
