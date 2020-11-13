using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

using UnityFrame;


[CustomEditor(typeof(UnityFrame.SceneMap), true)]
public class EditorSceneMap : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("构建网格布局")) {
            ResetSceneMapGrid();
        }

        if (GUILayout.Button("构建碰撞盒")) {
            ResetSenenMapCollider();
        }
    }

    private GameObject FindGameObject(GameObject root,string val,bool createWhileNull) {
        var temp = root.transform.Find(val);
        if (temp == null && createWhileNull) {
            temp = new GameObject(val).transform;
            temp.parent = root.transform;
            temp.transform.localPosition = Vector3.zero;
        }
        return temp == null ? null : temp.gameObject;
    }

    private void DestroyChild(GameObject root) {
        List<Transform> tempList = new List<Transform>();
        for (int k = 0; k < root.transform.childCount; k++) {
            tempList.Add(root.transform.GetChild(k));
        }
        foreach (Transform v in tempList) {
            Object.DestroyImmediate(v.gameObject);
        }
    }


    private GameObject CreateColliderBox(string strName,bool isTrigger = false) {
        var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(box.GetComponent<MeshRenderer>());
        Object.DestroyImmediate(box.GetComponent<MeshFilter>());
        box.GetComponent<BoxCollider>().isTrigger = isTrigger;
        //加入刚体属性
        Rigidbody rigibody = box.AddComponent<Rigidbody>();
        rigibody.isKinematic = true;
        rigibody.useGravity = false;
        box.tag = DefineTag.Wall;
        box.name = strName;
        //设置标签
        return box;
    }

    private void ResetSenenMapCollider() {

        SceneMap map = target as SceneMap;
        GameObject rootCollider = FindGameObject(map.gameObject, "collider", false);
        GameObject rootMark = FindGameObject(map.gameObject, "mark", false);
        Vector3 gridStartPoint = FindGameObject(rootMark.gameObject, "gridStartMark", false).transform.position;
        Vector3 gridEndPoint = FindGameObject(rootMark.gameObject, "gridEndMark", false).transform.position;
        GameObject box = null;
        Vector3 center = Vector3.zero;

        float thickness = 2f;
        float lenScale = 3;
        float heightScale = 8;
        float doorsize = 1;

        DestroyChild(rootCollider);

        //地板
        box = CreateColliderBox("ground");
        box.layer = DefineLayer.HitRaycast;
        lenScale = Vector3.Distance(gridEndPoint, gridStartPoint);
        box.transform.localScale = new Vector3(lenScale, thickness, map.mapTextureList.Count * lenScale);
        box.transform.position = (gridStartPoint + gridEndPoint) / 2;
        box.transform.position += new Vector3(0, -thickness/2, 0);
        box.transform.parent = rootCollider.transform;
        box.tag = DefineTag.Ground;

        //左
        box = CreateColliderBox("left");
        box.layer = DefineLayer.HitRaycast;
        center = new Vector3(gridStartPoint.x, 0, (gridEndPoint.z + gridStartPoint.z) / 2);
        lenScale = Mathf.Abs(gridEndPoint.z - gridStartPoint.z);
        box.transform.position = center;
        box.transform.localScale = new Vector3(thickness, heightScale, lenScale + thickness * 2);
        box.transform.position += new Vector3(-thickness/2,0,0);
        box.transform.parent = rootCollider.transform;
        //右
        box = CreateColliderBox("right");
        box.layer = DefineLayer.HitRaycast;
        center = new Vector3(gridEndPoint.x, 0, (gridEndPoint.z + gridStartPoint.z) / 2);
        lenScale = Mathf.Abs(gridEndPoint.z - gridStartPoint.z);
        box.transform.position = center;
        box.transform.localScale = new Vector3(thickness, heightScale, lenScale + thickness * 2);
        box.transform.position += new Vector3(thickness / 2, 0, 0);
        box.transform.parent = rootCollider.transform;
        //下
        box = CreateColliderBox("down");
        box.layer = DefineLayer.HitRaycast;
        center = new Vector3((gridEndPoint.x + gridStartPoint.x) / 2, 0, gridStartPoint.z);
        lenScale = Mathf.Abs(gridEndPoint.x - gridStartPoint.x) + thickness * 2;
        box.transform.position = center;
        box.transform.localScale = new Vector3(lenScale, heightScale, thickness);
        box.transform.position += new Vector3(0, 0, -thickness / 2);
        box.transform.parent = rootCollider.transform;
        //门

        //上R
        //上top
        box = CreateColliderBox("up");
        var rootUp = box.gameObject;
        box.layer = DefineLayer.HitRaycast;
        center = new Vector3((gridEndPoint.x + gridStartPoint.x) / 2, 0, gridEndPoint.z);
        lenScale = Mathf.Abs(gridEndPoint.x - gridStartPoint.x) + thickness * 2;
        box.transform.position = center;
        box.transform.localScale = new Vector3(lenScale, heightScale, thickness);
        box.transform.position += new Vector3(0, 0, thickness + thickness / 2);
        box.transform.parent = rootCollider.transform;

        box = CreateColliderBox("up_right");
        box.layer = DefineLayer.HitRaycast;
        center = new Vector3((gridEndPoint.x + gridStartPoint.x) / 2, 0, gridEndPoint.z);
        lenScale = Mathf.Abs(gridEndPoint.x - gridStartPoint.x) + thickness * 2;
        //lenScale = lenScale / 2;
        box.transform.position = center;
        box.transform.localScale = new Vector3(lenScale, heightScale, thickness);
        box.transform.position += new Vector3(-lenScale/2 - doorsize, 0, thickness / 2);
        box.transform.parent = rootUp.transform;
        //上L
        box = CreateColliderBox("up_left");
        box.layer = DefineLayer.HitRaycast;
        center = new Vector3((gridEndPoint.x + gridStartPoint.x) / 2, 0, gridEndPoint.z);
        lenScale = Mathf.Abs(gridEndPoint.x - gridStartPoint.x) + thickness * 2;
        //lenScale = lenScale / 2;
        box.transform.position = center;
        box.transform.localScale = new Vector3(lenScale, heightScale, thickness);
        box.transform.position += new Vector3(lenScale/2 + doorsize, 0, thickness / 2);
        box.transform.parent = rootUp.transform;

        box = CreateColliderBox("door");
        box.layer = DefineLayer.HitRaycast;
        center = new Vector3((gridEndPoint.x + gridStartPoint.x) / 2, 0, gridEndPoint.z);
        lenScale = Mathf.Abs(gridEndPoint.x - gridStartPoint.x) + thickness * 2;
        box.transform.position = center;
        box.transform.localScale = new Vector3(doorsize * 4, heightScale, thickness);
        box.transform.position += new Vector3(0, 0, thickness / 2);
        box.transform.parent = rootUp.transform;

        box = CreateColliderBox("gate",true);
        center = new Vector3((gridEndPoint.x + gridStartPoint.x) / 2, 0, gridEndPoint.z);
        box.transform.position = center + new Vector3(0,0, thickness);
        box.transform.localScale = new Vector3(1,2,1) * doorsize * 2;
        box.transform.parent = rootUp.transform;
        box.tag = DefineTag.Gate;


    }

    private void ResetSceneMapGrid() {
        float baseSize = 1;
        SceneMap map = target as SceneMap;
        GameObject rootMark = FindGameObject(map.gameObject, "mark", true);
        rootMark.transform.localPosition = Vector3.zero;

        GameObject gridStartMark = FindGameObject(rootMark.gameObject, "gridStartMark", true);
        GameObject gridEndMark = FindGameObject(rootMark.gameObject, "gridEndMark", true);
        GameObject limitStartMark = FindGameObject(rootMark.gameObject, "limitStartMark", true);
        GameObject limitEndMark = FindGameObject(rootMark.gameObject, "limitEndMark", true);

        GameObject mapGate = FindGameObject(rootMark.gameObject, "mapGate", true);

        gridStartMark.transform.position = new Vector3(3, 0, 12);
        gridEndMark.transform.position = new Vector3(15, 0, 30);
        limitStartMark.transform.position = new Vector3(0, 0, 0);
        limitEndMark.transform.position = new Vector3(18, 0, 42);

        GameObject rootGround = FindGameObject(map.gameObject, "ground", true);
        rootGround.transform.localPosition = Vector3.zero;


        GameObject rootCollider = FindGameObject(map.gameObject, "collider", true);
        rootCollider.transform.SetAsLastSibling();
        rootCollider.transform.localPosition = Vector3.zero;

        GameObject rootElement = FindGameObject(map.gameObject, "element", true);
        rootElement.transform.SetAsLastSibling();
        rootElement.transform.localPosition = Vector3.zero;


        GameObject rootEffect = FindGameObject(map.gameObject, "effect", true);
        rootElement.transform.SetAsLastSibling();
        rootEffect.transform.localPosition = Vector3.zero;

        map.gridStartMark = gridStartMark;
        map.gridEndMark = gridEndMark;
        map.limitStartMark = limitStartMark;
        map.limitEndMark = limitEndMark;

        //构建地图,自动创建贴图材质
        DestroyChild(rootGround);
        Mesh mesh = Resources.GetBuiltinResource(typeof(Mesh), "Quad.fbx") as Mesh;
        
        for (int k = 0; k < map.mapTextureList.Count; k++) {
            
            GameObject mapquad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Object.DestroyImmediate(mapquad.GetComponent<MeshCollider>());
            mapquad.name = "map_" + (k + 1);
            mapquad.transform.parent = rootGround.transform;
            mapquad.transform.localPosition = new Vector3(baseSize / 2, 0,k * baseSize + baseSize/2);
            mapquad.transform.localScale = Vector3.one;
            mapquad.transform.localEulerAngles = new Vector3(90, 0, 0);
            MeshRenderer mr = mapquad.GetComponent<MeshRenderer>();
            mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            mr.receiveShadows = false;
            if (map.mapTextureList[k] != null)
            {
                string texPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(map.mapTextureList[k]));
                string matName = "mat_" + map.mapTextureList[k].name;
                string matDir = texPath + "/Materials";
                if (!Directory.Exists(matDir)) {
                    Directory.CreateDirectory(matDir);
                    AssetDatabase.Refresh();
                }   
                string matPath = matDir + "/" + matName + ".mat";
                Material mat = AssetDatabase.LoadAssetAtPath(matPath,typeof(Material)) as Material;
                if (mat == null) {
                    mat = new Material(Shader.Find("Game/Map/MapDiffuse"));
                    AssetDatabase.CreateAsset(mat, matPath);
                }
                mat.mainTexture = map.mapTextureList[k];
                mr.sharedMaterial = mat;
            }
        }

        rootGround.transform.localScale = Vector3.one * map.size;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

    }






}
