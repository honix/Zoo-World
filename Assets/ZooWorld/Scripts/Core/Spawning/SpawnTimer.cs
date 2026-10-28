using System;
using ZooWorld.Core.Randomness;

namespace ZooWorld.Core.Spawning
{
    /// <summary>Fires once per random interval drawn uniformly from [min, max] seconds.</summary>
    public sealed class SpawnTimer
    {
        private readonly IRandom _random;
        private readonly float _minInterval;
        private readonly float _maxInterval;
        private float _untilNext;

        public SpawnTimer(IRandom random, float minInterval, float maxInterval)
        {
            if (minInterval <= 0f) throw new ArgumentOutOfRangeException(nameof(minInterval), "Interval must be positive.");
            if (maxInterval < minInterval) throw new ArgumentOutOfRangeException(nameof(maxInterval), "Max interval is below min.");

            _random = random ?? throw new ArgumentNullException(nameof(random));
            _minInterval = minInterval;
            _maxInterval = maxInterval;
            ScheduleNext();
        }

        /// <summary>The interval currently being waited out.</summary>
        public float CurrentInterval { get; private set; }

        /// <summary>Advances time and returns how many spawns became due (usually 0 or 1).</summary>
        public int Tick(float deltaTime)
        {
            if (deltaTime < 0f) throw new ArgumentOutOfRangeException(nameof(deltaTime));

            var due = 0;
            _untilNext -= deltaTime;
            while (_untilNext <= 0f)
            {
                due++;
                var overshoot = _untilNext;
                ScheduleNext();
                _untilNext += overshoot;
            }

            return due;
        }

        private void ScheduleNext()
        {
            CurrentInterval = (float)_random.Range(_minInterval, _maxInterval);
            _untilNext = CurrentInterval;
        }
    }
}
