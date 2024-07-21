using UnityEngine;

namespace HeartUnity
{
    public class VectorUtil
    {
        public static bool MoveTo(float maxDistance, ref Vector3 origin, Vector2 target) {
			var targetX = target.x;
			var targetY = target.y;
			var disX = targetX - origin.x;
			var disY = targetY - origin.y;
			var disSq = disX * disX + disY * disY;

			if (disSq < maxDistance * maxDistance)
			{
				origin.x = targetX;
				origin.y = targetY;
				return true;
			}
			else
			{
				var dis = Mathf.Sqrt(disSq);
				// calculate direction normalized
				var dirX = disX / dis;
				var dirY = disY / dis;
				origin.x += dirX * maxDistance;
				origin.y += dirY * maxDistance;
				return false;
			}
		}
    }
}