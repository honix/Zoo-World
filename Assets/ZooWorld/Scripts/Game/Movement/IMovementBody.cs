using UnityEngine;

namespace ZooWorld.Game.Movement
{
    /// <summary>
    /// The physical body a movement drives. In the game this is the animal's Rigidbody;
    /// in tests it is a plain object, so movement math runs without a scene.
    /// </summary>
    public interface IMovementBody
    {
        Vector3 Position { get; }

        /// <summary>Velocity on the ground plane (y is ignored).</summary>
        Vector3 Velocity { get; set; }
    }
}
