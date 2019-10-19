using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAS.Content.Feature.Azure
{
    public class CloudStorageService : ICloudStorageService
    {
        private readonly CloudStorageAccount _cloudStorageAccount;


        public CloudStorageService(IConfiguration configuration)
        {
            _cloudStorageAccount = StorageUtils.GetCloudStorageAccount(configuration["Azure:Storage:ConnectionString"]);
        }

        public async Task<BlobCreatedEvent> CreateBlob(string containerName, string fileName, byte[] fileContent, string folderName = null, Dictionary<string, string> properties = null, Dictionary<string, string> metadata = null)
        {

            BlobCreatedEvent result = new BlobCreatedEvent();

            var blob = await GetWritableBlob(containerName, fileName, folderName);
            try
            {
                var uploadSuccess = await UploadBlobToContainer(blob, fileContent);

                if (uploadSuccess)
                {
                    var propSuccess = false;
                    var metaSuccess = false;
                    if (properties != null)
                    {
                        propSuccess = await UpdateBlobProperties(blob, properties);
                    }

                    if (metadata != null)
                    {
                        metaSuccess = await UpdateBlobMetadata(blob, metadata);
                    }

                    if (!(propSuccess && metaSuccess))
                    {
                        result.Error = true;
                    }
                    else
                    {
                        result.Error = false;
                    }
                }
                else
                {
                    result.Error = true;
                }
            }
            catch (StorageException e)
            {
                throw e;
            }

            if (!result.Error)
            {
                result.Size = await GetBlobSize(blob);
            }

            return result;
        }

        public async Task<BlobUpdateEvent> UpdateBlob(string containerName, string fileName, Dictionary<string, string> metadata = null, string folderName = null)
        {
            BlobUpdateEvent result = new BlobUpdateEvent();

            var blob = await GetWritableBlob(containerName, fileName, folderName);

            var metaSuccess = false;

            if (metadata != null)
            {
                metaSuccess = await UpdateBlobMetadata(blob, metadata);
            }

            if (!metaSuccess)
            {
                result.Error = true;
            }
            else
            {
                result.Error = false;
            }

            return result;
        }

        public async Task<bool> DeleteBlob(string containerName, string fileName, string folderName = null)
        {
            // get/create container
            var container = await StorageUtils.GetBlobContainer(_cloudStorageAccount, containerName);

            // generate the nested file name
            string nestedFileName = folderName != null ? $"{folderName}{container.ServiceClient.DefaultDelimiter}{fileName}" : $"{fileName}";

            // get sas token
            var sasUri = StorageUtils.GetDeleteBlobSasUri(container, nestedFileName, null);

            // Return a reference to the blob using the SAS URI. This allows us to upload file contents without having to submit SAS as a query.
            var blob = new CloudBlockBlob(new Uri(sasUri));

            return await blob.DeleteIfExistsAsync();
        }

        private async Task<CloudBlockBlob> GetWritableBlob(string containerName, string fileName, string folderName = null)
        {
            // get/create container
            var container = await StorageUtils.GetBlobContainer(_cloudStorageAccount, containerName);

            // generate the nested file name
            string nestedFileName = folderName != null ? $"{folderName}{container.ServiceClient.DefaultDelimiter}{fileName}" : $"{fileName}";

            // get sas token
            var sasUri = StorageUtils.GetBlobSasUri(container, nestedFileName, null);

            // Return a reference to the blob using the SAS URI. This allows us to upload file contents without having to submit SAS as a query.
            return new CloudBlockBlob(new Uri(sasUri));
        }

        private async Task<bool> UploadBlobToContainer(CloudBlockBlob blob, byte[] fileContent)
        {
            return await StorageUtils.UploadBlob(blob, fileContent);
        }

        private async Task<bool> UpdateBlobProperties(CloudBlockBlob blob, Dictionary<string, string> properties)
        {
            try
            {
                blob.Properties.ContentType = properties["ContentType"];
                await blob.SetPropertiesAsync();
            }
            catch (StorageException e)
            {
                throw e;
            }
            return true;
        }

        private async Task<bool> UpdateBlobMetadata(CloudBlockBlob blob, Dictionary<string, string> metaData)
        {
            try
            {
                foreach (var key in metaData.Keys.ToList())
                {
                    blob.Metadata.Add(key, metaData[key]);
                }

                await blob.SetMetadataAsync();

            }
            catch (StorageException e)
            {
                throw e;
            }
            return true;
        }

        private async Task<long> GetBlobSize(CloudBlockBlob blob)
        {

            return await StorageUtils.GetBlobSize(blob);
        }

        public async Task<string> GetReadOnlyBlobSasToken(string containerName, string folderName, string fileName)
        {
            // container
            var container = await StorageUtils.GetBlobContainer(_cloudStorageAccount, containerName);

            var blobName = StorageUtils.GetFileName(container, folderName, fileName);

            return StorageUtils.GetReadOnlyBlobSasUri(container, blobName);
        }
    }

    public class BlobCreatedEvent
    {
        public long Size { get; set; }
        public bool Error { get; set; }

    }

    public class BlobUpdateEvent
    {
        public bool Error { get; set; }
    }
}
