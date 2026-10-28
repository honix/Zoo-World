using UnityEngine;

namespace ZooWorld.Game.Movement
{
    /// <summary>
    /// How an animal moves. One implementation per movement style (linear, jumping...),
    /// created from a <see cref="MovementSettings"/> asset. New style = one new class.
    /// </summary>
    public interface IMovement
    {
        /// <summary>Current direction on the ground plane, normalized.</summary>
        Vector3 Heading { get; }

        /// <summary>Advances the movement by one simulation step.</summary>
        void Tick(IMovementBody body, float deltaTime);

        /// <summary>Points the animal in a new direction, e.g. to bring it back on screen.</summary>
        void Steer(Vector3 heading);

        /// <summary>Called after a physical bump; physics has already changed the body's velocity.</summary>
        void OnBumped(IMovementBody body);
    }
}
