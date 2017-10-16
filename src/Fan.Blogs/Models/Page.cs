using Fan.Blogs.Enums;
using Fan.Enums;
using Fan.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fan.Blogs.Models
{
    public class Page : Post, IHierarchical<Page>
    {
        public IList<Page> Children { get; set; }

        public Page Parent { get; set; }

        public new EPostType Type { get; } = EPostType.Page;

        public bool IsRoot => RootId.HasValue && RootId.Value == 0;
    }
}
