//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace UnityFrame{
	[System.Serializable]
	public struct ClipEvent
	{
		//触发时间
		public float trigger;
		//事件名
		public string name;
		//事件参数
		public string param;
        //触发概率(万分比)
        public int rate;

        public ClipEvent(string strName,string strParams,float fTrigger){
			name = strName;
			param = strParams;
			trigger = fTrigger;
            rate = 0;
        }

        public ClipEvent(string strName, string strParams, float fTrigger, int fRate)
        {
            name = strName;
            param = strParams;
            trigger = fTrigger;
            rate = fRate;
        }

        public ClipEvent Clone(){
			var ret = new ClipEvent ();
			ret.name = this.name;
			ret.param = this.param;
			ret.trigger = trigger;
            ret.rate = rate;
            return ret;
		}
	}
}

