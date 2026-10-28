using UnityEngine;

namespace ZooWorld.Game.Views
{
    /// <summary>The shape of one step, shared by every walking leg.</summary>
    public static class Gait
    {
        /// <summary>
        /// Where a leg is in its step. <paramref name="swing"/> runs from -1 (fully back) to 1 (fully forward);
        /// <paramref name="lift"/> is 0 on the ground and up to 1 while the leg travels forward.
        /// </summary>
        /// <param name="cycle">Cycles completed so far; only the fraction matters.</param>
        /// <param name="phase">The leg's offset in the cycle, 0..1. Legs half a cycle apart alternate.</param>
        public static void Evaluate(float cycle, float phase, out float swing, out float lift)
        {
            var angle = (cycle + phase) * 2f * Mathf.PI;
            swing = Mathf.Sin(angle);

            // Forward travel is where the swing is increasing: lift the foot then, plant it on the way back.
            lift = Mathf.Max(0f, Mathf.Cos(angle));
        }
    }
}
