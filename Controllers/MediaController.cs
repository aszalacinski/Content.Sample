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
using System.Linq;
using System.Threading.Tasks;
using static HAS.Content.Feature.Media.FindById;
using static HAS.Content.Feature.Media.FindByProfileId;
using static HAS.Content.Feature.Media.GetMediaByArrayOfIds;
using static HAS.Content.Feature.Media.UpdateMedia;
using static HAS.Content.Feature.Media.UploadAudio;

namespace HAS.Content.Controllers
{
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

        [HttpGet("multi", Name = "GetMediaByMultipleIds")]
        public async Task<IActionResult> GetMediaByMultipleIds([FromQuery] string[] mediaIds)
        {
            var result = await _mediator.Send(new GetMediaByArrayOfIdsQuery(mediaIds));

            if(result.Count() <= 0)
            {
                return Ok("[]");
            }

            return Ok(result);
        }
        
        [HttpGet("all/{instructorId}", Name = "FindByProfileId")]
        public async Task<IActionResult> FindByProfileId(string instructorId)
        {
            var result = await _mediator.Send(new FindByProfileIdQuery(instructorId));

            if (result.Count() <= 0)
            {
                return Ok("[]");
            }

            return Ok(result);
        }

        [HttpPut("{mediaId}", Name = "UpdateMedia")]
        public async Task<IActionResult> UpdateMedia(string mediaId, [FromBody] UpdateMediaCommand details)
        {
            details.MediaId = mediaId;

            var result = await _mediator.Send(details);

            if(result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }
    }
}
