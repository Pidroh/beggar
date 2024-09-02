using UnityEngine;

namespace HeartUnity
{
    public class RobustDeltaTime 
	{
		public float lastTimeSinceUpdate;
        private float _dt;
		private float _dtUnit = 0.2f;
		private float _dtFixThreeshold = 1f;

        public RobustDeltaTime()
        {
			lastTimeSinceUpdate = Time.realtimeSinceStartup;
		}

		public void ManualUpdate() 
		{
			_dt = Time.realtimeSinceStartup - lastTimeSinceUpdate;
		}

		public bool TryGetProcessedDeltaTime(out float dt) 
		{
			dt = _dt;
			if (_dt <= 0) return false;
			if(_dt > _dtFixThreeshold) 
			{
				_dt -= _dtUnit;
				dt = _dtUnit;
				return true;
			}
			dt = _dt;
			_dt = -1;
			return true;
		}
	}
}