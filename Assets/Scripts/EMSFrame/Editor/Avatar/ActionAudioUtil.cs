using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

static class AudioUtil
{
    private static Type m_realType;
    private static MethodInfo m_methodPlay;
    private static MethodInfo m_methodStop;

    private static List<UnityEngine.AudioClip> s_ListAudioClips = new List<AudioClip>();

    private static Type realType
    {
        get
        {
            if (m_realType == null)
            {
                var assembly = Assembly.GetAssembly(typeof(Editor));
                m_realType = assembly.GetType("UnityEditor.AudioUtil");
            }
            return m_realType;
        }
    }

    private static MethodInfo methodPlay
    {
        get
        {
            if (m_methodPlay == null)
            {
                if (realType != null)
                {
                    m_methodPlay = realType.GetMethod("PlayClip", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(AudioClip) }, new ParameterModifier[] { });
                }
            }
            return m_methodPlay;
        }
    }

    private static MethodInfo methodStop
    {
        get
        {
            if (m_methodStop == null)
            {
                if (realType != null)
                {
                    m_methodStop = realType.GetMethod("StopClip", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(AudioClip) }, new ParameterModifier[] { });
                }
            }
            return m_methodStop;
        }
    }

    public static void Play(AudioClip clip)
    {
        if (clip == null) return;
        if (methodPlay != null)
        {
            if (!s_ListAudioClips.Contains(clip)) {
                s_ListAudioClips.Add(clip);
            }
            object[] args = new object[1];
            args[0] = clip;
            methodPlay.Invoke(null, args);
        }
    }

    public static void Stop(AudioClip clip)
    {
        if (clip == null) return;
        if (methodStop != null)
        {
            object[] args = new object[1];
            args[0] = clip;
            methodStop.Invoke(null, args);
        }
    }

    public static void Cleanup() {
        foreach (var clip in s_ListAudioClips) {
            Stop(clip);
        }
        s_ListAudioClips.Clear();
    }

}