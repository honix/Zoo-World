using System;
using UnityEngine;
using ZooWorld.Core.Randomness;

namespace ZooWorld.Game.Movement
{
    /// <summary>
    /// Every <c>interval</c> seconds, jumps a fixed distance over <c>duration</c> seconds,
    /// then sits still. The hop arc is purely visual; logic moves on the ground plane.
    /// </summary>
    public sealed class JumpMovement : IMovement, IJumpCues
    {
        private const float Arrived = 1e-4f;

        private readonly float _interval;
        private readonly float _distance;
        private readonly float _jumpSpeed;
        private readonly float _maxTurnDegrees;
        private readonly IRandom _random;

        private float _untilNextJump;
        private float _remainingDistance;
        private bool _steered;

        public JumpMovement(float interval, float distance, float duration, float maxTurnDegrees, IRandom random)
        {
            _interval = interval;
            _distance = distance;
            _jumpSpeed = distance / duration;
            _maxTurnDegrees = maxTurnDegrees;
            _random = random;

            Heading = Planar.RandomDirection(random);
            // Random phase, so frogs spawned together don't jump in sync.
            _untilNextJump = (float)random.Range(0d, interval);
        }

        public event Action Jumped;
        public event Action Landed;

        public Vector3 Heading { get; private set; }

        public bool IsJumping { get; private set; }

        public void Tick(IMovementBody body, float deltaTime)
        {
            _untilNextJump -= deltaTime;

            if (IsJumping && _remainingDistance <= Arrived) Land(body);
            if (!IsJumping && _untilNextJump <= 0f) StartJump();
            if (IsJumping) Advance(body, deltaTime);
        }

        public void Steer(Vector3 heading)
        {
            Heading = Planar.Direction(heading, Heading);
            _steered = true;
        }

        // A bump ends the jump; physics carries the frog and damping slows it down.
        public void OnBumped(IMovementBody body)
        {
            if (!IsJumping) return;

            IsJumping = false;
            _remainingDistance = 0f;
            Landed?.Invoke();
        }

        private void StartJump()
        {
            if (!_steered) Heading = Planar.Rotate(Heading, (float)_random.Range(-_maxTurnDegrees, _maxTurnDegrees));

            _steered = false;
            _untilNextJump += _interval;
            _remainingDistance = _distance;
            IsJumping = true;
            Jumped?.Invoke();
        }

        // Moves exactly the remaining distance, even when the jump doesn't fit whole steps.
        private void Advance(IMovementBody body, float deltaTime)
        {
            var step = Mathf.Min(_jumpSpeed * deltaTime, _remainingDistance);
            _remainingDistance -= step;
            body.Velocity = Heading * (step / deltaTime);
        }

        private void Land(IMovementBody body)
        {
            IsJumping = false;
            body.Velocity = Vector3.zero;
            Landed?.Invoke();
        }
    }
}
