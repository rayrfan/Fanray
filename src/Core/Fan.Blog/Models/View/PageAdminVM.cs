namespace Fan.Blog.Models.View
{
    /// <summary>
    /// Page view model for Admin Console.
    /// </summary>
    public class PageAdminVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Date { get; set; }
        public string Author { get; set; }
        public string ChildrenLink { get; set; }
        public string EditLink { get; set; }
        public string PageLink { get; set; }
        public bool IsDraft { get; set; }
        public bool IsChild { get; set; }
        public int ChildCount { get; set; }
    }
}
