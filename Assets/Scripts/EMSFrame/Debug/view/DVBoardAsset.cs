//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Text;

namespace UnityFrame
{
	public class DVBoardAsset : IDebugViewBoard
	{
		const int TAG_INFO_ENTITY = 0;
		const int TAG_INFO_AB = 2;
		const int TAG_LOAD_TRACK = 3;
		const int TAG_UI = 4;
		const int TAG_RefObject = 5;
		const int TAG_AUDIO = 6;
        const int TAG_SHADER = 7;
        const int TAG_ASSETDB = 8;


        private int m_CurrentTag;

		public void UF_DrawDetail (Rect rect){
			if (GUILayout.Button ("Entity Info",GUILayout.Height(40))) {
				m_CurrentTag = TAG_INFO_ENTITY;
			}
            if (GUILayout.Button("View Stack", GUILayout.Height(40)))
            {
                m_CurrentTag = TAG_UI;
            }
            if (GUILayout.Button("Res Track", GUILayout.Height(40)))
            {
                m_CurrentTag = TAG_LOAD_TRACK;
            }
            if (GUILayout.Button ("AssetBundles",GUILayout.Height(40))) {
				m_CurrentTag = TAG_INFO_AB;
			}
			if (GUILayout.Button ("RefObjects",GUILayout.Height(40))) {
				m_CurrentTag = TAG_RefObject;
			}

			if (GUILayout.Button ("Audios",GUILayout.Height(40))) {
				m_CurrentTag = TAG_AUDIO;
			}

            if (GUILayout.Button("Shaders", GUILayout.Height(40)))
            {
                m_CurrentTag = TAG_SHADER;
            }
            if (GUILayout.Button("AssetDB", GUILayout.Height(40)))
            {
                m_CurrentTag = TAG_ASSETDB;
            }
            
        }


		private string UF_TrackMsgForeach(string info,int usedTick){
			string strColor = "white";

			if (usedTick > 10 && usedTick < 30) {
				strColor = "#80FF80FF";
			}
			else if (usedTick >= 30 && usedTick <= 100) {
				strColor = "yellow";
			} else if (usedTick > 100) {
				strColor = "red";
			}
			return string.Format ("{0} | <color={1}>{2}</color>", info, strColor, usedTick);
		}

		public void UF_DrawInfo(Rect rect){
            if (m_CurrentTag == TAG_INFO_ENTITY)
            {
                GUILayout.BeginHorizontal();
                var sb = StrBuilderCache.Acquire();
                sb.AppendLine(string.Format("== Active Count: {0} ==", CEntitySystem.UF_GetInstance().ActiveCount));
                sb.AppendLine();
                sb.AppendLine(CEntitySystem.UF_GetInstance().UF_GetActiveEntityInfo());
                GUI.color = Color.green;
                GUILayout.Box(sb.ToString());

                sb.Clear();
                sb.AppendLine(string.Format("== Pool Count: {0} ==", CEntitySystem.UF_GetInstance().PoolCount));
                sb.AppendLine();
                sb.AppendLine(CEntitySystem.UF_GetInstance().UF_GetPoolEntityInfo());
                GUI.color = Color.white;
                GUILayout.Box(sb.ToString());
                StrBuilderCache.Release(sb);
                GUILayout.EndHorizontal();
            }
            else if (m_CurrentTag == TAG_INFO_AB)
            {
                GUILayout.Label("Total Count: " + AssetSystem.UF_GetInstance().count);
                GUILayout.Box(AssetSystem.UF_GetInstance().ToString());
            }
            else if (m_CurrentTag == TAG_LOAD_TRACK)
            {
                if (Debugger.UF_GetInstance().MsgTrackers.ContainsKey(Debugger.TRACK_RES_LOAD))
                {
                    MsgTracker tracker = Debugger.UF_GetInstance().MsgTrackers[Debugger.TRACK_RES_LOAD];
                    GUILayout.Box(tracker.UF_ForeachToString(UF_TrackMsgForeach));
                }
            }
            else if (m_CurrentTag == TAG_UI)
            {
                var lastAlignment = GUI.skin.box.alignment;
                GUI.skin.box.alignment = TextAnchor.MiddleLeft;
                GUILayout.Box(UIManager.UF_GetInstance().ToString());
                GUI.skin.box.alignment = lastAlignment;
            }
            else if (m_CurrentTag == TAG_RefObject)
            {
                GUILayout.Box(RefObjectManager.UF_GetInstance().ToString());
            }
            else if (m_CurrentTag == TAG_AUDIO)
            {
                GUILayout.Box(AudioManager.UF_GetInstance().ToString());
            }
            else if (m_CurrentTag == TAG_SHADER)
            {
                GUILayout.Box(ShaderManager.UF_GetInstance().UF_GetShadersInfo());
            }
            else if (m_CurrentTag == TAG_ASSETDB)
            {
                int countBundle = AssetDataBases.UF_GetAssetInfoCount(AssetDataBases.AssetFileType.Bundle);
                int countRebundle = AssetDataBases.UF_GetAssetInfoCount(AssetDataBases.AssetFileType.Rebundle);
                int countRuntimes = AssetDataBases.UF_GetAssetInfoCount(AssetDataBases.AssetFileType.Runtimes);
                int countNone = AssetDataBases.UF_GetAssetInfoCount(AssetDataBases.AssetFileType.None);

                GUILayout.Box(string.Format("Bundle-> {0}",countBundle));
                GUILayout.Box(string.Format("Runtime-> {0}",countRuntimes));
                GUILayout.Box(string.Format("Rebundle-> {0}",countRebundle));
                GUILayout.Box(string.Format("None-> {0}",countNone));

            }


        }

	}
}

