//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------   
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnityFrame {

    internal class RenderPreview {

        public GameObject root { get; private set; }
        public GameObject pivot { get; private set; }
        public Camera camera { get; private set; }
        public RenderTexture texture { get; private set; }

        private Dictionary<Object, string> m_PriviewObjects = new Dictionary<Object, string>();

        private bool m_Active;

        public Vector3 position {
            get {
                return root.transform.position;
            }
            set {
                root.transform.position = value;
            }
        }

        public RenderPreview(string name,Vector3 pos) {
            root = new GameObject(name);
            pivot = new GameObject("pivot");
            pivot.transform.parent = root.transform;
            pivot.transform.localPosition = Vector3.zero;

            GameObject goCamera = new GameObject("camera");
            goCamera.transform.parent = root.transform;
            goCamera.transform.localEulerAngles = new Vector3(0, 180, 0);
            root.transform.position = pos;
            
            camera = goCamera.AddComponent<Camera>();
            camera.nearClipPlane = 1;
            camera.farClipPlane = 6;
            camera.renderingPath = RenderingPath.UsePlayerSettings;
            camera.backgroundColor = new Color(0, 0, 0, 0);
            camera.allowHDR = false;
            camera.allowMSAA = false;
            camera.allowDynamicResolution = false;
            camera.useOcclusionCulling = false;

        }

        public void UF_SetAvtive(bool v) {
            m_Active = v;
            if (m_Active)
            {
                if (texture == null){
                    texture = RenderTexture.GetTemporary(new RenderTextureDescriptor(1024, 1024, RenderTextureFormat.ARGB32, 24));
                    texture.autoGenerateMips = false;
                }
            }
            else {
                if (texture != null) {
                    RenderTexture.ReleaseTemporary(texture);
                    texture = null;
                }
                UF_SetEuler(Vector3.zero);
            }
            if (camera != null) {
                camera.enabled = v;
                camera.gameObject.SetActive(v);
                camera.targetTexture = texture;
            }
        }

        public void UF_SetEuler(Vector3 euler) {
            if (pivot != null) {
                pivot.transform.localEulerAngles = euler;
            }
        }



        public void UF_Add(Object obj, bool normalize, string alias) {
            if (obj == null) return;
            GameObject go = null;
            if (obj is GameObject)
                go = obj as GameObject;
            else if (obj is Transform)
                go = (obj as Transform).gameObject;
            else if (obj is MonoBehaviour)
                go = (obj as MonoBehaviour).gameObject;
            else {
                Debugger.UF_Error(string.Format("Can not Add Object[{0}] to PreviewManger", obj.GetType().ToString()));
                return;
            }

            if (!m_PriviewObjects.ContainsKey(obj)) {
                go.transform.parent = pivot.transform;
                if(!string.IsNullOrEmpty(alias))
                    m_PriviewObjects.Add(obj, alias);
                else
                    m_PriviewObjects.Add(obj, obj.name);
            }

            if (normalize)
            {
                go.transform.localPosition = Vector3.zero;
                go.transform.localEulerAngles= Vector3.zero;
            }
        }

        public Object UF_Get(string name) {
            foreach (var v in m_PriviewObjects) {
                if (v.Value == name || v.Key.name == name) {
                    return v.Key;
                }
            }
            return null;
        }

        public void UF_Remove(string name)
        {
            Object tar = null;
            foreach (var v in m_PriviewObjects)
            {
                if (v.Value== name || v.Key.name == name)
                {
                    tar = v.Key;
                    break;
                }
            }
            if (tar != null) {
                m_PriviewObjects.Remove(tar);
                UF_DestoryObject(tar);
            }
        }

        public void UF_Remove(Object obj) {
            if (m_PriviewObjects.ContainsKey(obj)) {
                m_PriviewObjects.Remove(obj);
                UF_DestoryObject(obj);
            }
        }

        public void UF_Clear() {
            foreach (var v in m_PriviewObjects) {
                UF_DestoryObject(v.Key);
            }
            if (pivot != null) {
                pivot.transform.localEulerAngles = Vector3.zero;
            }
            
            m_PriviewObjects.Clear();
        }


        private void UF_ReleaseGameObject(GameObject go) {
            IEntityHnadle entity = go.GetComponent<IEntityHnadle>();
            if (entity != null)
            {
                entity.isReleased = true;
                go.transform.SetParent(null);
            }
            else {
                Object.Destroy(go);
            }
        }

        protected void UF_DestoryObject(Object obj) {
            if (obj == null) return;
            if (obj is MonoBehaviour)
            {
                UF_ReleaseGameObject(((MonoBehaviour)obj).gameObject);
            }
            if (obj is GameObject)
            {
                UF_ReleaseGameObject(((GameObject)obj).gameObject);
            }
            else if (obj is Transform)
            {
                UF_ReleaseGameObject(((Transform)obj).gameObject);
            }
        }

        public void UF_Release() {
            this.UF_Clear();
            if (texture != null)
            {
                RenderTexture.ReleaseTemporary(texture);
            }
            camera.targetTexture = null;
            texture = null;
            Object.Destroy(root);
        }
    }

    //预览渲染管理器
    //用于管理预览摄Camera及其RenderTexture的创建与使用
    public class RenderPreviewManager : HandleSingleton<RenderPreviewManager> ,IOnAwake
    {
        Stack<RenderPreview> m_PoolPreview = new Stack<RenderPreview>();

        private GameObject m_Root;

        static int s_Index = 0;
        static int s_Space = -12;

        //最大缓存栈
        static int s_MaxPreview = 3;

        public void UF_OnAwake() {
            m_Root = new GameObject("Preview Root");
            m_PoolPreview.Clear();
        }

        internal RenderPreview UF_AcquirePreview() {
            RenderPreview ret = null;
            if (m_PoolPreview.Count > 0)
                ret = m_PoolPreview.Pop();
            if (ret == null) {
                s_Index++;
                ret = new RenderPreview(s_Index.ToString(),new Vector3(s_Space * s_Index,0,0));
            }
            ret.root.transform.parent = m_Root.transform;
            ret.UF_SetAvtive(true);
            return ret;
        }

        internal void UF_ReleasePreview(RenderPreview v)
        {
            if (v == null) return;
            if (m_PoolPreview.Count < s_MaxPreview)
            {
                v.UF_SetAvtive(false);
                v.UF_Clear();
                m_PoolPreview.Push(v);
            }
            else {
                v.UF_SetAvtive(false);
                v.UF_Release();
            }
        }

    }



}

