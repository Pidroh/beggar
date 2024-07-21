using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeartUnity.View
{
    public class LoopMovement : MonoBehaviour
    {
        public Transform movingPart;
        public float multiplier;
        public int periodicDistance;
        private Vector3 _initPos;
        public int direction = 1;

        private void Awake()
        {
            _initPos = movingPart.position;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            var appliedPeriodicDis = periodicDistance * transform.lossyScale.x;
            var x = Time.time * multiplier * transform.lossyScale.x % appliedPeriodicDis;
            var position = _initPos;
            position.x += x * direction;
            movingPart.position = position;
        }
    }
}