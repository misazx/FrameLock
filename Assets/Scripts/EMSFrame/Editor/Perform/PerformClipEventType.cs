#if UNITY_EDITOR

using UnityEngine;
using System.Collections;

//表现行为类型
public enum PerformActionClipEventType
{

    Null,       //表示空事件

    Idle,       //待机 

    Anima,      //播放动画

    Attack,     //攻击目标

    Skill,     //播放技能

    Patrol,     //巡逻

    Move,       //移动指定方向

    MoveTar,    //移动到目标

    MovePoint,  //移动到指定点

    MoveReflex,  //反射移动

    MoveRand , //随机移动移动

    Check       //坚持AI是否在运行
}


#endif