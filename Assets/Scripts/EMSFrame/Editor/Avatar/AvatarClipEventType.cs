#if UNITY_EDITOR

using UnityEngine;

using System.Collections;


public enum AvatarClipEventType
{
    Null,       //表示空事件

    Hit,        //受击点(如果命中对方，对方播放受击动作，可带一个参数，参数表示击中特效)
    Dip,        //发射点

    //玩家
    PDip,       //玩家子弹发射点，效果会收到相关属性影响
    PDipSword,  //玩家剑气发射点，效果会收到相关属性影响

    FDip,       //指定方向发射点
    SDip,       //场景位置发射点
    BDip,       //绑定
    FreeBDip,   //解除绑定

    IDip,       //间隔发射弹道

    SPDip,      //分裂弹道

    AEDip,      //拖尾弹道

    //怪物
    MDip,        //子弹发射点
    MBar,        //弹幕发射点,普通弹幕
    MRBar,        //弹幕发射点,普通弹幕
    MRange,

    Perform,    //表现，播放表现效果

    ActSkill,    //表现，直接执行技能

    Efx,        //特效点(播放特效，特效播放位置在攻击者，参数表示播放特效的名字，带一个参数)
    EfxFol,     //特效特效跟随，参数为挂点位置

    EfxTar,     //目标特效播放
    EfxSce,     //场景特效

    Shake,      //震屏点（用于做震屏效果，带三个参数，｛震动幅度;震动频率;衰减时间｝，例如｛0.2；0.02；0.02｝）
    Sound,      //声音点(用于播放声音，带一个参数，参数为声音资源文件名)

    PreEfx,     //预警特效（目标）
    PreLine,    //预警射线
    PreBarLine, //预警弹幕射线

    TarLock,    //目标锁定



    JumpTar,    //跳跃到目标

    Rush,       //冲刺

    RushTh,

    Flash,      //闪现

    Ghost,      //残影

};

#endif