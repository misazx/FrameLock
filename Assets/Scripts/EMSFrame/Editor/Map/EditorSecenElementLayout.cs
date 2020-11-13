using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityFrame;
using UnityFrame.Assets;
using System;


[CustomEditor(typeof(AssetSceneLayout), true)]
public class EditorSecenElementLayout : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);

        if (GUILayout.Button("编辑障碍物"))
        {
            OpenSceneLayoutWindow();
        }
    }

    private void OpenSceneLayoutWindow()
    {
        SceneLayoutWindow window = EditorWindow.GetWindow<SceneLayoutWindow>(true, "障碍物编辑器");
        AssetSceneLayout pkg = target as AssetSceneLayout;
        window.Show(pkg);
    }
}



public class SceneLayoutWindow : EditorWindow
{
    struct MapCellSTWithTexture
    {
        public int index;
        public int x, y;
        public float rotation;
        public string block;
        public string param;
        public Sprite sprite;
        public int occupyWidth;
        public int occupyHeight;
        public Vector3 scale;
        public Vector3 pos;
    }
    struct MapCellSourceLink
    {
        public string name;
        public Sprite sprite;
    }

    static private int s_cellWPixel = 100;
    static private int s_cellHPixel = 100;

    private Vector2 scrollPos = Vector2.zero;
    private AssetSceneLayout m_pkg;
    private List<MapCellSTWithTexture> m_cellDataList;
    private string m_cellSourceNameFilter;//筛选预设名称前序
    private float m_editorCellXScale = 1f;
    private float m_editorPixelScale = 0.5f;
    private string m_cellRoot = "Assets/AssetBases/PrefabAssets/sce";
    private string map_editor_cell_x_scale_key = "MAP_CELL_XSCALE";
    private string map_editor_cell_filter_key = "SCE_OBJ_FILTER";

    private int m_row, m_col;
    private Color m_lineColor = new Color(7.0f / 255, 101.0f / 255, 0, 1);
    private Color m_cellColor = new Color(99f / 255, 106f / 255, 99f / 255, 1);
    private Color m_missingTexCellColor = Color.white;

    //资源库
    private List<MapCellSourceLink> m_cellSpriteReSources = new List<MapCellSourceLink>();
    //当前选中的格子数据
    private Dictionary<int, MapCellSTWithTexture> m_curSelectNodes = new Dictionary<int, MapCellSTWithTexture>();
    //剪切存储的格子数据,用于粘贴快捷键使用时赋值
    private List<MapCellSTWithTexture> m_copyNodesCache = new List<MapCellSTWithTexture>();

    #region 属性
    private float windowWidth { get { return base.position.size.x; } }
    private float windowHeight { get { return base.position.size.y; } }
    private Rect mapCellArea { get { return new Rect(0, 0, windowWidth - 205, windowHeight); } }
    private Rect detailContentArea { get { return new Rect(windowWidth - 203, 20, 200, windowHeight); } }
    public float unitW { get; set; }
    public float unitH { get; set; }
    public float mapW { get { return (int)(unitW * m_col); } }
    public float mapH { get { return (int)(unitH * m_row); } }
    #endregion

    private void OnDestroy()
    {
        this.m_cellDataList.Clear();
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }
    public void Show(AssetSceneLayout pkg)
    {
        ColorUtility.TryParseHtmlString("#FF9999", out m_missingTexCellColor);
        this.m_pkg = pkg;
        m_row = this.m_pkg.mapRow;
        m_col = this.m_pkg.mapCol;
        this.m_cellSourceNameFilter = EditorPrefs.GetString(map_editor_cell_filter_key, "sce_obj_");

        unitW = s_cellWPixel;
        unitH = s_cellHPixel;
        InitMapCellData(pkg.Data);
        this.m_editorCellXScale = EditorPrefs.GetFloat(map_editor_cell_x_scale_key, 0.5f);
        this.GetCellLinkSource();
    }
    private void OnGUI()
    {
        if (this.m_cellDataList.Count > 0)
        {
            DrawMapCell();
            DrawCellDetail();
            DrawCellSpriteResources();
            CheckDeleteSelectNodesEvent();
            ArrowMoveSelect();
            CheckCutNodesEvent();
            CheckPasteNodesEvent();
        }
    }

    #region draw map cell
    private void DrawMapCell()
    {
        GUILayout.BeginArea(mapCellArea);
        float rawMapW = unitW * m_row + 5;
        float rawMapH = unitH * m_col + 5;
        scrollPos = GUI.BeginScrollView(mapCellArea, scrollPos, new Rect(0, 0, rawMapW + 200, rawMapH + 200), true, true);
        GUILayout.BeginArea(new Rect(60, 60, rawMapW + 60, rawMapH + 60));
        DrawCells();
        DrawGrid();
        DrawCellTexture();

        foreach (var item in this.m_curSelectNodes)
        {
            var data = item.Value;
            Vector2 startPos = new Vector2(data.x * unitW, (m_col - 1 - data.y) * unitH);
            DrawSelectRect(startPos.x, startPos.y, unitW, unitH, Color.yellow);
        }
        GUILayout.EndArea();
        GUI.EndScrollView();
        GUILayout.EndArea();
    }

    #endregion

    #region draw cell data
    private bool m_isLeftShiftDown = false;
    private void DrawCellTexture()
    {
        this.m_isLeftShiftDown = Event.current.shift;
        //画贴图
        for (int i = this.m_cellDataList.Count - 1; i >= 0; i--)
        {
            var data = this.m_cellDataList[i];
            float posX = data.x * unitW;
            float posY = (m_col - 1 - data.y) * unitH;

            if (data.sprite != null)
            {
                Vector4 outerUV = UnityEngine.Sprites.DataUtility.GetOuterUV(data.sprite);
                //转化一层，outerUV为（左下角点 + 右上角点） rectUV 为 （位置起始点 + 尺寸大小）
                float spriteRawW = data.sprite.rect.width * data.scale.x;
                float spriteRawH = data.sprite.rect.height * data.scale.y;
                float perW = unitW / spriteRawW;
                float perH = unitH / spriteRawH;
                float spriteW = spriteRawW * perW * data.occupyWidth * this.m_editorCellXScale;
                float spriteH = spriteRawH * perH * data.occupyHeight * this.m_editorCellXScale;
                Rect rectImage = new Rect(
                    posX,// + data.pos.x * perW * unitW,
                    posY - spriteH + unitH,// + data.pos.z * perH * unitH,
                    spriteW,
                    spriteH);

                Rect rectUV = new Rect(outerUV.x, outerUV.y, outerUV.z - outerUV.x, outerUV.w - outerUV.y);
                GUI.DrawTextureWithTexCoords(rectImage, data.sprite.texture, rectUV);
                DrawSelectRect(rectImage.x, rectImage.y, rectImage.width, rectImage.height, Color.clear);
            }

            Rect rawRect = new Rect(posX, posY, unitW, unitH);
            if (Event.current.type == EventType.MouseDown && rawRect.Contains(Event.current.mousePosition))
            {
                if (!m_isLeftShiftDown)
                {
                    this.m_curSelectNodes.Clear();
                }
                if (this.m_curSelectNodes.ContainsKey(data.index))
                    this.m_curSelectNodes.Remove(data.index);
                else
                    this.m_curSelectNodes.Add(data.index, data);
                Event.current.Use();
            }
        }
    }
    private void DrawGrid()
    {
        //画格子网格
        for (int i = 0; i <= m_col; i++)
        {
            EditorGUI.DrawRect(new Rect(0, i * unitH, m_row * unitW, 1), m_lineColor);
        }
        for (int i = 0; i <= m_row; i++)
        {
            EditorGUI.DrawRect(new Rect(i * unitW, 0, 1, m_col * unitH), m_lineColor);
        }
    }
    private void DrawCells()
    {
        //画格子
        for (int idx = 0; idx < this.m_cellDataList.Count; idx++)
        {
            var data = this.m_cellDataList[idx];
            if (data.sprite == null)
            {
                float texW = unitW;
                float texH = unitH;
                float posX = data.x * unitW;
                float posY = (m_col - 1 - data.y) * unitH;
                if (!string.IsNullOrEmpty(data.block))
                {
                    //没找到贴图
                    EditorGUI.DrawRect(new Rect(posX, posY, texW, texH), m_missingTexCellColor);
                }
                else
                {
                    EditorGUI.DrawRect(new Rect(posX, posY, texW, texH), m_cellColor);
                }
            }
        }
    }
    #endregion

    #region detail rect
    private void DrawCellDetail()
    {
        GUILayout.BeginArea(detailContentArea);
        this.m_editorPixelScale = EditorGUILayout.FloatField("格子像素缩放值", this.m_editorPixelScale);
        this.m_editorCellXScale = EditorGUILayout.FloatField("格子宽度缩放值", this.m_editorCellXScale);
        this.unitW = s_cellWPixel * this.m_editorPixelScale;
        this.unitH = s_cellHPixel * this.m_editorPixelScale;
        var firstSelect = this.GetFirstSelectCell();
        if (firstSelect != null)
        {
            MapCellSTWithTexture data = (MapCellSTWithTexture)firstSelect;
            EditorGUILayout.LabelField($"格子行列:({data.x},{data.y})", GUILayout.Width(180), GUILayout.Height(20));
            EditorGUILayout.LabelField($"格子坐标:({data.x * unitW},{data.y * unitH})", GUILayout.Width(180), GUILayout.Height(20));
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("障碍物：");
            data.block = GUILayout.TextField(data.block, GUILayout.Width(200));
            EditorGUILayout.LabelField("旋转：");
            data.rotation = EditorGUILayout.FloatField(data.rotation, GUILayout.Width(200), GUILayout.Height(20));
            EditorGUILayout.LabelField("参数：");
            data.param = GUILayout.TextArea(data.param, GUILayout.Width(200), GUILayout.Height(100));
            if (EditorGUI.EndChangeCheck())
            {
                this.m_curSelectNodes[data.index] = data;
                this.SaveNode(data);
                RefreshSingleTexture(data);
            }
        }

        GUILayout.Space(20);
        if (GUILayout.Button("Save"))
        {
            if (EditorUtility.DisplayDialog("提示", "是否保存数据??", "确定", "取消"))
            {
                this.m_pkg.Data = this.ParseMapCellST();
                EditorUtility.SetDirty(this.m_pkg);
                EditorPrefs.SetFloat(map_editor_cell_x_scale_key, this.m_editorCellXScale);
                EditorPrefs.SetString(map_editor_cell_filter_key, this.m_cellSourceNameFilter);
                Debug.Log("保存MapCell数据成功");
            }
        }
        if (GUILayout.Button("Show All Texture"))
        {
            RefreshTexture();
        }
        GUILayout.Space(10);
        if (GUILayout.Button("Clear All Texture"))
        {
            if (EditorUtility.DisplayDialog("警 告", "是否拭除所有图片??", "确定", "取消"))
            {
                for (int i = 0; i < this.m_cellDataList.Count; i++)
                {
                    var data = this.m_cellDataList[i];
                    data.block = null;
                    this.m_cellDataList[i] = data;
                }
                RefreshTexture();
            }
        }
        GUILayout.EndArea();
    }
    //显示格子快捷方式
    private Vector2 linkScrollPos;
    private void DrawCellSpriteResources()
    {
        GUILayout.BeginArea(new Rect(detailContentArea.x, 400, detailContentArea.width, detailContentArea.height - 400));
        EditorGUI.BeginChangeCheck();
        this.m_cellSourceNameFilter = EditorGUILayout.TextField(this.m_cellSourceNameFilter);
        if (EditorGUI.EndChangeCheck())
        {
            this.GetCellLinkSource();
        }
        int col = 3;
        int row = Mathf.CeilToInt(this.m_cellDataList.Count / col * 1.0f);
        linkScrollPos = GUI.BeginScrollView(new Rect(0, 30, detailContentArea.width, windowHeight - 450), linkScrollPos,
            new Rect(0, 0, 250, (row * 55) + 200));

        int index = -1;
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                index++;
                if (index < this.m_cellSpriteReSources.Count)
                {
                    var data = this.m_cellSpriteReSources[index];
                    if (data.sprite == null)
                        continue;

                    Vector4 outerUV = UnityEngine.Sprites.DataUtility.GetOuterUV(data.sprite);
                    Rect rectUV = new Rect(outerUV.x, outerUV.y, outerUV.z - outerUV.x, outerUV.w - outerUV.y);
                    Rect rectImage = new Rect(j * 55, i * 55, 50, 50);
                    GUI.DrawTextureWithTexCoords(rectImage, data.sprite.texture, rectUV);
                    if (Event.current.type == EventType.MouseDown && rectImage.Contains(Event.current.mousePosition))
                    {
                        this.UpdateNodeBlock(data.name, data.sprite);
                        Event.current.Use();
                    }
                }
            }
        }
        GUI.EndScrollView();
        GUILayout.EndArea();
    }
    private void GetCellLinkSource()
    {
        this.m_cellSpriteReSources.Clear();
        if (string.IsNullOrEmpty(this.m_cellSourceNameFilter))
        {
            return;
        }

        string[] files = System.IO.Directory.GetFiles(this.m_cellRoot, "*.prefab");
        foreach (var item in files)
        {
            System.IO.FileInfo fi = new System.IO.FileInfo(item);
            if (fi.Name.StartsWith(this.m_cellSourceNameFilter))
            {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(item);
                var spr = go?.GetComponentInChildren<SpriteRenderer>();
                if (spr != null)
                {
                    MapCellSourceLink link = new MapCellSourceLink();
                    link.name = fi.Name.Replace(fi.Extension, "");
                    link.sprite = spr.sprite;
                    this.m_cellSpriteReSources.Add(link);
                }
            }
        }
    }
    #endregion

    #region 快捷键操作   
    //粘贴格子
    private void CheckPasteNodesEvent()
    {
        if (Event.current.modifiers == EventModifiers.Control && Event.current.keyCode == KeyCode.V)
        {
            PasteNodes();
            Event.current.Use();
        }
    }
    private void PasteNodes()
    {
        if (this.m_copyNodesCache.Count == 0 || this.m_curSelectNodes.Count == 0)
            return;
        MapCellSTWithTexture firstSelectNode = default(MapCellSTWithTexture);
        foreach (var item in this.m_curSelectNodes)
        {
            firstSelectNode = item.Value;
            break;
        }

        int offsetX = firstSelectNode.x - this.m_copyNodesCache[0].x;
        int offsetY = firstSelectNode.y - this.m_copyNodesCache[0].y;
        for (int j = 0; j < this.m_copyNodesCache.Count; j++)
        {
            var item = this.m_copyNodesCache[j];
        }
        foreach (var item in this.m_copyNodesCache)
        {
            for (int i = 0; i < this.m_cellDataList.Count; i++)
            {
                var data = this.m_cellDataList[i];
                if (data.x == item.x + offsetX && data.y == item.y + offsetY)
                {
                    var copy = item;
                    copy.x = data.x;
                    copy.y = data.y;
                    copy.index = data.index;
                    this.m_cellDataList[i] = copy;
                    this.DeleteNode(item);
                    break;
                }
            }
        }
        this.m_copyNodesCache.Clear();
    }
    //剪切选中格子
    private void CheckCutNodesEvent()
    {
        if (Event.current.modifiers == EventModifiers.Control && Event.current.keyCode == KeyCode.X)
        {
            CutSelectNodes();
            Event.current.Use();
        }
    }
    private void CutSelectNodes()
    {
        if (this.m_curSelectNodes.Count > 0)
        {
            this.m_copyNodesCache.Clear();
            this.m_copyNodesCache.AddRange(this.m_curSelectNodes.Values);
            this.DeleteSelectNodes();
        }
    }
    //删除选中
    private void CheckDeleteSelectNodesEvent()
    {
        if (Event.current.keyCode == KeyCode.Delete)
        {
            DeleteSelectNodes();
            Event.current.Use();
        }
    }

    private void DeleteSelectNodes()
    {
        foreach (var item in this.m_curSelectNodes)
        {
            this.DeleteNode(item.Value);
        }
        this.m_curSelectNodes.Clear();
    }

    //方向键移动选中格子    
    private long m_keyMoveTime = 0;
    private void ArrowMoveSelect()
    {
        var kc = Event.current.keyCode;
        if (kc == KeyCode.LeftArrow || kc == KeyCode.DownArrow || kc == KeyCode.RightArrow || kc == KeyCode.UpArrow)
        {
            if (this.m_curSelectNodes.Count == 1)
            {
                if (System.DateTime.Now.Ticks - this.m_keyMoveTime < 1800000)
                    return;
                this.m_keyMoveTime = System.DateTime.Now.Ticks;
                MapCellSTWithTexture? cell = null;
                var data = (MapCellSTWithTexture)this.GetFirstSelectCell();
                if (kc == KeyCode.LeftArrow) cell = this.GetNode(data.x - 1, data.y);
                else if (kc == KeyCode.RightArrow) cell = this.GetNode(data.x + 1, data.y);
                else if (kc == KeyCode.DownArrow) cell = this.GetNode(data.x, data.y - 1);
                else if (kc == KeyCode.UpArrow) cell = this.GetNode(data.x, data.y + 1);

                if (cell != null)
                {
                    this.m_curSelectNodes.Remove(data.index);
                    this.m_curSelectNodes.Add(((MapCellSTWithTexture)cell).index, (MapCellSTWithTexture)cell);
                }
                Event.current.Use();
            }
        }
    }
    #endregion

    #region common
    private MapCellSTWithTexture? GetNode(int x, int y)
    {
        foreach (var item in this.m_cellDataList)
        {
            if (item.x == x && item.y == y)
                return item;
        }
        return null;
    }
    private MapCellSTWithTexture? GetFirstSelectCell()
    {
        foreach (var item in this.m_curSelectNodes)
        {
            return item.Value;
        }
        return null;
    }
    private List<SecenLayoutData> ParseMapCellST()
    {
        List<SecenLayoutData> list = new List<SecenLayoutData>();
        foreach (var item in this.m_cellDataList)
        {
            list.Add(new SecenLayoutData()
            {
                block = item.block,
                x = item.x,
                y = item.y,
                param = item.param
            });
        }
        return list;
    }
    private void RefreshTexture()
    {
        for (int i = 0; i < this.m_cellDataList.Count; i++)
        {
            var item = this.m_cellDataList[i];
            RefreshSingleTexture(item);
        }
    }
    private MapCellSTWithTexture? RefreshSingleTexture(MapCellSTWithTexture item)
    {
        if (item.sprite != null)
        {
            Resources.UnloadAsset(item.sprite);
            item.sprite = null;
            this.m_cellDataList[item.index] = item;
        }
        if (!string.IsNullOrEmpty(item.block))
        {
            var tex = this.LoadSprite(item.block);
            if (null != tex.Item1)
            {
                item.sprite = tex.Item1;
                item.occupyWidth = tex.Item2;
                item.occupyHeight = tex.Item3;
                item.scale = tex.Item4 * this.m_editorCellXScale;
                item.pos = tex.Item5;
                this.m_cellDataList[item.index] = item;
                return item;
            }
        }
        return item;
    }
    private void DeleteNode(MapCellSTWithTexture node)
    {
        node.block = "";
        node.param = "";
        node.sprite = null;
        this.SaveNode(node);
    }
    private void SaveNode(MapCellSTWithTexture node)
    {
        this.m_cellDataList[node.index] = node;
    }
    private void UpdateNodeBlock(string str, Sprite sp)
    {
        var list = new List<MapCellSTWithTexture>();
        foreach (var item in this.m_curSelectNodes)
        {
            list.Add(item.Value);
        }
        foreach (var item in list)
        {
            var data = item;
            data.block = str;
            data.sprite = null;
            this.RefreshSingleTexture(data);
        }
    }
    #endregion

    #region init
    private void InitMapCellData(List<SecenLayoutData> list)
    {
        int count = m_row * m_col;
        this.m_cellDataList = new List<MapCellSTWithTexture>(count);
        var wrapMap = this.WrapMST(list);
        for (int i = 0, index = -1; i < m_col; i++)
        {
            for (int j = 0; j < m_row; j++)
            {
                index++;
                //兼容旧数据
                string key = j + "_" + i;
                if (wrapMap.ContainsKey(key))
                {
                    var cell = wrapMap[key];
                    MapCellSTWithTexture temp = new MapCellSTWithTexture();
                    temp.index = index;
                    temp.x = cell.x;
                    temp.y = cell.y;
                    temp.rotation = cell.rotation;
                    temp.block = cell.block;
                    temp.param = cell.param;

                    var tex = LoadSprite(temp.block);
                    temp.sprite = tex.Item1;
                    temp.occupyWidth = tex.Item2;
                    temp.occupyHeight = tex.Item3;
                    temp.scale = tex.Item4 * this.m_editorCellXScale;
                    temp.pos = tex.Item5;
                    this.m_cellDataList.Add(temp);
                }
                else
                {
                    MapCellSTWithTexture temp = new MapCellSTWithTexture();
                    temp.index = index;
                    temp.x = j;
                    temp.y = i;
                    temp.occupyWidth = 1;
                    temp.occupyHeight = 1;
                    this.m_cellDataList.Add(temp);
                }
            }
        }
    }
    private Dictionary<string, SecenLayoutData> WrapMST(List<SecenLayoutData> list)
    {
        Dictionary<string, SecenLayoutData> map = new Dictionary<string, SecenLayoutData>();
        foreach (var item in list)
        {
            map.Add(item.x + "_" + item.y, item);
        }
        return map;
    }
    private Tuple<Sprite, int, int, Vector3, Vector3> LoadSprite(string name)
    {
        if (string.IsNullOrEmpty(name))
            return new Tuple<Sprite, int, int, Vector3, Vector3>(null, 1, 1, Vector3.one, Vector3.zero);

        int ow = 1;
        int oh = 1;
        Vector3 scale = Vector3.one;
        Vector3 pos = Vector3.zero;
        string[] splitNames = name.Split('_');
        string path = "Assets/AssetBases/PrefabAssets/" + splitNames[0] + "/" + name + ".prefab";
        GameObject texObj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (texObj != null)
        {
            var sobj = texObj.GetComponent<UnityFrame.SceneElement>();
            if (sobj != null)
            {
                ow = sobj.occupyWidth;
                oh = sobj.occupyHeight;
            }
        }
        var spr = texObj?.GetComponentInChildren<SpriteRenderer>();
        if (spr != null)
        {
            scale = Vector3.one;// spr.transform.localScale;
            pos = spr.transform.localPosition;
        }
        Sprite sprite = spr?.sprite;
        return new Tuple<Sprite, int, int, Vector3, Vector3>(sprite, ow, oh, scale, pos);
    }
    private void DrawSelectRect(float x, float y, float w, float h, Color color)
    {
        //显示格子选中框
        EditorGUI.DrawRect(new Rect(x, y, w, 1), color);
        EditorGUI.DrawRect(new Rect(x + w, y, 1, h), color);
        EditorGUI.DrawRect(new Rect(x, y, 1, h), color);
        EditorGUI.DrawRect(new Rect(x, y + h, w, 1), color);
    }
    #endregion

}
