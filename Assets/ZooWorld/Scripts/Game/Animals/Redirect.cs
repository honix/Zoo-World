namespace ZooWorld.Game.Animals
{
    public enum RedirectCause
    {
        /// <summary>The animal was turned back at the edge of the play area.</summary>
        ScreenEdge,

        /// <summary>The animal bounced off another animal or scenery.</summary>
        Bump
    }

    /// <summary>Something outside the animal's own movement changed its heading.</summary>
    public readonly struct Redirect
    {
        public Redirect(RedirectCause cause, float angle)
        {
            Cause = cause;
            Angle = angle;
        }

        public RedirectCause Cause { get; }

        /// <summary>Signed turn in degrees, positive to the right.</summary>
        public float Angle { get; }
    }
}
