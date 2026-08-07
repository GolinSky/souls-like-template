using UnityEngine;

namespace Prospector.Utility.Timer
{
    internal class Timer : ITimer
    {
        private float _delay;
        private float _endTime;
        private bool _isRunning;
        
        private float CurrentTime => Time.time;
        
        public bool IsRunning => _isRunning;
        
        public bool IsComplete => _isRunning && CurrentTime >= _endTime;
        
        public float TimeLeft => _isRunning ? Mathf.Max(0f, _endTime - CurrentTime) : 0f;

        public Timer(float delay)
        {
            _delay = delay;
            _isRunning = false;
        }

        public void Start()
        {
            _isRunning = true;
            _endTime = CurrentTime + _delay;
        }

        public void Stop()
        {
            _isRunning = false;
        }

        public void Reset()
        {
            _isRunning = false;
            _endTime = 0f;
        }

        public ITimer ChangeDuration(float newDelay)
        {
            _delay = newDelay;
            return this;
        }
    }
}