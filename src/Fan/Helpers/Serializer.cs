using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Fan.Helpers
{
    /// <summary>
    /// Helps the different serializations.
    /// </summary>
    public static class Serializer
    {
        /// <summary>
        /// Asynchronously serializes the specified object to byte array.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <remarks>
        /// This method works with .NET Core v1 as it does not depend on BinaryFormatter.
        /// 
        /// This method asynchronously serializes obj into a string using Json.net, it needs a 
        /// <a href="http://www.newtonsoft.com/json/help/html/DefaultSettings.htm">DefaultSettings</a> 
        /// with <a href="https://stackoverflow.com/q/34753498/32240">ReferenceLoopHandling.Ignore</a>.
        /// After that it <a href="http://stackoverflow.com/a/10380166/32240">converts the string into byte array</a>.
        /// </remarks>
        public async static Task<byte[]> ObjectToBytesAsync(object obj)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var str = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(obj));
            byte[] bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        /// <summary>
        /// Asynchronously serializes the byte array to an object of T. For serialization to work 
        /// T must have parameterless constructor new().
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="bytes"></param>
        public async static Task<T> BytesToObjectAsync<T>(byte[] bytes) where T : class, new()
        {
            T obj = null;

            if (bytes != null && bytes.Length > 0)
            {
                char[] chars = new char[bytes.Length / sizeof(char)];
                Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
                var str = new string(chars);

                obj = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<T>(str));
            }

            return obj;
        }
    }
}