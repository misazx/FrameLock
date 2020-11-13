//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;  
using System.Text.RegularExpressions;
using System.IO;
using System.Text;

namespace UnityFrame{
    public static class GHelper {

        public static bool UF_FileExist(string path) {
            return File.Exists(path);
        }

        public static bool UF_FolderExist(string path) {
            return UF_FolderExist(path, false);
        }

        public static bool UF_FolderExist(string path, bool createWhileNull) {
            DirectoryInfo di = new DirectoryInfo(path);
            if (di.Exists) {
                return true;
            } else {
                if (createWhileNull) {
                    try {
                        di.Create();
                        return true;
                    }
                    catch (System.Exception e) {
                        Debugger.UF_Exception(e);
                    }
                }
            }
            return false;
        }

        public static bool UF_DeleteFolder(string path) {
            DirectoryInfo di = new DirectoryInfo(path);
            return UF_DeleteFolderDI(di);
        }

        public static bool UF_DeleteFolderDI(DirectoryInfo di) {
            try {
                if (di.Exists) {
                    FileInfo[] fis = di.GetFiles();
                    for (int k = 0; k < fis.Length; k++) {
                        fis[k].Delete();
                    }
                    DirectoryInfo[] dis = di.GetDirectories();
                    for (int k = 0; k < dis.Length; k++) {
                        UF_DeleteFolderDI(dis[k]);
                    }
                    di.Delete();
                }
                return true;
            } catch (System.Exception e) {
                Debugger.UF_Exception(e);
                return false;
            }
        }

        public static bool UF_CopyFolder(string SourcePath, string DestinationPath, bool overwrite)
        {
            bool ret = false;
            try
            {
                //				SourcePath = SourcePath.EndsWith(@"\") ? SourcePath : SourcePath + @"\";  
                //				DestinationPath = DestinationPath.EndsWith(@"\") ? DestinationPath : DestinationPath + @"\";  
                if (Directory.Exists(SourcePath))
                {
                    if (Directory.Exists(DestinationPath) == false)
                        Directory.CreateDirectory(DestinationPath);

                    foreach (string fls in Directory.GetFiles(SourcePath))
                    {
                        FileInfo flinfo = new FileInfo(fls);
                        flinfo.CopyTo(DestinationPath + "/" + flinfo.Name, overwrite);
                    }
                    foreach (string drs in Directory.GetDirectories(SourcePath))
                    {
                        DirectoryInfo drinfo = new DirectoryInfo(drs);
                        if (UF_CopyFolder(drs, DestinationPath + "/" + drinfo.Name, overwrite) == false)
                            ret = false;
                    }
                }
                ret = true;
            }
            catch (System.Exception ex)
            {
                Debugger.UF_Exception(ex);
                ret = false;
            }
            return ret;
        }

        public static void UF_ForeachFloder(string pathDirectory, DelegateStringMethod invokeMethod)
        {
            try
            {
                if (Directory.Exists(pathDirectory))
                {
                    foreach (string fls in Directory.GetFiles(pathDirectory))
                    {
                        string fpath = fls;
#if UNITY_STANDALONE || UNITY_EDITOR
                        fpath = fpath.Replace("\\", "/");
#endif
                        if (invokeMethod != null) { invokeMethod(fpath); }
                    }
                    foreach (string drs in Directory.GetDirectories(pathDirectory))
                    {
                        UF_ForeachFloder(drs, invokeMethod);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debugger.UF_Exception(ex);
            }
        }

        public static string UF_GetDirectoryName(string path)
        {
            path = Path.GetDirectoryName(path);
#if UNITY_STANDALONE || UNITY_EDITOR
            path = path.Replace("\\", "/");
#endif
            return path;
        }


        private static void UF_CopyFile(string strForm, string strTo) {
            if (File.Exists(strForm)) {
                File.Copy(strForm, strTo, true);
            }
        }

        private static void UF_DeleteFile(string filePath) {
            if (File.Exists(filePath)) {
                File.Delete(filePath);
            }
        }


        public static string UF_OpenFileString(string filePath) {
            string ret = "";
            try {
                StreamReader sr = new StreamReader(filePath, Encoding.UTF8);
                ret = sr.ReadToEnd();
                sr.Close();
            }
            catch (System.Exception e) {
                Debugger.UF_Exception(e);
            }
            return ret;
        }

        public static bool UF_SaveFileString(string context, bool append, string filePath) {
            try {
                StreamWriter sw = new StreamWriter(filePath, append, Encoding.UTF8);
                sw.Write(filePath);
                sw.Close();
            }
            catch (System.Exception e) {
                Debugger.UF_Exception(e);
                return false;
            }
            return true;
        }

        public static bool UF_TickTime(ref float tickBuffer, float duration, float deltaTime) {
            tickBuffer += deltaTime;
            if (tickBuffer >= duration) {
                tickBuffer = 0;
                return true;
            }
            return false;
        }

        public static string UF_GetNamePrefix(string name) {
            int idx = name.IndexOf('_');
            if (idx > -1) {
                return name.Substring(0, idx);
            } else {
                return string.Empty;
            }
        }


        public static bool UF_ExistString(string str,string pattern)
        {
            return str.IndexOf(pattern, System.StringComparison.Ordinal) > -1;
        }

        public static bool UF_CheckStringMask(string strMask,string pattern, char split = ';') {
            bool retVal = false;
            List<string> list = ListCache<string>.Acquire();
            UF_SplitString(strMask, list, split);
            foreach (var v in list) {
                if (pattern.IndexOf(v, System.StringComparison.Ordinal) > -1) {
                    retVal = true;
                }
            }
            ListCache<string>.Release(list);
            return retVal;
        }



        public static string[] UF_SplitString(string param, char split = ';')
        {
            if (string.IsNullOrEmpty(param)) {
                return null;
            }
            //string[] array = param.Split(new char[] { split }, System.StringSplitOptions.RemoveEmptyEntries);
            List<string> list = ListCache<string>.Acquire();
            UF_SplitString(param, list, split);
            string[] array = list.ToArray();
            ListCache<string>.Release(list);
            return array;
        }
          
        public static void UF_SplitString(string param,List<string> outlist, char split = ';') {
            if (outlist == null || string.IsNullOrEmpty(param)) {
                return;
            }
            outlist.Clear();

            StringBuilder sb = StrBuilderCache.Acquire();
            for (int k = 0; k < param.Length; k++)
            {
                if (param[k] == split)
                {
                    outlist.Add(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    sb.Append(param[k]);
                }
            }
            if(sb.Length > 0)
            outlist.Add(sb.ToString());

            StrBuilderCache.Release(sb);
        }



        public static string[] UF_SplitStringWithCount(string param, int count, char split = ';')
        {
            if (string.IsNullOrEmpty(param) || count <= 0)
            {
                return null;
            }
            List<string> list = ListCache<string>.Acquire();
            UF_SplitStringWithCount(param, count, list, split);
            string[] array = list.ToArray();
            ListCache<string>.Release(list);
            return array;
        }


        public static void UF_SplitStringWithCount(string param, int count, List<string> outlist, char split = ';')
        {
            if (count <= 0 || string.IsNullOrEmpty(param) || outlist == null)
                return;
            UF_SplitString(param, outlist, split);
            for (int k = outlist.Count; k < count; k++)
            {
                outlist.Add(string.Empty);
            }
        }


        public static string UF_InsertStringBetweenChar(string t_str,string t_char){
			char[] array = t_str.ToCharArray();
			char[] new_array = new char[array.Length * 2];
			for(int k = 0;k < array.Length;k++){
				new_array[k*2] = t_char[0];
				new_array[k*2 + 1] = array[k];
			}
			return new string(new_array);
		}

		public static void UF_AddItemToArray<T>(ref T[] source,T target){
			if (source == null || target == null)
				return;
			int lenght = source.Length;
			T[] ret = new T[lenght+1];
			System.Array.Copy (source, ret, lenght);
			ret [lenght] = target;
			source = ret;
		}

		public static void UF_RemoveItemToArray<T>(ref T[] source,T target){
			if (source == null || target == null)
				return;
			List<T> ret = new List<T> ();
			for (int k = 0; k < source.Length; k++) {
				if (!source [k].Equals (target)) {
					ret.Add (source [k]);
				}
			}
			source = ret.ToArray ();
		}
			

		public static void UF_WriteBytesToFile(byte[] bytes, string outPath){
			try{
				if(bytes != null && !string.IsNullOrEmpty(outPath)){
					FileStream fs = File.Open(outPath, FileMode.Create);
					BinaryWriter bw = new BinaryWriter(fs);
					bw.Write(bytes);
					bw.Flush();
					bw.Close();
					fs.Close();
				}
			}
			catch(System.Exception e){
				Debugger.UF_Exception (e);
			}
		}
			
		public static bool UF_CheckFileExists( string filePath){
			return File.Exists (filePath);
		}

		public static void UF_CreateDirectory(string dirPath)
		{
			if (!Directory.Exists(dirPath)) {
				Directory.CreateDirectory(dirPath);
			}
		}


		public static string UF_GetMD5HashFromString(string str)   
		{   
			byte[] result = Encoding.UTF8.GetBytes(str);  
			MD5 md5 = new MD5CryptoServiceProvider();  
			byte[] output = md5.ComputeHash(result);
			return System.BitConverter.ToString(output).Replace("-","");
		}

		public static string UF_GetMD5HashFromFile(string fileName)
		{
			try
			{
				FileInfo fi = new FileInfo(fileName);
				if(!fi.Exists){
					return "";
				}

				FileStream file = new FileStream(fileName, FileMode.Open);
				System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
				byte[] retVal = md5.ComputeHash(file);
				file.Close();

				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < retVal.Length; i++)
				{
					sb.Append(retVal[i].ToString("x2"));
				}
				return sb.ToString();
			}
			catch (System.Exception ex)
			{
				Debugger.UF_Exception (ex);
				return "";
			}
		}

        //同步加载Android asset文件夹中文件字节
        public static byte[] UF_LoadAndroidAssetFile(string filepath) {
#if UNITY_ANDROID && !UNITY_EDITOR
            UnityEngine.AndroidJavaClass m_AndroidJavaClass = new UnityEngine.AndroidJavaClass("com.unity.toolkit.StreamFileLoad");
            var bytes = m_AndroidJavaClass.CallStatic<byte[]>("load", filepath);
            return bytes == null || bytes.Length == 0 ? null : bytes;
#else
            return null;
#endif

        }


		public static void UF_BatchSetLayer(UnityEngine.GameObject target,int layer)
		{
			if (target == null)
				return;
			foreach(UnityEngine.Transform tran in target.GetComponentsInChildren<UnityEngine.Transform>()){//遍历当前物体及其所有子物体
				tran.gameObject.layer = layer;
			}
		}
			
		public static void UF_SafeCallDelegate(DelegateEntity method,EntityObject param){
			try{
				if(method != null){
					method(param);
				}
			}
			catch(System.Exception e){
				Debugger.UF_Exception (e);
			}
		}

		public static void UF_SafeCallDelegate(DelegateObject method,UnityEngine.Object param){
			try{
				if(method != null){
					method(param);
				}
			}
			catch(System.Exception e){
				Debugger.UF_Exception (e);
			}
		}

		public static void UF_SafeCallDelegate(DelegateMethod method,object param){
			try{
				if(method != null){
					method(param);
				}
			}
			catch(System.Exception e){
				Debugger.UF_Exception (e);
			}
		}

		public static void UF_SafeCallDelegate(DelegateVoid method){
			try{
				if(method != null){
					method();
				}
			}
			catch(System.Exception e){
				Debugger.UF_Exception (e);
			}
		}
			
		private static readonly Regex mRegexMath = new Regex(@"<(.*?)>", RegexOptions.Singleline);
		public static string UF_ParseTextArithmetic(string text){
			string ret = text;
			foreach (Match match in mRegexMath.Matches(text)) {
				string value = UF_ParseArithmeticValue (match.Groups [1].ToString()).ToString ();
				ret = ret.Replace(match.Groups[0].ToString(),value);
			}
			return ret;
		}

		private static StringBuilder mStrBufArithmetic = new StringBuilder ();
		private static double UF_ParseArithmeticValue(string text){
			double value = 0;
			double curValue = 0;
			char parttem = '+';
			mStrBufArithmetic.Remove (0, mStrBufArithmetic.Length);
			text = text + "+";
			for (int k = 0; k < text.Length; k++) {
				if (text [k] == '+'||text [k] == '-'||text [k] == '*'||text [k] == '/') {
					double.TryParse (mStrBufArithmetic.ToString (), out curValue);
					if (parttem == '+') {
						value += curValue;
					}
					else if (parttem == '-') {
						value -= curValue;
					}
					else if (parttem == '*') {
						value *= curValue;
					}
					else if (parttem == '/') {
						value /= curValue;
					}
					curValue = 0;
					parttem = text [k];
					mStrBufArithmetic.Remove (0, mStrBufArithmetic.Length);
				} else {
					mStrBufArithmetic.Append (text [k]);
				}
			}
			return value;
		}


		public static byte[] UF_LoadFileByte(string filePath){
            try
            {
                if (File.Exists(filePath))
                {
                    FileStream fs = File.OpenRead(filePath);
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    fs.Close();
                    return buffer;
                }
            }
            catch (System.Exception e)
            {
                Debugger.UF_Exception(e);
            }
            return null;
        }

		public static void UF_BytesOfs(byte[] buffer,int ofs){
			if (buffer != null) {
                if (ofs == 0) return;
				for(int k = 0;k < buffer.Length; k++){
					buffer[k] = (byte)(buffer[k] ^ ofs);
				}
			}
		}

        public static void UF_BytesKey(byte[] buffer, int key)
        {
            if (buffer != null)
            {
                if (key == 0) return;
                for (int k = 0; k < buffer.Length; k++)
                {
                    buffer[k] = (byte)(~(buffer[k] ^ key));
                }
            }
        }

        public static byte[] UF_BytesCopy(byte[] buffer) {
            if (buffer == null)
                return null;
            else {
                byte[] ret = new byte[buffer.Length];
                System.Array.Copy(buffer, ret, buffer.Length);
                return ret;
            }
        }

        public static string UF_InsertFilePrefix(string filePth,string prefix){
			int idx = filePth.LastIndexOf("/", System.StringComparison.Ordinal);
			if (idx >= 0) {
				return filePth.Insert (idx + 1, prefix);
			} else {
				return prefix + filePth;
			}
		}



        public static float ShortFloat(float v) {
            v = v * 10000.0f;
            int intv = (int)v;
            return ((float)intv) / 10000.0f;
        }


		public static int UF_ParseInt(string val){
            if (string.IsNullOrEmpty(val)) return 0;
			int ret = 0;
			int.TryParse (val,out ret);
			return ret;
		}

		public static float UF_ParseFloat(string val){
            if (string.IsNullOrEmpty(val)) return 0;
            float ret = 0;
			float.TryParse (val,out ret);
			return ret;
		}

        public static bool UF_ParseBool(string val) {
            if (string.IsNullOrEmpty(val)) return false;
            if (val == "0" || val == "false" || val == "False" || val == "FALSE")
            {
                return false;
            }
            else {
                return true;
            }
        }

        public static UnityEngine.Color UF_IntToColor(int v) {
            return new UnityEngine.Color(
                    ((v >> 24) & 0xff) / 255.0f,
                    ((v >> 16) & 0xff) / 255.0f,
                    ((v >> 8) & 0xff) / 255.0f,
                    (v & 0xff) / 255.0f
                    );
        }

        public static int UF_ColorToInt(UnityEngine.Color color) {
            uint val = 0;
            val += ((uint)(color.r * 255.0f)) << 24;
            val += ((uint)(color.g * 255.0f)) << 16;
            val += ((uint)(color.b * 255.0f)) << 8;
            val += ((uint)(color.a * 255.0f));
            return (int)val;
        }

        private static byte UF_ParseCharToByte(char a)
        {
            byte va = (byte)a;
            if (va - 97 > 0)
            {
                return (byte)UnityEngine.Mathf.Clamp(va - 87, 0, 15);
            }
            else if (va - 65 > 0)
            {
                return (byte)UnityEngine.Mathf.Clamp(va - 55, 0, 15);
            }
            else if (va - 48 > 0)
            {
                return (byte)UnityEngine.Mathf.Clamp(va - 48, 0, 9);
            }
            else
            {
                return 0;
            }
        }

        private static char[] m_ColorChunkCache = new char[8];
        public static UnityEngine.Color32 UF_UF_ParseStringToColor32(string value)
        {
            UnityEngine.Color32 ret = new UnityEngine.Color32(1, 1, 1, 1);
            m_ColorChunkCache[0] = '0';
            m_ColorChunkCache[1] = '0';
            m_ColorChunkCache[2] = '0';
            m_ColorChunkCache[3] = '0';
            m_ColorChunkCache[4] = '0';
            m_ColorChunkCache[5] = '0';
            m_ColorChunkCache[6] = 'F';
            m_ColorChunkCache[7] = 'F';
            for (int k = 0; k < value.Length; k++)
            {
                m_ColorChunkCache[k] = value[k];
            }
            ret.r = (byte)(UF_ParseCharToByte(m_ColorChunkCache[0]) * 16 + UF_ParseCharToByte(m_ColorChunkCache[1]));
            ret.g = (byte)(UF_ParseCharToByte(m_ColorChunkCache[2]) * 16 + UF_ParseCharToByte(m_ColorChunkCache[3]));
            ret.b = (byte)(UF_ParseCharToByte(m_ColorChunkCache[4]) * 16 + UF_ParseCharToByte(m_ColorChunkCache[5]));
            ret.a = (byte)(UF_ParseCharToByte(m_ColorChunkCache[6]) * 16 + UF_ParseCharToByte(m_ColorChunkCache[7]));
            return ret;
        }

        public static UnityEngine.Color UF_ParseStringToColor(string value) {
            var val = UF_UF_ParseStringToColor32(value);
            UnityEngine.Color color = UnityEngine.Color.white;
            color.r = ((float)val.r) / 255.0f;
            color.g = ((float)val.g) / 255.0f;
            color.b = ((float)val.b) / 255.0f;
            color.a = ((float)val.a) / 255.0f;
            return color;
        }



    }


}