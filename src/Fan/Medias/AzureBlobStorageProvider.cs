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
        private readonly CloudBlobContainer _container;
        private readonly CloudStorageAccount _storageAccount;

        public AzureBlobStorageProvider(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            var connString = configuration.GetConnectionString("BlobStorageConnectionString");

            _storageAccount = CloudStorageAccount.Parse(connString);
            if (_storageAccount == null)
                throw new Exception("Azure Blob Storage connection string is not valid.");

            var appSettings = serviceProvider.GetService<IOptionsSnapshot<AppSettings>>().Value;

            var blobClient = _storageAccount.CreateCloudBlobClient();
            // get a ref to container does not call server
            _container = blobClient.GetContainerReference(appSettings.MediaContainerName);

            PrepBlobContainer();
        }

        /// <summary>
        /// The absolute URI endpoint to blob, e.g. "http://127.0.0.1:10000/devstoreaccount1" in dev.
        /// </remarks>
        public string StorageEndpoint => _storageAccount.BlobEndpoint.AbsoluteUri.ToString();

        /// <summary>
        /// Prepares blob container.
        /// </summary>
        private async void PrepBlobContainer()
        {
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
        public async Task<string> SaveFileAsync(byte[] source, EAppType appId, int userId, DateTimeOffset uploadedOn, string fileName, EImageSize quality)
        {
            var (blob, blobName) = await GetBlobAsync(appId, userId, uploadedOn, fileName, quality);

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
        public async Task<string> SaveFileAsync(Stream source, EAppType appId, int userId, DateTimeOffset uploadedOn, string fileName, EImageSize quality)
        {
            var (blob, blobName) = await GetBlobAsync(appId, userId, uploadedOn, fileName, quality);

            await blob.UploadFromStreamAsync(source);

            // get the filename part
            var start = blobName.LastIndexOf('/') + 1;
            var uniqueFileName = blobName.Substring(start, blobName.Length - start);

            return uniqueFileName;
        }

        private async Task<(CloudBlockBlob blob, string blobName)> GetBlobAsync(EAppType appId, int userId, 
            DateTimeOffset uploadedOn, string fileName, EImageSize size)
        {
            // blobName "blog/optimized/1/2018/05/filename.ext", container is "media"
            string appName = appId.ToString().ToLowerInvariant();
            string qualityStr = size.ToString().ToLowerInvariant();
            var year = uploadedOn.Year.ToString();
            var month = uploadedOn.Month.ToString("d2");
            string blobName = string.Format("{0}/{1}/{2}/{3}/{4}/{5}",
                appName,
                qualityStr,
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
                i++;
            }

            // set blob properties
            blob.Properties.ContentType = MimeTypeMap.GetMimeType(Path.GetExtension(fileName));
            blob.Properties.CacheControl = "public, max-age=31536000"; // 1 yr

            return (blob: blob, blobName: blobName);
        }
    }
}
