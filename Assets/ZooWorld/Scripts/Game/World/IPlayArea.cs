namespace ZooWorld.Game.World
{
    /// <summary>Where animals are allowed to be: the visible part of the ground.</summary>
    public interface IPlayArea
    {
        /// <summary>The area animals stay inside; they turn back when they leave it.</summary>
        GroundRect Bounds { get; }

        /// <summary>A smaller area new animals spawn in, so they appear fully on screen.</summary>
        GroundRect SpawnBounds { get; }
    }
}
