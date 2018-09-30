using System.Threading.Tasks;

namespace Fan.Membership
{
    public interface IUserService
    {
        /// <summary>
        /// Find user by either email or username.  Returns null if not found.
        /// </summary>
        /// <param name="emailOrUsername"></param>
        /// <returns></returns>
        Task<User> FindByEmailOrUsernameAsync(string emailOrUsername);
    }
}