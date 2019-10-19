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
using static HAS.Content.Feature.Media.FindByProfileId;
using static HAS.Content.Feature.Media.UploadAudio;

namespace HAS.Content.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
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
            var result = await _mediator.Send(new FindByIdQuery(id));

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
            var result = await _mediator.Send(new UploadAudioCommand(Request));

            if(string.IsNullOrEmpty(result))
            {
                return NotFound();
            }

            Response.Headers.Add("Location", $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/media/{result}");
            return StatusCode(303);
        }

        [HttpGet("all/{instructorId}", Name = "FindByProfileId")]
        public async Task<IActionResult> FindByProfileId(string instructorId)
        {
            var result = await _mediator.Send(new FindByProfileIdQuery(instructorId));

            return Ok(result);
        }
    }
}
