using System;

namespace ZooWorld.Game.Movement
{
    /// <summary>Optional cues of a jumping movement, for views (squash and stretch, sounds...).</summary>
    public interface IJumpCues
    {
        event Action Jumped;
        event Action Landed;

        /// <summary>How long one jump lasts, so a view can match its hop to it.</summary>
        float JumpDuration { get; }
    }
}
