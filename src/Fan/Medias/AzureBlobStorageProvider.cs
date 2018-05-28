using Fan.Helpers;
using Fan.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Fan.Medias
{
    /// <summary>
    /// Azure Blob Storage provider.
    /// </summary>
    public class AzureBlobStorageProvider : IStorageProvider
    {
        private static CloudBlobContainer _container;
        private readonly string _connString;
        private readonly AppSettings _appSettings;
        public AzureBlobStorageProvider(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _connString = configuration.GetConnectionString("BlobStorageConnectionString");
            _appSettings = serviceProvider.GetService<IOptionsSnapshot<AppSettings>>().Value;
            PrepBlobContainer();
        }

        /// <summary>
        /// Prepares blob container.
        /// </summary>
        private async void PrepBlobContainer()
        {
            var storageAccount = CloudStorageAccount.Parse(_connString);
            if (storageAccount == null)
                throw new Exception("Azure Blob Storage connection string is not valid.");

            // get client
            var blobClient = storageAccount.CreateCloudBlobClient();

            // get a ref to contain which does not call server
            _container = blobClient.GetContainerReference(_appSettings.MediaContainerName);
            await _container.CreateIfNotExistsAsync();
            await _container.SetPermissionsAsync(new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            });
        }

        /// <summary>
        /// Returns unqiue file name after saveing file byte array to storage.
        /// </summary>
        /// <remarks>
        /// The storage type can be configured in appsettings.json. The file is stored like the following
        /// "container/appName/userId/year/month/fileName.ext".
        /// </remarks>
        /// <param name="source">The bytes of the file.</param>
        /// <param name="appId">Which app uploaded file.</param>
        /// <param name="userId">Who uploaded the file.</param>
        /// <param name="year">Upload year.</param>
        /// <param name="month">Upload month.</param>
        /// <param name="fileName">Slugged filename with ext.</param>
        public async Task<string> SaveFileAsync(byte[] source, EAppType appId, int userId, string year, string month, string fileName)
        {
            var (blob, blobName) = await GetBlobAsync(appId, userId, year, month, fileName);

            // create blob with contents
            await blob.UploadFromByteArrayAsync(source, 0, source.Length);

            // get the filename part
            var start = blobName.LastIndexOf('/') + 1;
            var uniqueFileName = blobName.Substring(start, blobName.Length - start);

            return uniqueFileName;
        }

        /// <summary>
        /// Returns unqiue file name after saveing file stream to storage.
        /// </summary>
        /// <remarks>
        /// The storage type can be configured in appsettings.json. The file is stored like the following
        /// "container/appName/userId/year/month/fileName.ext".
        /// </remarks>
        /// <param name="source">The stream of the file.</param>
        /// <param name="appId">Which app uploaded file.</param>
        /// <param name="userId">Who uploaded the file.</param>
        /// <param name="year">Upload year.</param>
        /// <param name="month">Upload month.</param>
        /// <param name="fileName">Slugged filename with ext.</param>
        public async Task<string> SaveFileAsync(Stream source, EAppType appId, int userId, string year, string month, string fileName)
        {
            var (blob, blobName) = await GetBlobAsync(appId, userId, year, month, fileName);

            await blob.UploadFromStreamAsync(source);

            // get the filename part
            var start = blobName.LastIndexOf('/') + 1;
            var uniqueFileName = blobName.Substring(start, blobName.Length - start);

            return uniqueFileName;
        }

        private async Task<(CloudBlockBlob blob, string blobName)> GetBlobAsync(EAppType appId, int userId, string year, string month, string fileName)
        {
            // blobName "blog/1/2017/11/filename", container is "media"
            string blobName = string.Format("{0}/{1}/{2}/{3}/{4}",
                appId.ToString().ToLowerInvariant(),
                userId,
                year,
                month,
                fileName);

            // get a ref to blob which does not call server
            var blob = _container.GetBlockBlobReference(blobName);

            // make sure blob is unique
            int i = 1;
            while (await blob.ExistsAsync())
            {
                blobName = blobName.Insert(blobName.LastIndexOf('.'), $"-{i}");
                blob = _container.GetBlockBlobReference(blobName);
            }

            // set blob properties
            blob.Properties.ContentType = MimeTypeMap.GetMimeType(Path.GetExtension(fileName));
            blob.Properties.CacheControl = "public, max-age=31536000"; // 1 yr

            return (blob: blob, blobName: blobName);
        }
    }
}
