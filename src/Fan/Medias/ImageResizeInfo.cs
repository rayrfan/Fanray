namespace Fan.Medias
{
    /// <summary>
    /// Assists media service on how to resize an image.
    /// </summary>
    /// <remarks>
    /// Each app may have different path on how the image is saved etc.
    /// Currently I'm resizing by the width of an image, I could pass in an enum to signal whether 
    /// it is the width or height I'm resizing by, EitherWorH, ByWidth or ByHeight.
    /// </remarks>
    public class ImageResizeInfo
    {
        /// <summary>
        /// The target size to resize to, <see cref="int.MaxValue"/> means to keep original size.
        /// </summary>
        public int TargetSize { get; set; } = int.MaxValue;
        /// <summary>
        /// The path to save for storage provider, it does not start or end with director separator.
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// The separator for path.
        /// </summary>
        public char PathSeparator { get; set; }
    }
}
