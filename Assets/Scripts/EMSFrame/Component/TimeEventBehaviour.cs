using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UnityFrame{
	public class TimeEventBehaviour : MonoBehaviour {
		[System.Serializable]
		class TickEvent{
			public string desc;
			public float triggerTime;
			[HideInInspector]public bool isTriggered;
			public UnityEvent tickEvent;
		}

		private float mDymicDuration;

		private float mTickBuffer;
		private bool mIsOver = true;

		public bool IsPlayOnActive = true;

		public bool IngoreTimeScale = false;

		[SerializeField] private List<TickEvent> ListTickEvent = new List<TickEvent>();

        public void Play(){
			mDymicDuration = 0;
			mTickBuffer = 0;
			for (int k = 0; k < ListTickEvent.Count; k++) {
				mDymicDuration = Mathf.Max(mDymicDuration, ListTickEvent[k].triggerTime);
				ListTickEvent[k].isTriggered = false;
			}
			mIsOver = false;
		}

		public void Stop(){
			mIsOver = true;
		}

		// Update is called once per frame
		void Update () {
			if (!mIsOver) {
				if(IngoreTimeScale){
					mTickBuffer += Time.unscaledDeltaTime;
				}else{
					mTickBuffer += Time.deltaTime;
				}

				for (int k = 0; k < ListTickEvent.Count; k++) {
					if (!ListTickEvent[k].isTriggered && mTickBuffer >= ListTickEvent[k].triggerTime) {
						ListTickEvent[k].isTriggered = true;
						ListTickEvent[k].tickEvent.Invoke();
					}
				}
				if (mTickBuffer > mDymicDuration) {
					mIsOver = true;
					return;   
				}
			}
		}

		void OnEnable(){
			if(IsPlayOnActive){
				Play ();
			}
		}
	}
}