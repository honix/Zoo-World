using System;

namespace ZooWorld.Game.Animals
{
    /// <summary>
    /// Optional view hook. The animal is already dead in logic when this plays;
    /// it only delays returning the object to the pool.
    /// </summary>
    public interface IDeathAnimation
    {
        void Play(Action onFinished);
    }
}
