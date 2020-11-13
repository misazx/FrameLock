using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class EdSerialObject : ScriptableObject
{
	[System.Serializable]
	internal struct CustomParams{
		public string key;
		public string val;
	}

	[SerializeField]private List<CustomParams> m_CustomParams = new List<CustomParams>();
	[SerializeField]protected string m_strName = "";

	public new string name{
		get{return m_strName;}
		set{ 
			this.BeginChange ();
			m_strName = value;
			this.EndChange ();
		}
	}

	private bool mMarkPChange = false;


	public string this[string key]{
		get{ 
			return GetParam (key);
		}
		set{ 
			SetParam (key, value);
		}
	}


	public void SetParam(string key,string value){
		for (int k = 0; k < m_CustomParams.Count; k++) {
			if (m_CustomParams [k].key == key && m_CustomParams [k].val != value) {
				this.BeginChange ();
				var obj = m_CustomParams [k];
				obj.val = value;
				m_CustomParams [k] = obj;
				this.EndChange ();
				return;
			}
		}
		var newObj = new CustomParams ();
		newObj.key = key;
		newObj.val = value;
		m_CustomParams.Add (newObj);
	}

	public string GetParam(string key,string defaultVal = ""){
		for (int k = 0; k < m_CustomParams.Count; k++) {
			if (m_CustomParams [k].key == key) {
				if(string.IsNullOrEmpty(m_CustomParams [k].val))
					return defaultVal;
				else
					return m_CustomParams [k].val;
			}
		}
		return defaultVal;
	}

	public int GetParamInt(string key){
		string val	= GetParam (key);
		if (string.IsNullOrEmpty (val))
			return 0;
		else {
			int outval = 0;
			int.TryParse (val, out outval);
			return outval;
		}
	}

	public float GetParamFloat(string key){
		string val	= GetParam (key);
		if (string.IsNullOrEmpty (val))
			return 0;
		else {
			float outval = 0;
			float.TryParse (val, out outval);
			return outval;
		}
	}

	public bool GetParamBool(string key){
		string val	= GetParam (key);
		if (string.IsNullOrEmpty (val))
			return false;
		else {
            if (val == "true" || val == "True" || val == "TRUE")
                return true;
			bool outval = false;
			bool.TryParse (val, out outval);
			return outval;
		}
	}


    public void RemoveParam(string key) {
        for (int k = 0; k < m_CustomParams.Count; k++)
        {
            if (m_CustomParams[k].key == key)
            {
                m_CustomParams.RemoveAt(k);
                return;
            }
        }
    }



    public void BeginChange(string extraName = ""){
		if (!mMarkPChange) {
			mMarkPChange = true;
			EditorTools.RegisterUndo(extraName + this.GetType().ToString(),this);
		}
	}

    


	public void EndChange(){
		if (mMarkPChange) {
			mMarkPChange = false;
			EditorTools.SetDirty (this);
		}
	}

	internal void CopyCustomParamsTo(EdSerialObject target){
		if (target == null)
			return;
		foreach (var item in this.m_CustomParams) {
			target.m_CustomParams.Add (item);
		}
	}


}