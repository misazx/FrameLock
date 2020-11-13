//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;

namespace UnityFrame{

	public static class DeviceInput
	{
		public static float DeltaZoom = 20.0f;

		public static bool HasTouch{get{return Input.touchCount > 0;}}

		public static bool UF_Down(int index){
			#if UNITY_EDITOR || UNITY_STANDALONE
			return Input.GetMouseButtonDown (index);
			#else
			if(Input.touchCount > 0 && index < Input.touchCount && index >= 0){
				return Input.touches[index].phase == TouchPhase.Began;
			}
			return false;
			#endif
		}

		public static bool UF_Down(){
			#if UNITY_EDITOR || UNITY_STANDALONE
			return (Input.GetMouseButtonDown (0));
#else
			if(Input.touchCount > 0){
				int touchCount = Input.touchCount;
				for(int k = 0;k < touchCount;k++){
					if(UF_Down(k)){
						return true;
					}
				}	
			}
			return false;
#endif

        }

        public static bool UF_Up(int index){
			#if UNITY_EDITOR || UNITY_STANDALONE
			return Input.GetMouseButtonUp (index);
			#else
			if(Input.touchCount > 0 && index < Input.touchCount && index >= 0){
			return Input.touches[index].phase == TouchPhase.Ended;
			}
			return false;
			#endif
		}


		public static bool UF_Up(){
			#if UNITY_EDITOR || UNITY_STANDALONE
			return (Input.GetMouseButtonUp (0));
#else
			if(Input.touchCount > 0){
				int touchCount = Input.touchCount;
				for(int k = 0;k < touchCount;k++){
					if(UF_Up(k)){
						return true;
					}
				}	
			}
			return false;
#endif

        }


        public static bool UF_Press(int index){
			#if UNITY_EDITOR || UNITY_STANDALONE
			return Input.GetMouseButton (index);
			#else
			return (Input.touchCount > 0 && index < Input.touchCount && index >= 0);
			#endif

		}

		public static bool UF_Press(){
			#if UNITY_EDITOR || UNITY_STANDALONE
			return Input.GetMouseButton(0);
			#else
			return Input.touchCount > 0;
			#endif
		}
			
		public static float UF_VerticalDelta(int index){
			#if UNITY_EDITOR || UNITY_STANDALONE
				return Input.GetAxis("Mouse Y");
			#else
				float _delta = 0;
				if(Input.touchCount > 0 && index < Input.touchCount && index >= 0){
				_delta = Input.touches[index].deltaPosition.y/DeltaZoom;
				_delta = Mathf.Clamp(_delta,-1.0f,1.0f);

				}
				return _delta;
			#endif
		}

		public static float UF_HorizontalDelta(int index){
			#if UNITY_EDITOR || UNITY_STANDALONE
			return Input.GetAxis("Mouse X");
			#else
			float _delta = 0;
			if(Input.touchCount > 0 && index < Input.touchCount && index >= 0){
			_delta = Input.touches[index].deltaPosition.x/DeltaZoom;
			_delta = Mathf.Clamp(_delta,-1.0f,1.0f);

			}
			return _delta;
			#endif
		}




		public static float UF_ScrollDelta(){
			#if UNITY_EDITOR || UNITY_STANDALONE
			return Input.GetAxis("Mouse ScrollWheel");
			#else
			float mov = 0;
			if(Input.touchCount > 1){
			Vector2 finger1 = new Vector2();
			Vector2 finger2 = new Vector2();

			Vector2 move1 = new Vector2();
			Vector2 move2 = new Vector2();

			for(int i = 0;i < 2;i++){
			Touch touch = Input.touches[i];
			if(touch.phase == TouchPhase.Ended)
			break;
			if(touch.phase == TouchPhase.Moved){
			if(i == 0)
			{
			finger1 = touch.position;
			move1 = touch.deltaPosition;
			}
			else{
			finger2 = touch.position;
			move2 = touch.deltaPosition;
			if(finger1.x > finger2.x){
			mov = move1.x;
			}
			else{
			mov = move2.x;
			}
			if(finger1.y > finger2.y)
			{
			mov +=move1.y;
			}
			else{
			mov += move2.y;
			}
			}
			}
			}
			}
			return mov;
			#endif
		}



		public static Vector3 UF_DownPosition(int index){
			#if UNITY_EDITOR || UNITY_STANDALONE
			if(Input.GetMouseButtonDown(0)){
				return Input.mousePosition;
			}
			else return Vector3.zero;
			#else
			if(Input.touchCount > 0 && index < Input.touchCount && index >= 0){
			return Input.touches[index].position;
			}
			else return Vector3.zero;
			#endif
		 
		}
			
		public static Vector3[] UF_DownPositions(){
			
			#if UNITY_EDITOR || UNITY_STANDALONE
			if(UF_Down(0)){
				Vector3[] ret = new Vector3[1];
				ret[0] = Input.mousePosition;
				return ret;
			}
			else return null;

			#else
			if(Input.touchCount > 0){
			Vector3[] ret = new Vector3[Input.touchCount];
			Touch[] touches = Input.touches;
			for(int k = 0;k < Input.touchCount;k++){
			ret[k] = touches[k].position;
			}
			return ret;
			}
			else return null;
			#endif
		}


		public static Vector3 UF_PressPosition(int index){
			#if UNITY_EDITOR || UNITY_STANDALONE
			if(UF_Press(index)){
				return Input.mousePosition;
			}
#else
			if(UF_Press(index)){
				return Input.touches[index].position;
			}
#endif

            return Vector3.zero;
		}

		public static Vector3[] UF_PressPositions(){
			#if UNITY_EDITOR || UNITY_STANDALONE
			if(UF_Press(0)){
				Vector3[] ret = new Vector3[1];
				ret[0] = Input.mousePosition;
				return ret;
			}
			else return null;
#else
			if (UF_Press ()) {
				Touch[] touches = Input.touches;
				Vector3[] ret = new Vector3[touches.Length];
				for (int k = 0; k < touches.Length; k++) {
					ret [k] = touches [k].position;
				}
				return ret;
			}
			return null;
#endif
        }







    }


}