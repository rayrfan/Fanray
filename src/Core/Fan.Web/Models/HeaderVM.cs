using Fan.Membership;

namespace Fan.Web.Models
{
    public class HeaderVM
    {
        public string Title { get; set; }
        public string Tagline { get; set; }
        public User CurrentUser { get; set; }
        public bool IsSignedIn { get; set; }
    }
}
