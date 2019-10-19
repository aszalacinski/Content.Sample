using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAS.Content.Feature.Azure
{
    public interface ICloudStorageService
    {
        Task<BlobCreatedEvent> CreateBlob(string containerName, string fileName, byte[] fileContent, string folderName = null, Dictionary<string, string> properties = null, Dictionary<string, string> metadata = null);
        Task<bool> DeleteBlob(string containerName, string fileName, string folderName = null);
        Task<string> GetReadOnlyBlobSasToken(string containerName, string folderName, string fileName);
        Task<BlobUpdateEvent> UpdateBlob(string containerName, string fileName, Dictionary<string, string> metadata = null, string folderName = null);
    }
}
