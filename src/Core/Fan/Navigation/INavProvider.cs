using System.Threading.Tasks;

namespace Fan.Navigation
{
    public interface INavProvider
    {
        /// <summary>
        /// Returns true if the provider's <see cref="ENavType"/> matches the given one,
        /// otherwise false.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool CanProvideNav(ENavType type);
        /// <summary>
        /// Returns a nav's URL. The URL is relative and must start with "/".
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<string> GetNavUrlAsync(int id);
    }
}
