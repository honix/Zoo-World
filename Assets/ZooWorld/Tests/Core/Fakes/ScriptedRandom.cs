using System.Collections.Generic;
using ZooWorld.Core.Randomness;

namespace ZooWorld.Core.Tests.Fakes
{
    /// <summary>Returns a scripted sequence of values, repeating the last one when it runs out.</summary>
    public sealed class ScriptedRandom : IRandom
    {
        private readonly Queue<double> _values;
        private double _last;

        public ScriptedRandom(params double[] values)
        {
            _values = new Queue<double>(values);
            _last = values.Length > 0 ? values[values.Length - 1] : 0d;
        }

        public int Draws { get; private set; }

        public double NextDouble()
        {
            Draws++;
            return _values.Count > 0 ? _values.Dequeue() : _last;
        }

        public int Next(int minInclusive, int maxExclusive) =>
            minInclusive + (int)(NextDouble() * (maxExclusive - minInclusive));
    }
}
