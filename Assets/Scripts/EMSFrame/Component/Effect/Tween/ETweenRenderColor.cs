//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace UnityFrame{

	public class ETweenRenderColor: ETweenBase
    {
		public string colorName = "_Color";
		public Color source;
		public Color target;
        public bool overly;

        [SerializeField]private AnimationCurve m_Curve = AnimationCurve.Linear(0,0,1,1);
		[SerializeField]private List<Renderer> m_Renders = new List<Renderer> ();
		public AnimationCurve curve{get{ return m_Curve;}}
		public List<Renderer> renders{ get{ return m_Renders;}}

        private Dictionary<int, Color> m_MapCacheColor;
        private Dictionary<int, Color> mapCacheColor {
            get {
                if (m_MapCacheColor == null)
                    m_MapCacheColor = new Dictionary<int, Color>();
                return m_MapCacheColor;
            }
        }

        protected override void UF_OnPlay()
		{
			if (m_Renders == null)
				return;
			if (delay > 0) {
				for (int k = 0; k < m_Renders.Count; k++) {
					if (m_Renders [k] != null) {
						for (int i = 0; i < m_Renders [k].materials.Length; i++) {
							if (m_Renders [k].materials [i] != null) {
                                if (overly)
                                {
                                    int key = m_Renders[k].materials[i].GetInstanceID();
                                    if (!mapCacheColor.ContainsKey(key))
                                    {
                                        mapCacheColor.Add(key, m_Renders[k].materials[i].GetColor(colorName));
                                    }
                                    m_Renders[k].materials[i].SetColor(colorName, source * mapCacheColor[key]);
                                }
                                else {
                                    m_Renders[k].materials[i].SetColor(colorName, source);
                                }
							}
						}
					}
				}
			}
		}

		protected override void UF_OnRun(float progress)
		{
			if (m_Renders == null)
				return;
			float K = curve.Evaluate(progress);
			Color current = source * (1.0f - K) + target * K;
			for (int k = 0; k < m_Renders.Count; k++) {
				if (m_Renders [k] != null) {
					for (int i = 0; i < m_Renders [k].materials.Length; i++) {
						if (m_Renders [k].materials[i] != null) {
                            if (overly)
                            {
                                int key = m_Renders[k].materials[i].GetInstanceID();
                                if (mapCacheColor.ContainsKey(key))
                                {
                                    m_Renders[k].materials[i].SetColor(colorName, current * mapCacheColor[key]);
                                }
                                else {
                                    m_Renders[k].materials[i].SetColor(colorName, current);
                                }
                            }
                            else {
                                m_Renders[k].materials[i].SetColor(colorName, current);
                            }
                        }
					}
				}
			}
		}
			

	}

}
