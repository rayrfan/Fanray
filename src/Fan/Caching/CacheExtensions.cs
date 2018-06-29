using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Caching.Distributed
{
    public static class CacheExtensions
    {
        /// <summary>
        /// Returns T from cache if available, else acquire T and puts it into the cache before 
        /// returning it.
        /// </summary>
        /// <typeparam name="T">The type of the obj to be returned.</typeparam>
        /// <param name="cache">The IDistributedCache.</param>
        /// <param name="key">A string to get your object from cache.</param>
        /// <param name="cacheTime">An absolute expiration relative to now.</param>
        /// <param name="acquire">The delegate to acquire T.</param>
        /// <returns></returns>
        /// <remarks>
        /// This method makes it really easy dealing with a common code pattern when working with
        /// cache.  First you check if your object is in the cache, if cache has it then just 
        /// return it.  If the cache does not have it, you execute some code to get it, usually 
        /// from database, then you put it in the cache before you finally return it.
        /// </remarks>
        public async static Task<T> GetAsync<T>(this IDistributedCache cache, string key,
           TimeSpan cacheTime, Func<Task<T>> acquire) where T : class
        {
            string str = await cache.GetStringAsync(key);

            if (str != null)
            {
                return JsonConvert.DeserializeObject<T>(str);
            }
            else
            {
                var task = acquire();
                if (task != null && task.Result != null)
                {
                    str = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(task.Result));

                    await cache.SetStringAsync(key, str, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = cacheTime });
                }

                return await task;
            }
        }
    }
}