namespace ZooWorld.Game.World
{
    /// <summary>A play area that doesn't depend on a camera: for headless runs and tests.</summary>
    public sealed class FixedPlayArea : IPlayArea
    {
        public FixedPlayArea(GroundRect visible, float screenMargin, float spawnMargin)
        {
            Visible = visible;
            Bounds = visible.Inset(screenMargin);
            SpawnBounds = visible.Inset(spawnMargin);
        }

        public GroundRect Visible { get; }

        public GroundRect Bounds { get; }

        public GroundRect SpawnBounds { get; }
    }
}
