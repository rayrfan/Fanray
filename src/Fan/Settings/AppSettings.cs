using Fan.Medias;

namespace Fan.Settings
{
    /// <summary>
    /// AppSettings section in appsettings.json.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// The preferred domain to use: "auto" (default), "www", "nonwww".
        /// </summary>
        /// <remarks>
        /// - "auto" will use whatever the url is given, will not do forward
        /// - "www" will forward root domain to www subdomain, e.g. fanray.com -> www.fanray.com
        /// - "nonwww" will forward www subdomain to root domain, e.g. www.fanray.com -> fanray.com
        /// 
        /// Note if you are running from a subdomian other than "www", preferred domain will be ignored.
        /// This setting is for SEO, it's good to decide on a preferred domain as indicated in this
        /// Google Search Console document https://support.google.com/webmasters/answer/44231?hl=en
        /// </remarks>
        public EPreferredDomain PreferredDomain { get; set; } = EPreferredDomain.Auto;

        /// <summary>
        /// Whether to use https: false (default) or true.
        /// </summary>
        /// <remarks>
        /// - false, will not forward http to https
        /// - true, will forward http to https
        /// 
        /// Note if user sets this value to false but is already using https, I don't downgrade 
        /// you to http as this is good for SEO, Google strongly recommend all website to use https.
        /// Also if you are running locally with console, set this value to false as console may 
        /// not support https.
        /// </remarks>
        public bool UseHttps { get; set; } = false;

        /// <summary>
        /// The storage type of uploaded media files: "FileSystem" (default) or "AzureBlob".
        /// </summary>
        /// <remarks>
        /// "FileSystem": files will be stored on file system.
        /// "AzureBlob": files will be stored in Azure Blob Storage.
        /// </remarks>
        public EMediaStorageType MediaStorageType { get; set; } = EMediaStorageType.FileSystem;

        /// <summary>
        /// The folder/container name of uploaded media files: "media" (default).
        /// </summary>
        /// <remarks>
        /// For FileSystem, it's a folder created in wwwroot, a typical url from file sys
        /// https://yoursite.com/media/2017/11/file-name.ext
        /// For AzureBlob, it's a container created in your Azure storage account, a typical url from blob
        /// https://your-blob-acct-name.blob.core.windows.net/media/2017/11/file-name.ext
        /// </remarks>
        public string MediaContainerName { get; set; } = "media";
    }
}