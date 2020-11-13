using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;


//预览窗口
public class PreviewWindow
{
    protected PreviewRenderUtility m_Preview;

    protected Dictionary<int, GameObject> m_MapObjects = new Dictionary<int, GameObject>();
    //protected Renderer[] m_Renderers = null;

    //视图控制操作
    protected enum ViewCtrlOpt
    {
        None,
        Pan,    //平移
        Zoom,   //缩放
        Orbit   //环绕
    }

    protected ViewCtrlOpt m_ViewCtrlOpt = ViewCtrlOpt.None;
    protected float m_ZoomFactor = 1.68f;
    protected Vector2 m_PreviewDir = new Vector2(145, -20);
    protected Vector3 m_PivotPositionOffset = new Vector3(0, 0.618f, 0);
    protected int m_PreviewHint = "Preview".GetHashCode();
    protected int m_PreviewSceneHint = "PreviewSene".GetHashCode();
    protected float m_FloorScale = 3.0f;
    protected Mesh m_FloorPlane;
    protected Texture2D m_FloorTexture;
    protected Material m_FloorMaterial;

    protected bool m_Inited = false;

    public bool inited { get { return m_Inited; } }

    protected void AssertInit()
    {
        if (m_Preview == null)
            throw new System.Exception("ModelPreview have not been Init,Call InitPreview First");
    }

    public Camera camera
    {
        get
        {
            return m_Preview.camera;
        }
    }



    virtual public void InitPreview()
    {
        if (m_Inited) return;

        m_Preview = new PreviewRenderUtility(true);
        camera.nearClipPlane = 0.5f;
        camera.farClipPlane = 1000;
        camera.fieldOfView = 30.0f;
        camera.cullingMask = 1 << PreviewHelper.CullingLayer;
        camera.allowHDR = false;
        camera.orthographic = false;
        camera.allowMSAA = false;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.transform.position = new Vector3(0, 1.5f, -5);
        camera.transform.eulerAngles = new Vector3(20, 0, 0);
        camera.backgroundColor = new Color(0.28f, 0.28f, 0.28f, 1.0f);
        camera.depthTextureMode = DepthTextureMode.Depth;

        var lights = m_Preview.lights;
        lights[0].intensity = 1.4f;
        lights[0].transform.rotation = Quaternion.Euler(40f, 40f, 0);
        lights[1].intensity = 1.4f;
        
        //地板资源
        m_FloorPlane = (Resources.GetBuiltinResource(typeof(Mesh), "New-Plane.fbx") as Mesh);
        m_FloorTexture = (Texture2D)EditorGUIUtility.Load("Avatar/Textures/AvatarFloor.png");
        Shader shader = EditorGUIUtility.LoadRequired("Previews/PreviewPlaneWithShadow.shader") as Shader;
        m_FloorMaterial = new Material(shader);
        m_FloorMaterial.mainTexture = m_FloorTexture;
        m_FloorMaterial.mainTextureScale = Vector2.one * 5f * 4f;
        m_FloorMaterial.SetVector("_Alphas", new Vector4(0.5f, 0.3f, 0f, 0f));
        m_FloorMaterial.hideFlags = HideFlags.HideAndDontSave;
        this.m_Inited = true;
    }


    //隐藏物体
    protected void SetEnabledRecursive(bool value)
    {
        if (m_MapObjects.Count > 0)
        {
            foreach (KeyValuePair<int, GameObject> item in m_MapObjects) {
                if(item.Value != null) {
                   PreviewHelper.SetEnabledRecursive(item.Value, value);
                }
            }
        }
    }

    protected Renderer[] GetRenderComponent(GameObject go)
    {
        var renders = go.GetComponentsInChildren<Renderer>();
        if (renders != null && renders.Length > 0)
        {
            List<Renderer> tempRenders = new List<Renderer>(renders);
            for (int k = 0; k < tempRenders.Count; k++)
            {
                if (!tempRenders[k].enabled)
                {
                    tempRenders.RemoveAt(k);
                    k--;
                }
            }
            return tempRenders.ToArray();
        }
        return renders;
    }

    //添加GO 到预览窗体
    virtual public GameObject AddGameObject(GameObject go,bool cloneInstance = true)
    {
        AssertInit();

        if (go == null)
            return null;

        if (m_MapObjects.ContainsKey(go.GetInstanceID())) {
            Debug.LogError(string.Format("GameObject[{0}] target is Exit in PreviewWindow", go.GetInstanceID()));
            return go;
        }

        GameObject target = null;
        if (cloneInstance)
        {
            target = PreviewHelper.InstantiateGameObject(go);
        }
        else {
            PreviewHelper.FormatGameObject(go);
            target = go;
        }

        target.transform.position = Vector3.zero;

        //加入到目标列表
        m_MapObjects.Add(target.GetInstanceID(), target);

        SetEnabledRecursive(false);

#if UNITY_2017_1_OR_NEWER
        m_Preview.AddSingleGO(target);
#endif
        return target;
    }

    virtual public void Reset()
    {
        List<GameObject> tempList = new List<GameObject>();
        foreach (KeyValuePair<int, GameObject> item in m_MapObjects)
        {
            tempList.Add(item.Value);
        }
        m_MapObjects.Clear();
        for (int k = 0; k < tempList.Count; k++) {
            Object.DestroyImmediate(tempList[k]);
        }
    }



    virtual protected void UF_OnUpdateCamera()
    {
        Quaternion camRot = Quaternion.Euler(-m_PreviewDir.y, -m_PreviewDir.x, 0);
        Vector3 camPos = camRot * (Vector3.forward * -5.5f * m_ZoomFactor) + m_PivotPositionOffset;
        camera.transform.position = camPos;
        camera.transform.rotation = camRot;
    }


    protected ViewCtrlOpt viewTool
    {
        get
        {
            Event evt = Event.current;
            if (m_ViewCtrlOpt == ViewCtrlOpt.None)
            {
                bool controlKeyOnMac = (evt.control && Application.platform == RuntimePlatform.OSXEditor);

                // actionKey could be command key on mac or ctrl on windows
                bool actionKey = EditorGUI.actionKey;

                bool noModifiers = (!actionKey && !controlKeyOnMac && !evt.alt);

                if ((evt.button == 1 && noModifiers) || (evt.button <= 1 && actionKey) || evt.button == 2)
                    m_ViewCtrlOpt = ViewCtrlOpt.Pan;
                else if ((evt.button == 1 && controlKeyOnMac) || (evt.button == 0 && evt.alt))
                    m_ViewCtrlOpt = ViewCtrlOpt.Zoom;
                else if (evt.button == 1 && evt.alt || evt.button == 0)
                    m_ViewCtrlOpt = ViewCtrlOpt.Orbit;
            }
            return m_ViewCtrlOpt;
        }
    }


    protected void DoPreviewOrbit(Event evt, Rect previewRect)
    {
        m_PreviewDir -= evt.delta * (evt.shift ? 3 : 1) / Mathf.Min(previewRect.width, previewRect.height) * 140.0f;
        m_PreviewDir.y = Mathf.Clamp(m_PreviewDir.y, -90, 90);
        evt.Use();
    }

    protected void DoPreviewPan(Event evt)
    {
        Camera cam = camera;
        Vector3 screenPos = cam.WorldToScreenPoint(m_PivotPositionOffset);
        Vector3 delta = new Vector3(-evt.delta.x, evt.delta.y, 0);
        // delta panning is scale with the zoom factor to allow fine tuning when user is zooming closely.
        screenPos += delta * Mathf.Lerp(0.25f, 2.0f, m_ZoomFactor * 0.5f);
        Vector3 worldDelta = cam.ScreenToWorldPoint(screenPos) - m_PivotPositionOffset;
        m_PivotPositionOffset += worldDelta;
        evt.Use();
    }


    virtual protected void UF_OnUpdateControl(Rect previewRect)
    {
        int controlID = GUIUtility.GetControlID(m_PreviewHint, FocusType.Passive, previewRect);
        Event evt = Event.current;
        EventType eventType = evt.GetTypeForControl(controlID);
        int id = GUIUtility.GetControlID(m_PreviewSceneHint, FocusType.Passive);
        eventType = evt.GetTypeForControl(id);
        //判断操作逻辑
        if (eventType == EventType.ScrollWheel)
        {
            if (previewRect.Contains(evt.mousePosition))
            {
                float delta = HandleUtility.niceMouseDeltaZoom * (evt.shift ? 2.0f : 0.5f);
                float zoomDelta = -delta * 0.05f;
                m_ZoomFactor += m_ZoomFactor * zoomDelta;
                m_ZoomFactor = Mathf.Max(m_ZoomFactor, 0.25f);
                evt.Use();
            }
        }
        else if (eventType == EventType.MouseDown)
        {
            if (viewTool != ViewCtrlOpt.None && previewRect.Contains(evt.mousePosition))
            {
                EditorGUIUtility.SetWantsMouseJumping(1);
                evt.Use();
                GUIUtility.hotControl = id;
            }
        }
        else if (eventType == EventType.MouseUp)
        {
            if (GUIUtility.hotControl == id)
            {
                m_ViewCtrlOpt = ViewCtrlOpt.None;
                GUIUtility.hotControl = 0;
                EditorGUIUtility.SetWantsMouseJumping(0);
                evt.Use();
            }
        }
        else if (eventType == EventType.MouseDrag)
        {
            if (GUIUtility.hotControl == id)
            {
                switch (m_ViewCtrlOpt)
                {
                    case ViewCtrlOpt.Orbit: DoPreviewOrbit(evt, previewRect); break;
                    case ViewCtrlOpt.Pan: DoPreviewPan(evt); break;
                    default: Debug.Log("Enum value not handled"); break;
                }
            }
        }
    }

    virtual protected void OnDrawModel()
    {
        if (m_Preview == null) return;
        //other mesh

        //model
        SetEnabledRecursive(true);
#if UNITY_2017_1_OR_NEWER
        m_Preview.Render();
#else
        m_Preview.camera.Render();
#endif

        SetEnabledRecursive(false);
    }


    virtual protected void OnDrawFloor()
    {
        if (m_FloorMaterial == null || m_FloorTexture == null || m_FloorPlane == null)
            return;
        Vector3 position = new Vector3(0f, 0f, 0f);
        Material floorMaterial = m_FloorMaterial;
        Matrix4x4 matrix2 = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one * m_FloorScale);
        floorMaterial.mainTextureOffset = -new Vector2(position.x, position.z) * 0.08f;
        floorMaterial.SetVector("_Alphas", new Vector4(0.5f * 1f, 0.3f * 1f, 0f, 0f));
        Graphics.DrawMesh(m_FloorPlane, matrix2, floorMaterial, PreviewHelper.CullingLayer, camera, 0);
    }


    virtual public void Draw(Rect rect)
    {
        if (!m_Inited)
            return;
        //更新控制操作
        UF_OnUpdateControl(rect);
        m_Preview.BeginPreview(rect, GUIStyle.none);
        //更新摄像机位置
        UF_OnUpdateCamera();
        //绘制地板
        OnDrawFloor();
        //绘制模型
        OnDrawModel();
        m_Preview.EndAndDrawPreview(rect);
    }



    virtual public void Dispose()
    {
        Reset();
        if (m_FloorMaterial != null)
            Object.DestroyImmediate(m_FloorMaterial);
        if (m_Preview != null)
            m_Preview.Cleanup();
    }


}
