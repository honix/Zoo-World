using System;

namespace ZooWorld.Game.Movement
{
    /// <summary>Optional cues of a jumping movement, for views (squash and stretch, sounds...).</summary>
    public interface IJumpCues
    {
        event Action Jumped;
        event Action Landed;
    }
}
