namespace ZooWorld.Core.Animals
{
    public readonly struct AnimalDeath
    {
        public AnimalDeath(AnimalId id, AnimalProfile profile)
        {
            Id = id;
            Profile = profile;
        }

        public AnimalId Id { get; }

        public AnimalProfile Profile { get; }
    }
}
