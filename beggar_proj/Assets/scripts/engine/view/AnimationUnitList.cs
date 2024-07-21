//using UnityEngine.U2D;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace HeartUnity.View
{
    public class AnimationUnitProcessor
    {
        public List<AnimationUnitList> running = new List<AnimationUnitList>();

        public void ToInitialState(AnimationUnitList list)
        {
            using (ListPool<AnimationUnit>.Get(out var pool))
            {
                pool.AddRange(list.animationUnits);
                pool.Sort(OrderByDelay);
                foreach (var unit in pool)
                {
                    if (unit.data.offsetMode == AnimationUnitData.OffsetMode.MOVE_TO_OFFSET)
                    {
                        unit.uiUnit.OffsetFromOriginal = unit.data.offset;
                    }
                    if (unit.data.offsetMode == AnimationUnitData.OffsetMode.MOVE_FROM_OFFSET)
                    {
                        unit.uiUnit.OffsetFromOriginal = Vector3.zero;
                    }
                    if (unit.data.transparencyMode == AnimationUnitData.TransparencyMode.TO_TRANSPARENCY)
                    {
                        unit.uiUnit.SetTrasparency(1);
                    }
                    if (unit.data.transparencyMode == AnimationUnitData.TransparencyMode.FROM_TRANSPARENCY)
                    {
                        unit.uiUnit.SetTrasparency(unit.data.transparency);
                    }
                }
            }

        }

        internal void Play(AnimationUnitList anim, AnimationUnitList.RealtimeAnimationConfig? config)
        {
            anim.rtProgress = 0;
            anim.rtPlayConfig = config.HasValue ? config.Value : AnimationUnitList.defaultConfig;
            ToInitialState(anim);
            if (!running.Contains(anim))
                running.Add(anim);
        }

        public void Update(float dt)
        {

            using (ListPool<AnimationUnitList>.Get(out var runningPool))
            {
                runningPool.AddRange(running);
                foreach (var run in runningPool)
                {
                    var animationRunning = false;
                    foreach (var unit in run.animationUnits)
                    {

                        float delay = unit.data.delay * run.rtPlayConfig.delayMultiplier;
                        if (run.rtProgress >= delay)
                        {
                            var timeAdv = (run.rtProgress - delay) / unit.data.duration;
                            timeAdv = Mathf.Min(timeAdv, 1f);
                            unit.uiUnit.OffsetFromOriginal = Vector3.Lerp(Vector3.zero, unit.data.offset, unit.data.offsetMode == AnimationUnitData.OffsetMode.MOVE_TO_OFFSET ? timeAdv : 1 - timeAdv);
                            unit.uiUnit.SetTrasparency(Mathf.Lerp(1, unit.data.transparency, unit.data.transparencyMode == AnimationUnitData.TransparencyMode.TO_TRANSPARENCY ? timeAdv : 1 - timeAdv));
                        }
                        if (run.rtProgress < delay + unit.data.duration)
                        {
                            animationRunning = true;
                        }
                    }
                    run.rtProgress += dt * run.configDeltaMultiplier;
                    if (!animationRunning)
                    {
                        if (run.loop)
                        {
                            Play(run, run.rtPlayConfig);
                        }
                        else
                        {
                            running.Remove(run);
                        }
                    }
                }
            }

        }

        private int OrderByDelay(AnimationUnit x, AnimationUnit y)
        {
            return y.data.delay.CompareTo(x.data.delay);
        }
    }



    public class AnimationUnitList : MonoBehaviour
    {
        public static RealtimeAnimationConfig defaultConfig = new RealtimeAnimationConfig() { delayMultiplier = 1f };
        public struct RealtimeAnimationConfig
        {
            public float delayMultiplier;
        }
        public List<AnimationUnit> animationUnits;
        public bool loop = false;
        public float configDeltaMultiplier = 1f;
        public float rtProgress;
        public RealtimeAnimationConfig rtPlayConfig = defaultConfig;

        public bool IsOver
        {
            get
            {
                foreach (var unit in animationUnits)
                {
                    if (rtProgress <= unit.data.duration + unit.data.delay * rtPlayConfig.delayMultiplier) return false;
                }
                return true;
            }
        }
    }
}
