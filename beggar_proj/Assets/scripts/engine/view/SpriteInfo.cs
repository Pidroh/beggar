//using UnityEngine.U2D;

namespace HeartUnity.View
{
    using System;
    using UnityEngine;

    [Serializable]
    public class SpriteInfo {
        public Sprite sprite;
        public Vector3 offset;
        //public Vector3 scale;
        public float scale = 1f;
        public string id;

    }
}
