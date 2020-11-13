//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace UnityFrame
{
	public class JsonConvert
	{

        [ThreadStatic]
        static StringBuilder s_Sbuilder = new StringBuilder();
        [ThreadStatic]
        static StringBuilder s_Sbkey = new StringBuilder();

        static StringBuilder sbuilder { get { return s_Sbuilder; } }
        static StringBuilder sbkey { get { return s_Sbkey; } }



        static bool UF_IsNumber(string value){
			return Regex.IsMatch(value, @"^\d+$");
		}

		static void UF_ClearStringBuilder(StringBuilder sb){
			sb.Remove(0, sb.Length);
		}

		public static bool UF_CheckIsJson(string jsonText){
			if (!string.IsNullOrEmpty(jsonText)) {
				if (jsonText.IndexOf(':') > -1 && jsonText.IndexOf('{') > -1 && jsonText.LastIndexOf('}') > -1)
					return true;
			}
			return false;
		}

		public static string UF_FormatLua(string jsonText){
			if (string.IsNullOrEmpty(jsonText))
				return "{}";
			


			bool mark = false;
			bool markReading = false;
			bool valueReading = false;

            sbuilder.Clear();
            sbkey.Clear();

            string keyvalue = string.Empty;
//           jsonText = jsonText.Replace('[', '{').Replace(']', '}');
			for (int k = 0; k < jsonText.Length; k++) {
				if (jsonText [k] == '"') {
					markReading = !markReading;
				}
				if (jsonText[k] == '"' && !valueReading) {
					if (mark) {
						mark = false;
						keyvalue = sbkey.ToString();
						if (UF_IsNumber(keyvalue)) {
							sbuilder.Append(string.Format("[{0}]", keyvalue));
						} else {
							sbuilder.Append(string.Format("{0}", keyvalue));
						}
                        UF_ClearStringBuilder(sbkey);
						valueReading = false;
						continue;
					} else {
						mark = true;
					}
					continue;
				} else if (jsonText[k] == ':') {
					if (!markReading) {
						sbuilder.Append ('=');
						valueReading = true;
						continue;
					}
				} else if (jsonText[k] == '[') {
					if (!markReading) {
						sbuilder.Append ('{');
						valueReading = false;
						continue;
					}
				} else if (jsonText[k] == ']') {
					if (!markReading) {
						sbuilder.Append ('}');
						valueReading = false;
						continue;
					}
				} else if (jsonText[k] == '{') {
					if (!markReading)
						valueReading = false;
				} else if (jsonText[k] == '}') {
					if (!markReading)
						valueReading = false;
				} else if (jsonText[k] == ',') {
					if (!markReading)
						valueReading = false;
				}
				if (mark) {
					sbkey.Append(jsonText[k]);
				} else {
					sbuilder.Append(jsonText[k]);
				}
			}
			return sbuilder.ToString();
		}



	}
}

