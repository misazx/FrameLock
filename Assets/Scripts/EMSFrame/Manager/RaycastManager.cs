//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace UnityFrame
{
	public class RaycastManager : HandleSingleton<RaycastManager>,IOnUpdate,IOnApplicationPause
	{
        //no need to use
		public bool IsActive = false;

		public float Distance = 1000.0f;
		public float PressInterval = 0.35f;

		public bool UsePress = true;

		private bool m_IsPress = false;

		private float m_CurrentPressTime = 0;

        private bool IsUIRayCast()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return EventSystem.current.IsPointerOverGameObject();
#else
			if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began){
				return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
			} else {
				return false; 
			}
#endif
        }

        public GameObject UF_GetRayHitScreenPoint(Vector3 screenPoint,float distance, int tarLayerMask)
        {
			RaycastHit hit;
			if (Physics.Raycast (Camera.main.ScreenPointToRay (screenPoint), out hit, distance,1 << tarLayerMask)) {
				return hit.collider.gameObject;
			}
			return null;
		}

        public GameObject UF_GetRayHitObject(Vector3 point, Vector3 forward, float distance, int tarLayerMask = 0) {
            RaycastHit hit;
            if (Physics.Raycast(point, forward * distance, out hit, distance, 1 << tarLayerMask))
            {
                return hit.collider.gameObject;
            }
            return null;
        }

        public float UF_GetRayHitDistance(Vector3 point, Vector3 forward, float distance, int tarLayerMask = 0)
        {
            RaycastHit hit;
            if (Physics.Raycast(point, forward * distance, out hit, distance, 1 << tarLayerMask))
            {
                return Vector3.Distance(hit.point,point);
            }
            return 0;
        }



        public Vector3 UF_GetRayHitPoint(Vector3 point, Vector3 forward, float distance,int tarLayerMask) {
            RaycastHit hit;
            if (Physics.Raycast(point, forward, out hit, distance, 1 << tarLayerMask))
            {
                return hit.point;
            }
            return Vector3.zero;
        }



        public Vector3[] UF_GetRayHitReflexPoints(Vector3 point, Vector3 forward, float distance, int tarLayerMask, int reflexTimes) {
            var listTemp = ListCache<RaycastHit>.Acquire();
            Vector3 sPoint = point;
            Vector3 sForward = forward;
            this.UF_GetRayHitReflex(point, forward, distance, tarLayerMask, reflexTimes, listTemp);
            var listVector = ListCache<Vector3>.Acquire();
            //加入源点
            listVector.Add(point);
            foreach (var v in listTemp) {
                listVector.Add(v.point);
            }
            ListCache<RaycastHit>.Release(listTemp);
            var ret = listVector.ToArray();
            ListCache<Vector3>.Release(listVector);
            return ret;
        }


        public RaycastHit[] UF_GetRayHitReflex(Vector3 point, Vector3 forward, float distance, int tarLayerMask, int reflexTimes) {
            var listTemp = ListCache<RaycastHit>.Acquire();
            Vector3 sPoint = point;
            Vector3 sForward = forward;
            this.UF_GetRayHitReflex(point, forward, distance, tarLayerMask, reflexTimes, listTemp);
            var ret = listTemp.ToArray();
            ListCache<RaycastHit>.Release(listTemp);
            return ret;
        }

        public void UF_GetRayHitReflex(Vector3 point, Vector3 forward, float distance, int tarLayerMask, int reflexTimes,List<RaycastHit> listTemp)
        {
            listTemp.Clear();
            Vector3 sPoint = point;
            Vector3 sForward = forward;
            for (int k = 0; k <= reflexTimes; k++)
            {
                RaycastHit hit;
                bool isHit = false;
                if (tarLayerMask == 0)
                {
                    isHit = Physics.Raycast(sPoint, sForward, out hit, distance);
                }
                else {
                    isHit = Physics.Raycast(sPoint, sForward, out hit, distance,1 << tarLayerMask);
                }
                if (isHit)
                {
                    //作为新的起始点
                    sPoint = hit.point;
                    //作为新方向
                    sForward = sForward - 2 * Vector3.Dot(hit.normal, sForward) * hit.normal;
                    listTemp.Add(hit);
                }
                else
                {
                    break;
                }
            }
        }



        public void UF_Reset()
        {
            m_CurrentPressTime = 0;
            m_IsPress = false;
        }


        // Update is called once per frame
        public void UF_OnUpdate()
		{
			if (!IsActive)
				return;
			if (m_IsPress && UsePress) {
				m_CurrentPressTime += GTime.RunDeltaTime;
				if (m_CurrentPressTime >= PressInterval) {
					m_CurrentPressTime = 0;
					RaycastHit hit;
					Camera camera = Camera.main;
					if(camera != null && !IsUIRayCast() && Physics.Raycast(camera.ScreenPointToRay(DeviceInput.UF_PressPosition(0)),out hit,Distance)){
                        MessageSystem.UF_GetInstance ().UF_Send(DefineEvent.E_LUA,DefineLuaEvent.E_RAYCAST_HIT, hit);
					}
				}
				if (!DeviceInput.UF_Press(0)) {
					m_CurrentPressTime = 0;
				}
			} else {
				if (DeviceInput.UF_Down(0)) {
					RaycastHit hit;
					Camera camera = Camera.main;
					if(camera != null && !IsUIRayCast() && Physics.Raycast(camera.ScreenPointToRay(DeviceInput.UF_PressPosition(0)),out hit,Distance)){
						if (UsePress) {
							m_IsPress = true;
							m_CurrentPressTime = 0;
						}
						MessageSystem.UF_GetInstance ().UF_Send(DefineEvent.E_LUA,DefineLuaEvent.E_RAYCAST_HIT, hit);
					}
				}
			}

			if(!DeviceInput.UF_Press()){
				m_IsPress = false;
			}
		}


        public void IgnoreLayerCollision(int a,int b,bool v) {
            Physics.IgnoreLayerCollision(a, b, v);
        }


        public void OnApplicationPause(bool state){
			if (state) {
                UF_Reset();
			}
		}

	}
}

