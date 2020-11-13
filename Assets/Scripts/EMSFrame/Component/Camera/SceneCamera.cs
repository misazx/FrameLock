//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;

namespace UnityFrame
{
	/// <summary>
	/// 控制地图摄像机类
	/// </summary>
	public class SceneCamera : MonoBehaviour
	{
		//缓动跟随
		public float smooth = 0.5f;

		public Vector3 targetOffset;

		public bool isCheckBorder = true;

		private Camera mCamera;

		private Vector3 mVel = Vector3.zero;
		private Vector3 moveToFrom;
		private Vector3 moveToTarget;
		private float moveToDuration;
		private float moveToTime;

		private bool isMoveTo = false;
		//目标跟随
		private GameObject mTarget;
		private bool mActiveTarget = true;
		public float worldToPixel{ get;private set;}
		
		private Vector3 mOffsetPosition = Vector3.zero;
		private ScreenShake mScreenShaker = new ScreenShake();
		private ScreenMask mScreenMask = new ScreenMask();
		private Vector3 mWorldPoint = Vector3.zero;


        public bool lockAixsHorizontal;
        public bool lockAixsVertical;


        public float slope{get;private set;}
		//边角4点
		public Vector3 cornerA{get;private set;}
		public Vector3 cornerB{get;private set;}
		public Vector3 cornerC{get;private set;}
		public Vector3 cornerD{get;private set;}

		public Vector3 cornerUp{get;private set;}
		public Vector3 cornerDown{get;private set;}

		public Vector3 desirePos{get;private set;}
		public float hfRealCamWidth{get;private set;}
		public float hfRealCamHight{get;private set;}
		public float realCamHight{get;private set;}

        public Vector3 position { get { return this.transform.position; } set { this.transform.position = value; } }
        public Vector3 euler { get { return this.transform.eulerAngles; } set { this.transform.eulerAngles = value; } }
        public Vector3 forward { get { return this.transform.forward; } set { this.transform.forward = value; } }

        //判断4个角点是否符合在Map中,限制摄像机移动范围
        protected Rect boardRange { get; private set; }

        public new Camera camera{
            get {
                if (mCamera == null) {
                    mCamera = this.GetComponentInChildren<Camera>();
                }
                return mCamera;
            }
        }

        public static SceneCamera main
        {
            get;
            private set;
        }

        public float cameraSize
        {
            get
            {
                return camera.orthographicSize;
            }
            set
            {
                camera.orthographicSize = value;
            }
        }

        public float nearClipPlane {
            get { return camera.nearClipPlane; }
            set { camera.nearClipPlane = value; }
        }

        public float farClipPlane
        {
            get { return camera.farClipPlane; }
            set { camera.farClipPlane = value; }
        }

        public void UF_SetActiveTarget(bool vlaue,bool useSmooth){
			mActiveTarget = vlaue;
			isMoveTo = false;
			if (!useSmooth) {
                UF_UpdateCamera(0);
				this.transform.position = desirePos;
			}
		}

		public void UF_SetCameraEnable(bool value){
			if(camera != null){
				camera.enabled = value;
			}
		}

        public GameObject UF_GetTarget() {
            return mTarget;
        }

		public void UF_SetTarget(GameObject target){
            UF_SetTarget(target,false);
		}

		public void UF_SetTarget(GameObject target,bool useSmooth){
			mTarget = target;
			isMoveTo = false;
			if (!useSmooth) {
                UF_UpdateCamera(0);
				this.transform.position = desirePos;
			}
		}

		private Vector3 UF_GetWorldPoint(Vector3 point){
			Vector3 pos = point;
			float k = Mathf.Cos (Mathf.Deg2Rad * slope);
			//映射地面
			float y = 0;
			float b = y - pos.z * k;
			float x = (y - b) / k;
			return new Vector3 (pos.x, y, x);
		}

		public float ViewWidth {
			get{ 
				return camera.orthographicSize * camera.aspect * 2.0f;
			}
		}

		public float ViewHight {
			get{ 
				return camera.orthographicSize * 2.0f;
			}
		}

		//设置斜率
		public void UF_SetSlope(float value){
			Vector3 eulerAngles = this.transform.eulerAngles;
			eulerAngles.x = value;
			this.transform.eulerAngles = eulerAngles;
            slope = value;
        }
		//设置世界单位像素比
		public void UF_SetWorldToPixel(float value){
			worldToPixel = value;
		}
		//是指区域范围
		public void UF_SetBoardRange(float x,float y,float width,float height){
			boardRange = new Rect(x,y,width,height);
		}
			
		public static SceneCamera UF_CreateToMainCamera(){
			SceneCamera ret = Camera.main.GetComponent<SceneCamera>();
			if (ret == null) {
				ret = Camera.main.gameObject.AddComponent<SceneCamera> ();
            }
            main = ret;
            return ret;
		}

		public static SceneCamera UF_CreateToGameObject(string objName){
			GameObject obj = GameObject.Find(objName);
			SceneCamera ret = null;
			if (obj != null) {
				ret = obj.GetComponent<SceneCamera>();
				if (ret == null) {
					ret = obj.gameObject.AddComponent<SceneCamera> ();
				}	
			}
            //默认第一个作为主
            if (main == null)
                main = ret;
            return ret;
		}

		public void UF_SetPosition(Vector3 pos){
			this.transform.position = pos;
			desirePos = this.transform.position;
		}


		public Vector3 UF_GetPosition(){
			return this.transform.position;
		}


		public float UF_GetSize(){
			return camera.orthographicSize;
		}

		public void UF_SetSize(float size){
			camera.orthographicSize = size;
		}

		public int UF_SmoothSize(float size,float duration){
			if (duration > 0) {
				if (this.gameObject.activeInHierarchy) {
					return FrameHandle.UF_AddCoroutine (UF_ISmoothSize(camera, size, duration));
				} else {
					camera.orthographicSize = size;
				}
			} else {
                UF_SetSize(size);
			}
			return 0;
		}

		IEnumerator UF_ISmoothSize(Camera _camera,float size,float duration){
			float _source = _camera.orthographicSize;
			float _target = size;
			float _duration = duration;
			float _buffer = 0;
			while(_buffer < _duration){
				_buffer += Time.deltaTime;
				float k = Mathf.Clamp01(_buffer / duration);
				_camera.orthographicSize = (1 - k) * _source + k * _target;
				yield return null;
			};
			_camera.orthographicSize = size;
		}


		public float UF_CalcPixelHeight(float value){
			//			float height = (ViewHight);
			//            float cosv = Mathf.Cos(Mathf.Deg2Rad * slope);
			//            float cValue = value * cosv;
			//            return (cValue / height) * Screen.height;

			return worldToPixel * value;
		}

		public float UF_CalcPixelWidth(float value){
			return  (float)((value / ViewWidth) * Screen.width);
		}

		void Awake(){
			mScreenMask.UF_OnAwake (camera);
			mScreenMask.UF_SetActive (false);
		}


		//检查是否在视野范围内
		public bool UF_CheckInViewRange(Vector3 point){
			return point.x < cornerA.x && point.x > cornerB.x && point.z > cornerA.z && point.z < cornerD.z;
		}


		private bool UF_checkInRange(Vector3 range,Vector3 point){
			return (point.x < range.x && point.z < range.z);
		}


		private void UF_UpdateCamera(float smoothVal){

			if (!mTarget || !mActiveTarget) {
				mWorldPoint = UF_GetWorldPoint(this.transform.position);
			} else {
				Vector3 current = this.transform.position;
				Vector3 target = mTarget.transform.position + targetOffset;
                Vector3 dpos = target;
                if (smoothVal <= 0) {
                    dpos = target;
				} else {
                    dpos = MathX.UF_SmoothDamp3(current, target, ref mVel, smoothVal);
				}
                if (lockAixsHorizontal) dpos.x = current.x;
                if (lockAixsVertical) dpos.z = current.z;
                desirePos = dpos;
                mWorldPoint = UF_GetWorldPoint(desirePos);
			}

			hfRealCamWidth = ViewWidth * 0.5f;
			realCamHight = (ViewHight / Mathf.Sin(Mathf.Deg2Rad * slope));
			hfRealCamHight = realCamHight * 0.5f;

			cornerA = new Vector3 (mWorldPoint.x + hfRealCamWidth, mWorldPoint.y, mWorldPoint.z - hfRealCamHight);
			cornerB = new Vector3 (mWorldPoint.x - hfRealCamWidth, mWorldPoint.y, mWorldPoint.z - hfRealCamHight);
			cornerC = new Vector3 (mWorldPoint.x - hfRealCamWidth, mWorldPoint.y, mWorldPoint.z + hfRealCamHight);
			cornerD = new Vector3 (mWorldPoint.x + hfRealCamWidth, mWorldPoint.y, mWorldPoint.z + hfRealCamHight);
			cornerUp = new Vector3 ((cornerA.x + cornerB.x)/2.0f,mWorldPoint.y,cornerA.z);
			cornerDown = new Vector3 ((cornerC.x + cornerD.x)/2.0f,mWorldPoint.y,cornerC.z);

			//得到4个角点
			if (isCheckBorder) {
				Vector3 tempDesirePos = desirePos;
                if (!lockAixsHorizontal) {
                    if (cornerA.x > boardRange.width)
                    {
                        tempDesirePos.x = boardRange.width - hfRealCamWidth;
                    }
                    else if (cornerB.x < boardRange.x)
                    {
                        tempDesirePos.x = boardRange.x + hfRealCamWidth;
                    }
                }

                if (!lockAixsVertical) {
                    if (cornerD.z > boardRange.height)
                    {
                        tempDesirePos.z = boardRange.height - hfRealCamHight;
                    }
                    else if (cornerA.z < boardRange.y)
                    {
                        tempDesirePos.z = boardRange.y + hfRealCamHight;
                    }
                }
				
				desirePos = tempDesirePos;
			}



		}


		public void LateUpdate(){
			float deltaTime = Time.deltaTime;

			mScreenShaker.Update (ref this.mOffsetPosition);

			mScreenMask.Update (deltaTime);

            UF_UpdateCamera(smooth);

			if (isMoveTo)
			{
				moveToTime += deltaTime;
				if (moveToTime < moveToDuration)
				{
					this.transform.position = Vector3.Lerp(moveToFrom, moveToTarget, moveToTime/moveToDuration) + mOffsetPosition;
					return;
				}
				else
					this.transform.position = moveToTarget;

				desirePos = this.transform.position;
				isMoveTo = false;
				return;
			}

			this.transform.position = desirePos + mOffsetPosition;
		}

		public void UF_MoveTo(Vector3 from , Vector3 target ,float duration)
		{
			if (duration <= 0) {
				this.UF_SetPosition(target);
				isMoveTo = false;
			} else {
				isMoveTo = true;
				moveToFrom = from;
				moveToTarget = target;
				moveToDuration = duration;
				moveToTime = 0;
			}
			mActiveTarget = false;
		}


		public void UF_Shake(float fRange,float fRate,float fAttenuation){
			mScreenShaker.UF_ShakeMotion(fRange,fRate,fAttenuation);
		}

		public void UF_SetMaskAplha(float source,float target,float duration){
			mScreenMask.UF_SetAplah(source,target,duration);
		}

		public void UF_SetMaskColor(Color source,Color target,float duration){
			mScreenMask.UF_SetColor(source,target,duration);
		}


#if UNITY_EDITOR
        [LuaInterface.NoToLuaAttribute]
        void OnDrawGizmos()
        {
            if (!Application.isPlaying) {
                slope = this.transform.eulerAngles.x;
                mWorldPoint = UF_GetWorldPoint(this.transform.position);

                hfRealCamWidth = ViewWidth * 0.5f;
                realCamHight = (ViewHight / Mathf.Sin(Mathf.Deg2Rad * slope));
                hfRealCamHight = realCamHight * 0.5f;
                cornerA = new Vector3(mWorldPoint.x + hfRealCamWidth, mWorldPoint.y, mWorldPoint.z - hfRealCamHight);
                cornerB = new Vector3(mWorldPoint.x - hfRealCamWidth, mWorldPoint.y, mWorldPoint.z - hfRealCamHight);
                cornerC = new Vector3(mWorldPoint.x - hfRealCamWidth, mWorldPoint.y, mWorldPoint.z + hfRealCamHight);
                cornerD = new Vector3(mWorldPoint.x + hfRealCamWidth, mWorldPoint.y, mWorldPoint.z + hfRealCamHight);
            }
            

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(cornerA, cornerB);
            Gizmos.DrawLine(cornerB, cornerC);
            Gizmos.DrawLine(cornerC, cornerD);
            Gizmos.DrawLine(cornerD, cornerA);
            


            //if (mSceneMap != null)
            //{
            //    Gizmos.color = Color.blue;
            //    Vector3 p1 = Vector3.zero;
            //    Vector3 p2 = new Vector3(mSceneMap.CullWorldMapSize.x, 0, 0);
            //    Vector3 p3 = new Vector3(mSceneMap.CullWorldMapSize.x, 0, mSceneMap.CullWorldMapSize.y);
            //    Vector3 p4 = new Vector3(0, 0, mSceneMap.CullWorldMapSize.y);

            //    Gizmos.DrawLine(p1, p2);
            //    Gizmos.DrawLine(p2, p3);
            //    Gizmos.DrawLine(p3, p4);
            //    Gizmos.DrawLine(p4, p1);
            //}
            //Gizmos.color = Color.white;


        }
#endif




    }
}

