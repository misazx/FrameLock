using LogicFrameSync.Src.LockStep.Net.Pt;
using NetServiceImpl;
using NetServiceImpl.Client;

namespace LogicFrameSync.Src.LockStep.Behaviours
{
    /// <summary>
    /// 回滚关键帧信息
    /// </summary>
    public class RollbackBehaviour : EntityBehaviour
    {
        public RollbackBehaviour() 
        {
           
        }
        LogicFrameBehaviour logicBehaviour;
        ComponentsBackupBehaviour backupBehaviour;

        int MaxCount = 1000;
        public override void Update()
        {
            lock (Entitas.EntityWorld.SyncRoot)
            {
                logicBehaviour = Sim.GetBehaviour<LogicFrameBehaviour>();
                backupBehaviour = Sim.GetBehaviour<ComponentsBackupBehaviour>();

                int count = 0;
                while (MgobeHelper.QueueKeyFrameCollection.Count > 0)
                {
                    count++;
                    if (count > MaxCount)
                        break;

                    PtKeyFrameCollection pt = null;
                    if (MgobeHelper.QueueKeyFrameCollection.TryPeek(out pt)  )
                    {
                        PtKeyFrameCollection keyframeCollection = null;
                        if(pt.FrameIdx < logicBehaviour.CurrentFrameIdx)
                        {
                            if (MgobeHelper.QueueKeyFrameCollection.TryDequeue(out keyframeCollection))
                                RollImpl(keyframeCollection);
                            else
                                break;
                        }
                        else if(pt.FrameIdx > logicBehaviour.CurrentFrameIdx)
                        {
                            if (MgobeHelper.QueueKeyFrameCollection.TryDequeue(out keyframeCollection))
                                QuickImpl(keyframeCollection);
                            else
                                break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }       

        /// <summary>
        /// 回滚关键帧数据
        /// </summary>
        /// <param name="collection"></param>
        void RollImpl(PtKeyFrameCollection collection)
        {
            int frameIdx = collection.FrameIdx;
            if (frameIdx < 1) return;

            collection.KeyFrames.Sort((a, b) => a.EntityId.CompareTo(b.EntityId));
            //回放命令存储;
            foreach (var frame in collection.KeyFrames)
                logicBehaviour.UpdateKeyFrameIdxInfoAtFrameIdx(collection.FrameIdx, frame);

            //从frameIdx-1数据中深度拷贝一份作为frameIdx的数据
            EntityWorldFrameData framePrevData = backupBehaviour.GetEntityWorldFrameByFrameIdx(frameIdx - 1);
            if(framePrevData!=null)
            {
                //回滚整个entityworld数据
                Sim.GetEntityWorld().RollBack(framePrevData.Clone(), collection);
                //迅速从frameIdx开始模拟至当前客户端frameIdx
                while (frameIdx < logicBehaviour.CurrentFrameIdx)
                {
                    base.Update();
                    backupBehaviour.SetEntityWorldFrameByFrameIdx(frameIdx, new EntityWorldFrameData(Sim.GetEntityWorld().FindAllEntitiesIds(), Sim.GetEntityWorld().FindAllCloneComponents()));
                    ++frameIdx;
                }
            }
        }

        /// <summary>
        /// 追赶帧
        /// </summary>
        /// <param name="collection"></param>
        void QuickImpl(PtKeyFrameCollection collection)
        {
            int frameIdx = collection.FrameIdx;
            while (frameIdx > logicBehaviour.CurrentFrameIdx)
            {
                this.IsActive = false;
                Sim.Run();
            }
            this.IsActive = true;
            EntityWorldFrameData framePrevData = backupBehaviour.GetEntityWorldFrameByFrameIdx(frameIdx);
            if (framePrevData != null)
            {
                Sim.GetEntityWorld().RollBack(framePrevData.Clone(), collection);
            }
        }
    }
}
