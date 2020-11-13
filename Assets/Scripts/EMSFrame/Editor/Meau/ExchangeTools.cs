using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityFrame;
 
public class ExchangeUpdateKey : Editor
{
    [MenuItem("GameTools/Tools/ExchangeTools/ExchangeUpdateKey")]
    public static void Exchange()
    {
        GameObject[] objs = Selection.gameObjects;
        for (int i = 0, count = objs.Length; i < count; i++)
        {
            
            UIUpdateGroup updategroup = objs[i].GetComponent<UIUpdateGroup>();
            if (updategroup)
            {
                updategroup.updateKey = objs[i].name;
                continue;
            }
            UISlider slider = objs[i].GetComponent<UISlider>();
            if (slider)
            {
                slider.updateKey = objs[i].name;
                continue;
            }
            UIView uiview = objs[i].GetComponent<UIView>();
            if (uiview)
            {
                uiview.updateKey = objs[i].name;
                continue;
            }
            UIToggle toggle = objs[i].GetComponent<UIToggle>();
            if (toggle)
            {
                toggle.updateKey = objs[i].name;
                continue;
            }
            UIGrid grid = objs[i].GetComponent<UIGrid>();
            if (grid)
            {
                grid.updateKey = objs[i].name;
                continue;
            }
            UITexture texture = objs[i].GetComponent<UITexture>();
            if (texture)
            {
                texture.updateKey = objs[i].name;
                continue;
            }
            UIButton button = objs[i].GetComponent<UIButton>();
            if (button)
            {
                button.updateKey = objs[i].name;
                continue;
            }
            UISprite sprite = objs[i].GetComponent<UISprite>();
            if (sprite)
            {
                sprite.updateKey = objs[i].name;
                continue;
            }
            UILabel label = objs[i].GetComponent<UILabel>();
            if (label)
            {
                label.updateKey = objs[i].name;
                continue;
            }
        }


    }


}

public class ExchangeFont : EditorWindow
{
    private List<ObjInfo> selections;
    private Font oldFont;
    private Font font;
    private Vector2 offset = new Vector2(3f, 6f);
    private Vector2 scroll;
    private string failedReasona = "target is not a Qualified GameObjectPrefab";
    private string failedReasonb = "target did not carry any UILabel";
    private Color redColor = Color.red;

    [MenuItem("GameTools/Tools/ExchangeTools/ExchangeUILabel")]
    public static void Exchange()
    {
        EditorWindow ew = GetWindow<ExchangeFont>(false, "ExchangeFont");
        ew.minSize = new Vector2(500, 300);
    }

    private void OnEnable()
    {
        selections = new List<ObjInfo>();
        font = null;
        oldFont = null;
    }


    protected void OnGUI()
    {
        Rect rect = new Rect(offset.x, offset.y, position.width - offset.x * 2, position.height - 2 * offset.y);
        DrawSelect(DrawTip(rect));
    }


    private Rect DrawTip(Rect rect)
    {
        Rect contentRect = rect;
        contentRect.height = EditorGUIUtility.singleLineHeight * 2;
        GUILayout.BeginArea(contentRect);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("OldFont");
        oldFont = EditorGUILayout.ObjectField(oldFont, typeof(Font)) as Font;
        GUILayout.Label("Font");
        font = EditorGUILayout.ObjectField(font, typeof(Font)) as Font;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("update selections", EditorStyles.miniButtonLeft))
        {
            UpdateSelections();
        }
        if (GUILayout.Button("update label font", EditorStyles.miniButtonMid))
        {
            if(font)
                UpdateLabel(true,false);
        }
        if (GUILayout.Button("update label raycast", EditorStyles.miniButtonLeft))
        {
            UpdateLabel(false, true);
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
            for (int i = 0; i < selections.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(selections[i].Name,GUILayout.Width(240));
                GUILayout.Label(selections[i].status.ToString(), GUILayout.Width(100));
                if (selections[i].status == ExchangeStatus.failed)
                {
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = redColor;
                    GUILayout.Label(selections[i].failedReason,style, GUILayout.Width(150));
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
            ObjInfo temp = new ObjInfo(objs[i]);
            selections.Add(temp);
        }
    }
    private void UpdateLabel(bool exFont , bool raycast)
    {
        for (int i = 0, count = selections.Count; i < count; i++)
        {
            if (PrefabUtility.GetPrefabType(selections[i].Obj) == PrefabType.None)
            {
                selections[i].status = ExchangeStatus.failed;
                selections[i].failedReason = failedReasona;
                continue;
            }
            GameObject obj = Instantiate(selections[i].Obj) as GameObject;
            if (!obj)
            {
                selections[i].status = ExchangeStatus.failed;
                selections[i].failedReason = failedReasona;
                continue;
            }
            bool isApply = false;
            UILabel[] lbs = obj.GetComponentsInChildren<UILabel>(true);
            if (lbs != null && lbs.Length > 0)
            {
                isApply = true;
                for (int j = 0, num = lbs.Length; j < num; j++)
                {
                    if (exFont)
                    {
                        if (lbs[j].font == null || lbs[j].font == oldFont)
                            lbs[j].font = font;
                    }
                    if (raycast)
                        lbs[j].raycastTarget = false;
                }
            }
            else
            {
                selections[i].status = ExchangeStatus.failed;
                selections[i].failedReason = failedReasonb;
            }
            if (isApply)
            {
                selections[i].Obj = PrefabUtility.ReplacePrefab(obj, selections[i].Obj);
                selections[i].status = ExchangeStatus.success;
            }
            DestroyImmediate(obj);



        }
    }





    public class ObjInfo
    {
        public Object Obj;
        public string Name
        {
            get { return Obj.name; }
        }

        public ExchangeStatus status;

        public string failedReason;

        public ObjInfo(Object _obj)
        {
            this.Obj = _obj;
            this.status = ExchangeStatus.none;
        }
    }

    public enum ExchangeStatus
    {
        none,
        failed,
        success,
    }


}
