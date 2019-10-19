using HAS.Content.Feature.Media;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Threading.Tasks;
using static HAS.Content.Feature.Media.FindById;
using static HAS.Content.Feature.Media.UploadAudio;

namespace HAS.Content.Controllers
{
    [AllowAnonymous]
    [Route("[controller]")]
    public class MediaController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MediaController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id}", Name = "GetMediaById")]
        public async Task<IActionResult> GetMediaBy(string id)
        {
            var result = await _mediator.Send(new Query(id));

            if(result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [Authorize(Policy = "instructor")]
        [HttpPost("new", Name = "UploadAudio")]
        [RequestSizeLimit(6000000000)]
        public async Task<IActionResult> UploadMedia()
        {
            var result = await _mediator.Send(new Command(Request));

            if(string.IsNullOrEmpty(result))
            {
                return NotFound();
            }

            // build api uri
            string url = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/media/{result}";

            Response.Headers.Add("Location", url);
            return StatusCode(303, url);
        }
    }
}
