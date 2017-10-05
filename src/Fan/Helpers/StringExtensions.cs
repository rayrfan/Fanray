namespace System
{
    public static class StringExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this String s)
        {
            return String.IsNullOrEmpty(s);
        }

        /// <summary>
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        /// <remarks>
        /// Same as return String.IsNullOrEmpty(value) || value.Trim().Length == 0;
        /// </remarks>
        public static bool IsNullOrWhiteSpace(this String s)
        {
            return String.IsNullOrWhiteSpace(s);
        }
    }
}
