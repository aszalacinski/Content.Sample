using HAS.Content.Feature.Azure;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Extensions.Configuration;

namespace HAS.Content.Feature.Media
{
    public class GetMediaCloudUrl
    {
        public class GetMediaCloudUrlQuery : IRequest<Uri>
        {
            public string UserAccountReference { get; private set; }
            public string FileType { get; private set; }
            public string FileName { get; private set; }
            public string FileExtenson { get; private set; }

            public GetMediaCloudUrlQuery(string userAccountReference, string fileType, string fileName, string fileExtension)
            {
                UserAccountReference = userAccountReference;
                FileType = fileType;
                FileName = fileName;
                FileExtenson = fileExtension;
            }
        }

        public class GetMediaCloudUrlQueryHandler : IRequestHandler<GetMediaCloudUrlQuery, Uri>
        {
            private readonly IConfiguration _configuration;

            public GetMediaCloudUrlQueryHandler(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public async Task<Uri> Handle(GetMediaCloudUrlQuery request, CancellationToken cancellationToken)
            {
                var storageAccount = StorageUtils.GetCloudStorageAccount(_configuration["Azure:Storage:ConnectionString"]);
                var container = await StorageUtils.GetBlobContainer(storageAccount, request.UserAccountReference);

                var file = StorageUtils.GetBlobSasUri(container, $"{request.FileType}/{request.FileName}.{request.FileExtenson}");

                return new Uri(file);
            }
        }
    }
}
