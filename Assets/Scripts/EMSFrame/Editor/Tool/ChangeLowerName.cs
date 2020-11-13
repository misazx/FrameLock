using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ChangeLowerName : MonoBehaviour { 

    [MenuItem("GameTools/Tools/改为小写")]
    static void ChangeToLowerNameHandle()
    {
        Object[] _objs = Selection.objects;
        foreach (var item in _objs)
        {            
            string _old = AssetDatabase.GetAssetPath(item);
            var _temp = AssetDatabase.RenameAsset(_old, item.name.ToLower());
            print(_temp);
        }
    }
}
