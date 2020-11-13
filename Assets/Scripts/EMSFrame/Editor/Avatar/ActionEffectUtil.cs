using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//预览特效
class PreviewEffect
{
    struct ParticleData
    {
        public ParticleSystem particle;
        public float delayTime;
        public ParticleData(ParticleSystem p, float d)
        {
            particle = p;
            delayTime = d;
        }
    }

    public bool isPlaying { get; private set; } = false;
    public GameObject target { get; private set; }
    public float life { get; private set; }
    public string name { get { return target.name; } }
    private ParticleData[] m_ParticleDatas;

    public Vector3 position { get { return target.transform.position; } set { target.transform.position = value; } }
    public Vector3 euler { get { return target.transform.eulerAngles; } set { target.transform.eulerAngles = value; } }


    private float m_lastUpdateTime = 0;
    private float m_startTime = 0;

    public PreviewEffect(GameObject fx)
    {
        if (fx == null) return;
        m_ParticleDatas = null;
        target = fx;
        var array = fx.gameObject.GetComponentsInChildren<ParticleSystem>();
        if (array != null && array.Length > 0)
        {
            m_ParticleDatas = new ParticleData[array.Length];
            life = 0;
            for (int k = 0; k < array.Length; k++)
            {
                var ptlife = array[k].startLifetime + array[k].startDelay;
                if (ptlife > life)
                {
                    //获取最大粒子存活值
                    life = ptlife;
                }
                m_ParticleDatas[k] = new ParticleData(array[k], array[k].startDelay);
            }
            life++;
        }
    }

    public void Play()
    {
        isPlaying = true;
        m_lastUpdateTime = Time.realtimeSinceStartup;
        m_startTime = Time.realtimeSinceStartup;
    }

    public void Stop()
    {
        isPlaying = false;
        m_IsMove = false;
        if (m_ParticleDatas != null)
        {
            foreach (var pd in m_ParticleDatas)
            {
                pd.particle.startDelay = pd.delayTime;
                pd.particle.Stop();
            }

        }
        m_Callback = null;
    }

    bool m_IsMove;
    float m_MoveTimeTick;
    float m_MoveDuration;
    float m_Rad;
    Vector3 m_MoveForm;
    Vector3 m_MoveTo;
    System.Action m_Callback;

    public void Move(float duration,Vector3 vform, Vector3 vto,float rad, System.Action callbak = null)
    {
        if (duration <= 0)
        {
            m_IsMove = false;
            this.position = vto;
            if (callbak != null) {
                callbak.Invoke();
            }
        }
        else {
            m_IsMove = true;
            m_MoveTimeTick = 0;
            m_MoveDuration = duration;
            m_MoveForm = vform;
            m_MoveTo = vto;
            m_Callback = callbak;
            m_Rad = rad;
            life += duration;
        }
    }

    private void UpdateMovement(float detal) {
        if (m_IsMove) {
            m_MoveTimeTick += detal;
            float progress = Mathf.Clamp01(m_MoveTimeTick/m_MoveDuration);
            float hoffset = m_Rad * Mathf.Sin(Mathf.PI * progress);
            Vector3 pos = m_MoveForm * (1 - progress) + progress * m_MoveTo;
            pos.y += hoffset;
            this.position = pos;
            if (progress >= 1) {
                m_IsMove = false;
                if (m_Callback != null)
                {
                    m_Callback.Invoke();
                }
            }
        }
    }

    public void Update()
    {
        if (!isPlaying) return;
        float detal = Time.realtimeSinceStartup - m_lastUpdateTime;
        m_lastUpdateTime = Time.realtimeSinceStartup;

        UpdateMovement(detal);

        float interval = m_lastUpdateTime - m_startTime;
        if (m_ParticleDatas != null) {
            foreach (var pd in m_ParticleDatas)
            {
                if (interval > pd.delayTime)
                {
                    pd.particle.startDelay = 0;
                    pd.particle.Simulate(detal, false, false);
                    pd.particle.Play();
                }
            }
        }
        if (interval > life)
        {
            Stop();
        }
    }

    public void Dispose()
    {
        target = null;
        m_Callback = null;
        m_ParticleDatas = null;
    }

}


static class EffectUtil {
    static List<PreviewEffect> s_ListPreviewEfx = new List<PreviewEffect>();

    public static PreviewEffect FindEffect(string name) {
        foreach (var efx in s_ListPreviewEfx)
        {
            if (efx.name == name && !efx.isPlaying)
            {
                return efx;
            }
        }
        return null;
    }

    public static void Update() {
        for (int k = 0; k < s_ListPreviewEfx.Count; k++) {
            if (s_ListPreviewEfx[k] != null) {
                s_ListPreviewEfx[k].Update();
            }
        }
    }

    public static PreviewEffect Play(GameObject fx) {
        //查看缓存中是否存在播放完成的
        var pefx = FindEffect(fx.name);
        if (pefx != null) {
            pefx.Stop();
            pefx.Play();
            return pefx;
        }
        pefx = new PreviewEffect(fx);
        Debug.Log("Create New EFX:" + pefx.name);
        s_ListPreviewEfx.Add(pefx);
        pefx.Play();
        return pefx;
    }

    public static void Stop(string name)
    {
        var pefx = FindEffect(name);
        if (pefx != null)
        {
            pefx.Stop();
        }
    }


    public static void Cleanup()
    {
        for (int k = 0; k < s_ListPreviewEfx.Count; k++)
        {
            s_ListPreviewEfx[k].Dispose();
        }
        s_ListPreviewEfx.Clear();
    }

}
