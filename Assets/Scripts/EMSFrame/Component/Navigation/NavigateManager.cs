//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace UnityFrame
{
    public class NavigateManager : HandleSingleton<NavigateManager>
    {


        private void UF_FindingPath(SceneMap map, List<Vector3> outList, int Ax, int Ay, int Bx, int By) {
            var listGridPoint = ListCache<Vector2>.Acquire();
            ASGridPathFinder.UF_FindingPath(map.asGrid, Ax,Ay,Bx,By, listGridPoint, true);
            if (listGridPoint.Count > 1)
            {
                //转化grid点为世界地图点
                foreach (var point in listGridPoint)
                {
                    outList.Add(map.UF_GetGridPosition((int)point.x, (int)point.y));
                }
            }
            ListCache<Vector2>.Release(listGridPoint);
        }


        public void UF_NavigateStop(int handle) {
            MotionManager.UF_GetInstance().UF_Remove(handle);
        }


        public int UF_NavigateTo(GameObject gameObject, SceneMap map, Vector3 tarPosition, float duration,DelegateVoid callback)
        {
            int ret = 0;
            Vector2 sorPoint = map.UF_GetGridPoint(gameObject.transform.position);
            Vector2 tarPoint = map.UF_GetGridPoint(tarPosition);
            float curdiatance = Vector3.Distance(sorPoint, tarPosition);

            var listGridPosition = ListCache<Vector3>.Acquire();
            UF_FindingPath(map, listGridPosition, (int)sorPoint.x, (int)sorPoint.y, (int)tarPoint.x, (int)tarPoint.y);
            if (listGridPosition.Count > 1)
            {
                //替换起始点为当前点
                listGridPosition[0] = sorPoint;
                //替换尾部点为目标点
                listGridPosition[listGridPosition.Count - 1] = tarPoint;
                ret = MotionManager.UF_GetInstance().UF_AddPathPoint(gameObject.transform, listGridPosition, duration,false,0,true,Vector3.zero,true, callback);
            }
            ListCache<Vector3>.Release(listGridPosition);
            return ret;
        }


        //导航角色到
        public bool UF_NavigateAvatarTo(AvatarController avatar,SceneMap map,Vector3 tarPosition,float minDistance, DelegateVoid callback = null) {
            bool ret = false;
            Vector2 sorPoint = map.UF_GetGridPoint(avatar.position);
            Vector2 tarPoint = map.UF_GetGridPoint(tarPosition);

            float curdiatance = Vector3.Distance(avatar.position, tarPosition);
            if (curdiatance <= minDistance) {
                avatar.motion.UF_ClearPathPoint(true);
                return ret;
            }

            var listGridPosition = ListCache<Vector3>.Acquire();
            UF_FindingPath(map, listGridPosition, (int)sorPoint.x, (int)sorPoint.y, (int)tarPoint.x, (int)tarPoint.y);
            if (listGridPosition.Count > 1) {
                //替换起始点为当前点
                listGridPosition[0] = avatar.position;
                //移除起始点
                //listGridPosition.RemoveAt(0);

                //替换尾部点为目标点
                listGridPosition[listGridPosition.Count - 1] = tarPosition;

                //驱动Avatar
                avatar.motion.UF_ClearPathPoint(true);
                avatar.motion.UF_AddPathPoints(listGridPosition, callback);
                ret = true;
            }
            ListCache<Vector3>.Release(listGridPosition);

            return ret;
        }


    }

}
