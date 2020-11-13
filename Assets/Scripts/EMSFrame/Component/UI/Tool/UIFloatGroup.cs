//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityFrame{
	public class UIFloatGroup : UIObject,IOnReset {
		internal class FloatObject{
			public float life;
			public float progress;
			public Vector3 directPos;
			public Vector3 sourcePos;
			public IUIUpdate ui;

			public void Update(AnimationCurve curve,float speed){
				progress += Time.unscaledDeltaTime * speed;
				progress = Mathf.Clamp01 (progress);
				float value = curve.Evaluate (progress);
				Vector3 pos = sourcePos * (1 - value) + directPos * value;
				if (ui != null) {
					ui.rectTransform.localPosition = pos;
				}
			}

			public void Reset(){
				if(ui != null){
                    if (ui is IReleasable)
                        (ui as IReleasable).Release();
                    else if (ui is IOnReset)
						(ui as IOnReset).UF_OnReset();
				}
				ui = null;
				progress = 0;
				sourcePos = Vector3.zero;
				directPos = Vector3.zero;
			}
		}

		//间距
		public float space = 30.0f;
		//衰减时间
		public float decayTime = 3.0f;

		public float speedFloat = 4.0f;

		public AnimationCurve curve = AnimationCurve.Linear(0,0,1,1);

		private List<FloatObject> mListFloat = new List<FloatObject> ();

		private Stack<FloatObject> mStackFloatBuffer = new Stack<FloatObject> ();

		private FloatObject UF_CreateFloat(){
			if (mStackFloatBuffer.Count > 0) {
				return mStackFloatBuffer.Pop ();
			} else {
				return new FloatObject ();
			}
		}

		//生成飘字
		public void UF_AddFloat(IUIUpdate ui){
			if (ui != null) {
				ui.rectTransform.SetParent (this.transform);
				ui.rectTransform.localPosition = Vector3.zero;
				ui.rectTransform.localScale = Vector3.one;
				//重新计算各个飘字浮动位置
				FloatObject floatword = UF_CreateFloat();
				floatword.progress = 0;
				floatword.life = decayTime;
				floatword.ui = ui;
				if (mListFloat.Count > 0) {
					mListFloat.Add (default(FloatObject));
					for (int k = mListFloat.Count - 1; k > 0; k--) {
						mListFloat [k] = mListFloat [k - 1];
						Vector3 directPos = mListFloat [k].directPos;
						mListFloat [k].progress = 0;
						mListFloat [k].directPos = new Vector3 (directPos.x,directPos.y + space,directPos.z);
						mListFloat [k].sourcePos = mListFloat [k].ui.rectTransform.localPosition;
					}
					mListFloat [0] = floatword;
				} else {
					mListFloat.Add (floatword);
				}
			}
		}


		public override void UF_SetValue (object value)
		{
			if (value == null)
				return;
            UF_AddFloat(value as IUIUpdate);
		}

		void Update(){
			for (int k = 0; k < mListFloat.Count; k++) {
				if (mListFloat [k].progress < 1) {
					mListFloat [k].Update(curve,speedFloat);
				}
				mListFloat [k].life -= Time.unscaledDeltaTime;
				if (mListFloat [k].life <= 0) {
					mListFloat [k].Reset ();
					mStackFloatBuffer.Push (mListFloat [k]);
					mListFloat.RemoveAt (k);
					k--;
				}
			}
		}

		public void UF_OnReset(){
			for (int k = 0; k < mListFloat.Count; k++) {
				mListFloat [k].Reset ();
				mStackFloatBuffer.Push (mListFloat [k]);
			}
			mListFloat.Clear ();
		}

	}

}