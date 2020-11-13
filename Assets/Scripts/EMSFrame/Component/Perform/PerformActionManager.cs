//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityFrame.Assets;

namespace UnityFrame {

    public class PerformActionManager : HandleSingleton<PerformActionManager>, IOnStart,IOnSyncUpdate
    {
        //播放节点类
        internal class PerformPlayNode{
            static TStack<PerformPlayNode> s_PoolData = new TStack<PerformPlayNode>(32);
            static int s_UidIdx = 0;

            public int uid;
            public int pValue;
            public int playedIdx;
            public float curTime;
            public string param;
            public PerformActionClip clip;

            public void UF_Reset() {
                uid = 0;
                pValue = 0;
                curTime = 0;
                playedIdx = 0;
                param = string.Empty;
                
                clip = null;
            }

            public static PerformPlayNode UF_Acquire() {
                var node = s_PoolData.Pop();
                if (node == null)
                {
                    node = new PerformPlayNode();
                }
                node.uid = ++s_UidIdx;
                return node;
            }

            public void UF_Release() {
                this.UF_Reset();
                s_PoolData.Push(this);
            }
        }
        
        private AssetPerformAction m_PerformPkg;

        private List<PerformPlayNode> m_ListPerformPlay = new List<PerformPlayNode>();

        public int tickTimes { get; private set; }

        public int actionCount{ get { return m_ListPerformPlay.Count; } }

        public void UF_OnStart() {
            //加载资源包
            m_PerformPkg = RefObjectManager.UF_GetInstance().UF_LoadRefObject<AssetPerformAction>("obj_perform_action", true) as AssetPerformAction;
            if (m_PerformPkg == null) {
                Debugger.UF_Error(string.Format("Can not load perform package"));
            }
        }

        //设置随机种子
        public void UF_SetSeed(int seed) {
            //TODO:后期设置为自定义随机数
            Random.InitState(seed);
        }



        //播放perform 并返回一个唯一ID
        public int UF_Play(string pName,int pValue,string param) {
            if (m_PerformPkg == null)
            {
                Debugger.UF_Warn("AssetPerformAction Not been Load,Play Failed");
                return 0;
            }

            var perform = m_PerformPkg.UF_Get(pName);
            if (perform == null) {
                Debugger.UF_Warn(string.Format("Can not find perform[{0}] in AssetPerformAction", pName));
                return 0;
            }

            var node = PerformPlayNode.UF_Acquire();
            node.clip = perform;
            node.pValue = pValue;
            node.param = param;
            
            m_ListPerformPlay.Add(node);

            return node.uid;
        }

        //根据唯一ID停止播放
        public void UF_Stop(int uid) {
            for (int k = 0; k < m_ListPerformPlay.Count; k++)
            {
                if (uid == m_ListPerformPlay[k].uid) {
                    m_ListPerformPlay[k].UF_Release();
                    m_ListPerformPlay.RemoveAt(k);
                    return;
                }
            }
        }

        //清空全部播放队列
        public void UF_Clear() {
            m_ListPerformPlay.Clear();
        }

        //执行动画事件
        private void UF_ExcutePerformActionClipEvent(PerformPlayNode perform,ClipEvent clipEvent)
        {
            if (clipEvent.rate > 0) {
                //根据概率设定是否触发
                if (Random.Range(1, 10000) > clipEvent.rate) {
                    return;
                }
            }
            MessageSystem.UF_GetInstance().UF_Send(DefineEvent.E_PERFORM_ACTION_CLIP, perform.uid, perform.pValue, perform.param, clipEvent.name, clipEvent.param);
        }


        
        public void UF_OnSyncUpdate() {
            tickTimes++;

            float dtime = GTime.RunDeltaTime;
            PerformPlayNode tempNode = null;
            for (int k = 0; k < m_ListPerformPlay.Count; k++) {
                tempNode = m_ListPerformPlay[k];
                if (tempNode == null || tempNode.clip == null)
                {
                    tempNode.UF_Release(); m_ListPerformPlay.RemoveAt(k);k--;continue;
                }


                tempNode.curTime += dtime;

                float progress = 1;

                if (tempNode.clip.length > 0) {
                    progress = tempNode.curTime/tempNode.clip.length;
                }

                //ClipEvent 必须为顺序列表，否则错乱
                for(int i = tempNode.playedIdx; i< tempNode.clip.clipEvents.Count;i++) {
                    if (progress >= tempNode.clip.clipEvents[i].trigger) {
                        //do something event trigger
                        //播放索引提前
                        tempNode.playedIdx++;
                        UF_ExcutePerformActionClipEvent(tempNode, tempNode.clip.clipEvents[i]);
                    }
                }

                if (tempNode.curTime >= tempNode.clip.length)
                {
                    if (tempNode.clip.loop)
                    {
                        tempNode.playedIdx = 0;
                        tempNode.curTime = 0;
                    }
                    else
                    {
                        tempNode.UF_Release(); m_ListPerformPlay.RemoveAt(k); k--; continue;
                    }
                }

            }
        }




    }
}


