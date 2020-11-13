using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;

// /////////////////////////////////////////////////////////////////////////////////////////////////////////
//
// Batch Texture import settings modifier.
//
// Modifies all selected textures in the project window and applies the requested modification on the 
// textures. Idea was to have the same choices for multiple files as you would have if you open the 
// import settings of a single texture. Put this into Assets/Editor and once compiled by Unity you find
// the new functionality in GameTools -> Texture. Enjoy! :-)
// 
// Based on the great work of benblo in this thread: 
// http://forum.unity3d.com/viewtopic.php?t=16079&start=0&postdays=0&postorder=asc&highlight=textureimporter
// 
// Developed by Martin Schultz, Decane in August 2009
// e-mail: ms@decane.net
//
// Updated for Unity 3.0 by col000r in August 2010
// http://col000r.blogspot.com
//
// /////////////////////////////////////////////////////////////////////////////////////////////////////////
public class EditorTextureChange : ScriptableObject {
	
//	
//	[MenuItem ("GameTools/Texture/Change Texture Format/RGB Compressed DXT1")]
//	static void ChangeTextureFormat_RGB_DXT1() { 
//		SelectedChangeTextureFormatSettings(TextureImporterFormat.DXT1);
//	}
//	
//	[MenuItem ("GameTools/Texture/Change Texture Format/RGB Compressed DXT5")]
//	static void ChangeTextureFormat_RGB_DXT5() { 
//		SelectedChangeTextureFormatSettings(TextureImporterFormat.DXT5);
//	}
//	
//	[MenuItem ("GameTools/Texture/Change Texture Format/RGB 16 bit")]
//	static void ChangeTextureFormat_RGB_16bit() { 
//		SelectedChangeTextureFormatSettings(TextureImporterFormat.RGB16);
//	}
//	
//	[MenuItem ("GameTools/Texture/Change Texture Format/RGB 24 bit")]
//	static void ChangeTextureFormat_RGB_24bit() { 
//		SelectedChangeTextureFormatSettings(TextureImporterFormat.RGB24);
//	}
//	
//	[MenuItem ("GameTools/Texture/Change Texture Format/Alpha 8 bit")]
//	static void ChangeTextureFormat_Alpha_8bit() { 
//		SelectedChangeTextureFormatSettings(TextureImporterFormat.Alpha8);
//	}
//	
//	[MenuItem ("GameTools/Texture/Change Texture Format/ARGB 16 bit")]
//	static void ChangeTextureFormat_RGBA_16bit() { 
//		SelectedChangeTextureFormatSettings(TextureImporterFormat.ARGB16);
//	}
//	
//	[MenuItem ("GameTools/Texture/Change Texture Format/RGBA 32 bit")]
//	static void ChangeTextureFormat_RGBA_32bit() { 
//		SelectedChangeTextureFormatSettings(TextureImporterFormat.RGBA32);
//	}
//	
//	[MenuItem ("GameTools/Texture/Change Texture Format/ARGB 32 bit")]
//	static void ChangeTextureFormat_ARGB_32bit() { 
//		SelectedChangeTextureFormatSettings(TextureImporterFormat.ARGB32);
//	}
//	
//	[MenuItem ("GameTools/Texture/Change Texture Format/RGB PVRTC 2bit")]
//	static void ChangeTextureFormat_RGB_PVRTC_2bit() { 
//		SelectedChangeTextureFormatSettings(TextureImporterFormat.PVRTC_RGB2);
//	}
//	
//	[MenuItem ("GameTools/Texture/Change Texture Format/RGBA PVRTC 2bit")]
//	static void ChangeTextureFormat_RGBA_PVRTC_2bit() { 
//		SelectedChangeTextureFormatSettings(TextureImporterFormat.PVRTC_RGBA2);
//	}    
//	
//	[MenuItem ("GameTools/Texture/Change Texture Format/RGB PVRTC 4bit")]
//	static void ChangeTextureFormat_RGB_PVRTC_4bit() { 
//		SelectedChangeTextureFormatSettings(TextureImporterFormat.PVRTC_RGB4);
//	}
//	
//	[MenuItem ("GameTools/Texture/Change Texture Format/RGBA PVRTC 4bit")]
//	static void ChangeTextureFormat_RGBA_PVRTC_4bit() { 
//		SelectedChangeTextureFormatSettings(TextureImporterFormat.PVRTC_RGBA4);
//	}
//    [MenuItem("GameTools/Texture/Change Texture Format/ETC 4bit")]
//    static void ChangeTextureFormat_ETC_4bit()
//    {
//        SelectedChangeTextureFormatSettings(TextureImporterFormat.ETC_RGB4);
//    }
	
	// ----------------------------------------------------------------------------
	
	[MenuItem ("GameTools/Texture/Change Texture Size/Change Max Texture Size/32")]
	static void ChangeTextureSize_32() { 
		SelectedChangeMaxTextureSize(32);
	}
	
	[MenuItem ("GameTools/Texture/Change Texture Size/Change Max Texture Size/64")]
	static void ChangeTextureSize_64() { 
		SelectedChangeMaxTextureSize(64);
	}
	
	[MenuItem ("GameTools/Texture/Change Texture Size/Change Max Texture Size/128")]
	static void ChangeTextureSize_128() { 
		SelectedChangeMaxTextureSize(128);
	}
	
	[MenuItem ("GameTools/Texture/Change Texture Size/Change Max Texture Size/256")]
	static void ChangeTextureSize_256() { 
		SelectedChangeMaxTextureSize(256);
	}
	
	[MenuItem ("GameTools/Texture/Change Texture Size/Change Max Texture Size/512")]
	static void ChangeTextureSize_512() { 
		SelectedChangeMaxTextureSize(512);
	}
	
	[MenuItem ("GameTools/Texture/Change Texture Size/Change Max Texture Size/1024")]
	static void ChangeTextureSize_1024() { 
		SelectedChangeMaxTextureSize(1024);
	}
	
	[MenuItem ("GameTools/Texture/Change Texture Size/Change Max Texture Size/2048")]
	static void ChangeTextureSize_2048() { 
		SelectedChangeMaxTextureSize(2048);
	}
	
	// ----------------------------------------------------------------------------
	
	[MenuItem ("GameTools/Texture/Change MipMap/Enable MipMap")]
	static void ChangeMipMap_On() { 
		SelectedChangeMimMap(true);
	}
	
	[MenuItem ("GameTools/Texture/Change MipMap/Disable MipMap")]
	static void ChangeMipMap_Off() { 
		SelectedChangeMimMap(false);
	}
	
	// ----------------------------------------------------------------------------
	
	
	[MenuItem ("GameTools/Texture/Change Non Power of 2/None")]
	static void ChangeNPOT_None() { 
		SelectedChangeNonPowerOf2(TextureImporterNPOTScale.None);
	}
	
	[MenuItem ("GameTools/Texture/Change Non Power of 2/ToNearest")]
	static void ChangeNPOT_ToNearest() { 
		SelectedChangeNonPowerOf2(TextureImporterNPOTScale.ToNearest);
	}
	
	[MenuItem ("GameTools/Texture/Change Non Power of 2/ToLarger")]
	static void ChangeNPOT_ToLarger() { 
		SelectedChangeNonPowerOf2(TextureImporterNPOTScale.ToLarger);
	}
	
	[MenuItem ("GameTools/Texture/Change Non Power of 2/ToSmaller")]
	static void ChangeNPOT_ToSmaller() { 
		SelectedChangeNonPowerOf2(TextureImporterNPOTScale.ToSmaller);
	}    
	
	// ----------------------------------------------------------------------------
	
	[MenuItem ("GameTools/Texture/Change Is Readable/Enable")]
	static void ChangeIsReadable_Yes() { 
		SelectedChangeIsReadable(true);
	}
	
	[MenuItem ("GameTools/Texture/Change Is Readable/Disable")]
	static void ChangeIsReadable_No() { 
		SelectedChangeIsReadable(false);
	}    
	
	// ----------------------------------------------------------------------------


    [MenuItem("GameTools/Texture/Change FilterMode/Bilinear")]
    static void FilterMode_Bilinear()
    {
        SelectedFilterMode(FilterMode.Bilinear);
    }

    [MenuItem("GameTools/Texture/Change FilterMode/Point")]
    static void FilterMode_Point()
    {
        SelectedFilterMode(FilterMode.Point);
    }

    [MenuItem("GameTools/Texture/Change FilterMode/Trilinear")]
    static void FilterMode_Trilinear()
    {
        SelectedFilterMode(FilterMode.Trilinear);
    }

    // ----------------------------------------------------------------------------


    [MenuItem("GameTools/Texture/Change WrapMode/Clamp")]
    static void WrapMode_Clamp()
    {
        SelectedWrapMode(TextureWrapMode.Clamp);
    }

    [MenuItem("GameTools/Texture/Change WrapMode/Repeat")]
    static void WrapMode_Repeat()
    {
        SelectedWrapMode(TextureWrapMode.Repeat);
    }

    // ----------------------------------------------------------------------------

    [MenuItem("GameTools/Texture/Change ClampAndBilinear")]
    static void Processor_ClampAndBilinear()
    {
        Object[] textures = GetSelectedTextures();
        Selection.objects = new Object[0];
        foreach (Texture2D texture in textures)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (textureImporter != null)
            {
                textureImporter.textureType = TextureImporterType.Default;
                textureImporter.wrapMode = TextureWrapMode.Clamp;
                textureImporter.filterMode = FilterMode.Bilinear;
                AssetDatabase.ImportAsset(path);
            }
        }
    }
		


    // ----------------------------------------------------------------------------

    static void SelectedWrapMode(TextureWrapMode tarMode)
    {

        Object[] textures = GetSelectedTextures();
        Selection.objects = new Object[0];
        foreach (Texture2D texture in textures)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (textureImporter != null)
            {
                textureImporter.wrapMode = tarMode;
                textureImporter.textureType = TextureImporterType.Default;
                AssetDatabase.ImportAsset(path);
            }
        }
    }

    static void SelectedFilterMode(FilterMode tarMode)
    {

        Object[] textures = GetSelectedTextures();
        Selection.objects = new Object[0];
        foreach (Texture2D texture in textures)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (textureImporter != null)
            {
                textureImporter.textureType = TextureImporterType.Default;
                textureImporter.filterMode = tarMode;
                AssetDatabase.ImportAsset(path);
            }
        }
    }
	
	static void SelectedChangeIsReadable(bool enabled) { 
		
		Object[] textures = GetSelectedTextures(); 
		Selection.objects = new Object[0];
		foreach (Texture2D texture in textures)  {
			string path = AssetDatabase.GetAssetPath(texture); 
			TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter; 
			textureImporter.isReadable = enabled;    
			AssetDatabase.ImportAsset(path); 
		}
	}
	
	
	static void SelectedChangeNonPowerOf2(TextureImporterNPOTScale npot) { 
		
		Object[] textures = GetSelectedTextures(); 
		Selection.objects = new Object[0];
		foreach (Texture2D texture in textures)  {
			string path = AssetDatabase.GetAssetPath(texture); 
			TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter; 
			textureImporter.npotScale = npot;    
			AssetDatabase.ImportAsset(path); 
		}
	}
	
	static void SelectedChangeMimMap(bool enabled) { 
		
		Object[] textures = GetSelectedTextures(); 
		Selection.objects = new Object[0];
		foreach (Texture2D texture in textures)  {
			string path = AssetDatabase.GetAssetPath(texture); 
			TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter; 
			textureImporter.mipmapEnabled = enabled;    
			AssetDatabase.ImportAsset(path); 
		}
	}
	
	static void SelectedChangeMaxTextureSize(int size) { 
		
		Object[] textures = GetSelectedTextures(); 
		Selection.objects = new Object[0];
		foreach (Texture2D texture in textures)  {
			string path = AssetDatabase.GetAssetPath(texture); 
			TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter; 
			textureImporter.maxTextureSize = size;  
			AssetDatabase.ImportAsset(path); 
		}
	}
	
	static void SelectedChangeTextureFormatSettings(TextureImporterFormat newFormat) { 
		
		Object[] textures = GetSelectedTextures(); 
		Selection.objects = new Object[0];
		foreach (Texture2D texture in textures)  {
			string path = AssetDatabase.GetAssetPath(texture); 
			TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            textureImporter.textureType = TextureImporterType.Default;
//			textureImporter.textureFormat = newFormat;  
			AssetDatabase.ImportAsset(path); 
		}
	}
	
	static Object[] GetSelectedTextures() 
	{ 
		return Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets); 
	}

}