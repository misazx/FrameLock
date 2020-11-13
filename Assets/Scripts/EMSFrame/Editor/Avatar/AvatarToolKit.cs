using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityFrame;
using UnityFrame.Assets;

public class AvatarToolKit
{

	public static string[] DEAFULT_ACTION_TRACK = {
		"idle",
        "idle2",
        "idle3",
        "run",
		"hurt",
		"die",
        "show",
        "jump",
        "attack",
        "skill1",
        "skill2",
	};

	static bool CheckKeyword(string str,string keywords){
		return str.IndexOf(keywords, System.StringComparison.Ordinal) > -1;
	}


    static void GenMarkPointRL(GameObject go) {
        foreach(var v in go.GetComponentsInChildren<Transform>()) {
            if (v.name == "GD_Left") {
                GameObject mp = new GameObject("MP_L");
                mp.transform.parent = v;
                mp.transform.localEulerAngles = Vector3.zero;
                mp.transform.localPosition = Vector3.zero;
            }
            else if (v.name == "GD_Right")
            {
                GameObject mp = new GameObject("MP_R");
                mp.transform.parent = v;
                mp.transform.localEulerAngles = Vector3.zero;
                mp.transform.localPosition = Vector3.zero;
            }
        }
    }

    static string GetAssertAbsPath(Object target) {
        string targetPath = AssetDatabase.GetAssetPath(target);
        return Application.dataPath.Replace("\\", "/");
    }

    static string GetAssertAbsFolder(Object target)
    {
        string targetPath = AssetDatabase.GetAssetPath(target);
        string dataPath = Application.dataPath.Replace("\\", "/");
        System.IO.FileInfo fi = new System.IO.FileInfo(dataPath + targetPath);
        string folder = fi.Directory.FullName.Replace("\\", "/").Replace(dataPath, "");
        return folder;
    }

    static AvatarController MakeAvatar(string name,GameObject target,Texture texture, AnimatorController animatorController)
    {
        GameObject controllerGO = new GameObject (name);
		AvatarController controller = controllerGO.AddComponent<AvatarController> ();

        GameObject model = Object.Instantiate<GameObject>(target);
        model.name = "model";
        model.transform.localPosition = Vector3.zero;
        model.transform.localEulerAngles = Vector3.zero;
        model.transform.localScale = Vector3.one;
        model.transform.SetParent(controller.transform);
        EditorAvatarController.AddToMarkPoint(controller);

        string targetPath = AssetDatabase.GetAssetPath(target);
        string dataPath = Application.dataPath.Replace("\\", "/");
        System.IO.FileInfo fi = new System.IO.FileInfo(dataPath + targetPath);
        string folder = fi.Directory.FullName.Replace("\\", "/").Replace(dataPath, "");

        string matPath = folder + "/Materials/" + target.name + ".mat";

        //render

        Material mt = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mt == null) {
            mt = new Material(Shader.Find("Game/Character/Diffuse"));
            mt.name = target.name;
            mt.mainTexture = texture;
            if (!AssetDatabase.IsValidFolder(folder + "/Materials"))
            {
                AssetDatabase.CreateFolder(folder, "Materials");
            }

            AssetDatabase.CreateAsset(mt, matPath);
        }


        SkinnedMeshRenderer[] renders = model.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (SkinnedMeshRenderer meshrender in renders)
        {
            meshrender.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        }

        //capsule
        controller.capsule.height = 2;
        controller.capsule.radius = 0.5f;
		controller.capsule.center = new Vector3 (0, 1, 0);

        //加入左右挂点
        GenMarkPointRL(model);


        //animation
        Animator animator = model.GetComponent<Animator>();
        if (animator == null) {
            animator = model.AddComponent<Animator>();
        }
        animator.runtimeAnimatorController = animatorController;
        var listClips = controller.animator.listClips;
        listClips.Clear();
        foreach (string value in DEAFULT_ACTION_TRACK)
        {
            var new_ActionClip = new AnimatorClip(value);
            new_ActionClip.length = 1;
            new_ActionClip.clipName = value;
            if (CheckKeyword(value, "idle"))
            {
                new_ActionClip.crossMode = AnimatorClip.CrossMode.CrossFabe;
            }
            if (CheckKeyword(value, "idle") || CheckKeyword(value, "run"))
            {
                new_ActionClip.wrapMode = WrapMode.Loop;
            }
            if (CheckKeyword(value, "die"))
            {
                new_ActionClip.wrapMode = WrapMode.ClampForever;
            }
            listClips.Add(new_ActionClip);
        }


        EditorTools.SetDirty(controller);
        AssetDatabase.SaveAssets();

        return controller;
        
	}



    static AnimatorController MakeAnimatorController(string name, string path) {

        string animPath = path + "/act_" + name.Replace("ava_","") + ".controller";

        var searchObjs = EditorTools.LoadAllAssetAtPath(path, "*.FBX|*.fbx|*.Fbx");

        List<Object> listAnimaObjs = new List<Object>();
        foreach (var v in searchObjs)
        {
            if (v != null && CheckKeyword(v.name, "@"))
            {
                listAnimaObjs.Add(v);
            }
        }

        if (listAnimaObjs.Count > 0)
        {
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(animPath);
            AnimatorControllerLayer layer = controller.layers[0];
            AnimatorStateMachine sm = layer.stateMachine;

            //把state添加在layer里面
            //		Anima trans = sm.AddAnyStateTransition(state);
            //		//把默认的时间条件删除
            //		trans.RemoveCondition(0);

            for (int k = 0; k < listAnimaObjs.Count; k++)
            {
                AnimationClip newClip = AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(listAnimaObjs[k]), typeof(AnimationClip)) as AnimationClip;
                if (newClip != null)
                {
                    AnimatorState astate = sm.AddState(newClip.name);
                    astate.motion = newClip;
                    //设置待机为默认状态
                    if (newClip.name == "idle")
                    {
                        sm.defaultState = astate;
                    }
                }
            }

            AssetDatabase.SaveAssets();

            return controller;
        }

        return null;
    }




    [MenuItem("GameTools/Avatar/Gen Animation")]
    [MenuItem("Assets/GameTools/Avatar/Gen Animation")]
    static void Gen_Animation()
    {
        var selObj = Selection.activeObject;
        if (selObj == null) return;
        string name = selObj.name;
        string path = AssetDatabase.GetAssetPath(selObj);

        MakeAnimatorController(name, path);
    }


    [MenuItem("GameTools/Avatar/Gen Avatar")]
    [MenuItem("Assets/GameTools/Avatar/Gen Avatar")]
    static void Gen_Avatar()
    {
        var selObj = Selection.activeObject;
        if (selObj == null) return;
        string name = selObj.name;
        string path = AssetDatabase.GetAssetPath(selObj);

        GameObject target = EditorTools.FindAssetAtPath<GameObject>(path, "*.FBX|*.fbx");

        if (target == null) { return; }

        Texture2D texture = EditorTools.FindAssetAtPath<Texture2D>(path, "*.TGA|*.tga");
        AnimatorController animatorController = EditorTools.FindAssetAtPath<AnimatorController>(path, "act*|*.controller");

        MakeAvatar(name, target, texture, animatorController);

    }

    [MenuItem("GameTools/Avatar/Gen Animation+Avatar")]
    [MenuItem("Assets/GameTools/Avatar/Gen Animation+Avatar")]
    static void Gen_All()
    {
        var selObj = Selection.activeObject;
        if (selObj == null) return;
        string name = selObj.name;
        string path = AssetDatabase.GetAssetPath(selObj);

        GameObject target = EditorTools.FindAssetAtPath<GameObject>(path, "*.FBX|*.fbx");
        if (target == null) { return; }
        Texture2D texture = EditorTools.FindAssetAtPath<Texture2D>(path, "*.TGA|*.tga");

        AnimatorController animatorController = MakeAnimatorController(name, path);

        MakeAvatar(name, target, texture, animatorController);
    }


    [MenuItem("GameTools/Avatar/Gen Weapon")]
    [MenuItem("Assets/GameTools/Avatar/Gen Weapon")]
    static void Gen_Weapon() {
        var objs = Selection.objects;
        Texture2D texture = null;
        GameObject go = null;
        foreach (var v in objs) {
            if (v is Texture2D)
                texture = v as Texture2D;
            else if (v is GameObject)
                go = v as GameObject;
        }
        if (go == null)
            return;

        GameObject weapon = new GameObject(go.name);
        GameObject model = Object.Instantiate<GameObject>(go);
        
        model.transform.parent = weapon.transform;
        model.transform.localEulerAngles = Vector3.zero;
        model.transform.localPosition = Vector3.zero;

        model.name = "model";

        weapon.AddComponent<EntityObject>();

        string folder = GetAssertAbsFolder(go);
        string matPath = folder + "/Materials/" + go.name + ".mat";
        Material mt = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mt == null) {
            mt = new Material(Shader.Find("Game/Character/Diffuse"));
            mt.name = go.name;
            mt.mainTexture = texture;
            if (!AssetDatabase.IsValidFolder(folder + "/Materials"))
            {
                AssetDatabase.CreateFolder(folder, "Materials");
            }
            AssetDatabase.CreateAsset(mt, matPath);
            AssetDatabase.SaveAssets();
        }

        MeshRenderer[] renders = weapon.GetComponentsInChildren<MeshRenderer>(true);
        foreach (MeshRenderer meshrender in renders)
        {
            meshrender.sharedMaterial = mt;
        }
        
    }


}


