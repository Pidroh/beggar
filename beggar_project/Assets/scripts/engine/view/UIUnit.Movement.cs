using System;
using UnityEngine;
//using UnityEngine.U2D;

namespace HeartUnity.View
{
    public partial class UIUnit
    {


        [Serializable]
        public class Movement
        {
            public Vector3 offsetToMoveTo;
            public float speed;
            public bool hideWhenReachGoal;
            private UIUnit _uiUnit;

            public UIUnit UiUnit { get => _uiUnit; set => _uiUnit = value; }
            public Vector3 GoalPosition { get; private set; }
            public bool MovingToGoal { get; private set; }

            public void StartMoveToOffset(Vector3 initialPosition, bool hideWhenReach)
            {
                hideWhenReachGoal = hideWhenReach;
                _uiUnit.gameObject.SetActive(true);
                _uiUnit.transform.position = initialPosition;
                GoalPosition = initialPosition + offsetToMoveTo * UiUnit.transform.parent.lossyScale.x;
                MovingToGoal = true;
            }

            public void Update()
            {
                if (MovingToGoal)
                {
                    var position = _uiUnit.transform.position;
                    var appliedSpeed = speed * _uiUnit.transform.lossyScale.x;
                    var result = VectorUtil.MoveTo(Time.deltaTime * appliedSpeed, ref position, GoalPosition);
                    _uiUnit.transform.position = position;
                    if (result && hideWhenReachGoal)
                    {
                        _uiUnit.gameObject.SetActive(false);
                    }
                }

            }
        }

    }
}