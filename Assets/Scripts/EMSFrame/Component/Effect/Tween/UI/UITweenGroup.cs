using UnityEngine;
using System.Collections.Generic;


namespace UnityFrame {
    public class UITweenGroup : UIObject
    {
        //Active时播放
        public bool playOnActive;

        [HideInInspector] [SerializeField] private List<ETweenBase> m_ListTween = new List<ETweenBase>();


        public void Play()
        {
            foreach (var v in m_ListTween) {
                if (v != null) v.Play();
            }
        }

        public void Reverse()
        {
            foreach (var v in m_ListTween)
            {
                if (v != null) v.Reverse();
            }
        }

        public void Stop()
        {
            foreach (var v in m_ListTween)
            {
                if (v != null) v.Stop();
            }
        }

        public void Pause()
        {
            foreach (var v in m_ListTween)
            {
                if (v != null) v.Pause();
            }
        }

        public void Continue()
        {
            foreach (var v in m_ListTween)
            {
                if (v != null) v.Continue();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (playOnActive) { this.Play(); }
        }


    }
}
