namespace Fan.Blogs.Enums
{
    /// <summary>
    /// A post can be in one of four states: Draft, Published, Scheduled or Trashed.
    /// </summary>
    public enum EPostStatus : byte
    {
        Draft = 0,
        Published,
        Scheduled,
        Trashed,
    }
}
