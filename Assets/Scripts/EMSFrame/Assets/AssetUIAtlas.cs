//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFrame.Assets {
    [CreateAssetMenu(menuName = "UnityFrame/Asset UI Atlas", order = 100)]
    public class AssetUIAtlas : ScriptableObject
    {
        public Texture2D texture;

        [SerializeField] private List<Sprite> m_Sprites = new List<Sprite>();

        private Dictionary<string, Sprite> m_DicSpriteMap = new Dictionary<string, Sprite>();

        private Dictionary<string, Sprite> DicSpriteMap
        {
            get
            {
                if (m_DicSpriteMap.Count == 0 && texture != null && m_Sprites != null)
                {
                    for (int k = 0; k < m_Sprites.Count; k++)
                    {
                        if (m_Sprites[k] != null)
                        {
                            m_DicSpriteMap.Add(m_Sprites[k].name, m_Sprites[k]);
                        }
                    }
                }
                return m_DicSpriteMap;
            }
        }


        public List<Sprite> sprites { get { return m_Sprites; } }

        public string ShortName
        {
            get
            {
                if (string.IsNullOrEmpty(mShortName))
                {
                    mShortName = this.name.Substring(this.name.LastIndexOf("_", System.StringComparison.Ordinal));
                }
                return mShortName;
            }
        }
        private string mShortName;


        public Sprite this[string spriteName]
        {
            get
            {
                return UF_GetSprite(spriteName);
            }
        }

        public bool UF_CheckSprite(string spriteName)
        {
            return DicSpriteMap.ContainsKey(spriteName);
        }

        /// <summary>
        /// sprite name can use short name
        /// </summary>
        public Sprite UF_GetSprite(string spriteName)
        {
            if (Application.isPlaying)
            {
                if (DicSpriteMap.ContainsKey(spriteName))
                {
                    return DicSpriteMap[spriteName];
                }
            }
            else
            {
                if (m_Sprites != null)
                {
                    for (int k = 0; k < m_Sprites.Count; k++)
                    {
                        if (m_Sprites[k].name.Equals(spriteName) || m_Sprites[k].name.Equals(spriteName))
                        {
                            return m_Sprites[k];
                        }
                    }
                }
            }
            if (Debugger.IsActive)
                Debugger.UF_Warn(string.Format("Sprite[{0}] in Atlas[{1}] is Null ", spriteName, this.name));

            return null;
        }


        public Sprite UF_GetSpriteInMap(string spriteName)
        {
            if (DicSpriteMap.ContainsKey(spriteName))
            {
                return DicSpriteMap[spriteName];
            }

#if UNITY_EDITOR
            if (m_Sprites != null)
            {
                for (int k = 0; k < m_Sprites.Count; k++)
                {
                    if (m_Sprites[k].name.Equals(spriteName) || m_Sprites[k].name.Equals(spriteName))
                    {
                        return m_Sprites[k];
                    }
                }
            }
#endif

            return null;
        }

        public void Dispose()
        {
            DicSpriteMap.Clear();
            m_Sprites = null;
            texture = null;
        }



    }
}
