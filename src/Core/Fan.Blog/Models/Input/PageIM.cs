using Fan.Themes;

namespace Fan.Blog.Models.Input
{
    /// <summary>
    /// Input Model for page.
    /// </summary>
    public class PageIM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string BodyMark { get; set; }
        public string Excerpt { get; set; }
        public string PostDate { get; set; }
        public int? ParentId { get; set; }
        public bool Published { get; set; }
        public bool IsDraft { get; set; }
        public bool IsParentDraft { get; set; }
        public string DraftDate { get; set; }
        public EPageLayout PageLayout { get; set; }
    }
}
