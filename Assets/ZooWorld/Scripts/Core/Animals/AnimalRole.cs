namespace ZooWorld.Core.Animals
{
    /// <summary>
    /// Coarse classification for statistics and UI only.
    /// Food chain rules never look at it; they use tags and diets.
    /// </summary>
    public enum AnimalRole
    {
        Prey,
        Predator
    }
}
