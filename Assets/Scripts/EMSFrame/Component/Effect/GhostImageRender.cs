using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFrame
{
    public class GhostImageRender
    {
        internal class GhostMesh
        {
            public Mesh mesh = new Mesh();
            public Material material;
            public Vector3 positon;
            public Quaternion quaternion;
            public Color color;
            public float life;
            public float Tick { get { return mTick; } }
            private float mTick = 0;
            private const string COLOR_NAME = "_Color";

            public GhostMesh(string shaderName)
            {
                UF_SetMaterialShader(shaderName);
            }

            public void UF_SetMaterialShader(string shaderName) {
                if (string.IsNullOrEmpty(shaderName))
                {
                    material = ShaderManager.UF_GetInstance().UF_GetRayMaterial(false);
                }
                else
                {
                    material = ShaderManager.UF_GetInstance().UF_GetMaterial(shaderName);
                }
            }

            public void UF_SetColor(Color col)
            {
                color = col;
            }

            public void UF_SetTexture(Texture texture)
            {
                if (material != null)
                {
                    material.mainTexture = texture;
                }
            }

            public void UF_BakeMesh(SkinnedMeshRenderer skinned)
            {
                skinned.BakeMesh(mesh);
            }

            public void UF_Reset()
            {
                mesh.Clear();
                mTick = 0;
            }

            public bool IsLifeOver
            {
                get
                {
                    return mTick > life;
                }
            }

            public void UF_Update(float delta)
            {
                mTick += delta;
                float pValue = mTick / life;
                //				pValue = Mathf.Clamp01 (1 - pValue);

                pValue = Mathf.Clamp01(Mathf.Cos(pValue * Mathf.PI / 2));

                color *= pValue;

                if (material != null) {
                    if (material.HasProperty(COLOR_NAME))
                    {
                        material.SetColor(COLOR_NAME, color);
                    }
                    Graphics.DrawMesh(mesh, positon, quaternion, material, 0);
                }
            }
        }

        private static TStack<GhostMesh> mMeshStack = new TStack<GhostMesh>(100);

        private static GhostMesh UF_CreateGhostMesh(string shaderName)
        {
            GhostMesh ret = mMeshStack.Pop();
            if (ret == null)
            {
                ret = new GhostMesh(shaderName);
            }
            if (ret.material == null || ret.material.shader == null)
            {
                ret.UF_SetMaterialShader(shaderName);
            }
            return ret;
        }

        private static void UF_RecoverGhostMesh(GhostMesh gMesh)
        {
            gMesh.UF_Reset();
            mMeshStack.Push(gMesh);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        public float rate = 0.5f;

        public float duration = 1.0f;

        public float distance = 0.5f;

        public float life = 1.0f;

        public Color gColor = new Color(0.2f, 0.2f, 0.8f, 1f);


        //旋转偏移,默认X轴旋转90度
        public Quaternion RotationOffset = new Quaternion(-0.7071068f, 0, 0, 0.7071068f);

        private float mTDuration = 0;
        private float mTbuffer = 0;
        private Vector3 mPosBuffer = Vector3.zero;
        private bool mIsShow = false;
        private string mShaderName = "Game/Transparent/RayTransparent";


        private List<GhostMesh> mListGhostMesh = new List<GhostMesh>();

        public void UF_SetColor(Color color) {
            gColor = color;
        }

        public void UF_SetDefaultShader(string name)
        {
            this.mShaderName = name;
        }

        public void UF_Show(float dura,float pval)
        {
            mIsShow = true;
            duration = dura;
            mTDuration = 0;
            rate = pval;
            distance = pval;
        }

        public void UF_Close()
        {
            mIsShow = false;
        }

        private bool UF_CheckTimeEnable(float deltaTime)
        {
            mTbuffer += deltaTime;
            if (mTbuffer > rate)
            {
                mTbuffer = 0;
                return true;
            }
            return false;
        }

        private bool UF_CheckDistanceEnable(Vector3 pos)
        {
            if (Vector3.Distance(pos, mPosBuffer) > distance)
            {
                mPosBuffer = pos;
                return true;
            }
            return false;
        }
    

        //添加绘制一个残影
        public void UF_Add(SkinnedMeshRenderer skinned, Vector3 pos, Quaternion quat, float life,Color color)
        {
            if (skinned == null) return;
            GhostMesh gMesh = UF_CreateGhostMesh(this.mShaderName);
            gMesh.UF_BakeMesh(skinned);
            gMesh.UF_SetColor(color);
            if(skinned.material != null)
            gMesh.UF_SetTexture(skinned.material.mainTexture);
            gMesh.positon = pos;
            gMesh.quaternion = quat * RotationOffset;
            gMesh.life = life;
            mListGhostMesh.Add(gMesh);
        }


        public void UF_OnUpdate(SkinnedMeshRenderer skinned, Vector3 pos, Quaternion quat)
        {
            if (mIsShow)
            {
                ////基于时间
                //if (CheckTimeEnable(Time.deltaTime)) {
                //    Add(skinned, pos, quat, life, gColor);
                //}

                //基于距离
                if (UF_CheckDistanceEnable(pos))
                {
                    UF_Add(skinned, pos, quat, life,gColor);
                }
                mTDuration += Time.deltaTime;
                float progerss = mTDuration / duration;
                if (progerss >= 1)
                {
                    mIsShow = false;
                }
            }
            for (int k = 0; k < mListGhostMesh.Count; k++)
            {
                if (mListGhostMesh[k].IsLifeOver)
                {
                    UF_RecoverGhostMesh(mListGhostMesh[k]);
                    mListGhostMesh.RemoveAt(k);
                    k--;
                }
                else
                {
                    mListGhostMesh[k].UF_Update(Time.deltaTime);
                }
            }
        }


        public void UF_OnReset()
        {
            mIsShow = false;
            for (int k = 0; k < mListGhostMesh.Count; k++)
            {
                UF_RecoverGhostMesh(mListGhostMesh[k]);
            }
            mListGhostMesh.Clear();
        }



    }
}