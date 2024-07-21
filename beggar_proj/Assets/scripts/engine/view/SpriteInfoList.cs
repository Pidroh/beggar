//using UnityEngine.U2D;

namespace HeartUnity.View
{

    using UnityEngine;
    using System.Collections.Generic;

    [CreateAssetMenu(fileName = "SpriteInfoList", menuName = "Custom/Sprite Info List", order = 1)]
    public class SpriteInfoList : ScriptableObject
    {
        public List<SpriteInfo> spriteInfos = new List<SpriteInfo>();
        public SpriteInfo GetSpriteInfoByID(string id)
        {
            foreach (var item in spriteInfos)
            {
                if (item.id == id) return item;
            }
            return null;
        }

        public SpriteInfo GetSpriteInfoByIndex(int index)
        {
            if (index >= 0 && index < spriteInfos.Count)
            {
                return spriteInfos[index];
            }
            else
            {
                Debug.LogError("Invalid index: " + index);
                return null;
            }
        }

        // Custom indexer for accessing SpriteInfo by string ID
        public SpriteInfo this[string id]
        {
            get { return GetSpriteInfoByID(id); }
        }

        // Custom indexer for accessing SpriteInfo by index
        public SpriteInfo this[int index]
        {
            get { return GetSpriteInfoByIndex(index); }
        }
    }


}
