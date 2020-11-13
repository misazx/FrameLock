//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace UnityFrame{

	public class EParticlePlayer : EffectBase
	{
		[HideInInspector][SerializeField]private List<ParticleSystem> m_Particles = new List<ParticleSystem> ();

		public List<ParticleSystem> particles{get{ return m_Particles;}}

		protected override void UF_OnStop()
		{
			base.UF_OnStop();
			if(m_Particles != null)
			{
				for(int k = 0;k < m_Particles.Count;k++){
					if(m_Particles[k] != null){
						m_Particles [k].Stop ();
					}
				}
			}
		}

        protected override void UF_OnPlay()
        {
            if (m_Particles != null)
            {
                for (int k = 0; k < m_Particles.Count; k++)
                {
                    if (m_Particles[k] != null)
                    {
                        m_Particles[k].Stop();
                    }
                }
            }
        }


        protected override void UF_OnStart()
        {
            if (m_Particles != null)
            {
                for (int k = 0; k < m_Particles.Count; k++)
                {
                    if (m_Particles[k] != null)
                    {
                        m_Particles[k].Play();
                    }
                }
            }
        }

	}

}