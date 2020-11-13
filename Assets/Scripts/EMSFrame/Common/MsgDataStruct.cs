using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace UnityFrame
{
	public class MsgDataStruct
	{
		protected Dictionary<string,string> m_HashData = new Dictionary<string, string>();

		public void UF_SetValue(string key,string value){
			if (!string.IsNullOrEmpty (key)) {
				if (m_HashData.ContainsKey (key)) {
					m_HashData [key] = value;
				} else {
					m_HashData.Add (key, value);
				}
			}
		}

		public string UF_GetValue(string key,string defaultValue = ""){
			if (m_HashData.ContainsKey (key)) {
				return m_HashData [key];
			} else {
				return defaultValue;
			}
		}

        public int UF_GetIntValue(string key,int defaultValue = 0) {
            if (m_HashData.ContainsKey(key))
            {
                return GHelper.UF_ParseInt(m_HashData[key]);
            }
            else
            {
                return defaultValue;
            }
        }

        public bool UF_GetBoolValue(string key, bool defaultValue = false)
        {
            if (m_HashData.ContainsKey(key))
            {
                return GHelper.UF_ParseBool(m_HashData[key]);
            }
            else
            {
                return defaultValue;
            }
        }


        public void UF_SetTable(string table){
			if(string.IsNullOrEmpty(table)){
				Debugger.UF_Warn("MsgDataStruct.setTable param is Empty");
				return;
			}
			try {
				int startIdx = table.IndexOf('{');
				int endIdx = table.IndexOf('}');
				if(startIdx > -1 && endIdx > -1){
					table = table.Substring(startIdx,endIdx -  startIdx);
				}
				string tabledata = table.Replace("{", "").Replace("}", "").Trim();
				tabledata = tabledata.Replace("[", "").Replace("]", "").Trim();
				// Log.d("MyTest", "se1: " + tabledata);
				tabledata = tabledata.Replace(",", "\n");
				// Log.d("MyTest", "se2: " + tabledata);
				StringReader sr = new StringReader(tabledata);

				string line = null;
				while (null != (line = sr.ReadLine())) {
					int idx = line.IndexOf("=");
					if (idx > -1) {
						string key = line.Substring(0, idx).Replace("\"", "").Trim();
						string value = line.Substring(idx + 1).Replace("\"", "").Trim();
						value = value.Replace("'","");
						// Log.d("MyTest", "Key: " + key + "Value: "+value);
						UF_SetValue(key, value);
					}
				}
				sr.Close();
			} catch (Exception e) {
				Debugger.UF_Exception(e);
			}

		}


		public string UF_Serialize() {
			string table_data = "local data = {";

			foreach (KeyValuePair<string,string> item in m_HashData) {
				table_data += item.Key + "=\"" + item.Value + "\",";
			}
            table_data += "}\n";
            table_data += "return data";

			return table_data;

		}

        public void UF_Clear()
        {
            m_HashData.Clear();
        }


    }
}

