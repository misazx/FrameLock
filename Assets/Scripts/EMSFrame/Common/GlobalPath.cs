//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;

namespace UnityFrame
{
	public static class GlobalPath
	{
		/// for public
		public static readonly string AppPath = Application.dataPath;

		public static readonly string StreamingAssetsPath = Application.streamingAssetsPath;

		public static readonly string PersistentPath = Application.persistentDataPath;

		public static readonly string SystemCachePath = Application.temporaryCachePath;

		public static readonly string SpecifiResourcePath = "Assets/Prefabs/";

		public static readonly string ResourcePath = "Prefabs/";

		public static readonly string AssetBasesPath = Application.dataPath + "/AssetBases/";

		public static readonly string RawBundlePath = Application.streamingAssetsPath + "/BundleAssets/";

		public static readonly string RawScriptPath = Application.streamingAssetsPath + "/Runtimes/";

		public static readonly string RawPath = Application.streamingAssetsPath + "/";

		#if UNITY_EDITOR

		public static readonly string ResPersistentPath = Application.persistentDataPath + "/";

		public static readonly string CachePath = Application.persistentDataPath + "/Cache/";

		public static readonly string BundlePath = Application.dataPath + "/AssetBases/BundleAssets/";

		public static readonly string ScriptPath = Application.dataPath + "/Runtimes/";

		public static readonly string VoicePath = Application.persistentDataPath + "/Voices/";

		public static readonly string LocalPath = Application.persistentDataPath + "/Locals/";

		public static readonly string TexturePath = Application.persistentDataPath + "/Textures/";

#elif UNITY_STANDALONE
		// 手动修改路径  Resources  为 StreamingAssets
		public static readonly string ResPersistentPath = Application.dataPath + "/StreamingAssets/";

		public static readonly string CachePath = Application.dataPath + "/StreamingAssets/Cache/";

		public static readonly string BundlePath = Application.dataPath + "/StreamingAssets/BundleAssets/";

		public static readonly string ScriptPath = Application.dataPath + "/StreamingAssets/Runtimes/";

		public static readonly string VoicePath = Application.dataPath + "/StreamingAssets/Voices/";

		public static readonly string LocalPath = Application.dataPath + "/StreamingAssets/Locals/";

		public static readonly string TexturePath = Application.dataPath + "/StreamingAssets/Textures/";
#else
		public static readonly string ResPersistentPath = Application.persistentDataPath + "/";

		public static readonly string CachePath = Application.persistentDataPath + "/Cache/";

		public static readonly string BundlePath = Application.persistentDataPath + "/BundleAssets/";

		public static readonly string ScriptPath = Application.persistentDataPath + "/Runtimes/";

		public static readonly string VoicePath = Application.persistentDataPath + "/Voices/";

		public static readonly string LocalPath = Application.persistentDataPath + "/Locals/";

		public static readonly string TexturePath = Application.persistentDataPath + "/Textures/";
#endif
	}
}

