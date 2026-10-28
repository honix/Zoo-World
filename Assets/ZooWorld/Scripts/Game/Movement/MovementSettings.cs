using System.Collections.Generic;
using UnityEngine;
using ZooWorld.Core.Randomness;

namespace ZooWorld.Game.Movement
{
    /// <summary>
    /// Designer-facing parameters of a movement style, and the factory for its runtime object.
    /// Animals reference a settings asset; several animals can share one.
    /// </summary>
    public abstract class MovementSettings : ScriptableObject
    {
        public abstract IMovement CreateMovement(IRandom random);

        /// <summary>Human-readable problems with the parameters; empty when valid.</summary>
        public virtual IEnumerable<string> Validate()
        {
            yield break;
        }
    }
}
