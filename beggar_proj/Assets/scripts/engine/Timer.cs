
namespace HeartUnity
{
    using System;

    public class Timer
    {
        private float elapsedTime;
        private float durationInSeconds;
        private bool isPaused;

        public Timer(float durationInSeconds)
        {
            Init(durationInSeconds);
        }

        public Timer()
        {
        }

        public bool IsComplete
        {
            get { return elapsedTime >= durationInSeconds; }
        }

        public bool IsPaused
        {
            get { return isPaused; }
            set { isPaused = value; }
        }

        public float CompletionRatio
        {
            get { return Math.Min(1.0f, elapsedTime / durationInSeconds); }
            set { elapsedTime = durationInSeconds * value; }
        }

        public float DurationInSeconds { get => durationInSeconds; }

        public void Init(float durationInSeconds)
        {
            this.durationInSeconds = durationInSeconds;
            elapsedTime = 0;
            isPaused = false;
        }

        public void Reset()
        {
            Init(durationInSeconds);
        }

        public void Pause()
        {
            isPaused = true;
        }

        public void Resume()
        {
            isPaused = false;
        }

        public bool Update(float deltaTime)
        {
            if (!isPaused && !IsComplete)
            {
                elapsedTime += deltaTime;

                if (elapsedTime >= durationInSeconds)
                {
                    elapsedTime = durationInSeconds; // Ensure elapsed time doesn't surpass duration
                }
            }
            return IsComplete;
        }
    }


}