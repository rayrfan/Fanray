using Fan.Models;

namespace Fan.Tests
{
    /// <summary>
    /// Different users that interact with the system.
    /// </summary>
    public class Actor
    {
        public const int AUTHOR_ID = 1;
        public const string AUTHOR_USERNAME = "user";
        public static User User = new User { Id = 1, UserName = "user", DisplayName = "My Name", Email = "user@email.com" };
    }
}