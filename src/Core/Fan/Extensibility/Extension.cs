namespace Fan.Extensibility
{
    /// <summary>
    /// The base of all extensible items.
    /// </summary>
    public class Extension
    {
        public virtual string DetailsUrl => null;

        public virtual string EditUrl => null;
    }
}
