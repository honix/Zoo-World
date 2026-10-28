using ZooWorld.Core.Time;

namespace ZooWorld.Game.Infrastructure
{
    public sealed class UnityTimeProvider : ITimeProvider
    {
        public float DeltaTime => UnityEngine.Time.fixedDeltaTime;
    }
}
