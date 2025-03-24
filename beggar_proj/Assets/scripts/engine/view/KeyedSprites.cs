using System;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.U2D;

namespace HeartUnity.View
{
    [Serializable]
    public class KeyedSprites
    {

        public List<KeyedSprite> spritesList;

        public KeyedSprites()
        {
            spritesList = new List<KeyedSprite>();
        }

        public Sprite this[string key]
        {
            get
            {
                foreach (KeyedSprite ks in spritesList)
                {
                    if (ks.key == key)
                    {
                        return ks.sprite;
                    }
                }
                return null;
            }
            set
            {
                for (int i = 0; i < spritesList.Count; i++)
                {
                    if (spritesList[i].key == key)
                    {
                        spritesList[i].sprite = value;
                        return;
                    }
                }
                spritesList.Add(new KeyedSprite { _key = key, sprite = value });
            }
        }

        [Serializable]
        public class KeyedSprite
        {
            public string key => string.IsNullOrEmpty(_key) ? sprite.name : _key;
            public string _key;
            public Sprite sprite;

        }

#if UNITY_EDITOR
        public void Add(Sprite sprite)
        {
            spritesList.Add(new KeyedSprite() { 
                _key = "",
                sprite = sprite
            });
        }
#endif
    }
}