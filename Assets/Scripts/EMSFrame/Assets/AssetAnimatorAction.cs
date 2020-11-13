//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFrame.Assets {
    //[CreateAssetMenu(menuName = "UnityFrame/Asset Animator Action", order = 100)]
    public class AssetAnimatorAction : ScriptableObject
    {
        public RuntimeAnimatorController animatorController;

        [SerializeField] private List<AnimatorClip> m_AnimatorClips = new List<AnimatorClip>();
        public List<AnimatorClip> listClips { get { return m_AnimatorClips; } }

    }

}
