using HAS.Content.Feature.Azure;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;

namespace HAS.Content.Feature.Media
{
    public class GetMediaCloudUrlByArrayOfFilenames
    {
        public class GetMediaCloudUrlByArrayOfFilenamesQuery : IRequest<Dictionary<string, Uri>>
        {
            public string UserAccountReference { get; private set; }
            public string FileType { get; private set; }
            public string[] FileNames { get; private set; }
            public string FileExtenson { get; private set; }

            public GetMediaCloudUrlByArrayOfFilenamesQuery(string userAccountReference, string fileType, string[] fileNames, string fileExtension)
            {
                UserAccountReference = userAccountReference;
                FileType = fileType;
                FileNames = fileNames;
                FileExtenson = fileExtension;
            }
        }

        public class GetMediaCloudUrlByArrayOfFilenamesQueryHandler : IRequestHandler<GetMediaCloudUrlByArrayOfFilenamesQuery, Dictionary<string, Uri>>
        {
            private readonly IConfiguration _configuration;

            public GetMediaCloudUrlByArrayOfFilenamesQueryHandler(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public async Task<Dictionary<string, Uri>> Handle(GetMediaCloudUrlByArrayOfFilenamesQuery request, CancellationToken cancellationToken)
            {
                var storageAccount = StorageUtils.GetCloudStorageAccount(_configuration["Azure:Storage:ConnectionString"]);
                var container = await StorageUtils.GetBlobContainer(storageAccount, request.UserAccountReference);

                BlobContinuationToken continueToken = null;

                Dictionary<string, Uri> uris = new Dictionary<string, Uri>();

                do
                {
                    var results = await container.ListBlobsSegmentedAsync(continueToken);
                    continueToken = results.ContinuationToken;


                    foreach (var name in request.FileNames)
                    {
                        string fileName = $"{request.FileType}/{name}.{request.FileExtenson}";

                        var file = StorageUtils.GetBlobSasUri(container, $"{request.FileType}/{name}.{request.FileExtenson}");

                        uris.Add(name, new Uri(file));
                    }

                } while (continueToken != null);

                return uris;
            }
        }
    }
}
