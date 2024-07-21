using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeartUnity.View
{
    [Serializable]
    public class PositionHolder : MonoBehaviour
    {
        public List<Vector2> positions = new List<Vector2>();
        public int desiredDistance = 0;
        public Vector2 center;
        public bool horizontal = true;
        public void SetNumberOfPositions(int newLength) 
        {
            
            while (positions.Count < newLength) {
                positions.Add(new Vector2(0,0));
            }
            if (positions.Count > newLength) {
                positions.RemoveRange(newLength, positions.Count - newLength);
            }
            RecalculatePositions();
        }

        private void RecalculatePositions()
        {
            var scale = transform.lossyScale;
            center = transform.position;
            var scaleF = horizontal ? scale.x : scale.y;
            var appliedDistance = desiredDistance * scaleF;
            var distanceSize = appliedDistance * (positions.Count - 1);
            var offset = distanceSize / 2;
            // var initPos = centerX;
            var centerX = center.x;
            var centerY = center.y;
            var initPos = (horizontal ? centerX : centerY) - offset;

            for (int i = 0; i < positions.Count; i++)
            {
                Vector2 value = positions[i];
                var positionAmount = initPos + i * appliedDistance;
                if (horizontal)
                {
                    value.x = positionAmount;
                    value.y = centerY;
                }
                else
                {
                    value.x = centerX;
                    value.y = positionAmount;
                }
                positions[i] = value;
            }
        }

        // Start is called before the first frame update
        void Awake()
        {
            center = transform.position;
        }

        // Update is called once per frame
        public void Update()
        {
            
        }
    }
}