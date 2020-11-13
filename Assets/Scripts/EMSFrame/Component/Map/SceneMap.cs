//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityFrame.Assets;
namespace UnityFrame {
    public class SceneMap : EntityObject,IOnAwake,IOnReset
    {
        //地图缩放尺寸
        public float size = 15;
        public List<Texture2D> mapTextureList;

        //地图行走网格位置
        public int gridWidth = 5;
        public int gridHeight = 10;

        public GameObject gridStartMark;
        public GameObject gridEndMark;
        public GameObject limitStartMark;
        public GameObject limitEndMark;

        //设置是否宽屏地图
        public bool isWide = false;

        private float m_ElementPivotScale = 1;

        //记录加载的场景元素
        private List<SceneElement> m_ListElements = new List<SceneElement>();
        //场景元素root
        private GameObject m_RootElements = null;
        //寻路网格
        private ASGrid m_ASGrid = new ASGrid();


        //场景地图Grid定位点
        public Vector3 gridStartPoint { get { return gridStartMark == null ? Vector3.zero : gridStartMark.transform.position; } }
        public Vector3 gridEndPoint { get { return gridEndMark == null ? Vector3.zero : gridEndMark.transform.position; } }
        //视图限制区域，影响摄像机显示视野范围
        public Vector3 limitStartPoint { get { return limitStartMark == null ? Vector3.zero : limitStartMark.transform.position; } }
        public Vector3 limitEndPoint { get { return limitEndMark == null ? Vector3.zero : limitEndMark.transform.position; } }

        private float mapHeight { get { return this.transform.position.y; } }

        public float slope { get; private set; }

        internal ASGrid asGrid { get { return m_ASGrid; } }

        public Vector3 gridCenterPoint {
            get {
                return (gridStartPoint + gridEndPoint) / 2;
            }
        }

        public Vector3 limitCenterPoint {
            get {
                return (limitStartPoint + limitEndPoint) / 2;
            }
        }


        protected GameObject rootElements {
            get {
                if (m_RootElements == null) {
                    var temp = this.transform.Find("element");
                    m_RootElements = temp == null ? this.gameObject : temp.gameObject;
                }
                return m_RootElements;
            }
        }

        //获取Grid中的位置值
        public Vector3 UF_GetGridPosition(int x,int y) {
            float width = Mathf.Abs(gridStartPoint.x - gridEndPoint.x);
            float height = Mathf.Abs(gridStartPoint.z - gridEndPoint.z);
            float unitWidth = width / gridWidth;
            float unitHeight = height / gridHeight;
            float hfUnitWidth = unitWidth / 2;
            float hfUnitHeight = unitHeight / 2;
            return new Vector3(unitWidth * x + hfUnitWidth, 0, unitHeight * y + hfUnitHeight) + gridStartPoint;
        }


        //获取合理的位置
        public Vector3 UF_GetResonableGridPosition(int x, int y) {
            if (UF_GetGridState(x, y) != 0)
            {
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        int fx = Mathf.Clamp(x + i, 0, gridWidth - 1);
                        int fy = Mathf.Clamp(y + j, 0, gridHeight - 1);
                        if (UF_GetGridState(fx, fy) == 0) {
                            return UF_GetGridPosition(fx, fy);
                        }
                    }
                }
                Debugger.UF_Error(string.Format("Can not get resonable grid positon with X:{0} Y:{1}",x,y));
            }

            return UF_GetGridPosition(x, y);
        }


        //获取grid位置点
        public Vector2 UF_GetGridPoint(Vector3 worldPos) {
            float x = 0;
            float y = 0;
            float width = Mathf.Abs(gridStartPoint.x - gridEndPoint.x);
            float height = Mathf.Abs(gridStartPoint.z - gridEndPoint.z);
            x = Mathf.Clamp(gridWidth * (worldPos.x - gridStartPoint.x) / width, 0, gridWidth - 1);
            y = Mathf.Clamp(gridHeight * (worldPos.z - gridStartPoint.z) / height, 0, gridHeight - 1);
            return new Vector2((int)x, (int)y);
        }


        public int UF_GetGridState(int x, int y) {
            return m_ASGrid.UF_GetState(x, y);
        }

        //获取附近一个点
        public Vector2 UF_GetGridPointInLocal(int x,int y,int state,int range,int randIndex) {
            int startX = Mathf.Max(0, x - range);
            int endX = Mathf.Min(gridWidth, x + range);
            int startY = Mathf.Max(0, y - range);
            int endY = Mathf.Min(gridHeight, y + range);
            int lenX = Mathf.Abs(startX - endX);
            int lenY = Mathf.Abs(startY - endY);
            Vector2 ret = new Vector2(x, y);
            List<Vector2> tempList = ListCache<Vector2>.Acquire();

            for (int i = 0; i <= lenX; i++) {
                for (int j = 0; j <= lenY; j++) {
                    int tx = Mathf.Min(startX + i,gridWidth -1);
                    int ty = Mathf.Min(startY + j, gridHeight - 1);
                    if (UF_GetGridState(tx, ty) == state)
                    {
                        tempList.Add(new Vector2(tx, ty));
                    }
                }
            }
            if (tempList.Count == 0) {
                ListCache<Vector2>.Release(tempList);
                return ret;
            }
            if (randIndex == 0)
            {
                ret = tempList[0];
            }
            else {
                //取出对应的索引
                int index = randIndex;
                if (randIndex > tempList.Count)
                    index = randIndex % tempList.Count;
                ret = tempList[Mathf.Max(0,index - 1)];
            }
            ListCache<Vector2>.Release(tempList);
            return ret;
        }

        public Vector3 UF_GetGridPositionInLocal(Vector3 worldPos,int state,int range,int randIndex) {
            Vector2 point = UF_GetGridPoint(worldPos);
            Vector2 localPoint = UF_GetGridPointInLocal((int)point.x,(int)point.y, state, range, randIndex);
            return UF_GetGridPosition((int)localPoint.x, (int)localPoint.y);
        }



        public Vector4 UF_GetLimitRect() {
            return new Vector4(limitStartPoint.x, limitStartPoint.z, Mathf.Abs(limitStartPoint.x - limitEndPoint.x), Mathf.Abs(limitStartPoint.z - limitEndPoint.z));
        }

        //添加场景物体到场景指定索引位置中
        public void UF_AddSceneElement(SceneElement obj,int x,int y) {
            if (obj == null)
            {
                Debugger.UF_Error("Object is null , add to scene failed!");
                return;
            }
            var wpos = UF_GetGridPosition(x, y);
            obj.position = wpos;
            obj.transform.parent = rootElements.transform;
            //计算缩放尺寸,机遇宽度缩放
            float scale = Mathf.Abs(gridStartPoint.x - gridEndPoint.x) / gridWidth;

            obj.transform.localScale = new Vector3(scale, scale, scale);

            //设置层
            //if (obj.tag == DefineTag.Unwalk)
            //    obj.SetLayer(DefineLayer.Default);
            //else
            if (obj.tag == DefineTag.Block)
            {
                obj.gameObject.layer = DefineLayer.HitRaycast;
                obj.collider.enabled = true;
                obj.collider.isTrigger = false;
                if (obj.pivotTransform != null)
                {
                    //缩放旋转
                    obj.pivotTransform.localEulerAngles = new Vector3(-slope, 0, 0);
                    obj.pivotTransform.localScale = new Vector3(1, 1, m_ElementPivotScale);
                    if (obj.sprite != null)
                        obj.sprite.sortingOrder = x - 100;
                }
            }
            else if (obj.tag == DefineTag.Unwalk)
            {
                obj.gameObject.layer=DefineLayer.IgoreRaycast;
                if (obj.sprite != null)
                    obj.sprite.sortingOrder = x - 1000;
            }
            else if (obj.tag == DefineTag.Walkable)
            {
                obj.gameObject.layer=DefineLayer.IgoreRaycast;
                if (obj.sprite != null) {
                    obj.sprite.sortingOrder = x - 1000;
                    obj.collider.isTrigger = true;
                }
            }
            m_ListElements.Add(obj);
        }

        public SceneElement UF_GetSceneElement(int x,int y) {
            for (int k = 0; k < m_ListElements.Count; k++) {
                if (m_ListElements[k] != null) {
                    if (m_ListElements[k].x == x && m_ListElements[k].y == y) {
                        return m_ListElements[k];
                    }
                }
            }
            return null;
        }

        //载入布局
        public void UF_LoadSceneElementLayout(string dataName) {
            UF_ClearSceneElement();
            //var layoutData =  AssetSystem.UF_GetInstance().LoadObjectImageComponent<SecenElementLayout>(dataName);
            AssetSceneLayout layoutData = RefObjectManager.UF_GetInstance().UF_LoadRefObject<AssetSceneLayout>(dataName, false) as AssetSceneLayout;
            if (layoutData != null) {
                foreach (var item in layoutData.Data) {
                    if (!string.IsNullOrEmpty(item.block)) {
                        var element = CEntitySystem.UF_GetInstance().UF_Create<SceneElement>(item.block);
                        if (element != null) {
                            this.UF_AddSceneElement(element, item.x, item.y);
                            //寻路相关值设置
                            UF_SetASGridBlockStateArea(element, item.x, item.y);
                        }
                    }
                }
            }
        }

        private void UF_SetASGridBlockStateArea(SceneElement element,int x,int y) {
            for (int i = 0; i < element.occupyWidth; i++) {
                for (int j = 0; j < element.occupyHeight; j++) {
                    int state = element.tag == DefineTag.Walkable ? 0 : 1;
                    //设置不可行走区域区域
                    m_ASGrid.UF_SetState(x + i, y + j, (byte)state);
                }
            }
        }


        public void UF_SetChildActive(string childName,bool value) {
            var child = this.transform.Find(childName);
            if (child != null) {
                child.gameObject.SetActive(value);
            }
        }

        public Vector3 UF_GetChildPosition(string childName) {
            var child = this.transform.Find(childName);
            if (child != null)
            {
                return child.transform.position;
            }
            else {
                return Vector3.zero;
            }
        }


        public void UF_ClearSceneElement() {
            for (int k = 0; k < m_ListElements.Count; k++)
            {
                if (m_ListElements[k] != null)
                {
                    m_ListElements[k].Release();
                }
            }
            m_ListElements.Clear();
            //清除可行走区域状态
            m_ASGrid.UF_FlushState(0);
        }


        protected float UF_GetScaleBaseSlope(float value) {
            float tv = Mathf.Cos(Mathf.Deg2Rad * value);
            tv = tv <= 0 ? 1 : tv;
            return 1.0f / tv;
        }


        //设置斜率，影响地图缩放
		public void UF_SetSlope(float value) {
            slope = value;
            float sval = UF_GetScaleBaseSlope(value);
            //缩放尺寸
            this.localScale = new Vector3(1, 1, sval);

            //计算element的描点缩放值 
            float cosv = Mathf.Cos(Mathf.Deg2Rad * slope);
            float spvi = Mathf.Sqrt(sval) * cosv;
            m_ElementPivotScale = spvi;
        }

        public void UF_OnAwake() {
            m_ASGrid.UF_Reset(gridWidth,gridHeight);
        }


        public void UF_OnReset() {
            //清除元素
            UF_ClearSceneElement();
            //SetSlope(0);
        }



#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            //绘制grid定位点
            float width = Mathf.Abs(gridStartPoint.x - gridEndPoint.x);
            float height = Mathf.Abs(gridStartPoint.z - gridEndPoint.z);
            Vector3 posA = gridStartPoint;
            Vector3 posB = posA + new Vector3(width, 0, 0);
            Vector3 posC = posB + new Vector3(0, 0, height);
            Vector3 posD = posA + new Vector3(0, 0, height);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(posA, 0.3f);
            Gizmos.DrawSphere(posC, 0.3f);
            Gizmos.DrawLine(posA, posB);
            Gizmos.DrawLine(posB, posC);
            Gizmos.DrawLine(posC, posD);
            Gizmos.DrawLine(posD, posA);

            //绘制网格区域
            float unitWidth = width / gridWidth;
            float unitHeight = height / gridHeight;
            for (int w = 0; w < gridWidth; w++)
            {
                Vector3 vForm = posA + new Vector3(w * unitWidth, 0, 0);
                Vector3 vTo = posD + new Vector3(w * unitWidth, 0, 0);
                Gizmos.DrawLine(vForm, vTo);
            }
            for (int h = 0; h < gridHeight; h++)
            {
                Vector3 vForm = posA + new Vector3(0, 0, h * unitHeight);
                Vector3 vTo = posB + new Vector3(0, 0, h * unitHeight);
                Gizmos.DrawLine(vForm, vTo);
            }

            //绘制grid定位点
            width = Mathf.Abs(limitStartPoint.x - limitEndPoint.x);
            height = Mathf.Abs(limitStartPoint.z - limitEndPoint.z);
            posA = limitStartPoint;
            posB = posA + new Vector3(width, 0, 0);
            posC = posB + new Vector3(0, 0, height);
            posD = posA + new Vector3(0, 0, height);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(posA, 0.3f);
            Gizmos.DrawSphere(posC, 0.3f);
            Gizmos.DrawLine(posA, posB);
            Gizmos.DrawLine(posB, posC);
            Gizmos.DrawLine(posC, posD);
            Gizmos.DrawLine(posD, posA);


            //绘制每个grid中点位置
            Gizmos.color = Color.green;
            for (int w = 0; w < gridWidth; w++) {
                for (int h = 0; h < gridHeight; h++) {
                    //如果为不可行区域，绘制红色
                    Gizmos.color = !Application.isPlaying ? Color.green : UF_GetGridState(w, h) == 0 ? Color.green : Color.red;
                    Gizmos.DrawSphere(UF_GetGridPosition(w, h), 0.06f);
                }
            }


        }
#endif
    }
}

