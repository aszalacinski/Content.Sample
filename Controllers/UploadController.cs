using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using HAS.Content.Data;
using HAS.Content.Feature.Azure;
using HAS.Content.Model;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using static HAS.Content.Data.ContentContext;
using static HAS.Content.Feature.Media.FindById;
using static HAS.Content.Feature.Media.UploadAudio;

namespace HAS.Content.Controllers
{
    [Authorize(Policy = "instructor")]
    [Route("[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IMediator _mediator;
        
        public UploadController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("audio", Name="UploadAudio")]
        [DisableFormValueModelBinding]
        [GenerateAntiforgeryTokenCookie]
        [RequestSizeLimit(6000000000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 6000000000)]
        public async Task<IActionResult> Audio()
        {
            var result = await _mediator.Send(new UploadAudioCommand(Request));

            if (result == null)
            {
                return NotFound();
            }

            var media = await _mediator.Send(new FindByIdQuery(result.MediaId));
            return Ok(media);
        }

        [HttpPost("video", Name = "UploadVideo")]
        public IActionResult Video()
        {
            return NotFound();
        }


        [HttpPost("image", Name = "UploadImage")]
        public IActionResult Image()
        {
            return NotFound();
        }


        [HttpPost("shortVideo", Name = "UploadShortVideo")]
        public IActionResult ShortVideo()
        {
            return NotFound();
        }
    }

}