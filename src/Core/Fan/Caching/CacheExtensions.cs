using Fan.Exceptions;
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
        /// <param name="includeTypeName">
        /// True to include type information when serializing JSON and read type information 
        /// so that the create types are created when deserializing JSON
        /// <see cref="https://www.newtonsoft.com/json/help/html/SerializeTypeNameHandling.htm"/>
        /// and <see cref="https://stackoverflow.com/a/22486943/32240"/>.
        /// </param>
        /// <returns></returns>
        /// <remarks>
        /// This method makes it really easy dealing with a common code pattern when working with
        /// cache.  First you check if your object is in the cache, if cache has it then just 
        /// return it.  If the cache does not have it, you execute some code to get it, usually 
        /// from database, then you put it in the cache before you finally return it.
        /// </remarks>
        public async static Task<T> GetAsync<T>(this IDistributedCache cache, string key,
           TimeSpan cacheTime, Func<Task<T>> acquire, bool includeTypeName = false) where T : class
        {
            try
            {
                var str = await cache.GetStringAsync(key);

                if (str != null)
                {
                    return includeTypeName ?
                        JsonConvert.DeserializeObject<T>(str, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto }) :
                        JsonConvert.DeserializeObject<T>(str);
                }
                else
                {
                    var task = acquire();
                    if (task != null && task.Result != null)
                    {
                        str = includeTypeName ?
                            await Task.Factory.StartNew(() => JsonConvert.SerializeObject(task.Result, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto })) :
                            await Task.Factory.StartNew(() => JsonConvert.SerializeObject(task.Result));

                        await cache.SetStringAsync(key, str, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = cacheTime });
                    }

                    return await task;
                }
            }
            catch(AggregateException ex)
            {
                foreach (var e in ex.Flatten().InnerExceptions)
                {
                    if (e is FanException) throw e;
                }
                throw new FanException(ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }
        }       
    }
}