using VContainer.Unity;
using ZooWorld.Core.Time;
using ZooWorld.Game.Animals;

namespace ZooWorld.Game.World
{
    /// <summary>
    /// One simulation step: spawn what's due, keep animals on screen, move them.
    /// The game runs it every FixedUpdate; tests call <see cref="Step"/> directly
    /// together with Physics.Simulate to run minutes of zoo life in milliseconds.
    /// </summary>
    public sealed class ZooSimulation : IFixedTickable
    {
        private readonly AnimalSpawner _spawner;
        private readonly AnimalPopulation _population;
        private readonly IPlayArea _playArea;
        private readonly OffScreenSteering _steering;
        private readonly ITimeProvider _time;

        public ZooSimulation(AnimalSpawner spawner, AnimalPopulation population,
            IPlayArea playArea, OffScreenSteering steering, ITimeProvider time)
        {
            _spawner = spawner;
            _population = population;
            _playArea = playArea;
            _steering = steering;
            _time = time;
        }

        void IFixedTickable.FixedTick() => Step(_time.DeltaTime);

        public void Step(float deltaTime)
        {
            _spawner.Tick(deltaTime);

            var bounds = _playArea.Bounds;
            var animals = _population.Animals;
            for (var i = 0; i < animals.Count; i++)
            {
                var animal = animals[i];
                if (_steering.TrySteer(bounds, animal.Position, animal.Movement.Heading, out var heading))
                {
                    animal.Steer(heading);
                }

                animal.Tick(deltaTime);
            }
        }
    }
}
