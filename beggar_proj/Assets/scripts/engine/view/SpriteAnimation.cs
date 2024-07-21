//using UnityEngine.U2D;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace HeartUnity.View
{

    public class SpriteAnimationProcessor
    {
        // add to list if necessary later on
        // AnimationUnitProcessor might be a good example on how to expand this
        public List<SpriteAnimation> spriteAnimations = new();
        public void Play(SpriteAnimation sa)
        {
            sa.target.Active = true;
            sa.rtCurrentFrame = 0;
            sa.Apply();
            spriteAnimations.Add(sa);
            /*var seq = DOTween.Sequence();
            foreach (var sprite in sa.spriteAnimationData.sprites)
            {
                seq.AppendInterval(sa.spriteAnimationData.timePerFrame).AppendCallback(sa.AdvanceAndApply);
            }
            if (sa.loop)
                seq.AppendCallback(() =>
                {
                    Play(sa);
                });
            */
        }

        public void ManualUpdate(float dt) {
            using (ListPool<SpriteAnimation>.Get(out var list)) {
                list.AddRange(spriteAnimations);
                foreach (var sa in list)
                {
                    sa.rtTimeProgress += dt;
                    if (sa.rtTimeProgress > sa.spriteAnimationData.timePerFrame)
                    {
                        sa.rtTimeProgress = 0;
                        sa.AdvanceAndApply();
                    }
                    if (sa.IsOver) {
                        spriteAnimations.Remove(sa);
                    }
                }
            }
                
            
        }
    }

    public class SpriteAnimation : MonoBehaviour
    {
        public UIUnit target;
        public SpriteAnimationFixedData spriteAnimationData;
        public bool loop = false;
        public int rtCurrentFrame = 0;
        public bool rtAwakenOnce = false;
        public float rtTimeProgress = 0f;

        public bool IsOver => !loop && rtCurrentFrame >= spriteAnimationData.sprites.Count;

        public void Awake()
        {
            if (spriteAnimationData.hideOnAwake && !rtAwakenOnce)
                target.Active = false;
            rtAwakenOnce = true;
        }

        internal void AdvanceAndApply()
        {
            rtCurrentFrame++;
            if (loop)
            {
                rtCurrentFrame = rtCurrentFrame % spriteAnimationData.sprites.Count;
            }
            Apply();
        }

        internal void Apply()
        {
            if (spriteAnimationData.sprites.Count > rtCurrentFrame)
                target.ChangeSprite(spriteAnimationData.sprites[rtCurrentFrame], true);
            else if(!loop)
                target.Active = false;
        }
    }

    [Serializable]
    public class SpriteAnimationFixedData
    {
        public List<Sprite> sprites;
        public float timePerFrame = 1 / 30f;
        public bool hideWhenOver = true;
        public bool hideOnAwake = true;
    }
}
