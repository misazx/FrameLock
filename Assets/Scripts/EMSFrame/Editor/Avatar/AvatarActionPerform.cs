using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityFrame;


//表现部分相关
partial class AvatarActionPreview
{
    private PreviewEffect PlayEffect(string name,Vector3 pos,Vector3 euler) {
        if (string.IsNullOrEmpty(name)) return null;
        var eft = EffectUtil.FindEffect(name);
        if (eft != null) {
            eft.position = pos;
            eft.euler = euler;
            eft.Play();
            return eft;
        }
        //创建新的特效
        string efxPath = "Assets/AssetBases/PrefabAssets/efx/";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(efxPath + name + ".prefab");
        if (prefab != null)
        {
            GameObject efxgo = UnityEngine.Object.Instantiate(prefab);
            efxgo.name = name;
            this.AddGameObject(efxgo, false);
            eft = EffectUtil.Play(efxgo);
            eft.position = pos;
            eft.euler = euler;
        }
        return eft;
    }

    private PreviewEffect PlayEffect(string name) {
        return PlayEffect(name, Vector3.zero, Vector3.zero);
    }
     

    private void PlaySound(string name) {
        if (string.IsNullOrEmpty(name)) return;
        string wavPath = "Assets/AssetBases/PrefabAssets/wav/";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(wavPath + name + ".prefab");
        var audio = prefab.GetComponent<AudioSource>();
        if (audio != null && audio.clip != null)
        {
            AudioUtil.Play(audio.clip);
        }
    }


    

    private Vector3 GetParamPosition(AvatarController avatar, string param)
    {
        if (param == "1")
        {
            return avatar.position;
        }
        else if (param == "2")
        {
            return avatar.focusPosition;
        }
        else if (param == "3")
        {
            return avatar.topPosition;
        }
        else if (!string.IsNullOrEmpty(param))
        {
            if (avatar.markPoint.Exist(param))
            {
                return avatar.markPoint.UF_GetPosition(param);
            }
            else {
                Debug.LogError(string.Format("Can not find Markpoint[{0}] in Avatar", param));
            }
        }

        return avatar.position;
    }




    private void UpdatePreviewEffect() {
        EffectUtil.Update();
    }


    private void PerformActionClipEvent(AvatarController avatar,EditorClipEvent clipEvent) {
        string type = clipEvent.name;
        string[] args = GHelper.UF_SplitStringWithCount(clipEvent.GetParam("param"),EditorActionClipTool.GetParamCount(type),'\n');
        switch (type)
        {
            case "Hit": Action_Hit(avatar, args); break;
            case "Dip": Action_Dip(avatar, args); break;
            case "Link": Action_Link(avatar, args); break;
            case "Efx": Action_Efx(avatar, args); break;
            case "Sound": Action_Sound(avatar, args); break;
            case "Efx_Follow": Action_Efx_Follow(avatar, args); break;
            case "Efx_Target": Action_Efx_Target(avatar, args); break;
            case "Efx_Range": Action_Efx_Range(avatar, args); break;
        }

    }


    

    private void Action_Hit(AvatarController avatar, string[] args) {
        string efxName = args[0];
        Vector3 pos = GetParamPosition(avatar, args[1]);
        PlayEffect(efxName, pos, Vector3.zero);
    }

    private void Action_Dip(AvatarController avatar, string[] args)
    {
        //float distance = 4;
        //float duration = float.Parse(args[0]);
        //float rad = float.Parse(args[1]);
        //string efxBall = args[2];
        //string efxHit = args[3];
        //Vector3 tpos = GetParamPosition(avatar, args[4]);
        //var eft = PlayEffect(efxBall, avatar.focusPosition, Vector3.zero);
        //eft.Move(duration, avatar.focusPosition, tpos, rad, ()=> {
        //    PlayEffect(efxHit, tpos,Vector3.zero);
        //});


    }


    private void Action_Link(AvatarController avatar, string[] args)
    {

    }

    private void Action_Sound(AvatarController avatar, string[] args)
    {
        string wavName = args[0];
        PlaySound(wavName);
    }
    
    private void Action_Efx(AvatarController avatar, string[] args)
    {
        string efxName = args[0];
        Vector3 pos = GetParamPosition(avatar, args[1]);
        PlayEffect(efxName, pos, Vector3.zero);
    }

    private void Action_Efx_Follow(AvatarController avatar, string[] args) {
        string efxName = args[0];
        string mpName = args[1];
        var eft = PlayEffect(efxName);
        if (eft == null || eft.target == null)
        {
            return;
        }
        var markpoint = avatar.markPoint.UF_Find(mpName);
        if (markpoint != null)
        {
            eft.target.transform.parent = markpoint.transform;
            eft.target.transform.localPosition = Vector3.zero;
        }
        else {
            Debug.LogError("Can not find Markpoint: " + mpName);
        }
    }

    private void Action_Efx_Target(AvatarController avatar, string[] args)
    {

    }

    private void Action_Efx_Range(AvatarController avatar, string[] args)
    {

    }




}
