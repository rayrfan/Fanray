using System.Collections.Generic;

namespace Fan.Models
{
    public interface IHierarchical<T> where T : IHierarchical<T>
    {
        T Parent { get; set; }

        IList<T> Children { get; set; }
    }
}
