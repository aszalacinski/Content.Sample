using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HAS.Content.Feature.Azure
{
    public static class StorageUtils
    {
        public static CloudStorageAccount GetCloudStorageAccount(string connectionString)
        {
            // upload to Storage
            CloudStorageAccount storageAccount;

            try
            {
                storageAccount = CloudStorageAccount.Parse(connectionString);
            }
            catch (StorageException e)
            {
                throw e;
            }

            return storageAccount;
        }

        public static async Task<CloudBlobContainer> GetBlobContainer(CloudStorageAccount storageAccount, string containerName)
        {
            // Create service client for credentialed access to the Blob service.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            try
            {
                // Create the container if it does not already exist.
                await container.CreateIfNotExistsAsync();
            }
            catch (StorageException e)
            {
                // Ensure that the storage emulator is running if using emulator connection string.
                throw e;
            }

            return container;
        }

        public static string GetBlobSasUri(CloudBlobContainer container, string blobName, string policyName = null)
        {
            string sasBlobToken;

            // Get a reference to a blob within the container.
            // Note that the blob may not exist yet, but a SAS can still be created for it.
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);

            if (policyName == null)
            {
                // Create a new access policy and define its constraints.
                // Note that the SharedAccessBlobPolicy class is used both to define the parameters of an ad-hoc SAS, and 
                // to construct a shared access policy that is saved to the container's shared access policies. 
                SharedAccessBlobPolicy adHocSAS = new SharedAccessBlobPolicy()
                {
                    // When the start time for the SAS is omitted, the start time is assumed to be the time when the storage service receives the request. 
                    // Omitting the start time for a SAS that is effective immediately helps to avoid clock skew.
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24),
                    Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Create | SharedAccessBlobPermissions.Add
                };

                // Generate the shared access signature on the blob, setting the constraints directly on the signature.
                sasBlobToken = blob.GetSharedAccessSignature(adHocSAS);

            }
            else
            {
                // Generate the shared access signature on the blob. In this case, all of the constraints for the
                // shared access signature are specified on the container's stored access policy.
                sasBlobToken = blob.GetSharedAccessSignature(null, policyName);

            }

            // Return the URI string for the container, including the SAS token.
            return blob.Uri + sasBlobToken;
        }

        public static string GetDeleteBlobSasUri(CloudBlobContainer container, string blobName, string policyName = null)
        {
            string sasBlobToken;

            // Get a reference to a blob within the container.
            // Note that the blob may not exist yet, but a SAS can still be created for it.
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);

            if (policyName == null)
            {
                // Create a new access policy and define its constraints.
                // Note that the SharedAccessBlobPolicy class is used both to define the parameters of an ad-hoc SAS, and 
                // to construct a shared access policy that is saved to the container's shared access policies. 
                SharedAccessBlobPolicy adHocSAS = new SharedAccessBlobPolicy()
                {
                    // When the start time for the SAS is omitted, the start time is assumed to be the time when the storage service receives the request. 
                    // Omitting the start time for a SAS that is effective immediately helps to avoid clock skew.
                    SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(15),
                    Permissions = SharedAccessBlobPermissions.Delete
                };

                // Generate the shared access signature on the blob, setting the constraints directly on the signature.
                sasBlobToken = blob.GetSharedAccessSignature(adHocSAS);

            }
            else
            {
                // Generate the shared access signature on the blob. In this case, all of the constraints for the
                // shared access signature are specified on the container's stored access policy.
                sasBlobToken = blob.GetSharedAccessSignature(null, policyName);

            }

            // Return the URI string for the container, including the SAS token.
            return blob.Uri + sasBlobToken;
        }

        public static string GetReadOnlyBlobSasUri(CloudBlobContainer container, string blobName, string policyName = null)
        {
            string sasBlobToken;

            // Get a reference to a blob within the container.
            // Note that the blob may not exist yet, but a SAS can still be created for it.
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);

            if (policyName == null)
            {
                // Create a new access policy and define its constraints.
                // Note that the SharedAccessBlobPolicy class is used both to define the parameters of an ad-hoc SAS, and 
                // to construct a shared access policy that is saved to the container's shared access policies. 
                SharedAccessBlobPolicy adHocSAS = new SharedAccessBlobPolicy()
                {
                    // When the start time for the SAS is omitted, the start time is assumed to be the time when the storage service receives the request. 
                    // Omitting the start time for a SAS that is effective immediately helps to avoid clock skew.
                    SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(60),
                    Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.List
                };

                // Generate the shared access signature on the blob, setting the constraints directly on the signature.
                sasBlobToken = blob.GetSharedAccessSignature(adHocSAS);

            }
            else
            {
                // Generate the shared access signature on the blob. In this case, all of the constraints for the
                // shared access signature are specified on the container's stored access policy.
                sasBlobToken = blob.GetSharedAccessSignature(null, policyName);

            }

            // Return the URI string for the container, including the SAS token.
            return blob.Uri + sasBlobToken;
        }

        public static async Task<bool> UploadBlob(CloudBlockBlob blob, byte[] streamedFileContent)
        {
            // Create operation: Upload a blob with the specified name to the container.
            // If the blob does not exist, it will be created. If it does exist, it will be overwritten.
            try
            {
                MemoryStream fileContent = new MemoryStream(streamedFileContent);
                fileContent.Position = 0;
                using (fileContent)
                {
                    await blob.UploadFromStreamAsync(fileContent);
                }

            }
            catch (StorageException e)
            {
                if (e.RequestInformation.HttpStatusCode == 403)
                {
                    throw e;
                }
                else
                {
                    throw e;
                }
            }

            return true;
        }

        public async static Task<long> GetBlobSize(CloudBlockBlob blob)
        {
            await blob.FetchAttributesAsync();
            return blob.Properties.Length;
        }

        public static string GetFileName(CloudBlobContainer container, string folder, string fileName)
        {
            return $"{folder}{container.ServiceClient.DefaultDelimiter}{fileName}";
        }
    }
}
