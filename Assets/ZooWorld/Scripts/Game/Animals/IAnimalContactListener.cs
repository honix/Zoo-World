namespace ZooWorld.Game.Animals
{
    /// <summary>Receives the physical contacts animals report. Animals don't decide outcomes themselves.</summary>
    public interface IAnimalContactListener
    {
        /// <param name="self">The animal whose collider reported the contact.</param>
        /// <param name="other">The animal it touched, or null for scenery.</param>
        void OnContact(Animal self, Animal other);
    }
}
