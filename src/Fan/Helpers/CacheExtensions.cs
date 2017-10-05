using Fan.Helpers;
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
        /// 
        /// <see cref="IDistributedCache"/> works only with byte[], so I have to convert T to byte[] 
        /// before putting it into the cache, and convert it back from byte[] into T after taking it 
        /// out of the cache.
        /// </remarks>
        public async static Task<T> GetAsync<T>(this IDistributedCache cache, string key,
            TimeSpan cacheTime, Func<Task<T>> acquire) where T : class, new()
        {
            byte[] bytes = await cache.GetAsync(key);

            if (bytes != null)
            {
                return await Serializer.BytesToObjectAsync<T>(bytes);
            }
            else
            {
                var task = acquire();
                if (task != null && task.Result != null)
                {
                    bytes = await Serializer.ObjectToBytesAsync(task.Result);
                    await cache.SetAsync(key, bytes, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = cacheTime });
                }

                return await task;
            }
        }
    }
}