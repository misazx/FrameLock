//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityFrame
{
	public class MotionManager : HandleSingleton<MotionManager>
	{

		private Transform UF_ObjectToTransform(UnityEngine.Object value){
			Transform ret = null;
			if (value != null) {
				if (value is GameObject) {
					GameObject go = value as GameObject;
					ret = go.transform;
				} else if (value is MonoBehaviour) {
					MonoBehaviour mo = value as MonoBehaviour;
					ret = mo.transform;
				} else if (value is Transform) {
					ret = value as Transform;
				}
			}
			return ret;
		}
			
			
		public int UF_AddPosition(
			UnityEngine.Object target,
			Vector3 vfrom,
			Vector3 vto,
			float duration,
			bool ingoreTimeScale,
			float heightOffset,
			bool spaceWorld,
			Vector2 offset,
			bool focusAngle,
			DelegateVoid eventFinish)
		{
			int id = 0;
			Transform transform = UF_ObjectToTransform (target);
			if (transform != null) {
				id = FrameHandle.UF_AddCoroutine (UF_MotionPosition(transform,vfrom,vto,duration,ingoreTimeScale,heightOffset,spaceWorld,offset,focusAngle,eventFinish));
			}
			return id;}


		public int UF_AddTransPostion(
			UnityEngine.Object target,
			Vector3 vfrom,
			UnityEngine.Object toObject,
			float duration,
			bool ingoreTimeScale,
			float heightOffset,
			bool spaceWorld,
			Vector2 offset,
			bool focusAngle,
			DelegateVoid eventFinish
		){
			int id = 0;
			Transform transform = UF_ObjectToTransform (target);
			Transform tarTransform = UF_ObjectToTransform (toObject);
			if (transform != null) {
				id = FrameHandle.UF_AddCoroutine (UF_MotionTransPosition(transform,vfrom,tarTransform,duration,ingoreTimeScale,heightOffset,spaceWorld,offset,focusAngle,eventFinish));
			}
			return id;
		}

        public int UF_AddSpeedTransPostion(
            UnityEngine.Object target,
            Vector3 vfrom,
            UnityEngine.Object toObject,
            float speed,
            bool ingoreTimeScale,
            float heightOffset,
            bool spaceWorld,
            Vector2 offset,
            bool focusAngle,
            DelegateVoid eventFinish
        )
        {
            int id = 0;
            Transform transform = UF_ObjectToTransform(target);
            Transform tarTransform = UF_ObjectToTransform(toObject);
            if (transform != null)
            {
                if(speed <= 0)
                {
                    throw new System.Exception("speed can not less than 0");
                }
                float dis = Vector3.Distance(tarTransform.position , transform.position);
                float duration = dis / speed;
                id = FrameHandle.UF_AddCoroutine(UF_MotionTransPosition(transform, vfrom, tarTransform, duration, ingoreTimeScale, heightOffset, spaceWorld, offset, focusAngle, eventFinish));
            }
            return id;
        }

        public int UF_AddPathPoint(
            UnityEngine.Object target,
            List<Vector3> pathPoints,
            float duration,
            bool ingoreTimeScale,
            float heightOffset,
            bool spaceWorld,
            Vector3 offset,
            bool focusAngle,
            DelegateVoid eventFinish) {
            int id = 0;
            Transform transform = UF_ObjectToTransform(target);
            if (transform != null) {
                id = FrameHandle.UF_AddCoroutine(UF_MotionPathPoint(transform,pathPoints, duration, ingoreTimeScale, heightOffset, spaceWorld, offset, focusAngle, eventFinish));
            }
            return id;
        }



        public int UF_AddEuler(
			UnityEngine.Object target,
			Vector3 vfrom,
			Vector3 vto,
			float duration,
			bool ingoreTimeScale,
			bool spaceWorld,
			DelegateVoid eventFinish)
		{

			int id = 0;
			Transform transform = UF_ObjectToTransform (target);
			if (transform != null) {
				id = FrameHandle.UF_AddCoroutine (UF_MotionEuler(transform,vfrom,vto,duration,ingoreTimeScale,spaceWorld,eventFinish));
			}
			return id;
		}


		public int UF_AddScale(
			Transform target,
			Vector3 vfrom,
			Vector3 vto,
			float duration,
			bool ingoreTimeScale,
			DelegateVoid eventFinish)
		{
			int id = 0;
			Transform transform = UF_ObjectToTransform (target);
			if (transform != null) {
				id = FrameHandle.UF_AddCoroutine (UF_MotionScale(transform,vfrom,vto,duration,ingoreTimeScale,eventFinish));
			}
			return id;
		
		}



		protected IEnumerator UF_MotionPosition(
			Transform target,
			Vector3 vfrom,
			Vector3 vto,
			float duration,
			bool ingoreTimeScale,
			float heightOffset,
			bool spaceWorld,
			Vector3 offset,
			bool focusAngle,
			DelegateVoid eventFinish)
		{
			float progress = 0;
			float tickBuff = 0;
			while (progress < 1) {
				float delta = ingoreTimeScale ? GTime.RunDeltaTime : GTime.UnscaleDeltaTime;
				tickBuff += delta;
				progress = Mathf.Clamp01 (tickBuff / duration);
				Vector3 current = progress * (vto + offset) + (1 - progress) * vfrom;
				Vector3 currentOffset = new Vector3 (0, heightOffset * Mathf.Sin (Mathf.PI * progress), 0);
				Vector3 lastPositon = default(Vector3);
				if (spaceWorld) {
					lastPositon = target.position;
					target.position = current + currentOffset;
					if(focusAngle)
					target.eulerAngles = MathX.UF_EulerAngle(lastPositon,target.position);
				} else {
					lastPositon = target.localPosition;
					target.localPosition = current + currentOffset;
					if(focusAngle)
					target.localEulerAngles = MathX.UF_EulerAngle(lastPositon,target.position);
				}
				yield return null;
			}

            GHelper.UF_SafeCallDelegate(eventFinish);
        }

		protected IEnumerator UF_MotionTransPosition(
			Transform target,
			Vector3 vfrom,
			Transform toObj,
			float duration,
			bool ingoreTimeScale,
			float heightOffset,
			bool spaceWorld,
			Vector3 offset,
			bool focusAngle,
			DelegateVoid eventFinish)
		{
			float progress = 0;
			float tickBuff = 0;
			while (progress < 1) {
				float delta = ingoreTimeScale ? GTime.RunDeltaTime : GTime.UnscaleDeltaTime;
				tickBuff += delta;
				progress = Mathf.Clamp01 (tickBuff / duration);
				Vector3 current = progress * (toObj.position + offset) + (1 - progress) * vfrom;
				Vector3 currentOffset = new Vector3 (0, heightOffset * Mathf.Sin (Mathf.PI * progress), 0);
				Vector3 lastPositon = default(Vector3);
				if (spaceWorld) {
					lastPositon = target.position;
					target.position = current + currentOffset;
					if(focusAngle)
					target.eulerAngles = MathX.UF_EulerAngle(lastPositon,target.position);
				} else {
					lastPositon = target.localPosition;
					target.localPosition = current + currentOffset;
					if(focusAngle)
					target.localEulerAngles = MathX.UF_EulerAngle(lastPositon,target.position);
				}
				yield return null;
			}

            GHelper.UF_SafeCallDelegate(eventFinish);
        }


        protected IEnumerator UF_MotionPathPoint(
            Transform target,
            List<Vector3> pathPoints,
            float duration,
            bool ingoreTimeScale,
            float heightOffset,
            bool spaceWorld,
            Vector3 offset,
            bool focusAngle,
            DelegateVoid eventFinish)
        {
            if (pathPoints.Count == 1) {
                target.position = pathPoints[0];
                yield break;
            }
            //计算总长度
            float totalLenght = 0;
            for (int k = 1; k < pathPoints.Count; k++) {
                totalLenght += Vector3.Distance(pathPoints[k - 1], pathPoints[k]);
            }
            if (totalLenght <= 0) {
                target.position = pathPoints[pathPoints.Count - 1];
                yield break;
            }
            for (int k = 1; k < pathPoints.Count; k++) {
                float progress = 0;
                float tickBuff = 0;
                Vector3 vfrom = pathPoints[k];
                Vector3 vto = pathPoints[k - 1];
                //计算每段的时间片段
                float unitDuration = Vector3.Distance(vfrom, vto) / totalLenght;
                if (unitDuration <= 0)
                    continue;
                while (progress < 1)
                {
                    float delta = ingoreTimeScale ? GTime.RunDeltaTime : GTime.UnscaleDeltaTime;
                    tickBuff += delta;
                    progress = Mathf.Clamp01(tickBuff / duration);
                    Vector3 current = progress * (vto + offset) + (1 - progress) * vfrom;
                    Vector3 currentOffset = new Vector3(0, heightOffset * Mathf.Sin(Mathf.PI * progress), 0);
                    Vector3 lastPositon = default(Vector3);
                    if (spaceWorld)
                    {
                        lastPositon = target.position;
                        target.position = current + currentOffset;
                        if (focusAngle)
                            target.eulerAngles = MathX.UF_EulerAngle(lastPositon, target.position);
                    }
                    else
                    {
                        lastPositon = target.localPosition;
                        target.localPosition = current + currentOffset;
                        if (focusAngle)
                            target.localEulerAngles = MathX.UF_EulerAngle(lastPositon, target.position);
                    }
                    yield return null;
                }
            }
            GHelper.UF_SafeCallDelegate(eventFinish);
        }




        protected IEnumerator UF_MotionEuler(
			Transform target,
			Vector3 vfrom,
			Vector3 vto,
			float duration,
			bool ingoreTimeScale,
			bool spaceWorld,
			DelegateVoid eventFinish)
		{
			float progress = 0;
			float tickBuff = 0;
			while (progress < 1) {
				float delta = ingoreTimeScale ? GTime.RunDeltaTime : GTime.UnscaleDeltaTime;
				tickBuff += delta;
				progress = Mathf.Clamp01 (tickBuff / duration);
				Vector3 current = progress * vto + (1 - progress) * vfrom;
				if (spaceWorld) {
					target.eulerAngles = current;
				} else {
					target.localEulerAngles = current;
				}
				yield return null;
			}

            GHelper.UF_SafeCallDelegate(eventFinish);
        }



		protected IEnumerator UF_MotionScale(
			Transform target,
			Vector3 vfrom,
			Vector3 vto,
			float duration,
			bool ingoreTimeScale,
			DelegateVoid eventFinish)
		{
			float progress = 0;
			float tickBuff = 0;
			while (progress < 1) {
				float delta = ingoreTimeScale ? GTime.RunDeltaTime : GTime.UnscaleDeltaTime;
				tickBuff += delta;
				progress = Mathf.Clamp01(tickBuff / duration);
				Vector3 current = progress * vto + (1 - progress) * vfrom;
				target.localScale = current;
				yield return null;
			}
            GHelper.UF_SafeCallDelegate(eventFinish);
        }



		public void UF_Remove(int id){
			FrameHandle.UF_RemoveCouroutine (id);
		}

	}
}



