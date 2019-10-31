using AutoMapper;
using HAS.Content.Data;
using HAS.Content.Feature.Azure;
using HAS.Content.Feature.EventLog;
using HAS.Content.Model;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Storage;
using Microsoft.Net.Http.Headers;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static HAS.Content.Data.ContentContext;

namespace HAS.Content.Feature.Media
{
    public class UploadAudio
    {
        public class UploadAudioCommand : IRequest<UploadAudioCommandResult>, ICommandEvent
        {
            public HttpRequest Request { get; set; }

            public UploadAudioCommand(HttpRequest request) => Request = request;
        }

        public class UploadAudioCommandResult
        {
            public string MediaId { get; set; }
            public string ProfileId { get; set; }
        }

        public class UploadAudioCommandHandler : IRequestHandler<UploadAudioCommand, UploadAudioCommandResult>
        {
            private static readonly string[] _permittedExtensions = { ".m4a" };
            private readonly long _fileSizeLimit = 6000000000;
            private readonly ContentContext _db;
            private static readonly FormOptions _defaultFormOptions = new FormOptions();
            private readonly ICloudStorageService _cloudStorageService;
            private readonly MapperConfiguration _mapperConfiguration;

            public UploadAudioCommandHandler(ContentContext db, ICloudStorageService cloudStorageservice)
            {
                _db = db;
                _cloudStorageService = cloudStorageservice;

                _mapperConfiguration = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<ContentDAOProfile>();
                });
            }

            public async Task<UploadAudioCommandResult> Handle(UploadAudioCommand cmd, CancellationToken cancellationToken)
            {
                var request = cmd.Request;

                if (!MultipartRequestHelper.IsMultipartContentType(request.ContentType))
                {
                    throw new ArgumentException("The media file could not be processed.");
                }

                string mediaId = string.Empty;

                UploadAudioCommandResult cmdResult = new UploadAudioCommandResult();

                try
                {
                    var processResult = await ProcessFile(request);

                    var form = processResult.Form;
                    var file = processResult.File;
                    
                    var results = form.GetResults();

                    string instructorId = results["instructorId"];
                    string title = results["title"];
                    string firstName = results["firstName"];
                    string lastName = results["lastName"];
                    string duration = results["durationInMinutes"];

                    mediaId = await AddInstructorAudioMedia(instructorId, firstName, lastName, title, Convert.ToInt32(duration), DateTime.UtcNow, DateTime.UtcNow, file);

                    cmdResult.MediaId = mediaId;
                    cmdResult.ProfileId = instructorId;

                    if(string.IsNullOrEmpty(mediaId))
                    {
                        throw new Exception($"Could not add instructor media");
                    }

                }
                catch (Exception ex)
                {
                    throw ex;
                }

                return cmdResult;
            }

            private async Task<(byte[] File, KeyValueAccumulator Form)> ProcessFile(HttpRequest request)
            {
                KeyValueAccumulator formAccumulator = new KeyValueAccumulator();

                var streamedFileContent = new byte[0];

                var boundary = MultipartRequestHelper.GetBoundary(
                    MediaTypeHeaderValue.Parse(request.ContentType),
                    _defaultFormOptions.MultipartBoundaryLengthLimit
                    );
                var reader = new MultipartReader(boundary, request.Body);
                var section = await reader.ReadNextSectionAsync();

                while (section != null)
                {
                    var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(
                        section.ContentDisposition, out var contentDisposition);

                    if (hasContentDispositionHeader)
                    {
                        if (MultipartRequestHelper
                            .HasFileContentDisposition(contentDisposition))
                        {
                            streamedFileContent =
                                await FileHelper.ProcessStreamedFile(section, contentDisposition,
                                    _permittedExtensions, _fileSizeLimit);

                        }
                        else if (MultipartRequestHelper
                            .HasFormDataContentDisposition(contentDisposition))
                        {
                            var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name).Value;
                            var encoding = FileHelper.GetEncoding(section);

                            if (encoding == null)
                            {
                                throw new AddMediaException($"The request could not be processed: Bad Encoding");
                            }

                            using (var streamReader = new StreamReader(
                                section.Body,
                                encoding,
                                detectEncodingFromByteOrderMarks: true,
                                bufferSize: 1024,
                                leaveOpen: true))
                            {
                                // The value length limit is enforced by 
                                // MultipartBodyLengthLimit
                                var value = await streamReader.ReadToEndAsync();

                                if (string.Equals(value, "undefined",
                                    StringComparison.OrdinalIgnoreCase))
                                {
                                    value = string.Empty;
                                }

                                formAccumulator.Append(key, value);

                                if (formAccumulator.ValueCount >
                                    _defaultFormOptions.ValueCountLimit)
                                {
                                    throw new AddMediaException($"The request could not be processed: Key Count limit exceeded.");
                                }
                            }
                        }
                    }

                    // Drain any remaining section body that hasn't been consumed and
                    // read the headers for the next section.
                    section = await reader.ReadNextSectionAsync();
                }

                return (streamedFileContent, formAccumulator);
            }

            public async Task<string> AddInstructorAudioMedia(string instructorId, string firstName, string lastName, string title, int durationInMinutes, DateTime recordingDate, DateTime uploadDate, byte[] streamedFileContent)
            {
                // TODO: calculate this some way automatically... someday
                string fileExtension = "m4a";

                // Generate Container Name - InstructorSpecific
                string containerName = $"{firstName[0].ToString().ToLower()}{lastName.ToLower()}-{instructorId}";

                // TODO: calculate this some way automatically... someday
                string contentType = "audio/mp4";
                // TODO: calculate this some way automatically... someday
                FileType fileType = FileType.audio;

                string authorName = $"{firstName} {lastName}";
                string authorShortName = $"{firstName[0]}{lastName}";
                string description = $"{authorShortName} - {title}";

                // TODO: Get this from FFMPEG... someday
                long duration = (durationInMinutes * 60000);

                // Generate new filename
                string fileName = $"{firstName[0].ToString().ToLower()}{lastName.ToLower()}-{Guid.NewGuid()}";

                // TODO: Move this to a BlobCreatedResponse object
                BlobCreatedEvent blobCreatedResult;

                try
                {
                    // Update file properties in storage
                    Dictionary<string, string> fileProperties = new Dictionary<string, string>();
                    fileProperties.Add("ContentType", contentType);

                    // update file metadata in storage
                    Dictionary<string, string> metadata = new Dictionary<string, string>();
                    metadata.Add("author", authorShortName);
                    metadata.Add("tite", title);
                    metadata.Add("description", description);
                    metadata.Add("duration", duration.ToString());
                    metadata.Add("recordingDate", recordingDate.ToString());
                    metadata.Add("uploadDate", uploadDate.ToString());

                    var fileNameWExt = $"{fileName}.{fileExtension}";

                    blobCreatedResult = await _cloudStorageService.CreateBlob(containerName, fileNameWExt, streamedFileContent, "audio", fileProperties, metadata);

                }
                catch (StorageException e)
                {
                    throw e;
                }

                Model.Media media = Model.Media.Create(string.Empty, instructorId, authorName, fileName, fileType, fileExtension, recordingDate, uploadDate, ContentDetails.Create(title, description, duration, blobCreatedResult.Size, 0, new List<string>()), StateDetails.Create(StatusType.STAGED, DateTime.MinValue, DateTime.UtcNow, DateTime.MaxValue), Manifest.Create(new Dictionary<string, string>()));

                // upload to MongoDB
                if (!blobCreatedResult.Error)
                {
                    var mapper = new Mapper(_mapperConfiguration);

                    var dao = mapper.Map<ContentDAO>(media);

                    try
                    {
                        await _db.Content.InsertOneAsync(dao);
                    }
                    catch(Exception)
                    {
                        return string.Empty;
                    }

                    return dao.Id.ToString();
                }
                else
                {
                    // metadata wasn't stored, remove blob
                    await _cloudStorageService.DeleteBlob(containerName, fileName, "audio");
                    throw new AddMediaException($"An issue occurred during media upload: rolling back storage change");
                }

            }
        }

    }

    #region Utilities

    public static class MultipartRequestHelper
    {
        // Content-Type: multipart/form-data; boundary="----WebKitFormBoundarymx2fSWqWSd0OxQqq"
        // The spec at https://tools.ietf.org/html/rfc2046#section-5.1 states that 70 characters is a reasonable limit.
        public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
        {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

            if (string.IsNullOrWhiteSpace(boundary))
            {
                throw new InvalidDataException("Missing content-type boundary.");
            }

            if (boundary.Length > lengthLimit)
            {
                throw new InvalidDataException(
                    $"Multipart boundary length limit {lengthLimit} exceeded.");
            }

            return boundary;
        }

        public static bool IsMultipartContentType(string contentType)
        {
            return !string.IsNullOrEmpty(contentType)
                   && contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool HasFormDataContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            // Content-Disposition: form-data; name="key";
            return contentDisposition != null
                && contentDisposition.DispositionType.Equals("form-data")
                && string.IsNullOrEmpty(contentDisposition.FileName.Value)
                && string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);
        }

        public static bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            // Content-Disposition: form-data; name="myfile1"; filename="Misc 002.jpg"
            return contentDisposition != null
                && contentDisposition.DispositionType.Equals("form-data")
                && (!string.IsNullOrEmpty(contentDisposition.FileName.Value)
                    || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
        }
    }

    public class FileHelper
    {
        public static async Task<byte[]> ProcessStreamedFile(
            MultipartSection section, ContentDispositionHeaderValue contentDisposition,
            string[] permittedExtensions,
            long sizeLimit)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await section.Body.CopyToAsync(memoryStream);

                    if (memoryStream.Length == 0)
                    {
                        throw new InvalidDataException($"The file is empty.");
                    }
                    else if (memoryStream.Length > sizeLimit)
                    {
                        var megabySizeLimit = sizeLimit / 1048576;
                        throw new InvalidDataException($"The file exceeds {megabySizeLimit:N1} MB.");
                    }
                    else if (!permittedExtensions.Any(x => x.Equals(Path.GetExtension(contentDisposition.FileName.Value))))
                    {
                        throw new InvalidDataException($"The file type is not permitted");
                    }
                    else
                    {
                        return memoryStream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public static Encoding GetEncoding(MultipartSection section)
        {
            var hasMediaTypeHeader =
                MediaTypeHeaderValue.TryParse(section.ContentType, out var mediaType);

            // UTF-7 is insecure and shouldn't be honored. UTF-8 succeeds in 
            // most cases.
            if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
            {
                return Encoding.UTF8;
            }

            return mediaType.Encoding;
        }
    }

    public class AddMediaException : Exception
    {
        public AddMediaException() : base() { }

        public AddMediaException(string message) : base(message) { }
    }

    #endregion
}
