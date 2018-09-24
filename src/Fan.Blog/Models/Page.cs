using Fan.Blog.Enums;
using System.Collections.Generic;

namespace Fan.Blog.Models
{
    public class Page : Post, IHierarchical<Page>
    {
        public IList<Page> Children { get; set; }

        public Page Parent { get; set; }

        public new EPostType Type { get; } = EPostType.Page;

        public bool IsRoot => RootId.HasValue && RootId.Value == 0;
    }
}
