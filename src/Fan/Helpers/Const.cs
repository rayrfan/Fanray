namespace Fan.Helpers
{
    public class Const
    {
        /// <summary>
        /// Max length for a category and tag's title or slug is 24.
        /// </summary>
        /// <remarks>
        /// I'm treating a taxonomy's title and slug with same length requirement.
        /// </remarks>
        public const int TAXONOMY_TITLE_SLUG_MAXLEN = 24;

        /// <summary>
        /// Max length for a post's title or slug is 256.
        /// </summary>
        /// <remarks>
        /// I'm treating a post's title and slug with same length requirement.
        /// </remarks>
        public const int POST_TITLE_SLUG_MAXLEN = 256;
    }
}
