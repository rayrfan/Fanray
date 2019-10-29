namespace Fan.Exceptions
{
    public enum EExceptionType
    {
        /// <summary>
        /// When validation fails.
        /// </summary>
        ValidationError = 0,
        /// <summary>
        /// Database table does not allow duplicate record.
        /// </summary>
        DuplicateRecord,
        /// <summary>
        /// When an app's resource is not found, such as a blog post or page.
        /// </summary>
        ResourceNotFound,
    }
}