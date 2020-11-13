using UnityEngine;
using System.Collections;

public static class EditorActionClipTool
{

    public static ActionClipDefineStruct ActionExtraParam = ActionClipDefineStruct.Create("Null", "Action 附带参数", 1, "", "行动方式", Color.white);

    private static ActionClipDefineStruct[] m_ListActionClipDefine = {
        ActionClipDefineStruct.Create("Null","空",1,"","",Color.white),

        //Avatar
        ActionClipDefineStruct.Create("Dip","弹道|发射弹道",5,"dip_null;0;0;0;0","弹道资源;位置参数;绑定弹道;锁定方向;随机偏移",new Color(1f, 0.2f, 0.2f, 1f)),
        ActionClipDefineStruct.Create("PDip","弹道|发射弹道(角色)",2,"dip_null;","弹道资源;位置参数",new Color(1f, 0.4f, 0.4f, 1f)),
        ActionClipDefineStruct.Create("PDipSword","弹道|发射剑气(角色)",3,"dip_null;dip_null;1","剑气A;剑气B;位置参数",new Color(1f, 0.4f, 0.4f, 1f)),
        
        ActionClipDefineStruct.Create("FDip","弹道|向前弹道",3,"dip_null;0;1","弹道资源;位置参数;向前距离",new Color(1f, 0.5f, 0.5f, 1f)),
        ActionClipDefineStruct.Create("SDip","弹道|场景弹道",5,"dip_null;0;0;0;0","弹道资源;源x;源y;目标x;目标y",new Color(1f, 0.5f, 0.5f, 1f)),
        ActionClipDefineStruct.Create("BDip","弹道|绑定弹道",2,"dip_null;null","弹道资源;绑定挂点",new Color(1f, 0.5f, 0.5f, 1f)),
        ActionClipDefineStruct.Create("FreeBDip","解除绑定弹道",0,"","",new Color(1f, 0.5f, 0.5f, 1f)),
        ActionClipDefineStruct.Create("IDip","弹道|持续发射弹道",5,"dip_null;0;0;0;0;0.2;5","弹道资源;位置参数;绑定弹道;锁定方向;随机偏移;间隔;数量",new Color(1f, 0.2f, 0.2f, 1f)),
        ActionClipDefineStruct.Create("SPDip","弹道|分裂弹道",4,"dip_null;0;dip_null;0","弹道资源;位置参数;分裂弹道;分裂数量",new Color(1f, 0.5f, 0.5f, 1f)),
        ActionClipDefineStruct.Create("AEDip","弹道|拖尾弹道",3,"dip_null;0;0","弹道资源;间隔;数量",new Color(1f, 0.5f, 0.5f, 1f)),
        
        ActionClipDefineStruct.Create("MDip","弹道|发射弹道(怪物)",2,"dip_null;","弹道资源;位置参数",new Color(1f, 0.6f, 0.6f, 1f)),
        ActionClipDefineStruct.Create("MBar","弹幕|弧度弹幕(怪物)",6,"dip_null;6;60;0;0;","弹道资源;数量;弧度;间隔;随机数;位置参数",new Color(1f, 0.8f, 0.8f, 1f)),

        ActionClipDefineStruct.Create("MRBar","弹幕|目标范围弹幕(怪物)",6,"dip_null;6;60;0;0;","弹道资源;数量;范围;间隔;随机数;位置参数",new Color(1f, 0.8f, 0.8f, 1f)),

        ActionClipDefineStruct.Create("MRange","范围|全场范围(怪物)",6,"dip_null;6;0;0;0","弹道资源;数量;间隔;随机数;位置参数;目标起始",new Color(1f, 0.8f, 0.8f, 1f)),

        ActionClipDefineStruct.Create("Perform","表现效果",1,"","参数",new Color(0, 1f, 1f, 1f)),
        ActionClipDefineStruct.Create("ActSkill","执行技能",1,"","技能id",new Color(0, 1f, 1f, 1f)),
        
        ActionClipDefineStruct.Create("Hit","击中|直接触发表现",2,"efx_null;1;1","击中特效;特效位置",new Color(1f, 0f, 0f, 1f)),
        ActionClipDefineStruct.Create("Rush","冲刺|向前方冲刺",2,"4;0.25","距离;时间",new Color(0.8f, 0.0f, 0.8f,1f)),
        ActionClipDefineStruct.Create("RushTh","冲刺穿透|向前方冲刺",2,"4;0.25","距离;时间",new Color(0.8f, 0.0f, 0.8f,1f)),
        ActionClipDefineStruct.Create("JumpTar","跳跃到目标",2,"8;1","跳跃高度;持续时间",new Color(0.6f, 0, 0.6f, 1f)),

        ActionClipDefineStruct.Create("Efx","特效|[1下,2中,3上]|播放于动作播放者自己",2,"efx_null;1","特效;位置",new Color(0, 1f, 0f, 1f)),
        ActionClipDefineStruct.Create("EfxFol","特效|跟随",2,"efx_null;1","特效;位置名",new Color(0.2f, 1f, 0.2f, 1f)),
        ActionClipDefineStruct.Create("EfxTar","特效|目标",1,"efx_null","特效",new Color(0.3f, 1f, 0.3f, 1f)),
        ActionClipDefineStruct.Create("EfxSce","特效|场景",1,"efx_null;0;0;0","特效;位置X;位置Y;角度",new Color(0.3f, 1f, 0.3f, 1f)),
        

        ActionClipDefineStruct.Create("TarLock","目标锁定|[0可锁定|1不可锁定]",1,"0","参数",new Color(0.3f, 1f, 0.3f, 1f)),

        ActionClipDefineStruct.Create("Shake","震屏",3,"0.2;0.02;0.02","震动幅度;震动频率;衰减时间",new Color(0.5f, 0.0f, 0.5f,1f)),
        ActionClipDefineStruct.Create("Sound","播放音效",1,"wav_sound","音效",new Color(1f, 1f, 0f, 1f)),

        
        ActionClipDefineStruct.Create("PreEfx","预警特效",1,"efx_null","特效",new Color(0.3f, 1f, 0.3f, 1f)),
        ActionClipDefineStruct.Create("PreLine","预警射线",4,"efx_1@preline;0;0;0;0","射线特效;折射次数;偏移角度;偏移高度;固定长度",new Color(0.8f, 0.8f, 0f, 1f)),
        ActionClipDefineStruct.Create("PreBarLine","预警弹幕射线",5,"efx_1@preline;3;60;0;0;0","射线特效;数量;弧度;折射次数;偏移高度;固定长度",new Color(0.8f, 0.8f, 0f, 1f)),

        ActionClipDefineStruct.Create("Flash","闪现",3,"0;1;efx_null","随机数;消失时间;闪现特效",new Color(0.8f, 0.8f, 0f, 1f)),

        ActionClipDefineStruct.Create("Ghost","残影",3,"0.3;1;ffffff","持续时间;距离间隔;残影颜色",new Color(0.8f, 0.8f, 0f, 1f)),
        
        //periform action
        ActionClipDefineStruct.Create("Idle","待机",1,"","指定动作",new Color( 0.8f, 0f,0.8f, 1f)),
        ActionClipDefineStruct.Create("Anima","播放动画",1,"","动画名称",new Color(0.6f,  0f,0.6f, 1f)),
        ActionClipDefineStruct.Create("Attack","普通攻击",1,"","指定普攻",new Color(0.4f,  0f,0.4f, 1f)),
        ActionClipDefineStruct.Create("Skill","释放技能",1,"","指定技能",new Color(1f,  0f,1f, 1f)),
        ActionClipDefineStruct.Create("Patrol","巡逻",1,"2","巡逻范围",new Color(1, 1f, 0f, 1f)),
        ActionClipDefineStruct.Create("Move","移动|指定方向移动",3,"0;0;0","x;y;z",new Color(0.8f, 0.8f, 0f, 1f)),
        ActionClipDefineStruct.Create("MoveTar","移动|移动到目标对象",0,"","",new Color(0.6f, 0.6f, 0f, 1f)),
        ActionClipDefineStruct.Create("MovePoint","移动|移动到目标点",1,"","x;y;z",new Color(0.4f, 0.4f, 0f, 1f)),

        ActionClipDefineStruct.Create("MoveReflex","移动|反射移动",0,"","",new Color(0.3f, 0.3f, 0f, 1f)),
        ActionClipDefineStruct.Create("MoveRand","移动|全地图随机移动",0,"0","随机数",new Color(0.3f, 0.3f, 0f, 1f)),

        ActionClipDefineStruct.Create("Check","检测AI运行状态",0,"0","",new Color(0.3f, 1f, 0f, 1f)),
        

    };


    public struct ActionClipDefineStruct
    {
        public string eventType;
        public int paramCount;
        public string deafultParam;
        public string descParam;
        public string desc;
        public Color color;
        public static ActionClipDefineStruct Create(string _eventType, string _desc, int _paramCount, string _deafultParam, string _descParam ,Color _color)
        {
            ActionClipDefineStruct ret;
            ret.eventType = _eventType;
            ret.paramCount = _paramCount;
            ret.deafultParam = _deafultParam.Replace(';','\n');
            ret.descParam = _descParam.Replace(';', '\n');
            ret.desc = _desc;
            ret.color = _color;
            return ret;
        }
    }


    public static ActionClipDefineStruct[] ListActionClipDefine { get { return m_ListActionClipDefine; } }



   

    private static ActionClipDefineStruct GetStruct(string pType)
    {
        ActionClipDefineStruct ret = m_ListActionClipDefine[0];
        for (int k = 0; k < m_ListActionClipDefine.Length; k++)
        {
            if (m_ListActionClipDefine[k].eventType == pType)
            {
                return m_ListActionClipDefine[k];
            }
        }
        return ret;
    }

    public static bool CheckActionClipDefine(string pType)
    {
        for (int k = 0; k < m_ListActionClipDefine.Length; k++)
        {
            if (m_ListActionClipDefine[k].eventType == pType)
            {
                return true;
            }
        }
        return false;
    }

    //获取类型参数数量
    public static int GetParamCount(string pType)
    {
        return GetStruct(pType).paramCount;
    }

    //获取默认值
    public static string GetDefaultParams(string pType)
    {
        return GetStruct(pType).deafultParam;
    }

    //获取参数描述
    public static string GetDefaultDescParam(string pType)
    {
        return GetStruct(pType).descParam;
    }

    public static string GetDeafultDesc(string pType)
    {
        return GetStruct(pType).desc;
    }
}
