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

        // -------------------------------------------------------------------- public method

        public async Task SaveFileAsync(byte[] source, string fileName, string path, char pathSeparator)
        {
            var blob = GetBlob(fileName, path, pathSeparator);

            // set blob properties
            blob.Properties.ContentType = MimeTypeMap.GetMimeType(Path.GetExtension(fileName));
            blob.Properties.CacheControl = "public, max-age=31536000"; // 1 yr

            //await blob.UploadFromStreamAsync(source);
            await blob.UploadFromByteArrayAsync(source, 0, source.Length);
        }

        /// <summary>
        /// Saves the file to Azure Blob Storage.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="info"></param>
        /// <param name="fileNameUnique"></param>
        /// <returns></returns>
        public async Task SaveFileAsync(Stream source, string fileName, string path, char pathSeparator)
        {
            var blob = GetBlob(fileName, path, pathSeparator);

            // set blob properties
            blob.Properties.ContentType = MimeTypeMap.GetMimeType(Path.GetExtension(fileName));
            blob.Properties.CacheControl = "public, max-age=31536000"; // 1 yr

            await blob.UploadFromStreamAsync(source);
        }

        /// <summary>
        /// Deletes a file from blob storage.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="path"></param>
        /// <param name="pathSeparator"></param>
        /// <returns></returns>
        public async Task DeleteFileAsync(string fileName, string path, char pathSeparator)
        {
            var blob = GetBlob(fileName, path, pathSeparator);
            await blob.DeleteIfExistsAsync();
        }

        // -------------------------------------------------------------------- private method

        private CloudBlockBlob GetBlob(string fileName, string path, char pathSeparator)
        {
            var imgPath = path.Replace(pathSeparator, '/');  // azure blob uses '/'
            var blobName = $"{imgPath}/{fileName}";

            // get a ref to blob which does not call server
            return _container.GetBlockBlobReference(blobName);
        }
    }
}
