using Fan.Helpers;
using Microsoft.Extensions.Configuration;
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
        /// <summary>
        /// All files will be saved into a "media" container, such that a file will have this url
        /// https://your-blob-acct-name.blob.core.windows.net/media/2017/11/file-name.ext
        /// </summary>
        public const string MEDIA_UPLOADS_CONTAINER = "media";

        private static CloudBlobContainer _container;
        private readonly string _connString;
        public AzureBlobStorageProvider(IConfiguration configuration)
        {
            _connString = configuration.GetConnectionString("BlobStorageConnectionString");
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
            _container = blobClient.GetContainerReference(MEDIA_UPLOADS_CONTAINER);
            await _container.CreateIfNotExistsAsync();
            await _container.SetPermissionsAsync(new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            });
        }

        /// <summary>
        /// Returns full path to a file after saving it to Azure Blob Storage.
        /// </summary>
        /// <param name="fileName">Slugged filename with ext.</param>
        /// <param name="year">Upload year.</param>
        /// <param name="month">Upload month.</param>
        /// <param name="content">The content of file.</param>
        /// <param name="appId">Which app it uploaded it.</param>
        /// <returns></returns>
        public async Task<string> SaveFileAsync(string fileName, string year, string month, byte[] content, EAppType appId)
        {
            // blobName "2017/11/filename", container is "media"
            string blobName = string.Format("{0}/{1}/{2}",
                year,
                month, 
                fileName);

            // get a ref to blob which does not call server
            var blob = _container.GetBlockBlobReference(blobName); 

            // make sure blob is unique
            int i = 2;
            while (await blob.ExistsAsync())
            {
                blobName = blobName.Insert(blobName.LastIndexOf('.'), $"-{i}");
                blob = _container.GetBlockBlobReference(blobName);
            }

            // set content type
            blob.Properties.ContentType = MimeTypeMap.GetMimeType(Path.GetExtension(fileName));

            // create blob with contents
            await blob.UploadFromByteArrayAsync(content, 0, content.Length);

            return blob.Uri.ToString();
        }
    }
}
