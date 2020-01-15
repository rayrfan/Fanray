using Fan.Membership;

namespace Fan.Blog.Tests.Helpers
{
    /// <summary>
    /// Different users that interact with the system. Used for tests.
    /// </summary>
    public class Actor
    {
        public const int ADMIN_ID = 1;
        public const string ADMIN_USERNAME = "admin";
        public const int AUTHOR_ID = 2;
        public const string AUTHOR_USERNAME = "author";
        public static User User = new User { Id = 1, UserName = "admin", DisplayName = "My Name", Email = "user@email.com" };
    }
}