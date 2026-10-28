namespace ZooWorld.Core.Spawning
{
    /// <summary>
    /// Decides what spawns next. The default is a weighted random pick; other
    /// strategies (a shuffle bag that avoids long streaks, event presets...)
    /// plug in without touching the spawner.
    /// </summary>
    public interface ISpawnStrategy<out T>
    {
        T Next();
    }
}
