namespace Fan.Data
{
    /// <summary>
    /// Base class for all entities to inherit from.
    /// </summary>
    /// <remarks>
    /// On the entities that derive from this class, note if you delete a column from db table but has prop in class, 
    /// it'll fail with "Invalid column name ...", but if you have the column in db but delete property in class, it works.
    /// </remarks>
    public class Entity
    {
        public int Id { get; set; }
    }
}