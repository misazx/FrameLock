using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityFrame;
using UnityEngine;
using UnityEditor;
using UnityEngine.SocialPlatforms;
using Object = UnityEngine.Object;

public class AdjustUIControlTools : Editor
{


    private static Transform root;

    static Dictionary<string,List<string>> atlasUsageDic;
    [MenuItem("Assets/GameTools/UI/调整所选预设的UI控件层级")]
    public static void AdjustUIControls()
    {
        if (Selection.objects == null || Selection.objects.Length < 1)
        {
            Debug.LogError(" You didn't select any objects");
            return;

        }
        root = GameObject.Find("UI Root").transform;
        if (!root) return;
        Object[] selects = Selection.objects;
        for (int i = 0; i < selects.Length; i++)
        {
            if (!selects[i].name.StartsWith("ui")) continue;
            if (selects[i].name.StartsWith("ui_atlas")) continue;
            if (!(selects[i] is GameObject)) continue;
            AdjustUIControl((selects[i] as GameObject));
        }

    }


    private static void AdjustUIControl(GameObject obj)
    {
        if(!obj) return;
        Vector2 size = Vector2.zero;
        if (obj.transform is RectTransform)
        {
            size = (obj.transform as RectTransform).sizeDelta;
        }
        GameObject cur = PrefabUtility.InstantiatePrefab(obj) as GameObject;
        cur.transform.parent = root;
        cur.transform.localScale = Vector3.one;
        if (cur.transform is RectTransform)
        {
            (cur.transform as RectTransform).sizeDelta = size;
        }
        cur.transform.localPosition = Vector3.zero;
        cur.SetActive(true);
        AdjustUILabel(cur.transform);
    }

    private static void AdjustUILabel(Transform transform)
    {
        int count = transform.childCount;
        List<Transform> childs = new List<Transform>();
        for (int i = 0; i < count; i++) 
        {
            Transform child = transform.GetChild(i);
            if (child.childCount > 0)
                AdjustUILabel(child);
            else if (child.GetComponent<UILabel>())
                childs.Add(child);
        }
        for (int i = 0; i < childs.Count; i++)
        {
            Vector3 localPos = childs[i].transform.localPosition;
            childs[i].parent = null;
            childs[i].transform.parent = transform;
            childs[i].transform.localScale = Vector3.one;
            childs[i].transform.localPosition = localPos;   
        }
        
    }

    [MenuItem("Assets/GameTools/UI/替换当前UIRoot下对象的预设")]
    public static void ApplyPrefab()
    {
        if (EditorUtility.DisplayDialog("Warning!",
                            "确定要用UIRoot的实例替换对应的Prefab吗?", "Yes",
                            "No"))
        {
            if (!root) root = GameObject.Find("UI Root").transform;
            int count = root.childCount;
            if(count.Equals(0)) return;
            for (int i = 0; i < count; i++)
            {
                Transform p = root.GetChild(i);
                if (!p.name.StartsWith("ui")) continue;
                //Debug.Log(PrefabUtility.GetPrefabType(p.gameObject));
                if (PrefabUtility.GetPrefabType(p.gameObject) == PrefabType.PrefabInstance)
                {
                    Object o = PrefabUtility.GetPrefabParent(p);
                    //Debug.Log(AssetDatabase.GetAssetPath(o));
                    PrefabUtility.ReplacePrefab(p.gameObject, o, ReplacePrefabOptions.Default);
                }
            }
            AssetDatabase.Refresh();
        }
        
    }
    [MenuItem("Assets/GameTools/UI/查找当前引用atlas集合")]
    public static void CheckUseAtlas()
    {
        if (Selection.objects == null || Selection.objects.Length < 1)
        {
            Debug.LogError(" You didn't select any objects");
            return;

        }
        Object select = Selection.objects[0];
        if (!(select is GameObject)) return;
        if (!select.name.StartsWith("ui")) return;
        atlasUsageDic = new Dictionary<string, List<string>>();
        GameObject obj = select as GameObject;
        UISprite[] sprites = obj.GetComponentsInChildren<UISprite>(true);
        for (int i = 0; i < sprites.Length; i++)
        {
            if (!sprites[i].sprite)
            {
                if(!atlasUsageDic.ContainsKey("Null"))
                    atlasUsageDic.Add("Null", new List<string>());
                atlasUsageDic["Null"].Add(sprites[i].name);
                continue;
            }
            string[] temp = sprites[i].sprite.name.Split(@"@".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (!atlasUsageDic.ContainsKey(temp[0]))
                atlasUsageDic.Add(temp[0],new List<string>());
            atlasUsageDic[temp[0]].Add(sprites[i].name);
        }
        foreach (KeyValuePair<string, List<string>> pair in atlasUsageDic)
        {
            string lg = "ui_atlas_" + pair.Key + ":\n";
            for (int i = 0; i < pair.Value.Count; i++)
            {
                lg += "     " + pair.Value[i] + "\n";
            }
            Debug.Log(lg);
        }
    }


}


public class ExchangeUIControlAtlas:EditorWindow
{
    private List<ObjInfo> selections;
    private Vector2 offset = new Vector2(3f, 6f);
    private Vector2 scroll;
    private string oldPrefix = "";
    private string newPrefix = "";
    private  Transform root;
    private Color redColor = Color.red;
    private Color greenColor = Color.green;
    [MenuItem("GameTools/Tools/ExchangeTools/ExchangeUIControlAtlas")]
    public static void Exchange()
    {
        EditorWindow ew = GetWindow<ExchangeUIControlAtlas>(false, "ExchangeUIControlAtlas");
        ew.minSize = new Vector2(980, 300);
    }

    private void OnEnable()
    {
        selections = new List<ObjInfo>();
        oldPrefix = "";
        newPrefix = "";
        root = GameObject.Find("UI Root").transform;
    }

    protected void OnGUI()
    {
        Rect rect = new Rect(offset.x, offset.y, position.width - offset.x * 2, position.height - 2 * offset.y);
        DrawSelect(DrawTip(rect));
    }

    private Rect DrawTip(Rect rect)
    {   
        Rect contentRect = rect;
        contentRect.height = EditorGUIUtility.singleLineHeight * 3;
        GUILayout.BeginArea(contentRect);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Atlas Exchange Procedure ",GUILayout.Width(160));
        GUILayout.Label("OldPrefix:", GUILayout.Width(80));
        oldPrefix = GUILayout.TextField(oldPrefix, GUILayout.Width(80));
        GUILayout.Label("NewPrefix:", GUILayout.Width(80));
        newPrefix = GUILayout.TextField(newPrefix, GUILayout.Width(80));
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("update selections", GUILayout.Width(240)))
        {
            UpdateSelections();
        }
        if (GUILayout.Button("Instantiate Selection ", GUILayout.Width(240)))
        {
            InsSelections();
        }
        if (GUILayout.Button("Update sprite name", GUILayout.Width(240)))
        {
            UpdateSpriteName();
        }
        if (GUILayout.Button("Apply Prefab ", GUILayout.Width(240)))
        {
            ApplyPrefab();
        }
        EditorGUILayout.EndHorizontal();


        GUILayout.EndArea();
        return new Rect(contentRect.xMin, contentRect.yMax, contentRect.width, rect.height - contentRect.height);
    }


    private void DrawSelect(Rect rect)
    {
        Rect contentRect = rect;
        GUILayout.BeginArea(contentRect);
        scroll = EditorGUILayout.BeginScrollView(scroll);
        {
            GUIStyle style = new GUIStyle();
            for (int i = 0; i < selections.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(selections[i].Name, GUILayout.Width(240));
                
                if (selections[i].InsStatus != ExchangeStatus.none)
                {
                    style.normal.textColor = selections[i].InsStatus == ExchangeStatus.failed ? redColor : greenColor;
                    GUILayout.Label(selections[i].InsStatus.ToString(), style,GUILayout.Width(240));
                }
                if (selections[i].UpdateStatus != ExchangeStatus.none)
                {
                    style.normal.textColor = selections[i].UpdateStatus == ExchangeStatus.failed ? redColor : greenColor;
                    GUILayout.Label(selections[i].UpdateStatus.ToString(),style, GUILayout.Width(240));
                }
                if (selections[i].ApplyStatus != ExchangeStatus.none)
                {
                    style.normal.textColor = selections[i].ApplyStatus == ExchangeStatus.failed ? redColor : greenColor;
                    
                    GUILayout.Label(selections[i].ApplyStatus.ToString(),style, GUILayout.Width(240));
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();
    }




    private void UpdateSelections()
    {
        selections = new List<ObjInfo>();
        Object[] objs = Selection.objects;
        for (int i = 0, count = objs.Length; i < count; i++)
        {
            selections.Add(new ObjInfo(objs[i]));
        }
    }


    private void InsSelections()
    {
        if(selections == null || selections.Count <= 0) return;
        for (int i = 0; i < selections.Count; i++)
        {
            if (selections[i].Obj is GameObject)
            {
                Vector2 size = Vector2.zero;
                if ((selections[i].Obj as GameObject).transform is RectTransform)
                {
                    size = ((selections[i].Obj as GameObject).transform as RectTransform).sizeDelta;
                }
                selections[i].Instance = PrefabUtility.InstantiatePrefab(selections[i].Obj) as GameObject;
                selections[i].Instance.transform.parent = root;
                selections[i].Instance.transform.localScale = Vector3.one;
                selections[i].Instance.transform.localPosition = Vector3.zero;
                if (selections[i].Instance.transform is RectTransform)
                {
                    (selections[i].Instance.transform as RectTransform).sizeDelta = size;
                }
                selections[i].Instance.SetActive(true);
                selections[i].InsStatus = ExchangeStatus.success;
            }
            else
            {
                selections[i].InsStatus = ExchangeStatus.failed;
            }
        }
    }

    private void UpdateSpriteName()
    {
        if (selections == null || selections.Count <= 0) return;
        for (int i = 0; i < selections.Count; i++)
        {
            if (selections[i].InsStatus != ExchangeStatus.success || !selections[i].Instance)
            {
                selections[i].UpdateStatus = ExchangeStatus.failed;
                continue;
            }

            List<UISprite> spriteList = selections[i].Instance.GetComponentsInChildren<UISprite>(true).ToList();
            for (int j = 0; j < spriteList.Count; j++)
            {
                if (spriteList[j].sprite )
                {
                    if (spriteList[j].sprite.name.StartsWith(oldPrefix))
                    {
                        string n = spriteList[j].sprite.name.Replace(oldPrefix, newPrefix);
                        spriteList[j].UF_SetValue(n);
                    }
                }
            }
            selections[i].UpdateStatus = ExchangeStatus.success;
        }
    }


    private void ApplyPrefab()
    {
        if (selections == null || selections.Count <= 0) return;
        for (int i = 0; i < selections.Count; i++)
        {
            if (selections[i].InsStatus != ExchangeStatus.success || !selections[i].Instance)
            {
                selections[i].ApplyStatus = ExchangeStatus.failed;
                continue;
            }
            if (PrefabUtility.GetPrefabType(selections[i].Instance) == PrefabType.PrefabInstance)
            {
                Object o = PrefabUtility.GetPrefabParent(selections[i].Instance);
                PrefabUtility.ReplacePrefab(selections[i].Instance, o, ReplacePrefabOptions.Default);
                selections[i].ApplyStatus = ExchangeStatus.success;
            }
            else
            {
                selections[i].ApplyStatus = ExchangeStatus.failed;
            }
            
        }
        AssetDatabase.Refresh();
    }

    public class ObjInfo
    {
        public Object Obj;
        public string Name
        {
            get { return Obj.name; }
        }
        public ExchangeStatus InsStatus;
        public ExchangeStatus UpdateStatus;
        public ExchangeStatus ApplyStatus;

        public GameObject Instance;

        public ObjInfo(Object _obj)
        {
            this.Obj = _obj;
            this.InsStatus = ExchangeStatus.none;
            this.UpdateStatus = ExchangeStatus.none;
            this.ApplyStatus = ExchangeStatus.none;
            Instance = null;
        }
    }

    public enum ExchangeStatus
    {
        none,
        failed,
        success,
    }
 
}


