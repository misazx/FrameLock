//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;

namespace UnityFrame {
    //资源数据库
    //扫码可用的资源文件
    public static class AssetDataBases
    {
        [System.Flags]
        public enum AssetFileType {
            None = 0,
            Bundle = 1,
            Runtimes = 2,
            Rebundle = 4,
        }

		public sealed class AssetFileInfo {
            public AssetFileType type;           //类型0-assetbundle,1-lua,2-rebundle
			public string name;        //真实名字
            public string alias;       //别名
            public string absName;     //资源相对路径名字
            public string path;        //资源文件全路径

            public AssetFileInfo(AssetFileType t,string n, string a, string abs, string pa) {
                type = t;
                name = n;
                alias = a;
                absName = abs;
                path = pa;
            }

            public override string ToString()
            {
                return string.Format("AssetFileInfo[{0}]", name);
            }
        }

		//资源名映射<名字唯一>
		private static Dictionary<string, AssetFileInfo> s_DicAssetsMap = new Dictionary<string, AssetFileInfo>();
        //资源名字列表，允许存在多个相同名字
        private static List<AssetFileInfo> s_ListAssetsMap = new List<AssetFileInfo>();

        private static AssetFileType UF_WarpAssetType(string chunk)
        {
            if (chunk == "1")
            {
                return AssetFileType.Bundle;
            }
            else if (chunk == "2")
            {
                return AssetFileType.Runtimes;
            }
            else if (chunk == "3")
            {
                return AssetFileType.Rebundle;
            }
            else
                return AssetFileType.None;
        }


        private static bool UF_CheckIsAssetFile(string chunk) {
            string extension = Path.GetExtension(chunk);
            if (extension == ".meta" || extension == ".DS_Store") return false;
            else return true;
        }

        //通过加载本地持久化路径下资源目录结构获取所有资源信息
        internal static void UF_LoadAssetInfoFromPersistent() {
            Debugger.UF_Log("Load All AssetInfo From Persistent");

            //重置所有路径,避免已删除文件误加载
            //foreach (var v in s_DicAssetsMap.Values) {
            //    v.path = string.Empty;
            //}
            s_DicAssetsMap.Clear();
            s_ListAssetsMap.Clear();
            Debugger.UF_Log("GlobalPath.BundlePath:" + GlobalPath.BundlePath);
            //读取Bundle资源信息
            GHelper.UF_ForeachFloder(GlobalPath.BundlePath, (e) => {
                if (UF_CheckIsAssetFile(e)) {
                    string fName = Path.GetFileName(e);
                    string absPath = e.Substring(e.IndexOf("BundleAssets", System.StringComparison.Ordinal));
                    var afi = new AssetFileInfo(AssetFileType.Bundle, fName, fName, absPath, e);
                    s_ListAssetsMap.Add(afi);

                    if (!s_DicAssetsMap.ContainsKey(fName))
                    {
                        s_DicAssetsMap.Add(fName, afi);
                    }
                    else
                    {
                        //替换为持久化路径
                        //s_DicAssetsMap[fName].path = e;
                        Debugger.UF_Error(string.Format("AssetDataBase Error -> Same Assets File Name: {0}", fName));
                    }
                }
            });

            //读取Lua代码资源信息
            GHelper.UF_ForeachFloder(GlobalPath.ScriptPath, (e) => {
                if (UF_CheckIsAssetFile(e)) {
                    string fName = Path.GetFileName(e);
                    if (Path.GetExtension(fName) == ".lua") {
                        string absPath = e.Substring(e.IndexOf("Runtimes", System.StringComparison.Ordinal));
                        var afi = new AssetFileInfo(AssetFileType.Runtimes, fName, fName, absPath, e);
                        s_ListAssetsMap.Add(afi);

                        if (!s_DicAssetsMap.ContainsKey(fName))
                        {
                            s_DicAssetsMap.Add(fName, new AssetFileInfo(AssetFileType.Runtimes, fName, fName, absPath, e));
                        }
                        else
                        {
                            //s_DicAssetsMap[fName].path = e;
                            Debugger.UF_Error(string.Format("AssetDataBase Error -> Same Lua File Name: {0}", fName));
                        }
                    }
                }
            });
        }


        //通过Raw中载入资源信息表，获取所有资源信息
        internal static void UF_LoadAssetInfoFormAssetTable(string path)
        {
            Debugger.UF_Log("Load All AssetInfo From AssetTable");

            if (!File.Exists(path)) {
                Debugger.UF_Warn("AssetData file not exist: "+path);
                return;
            }            
            try
            {
                s_DicAssetsMap.Clear();
                s_ListAssetsMap.Clear();
                string rawPath = Path.GetDirectoryName(path);
                //为避免路径变更，先遍历RAW文件夹中所有资源名映射路径，生成资源路径映射表
                Dictionary<string, string> rawFileTable = new Dictionary<string, string>();
                GHelper.UF_ForeachFloder(rawPath, (e) => {
                    string name = Path.GetFileName(e);
                    if (!rawFileTable.ContainsKey(name))
                    {
                        rawFileTable.Add(name, e);
                    }
                    else
                    {
                        Debugger.UF_Error(string.Format("AssetDataBase Error -> Same raw file name[{0}]", name));
                    }
                });

                //加载assetdata文件
                var fs = File.Open(path, FileMode.Open, FileAccess.Read);
                byte[] bytedata = new byte[fs.Length];
                fs.Read(bytedata, 0, (int)fs.Length);
                fs.Close();
                //解密文件
                GHelper.UF_BytesKey(bytedata, GlobalSettings.ConfigEncBKey);
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(bytedata))
                {
                    using (var sr = new StreamReader(ms))
                    {
                        string line = "";
                        while (null != (line = sr.ReadLine()))
                        {
                            var arrparams = GHelper.UF_SplitStringWithCount(line.Trim(), 3, ';');
                            var aft = UF_WarpAssetType(arrparams[0]);
                            //alias name
                            string aliasname = arrparams[1];
                            // asset abs name
                            string absname = arrparams[2];
                            //asset real name
                            string assetname = Path.GetFileName(absname);

                            string assetpath = "";
                            if (rawFileTable.ContainsKey(aliasname))
                            {
                                //映射文件路径
                                assetpath = rawFileTable[aliasname];
                            }
                            else
                            {
                                Debugger.UF_Error(string.Format("AssetDataBase Error -> Can not find file name[{0}] in raw file table", aliasname));
                                continue;
                            }

                            var afi = new AssetFileInfo(aft, assetname, aliasname, absname, assetpath);
                            s_ListAssetsMap.Add(afi);
                            //资源文件名必须唯一，否则会导致资读取失败
                            if (!s_DicAssetsMap.ContainsKey(assetname))
                            {
                                s_DicAssetsMap.Add(afi.name, afi);
                            }
                            else
                            {
                                Debugger.UF_Error(string.Format("AssetDataBase Error -> Same Key in Asset Table! AssetName[{0}] AliasName[{1}]", assetname, aliasname));
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debugger.UF_Exception(e);
            }
        }


        
        public static AssetFileInfo UF_GetAssetInfo(string assetName) {
            //如果存在多个同名字到资源，则可能会被后者覆盖
            if (s_DicAssetsMap.ContainsKey(assetName))
                return s_DicAssetsMap[assetName];
            else
                return default(AssetFileInfo);
        }


        public static int UF_GetAssetInfoCount(AssetFileType flag) {
            int count = 0;
            foreach (AssetFileInfo afi in s_ListAssetsMap)
            {
                if ((afi.type & flag) > 0)
                {
                    count++;
                }
            }
            return count;
        }


        public static AssetFileInfo UF_GetAssetInfoInAlias(string aliasName) {
            foreach (var v in s_ListAssetsMap) {
                if (v.alias == aliasName)
                    return v;
            }
            return default(AssetFileInfo);
        }


        //获取全部资源
        public static List<AssetFileInfo> UF_GetAllAssets(AssetFileType flag) {
            List<AssetFileInfo> list = new List<AssetFileInfo>();
            foreach (AssetFileInfo afi in s_ListAssetsMap)
            {
                if ((afi.type & flag) > 0)
                {
                    list.Add(afi);
                }
            }
            return list;
        }

        //获取所有可用资源文件
        public static List<AssetFileInfo> UF_GetAllBundleInfos() {
            return UF_GetAllAssets(AssetFileType.Bundle);
        }

        //获取全部可执行的脚本文件,返回文件的信息
		public static List<AssetFileInfo> UF_GetAllRuntimeInfos() {
            return UF_GetAllAssets(AssetFileType.Runtimes);
        }

		//获取所有可用资源替换文件
		public static List<AssetFileInfo> UF_GetAllRebundleInfos()
		{
            return UF_GetAllAssets(AssetFileType.Rebundle);
        }


	}
}
