using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static HAS.Content.Feature.Library.CreateLibraryHub;
using static HAS.Content.Feature.Library.CreateNewLibraryInHub;
using static HAS.Content.Feature.Library.GetHubById;
using static HAS.Content.Feature.Library.GetHubByProfileId;
using static HAS.Content.Feature.Library.GetLibraryById;
using static HAS.Content.Feature.Library.SetLibraryAccess;

namespace HAS.Content.Controllers
{
    [AllowAnonymous]
    [Route("[controller]")]
    [ApiController]
    public class LibraryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LibraryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Create Library Hub
        [HttpPost("{profileId}", Name="CreateLibraryHub")]
        public async Task<IActionResult> CreateLibraryHub(string profileid)
        {
            var result = await _mediator.Send(new CreateLibraryHubCommand(profileid));

            if(string.IsNullOrEmpty(result))
            {
                return NotFound();
            }

            var uri = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/library/i/{result}";

            Response.Headers.Add("Location", uri);
            return StatusCode(303);
        }

        // Get Library Hub by Id
        [HttpGet("{hubId}", Name = "GetLibraryHubById")]
        public async Task<IActionResult> GetLibraryHubById(string hubId)
        {
            var result = await _mediator.Send(new GetHubByIdQuery(hubId));

            if(result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        // Get Library Hub by Profile Id
        [HttpGet("i/{profileId}", Name = "GetLibraryHubByProfileId")]
        public async Task<IActionResult> GetLibraryHubByProfileId(string profileId)
        {
            var result = await _mediator.Send(new GetHubByProfileIdQuery(profileId));

            if(result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        // Add Library to Hub
        [HttpPost("{hubId}/lib", Name = "AddLibraryToHub")]
        public async Task<IActionResult> AddLibraryToHub(string hubId, [FromBody] CreateNewLibraryInHubCommand details)
        {
            details.HubId = hubId;

            var result = await _mediator.Send(details);

            if(result == null)
            {
                return NotFound();
            }

            var uri = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/library/{hubId}/lib/{result}";

            Response.Headers.Add("Location", uri);
            return StatusCode(303);
        }


        // Get Library By Id
        [HttpGet("{hubId}/lib/{libId}", Name = "GetLibraryById")]
        public async Task<IActionResult> GetLibraryById(string hubId, string libId)
        {
            var result = await _mediator.Send(new GetLibraryByIdQuery(hubId, libId));

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        // Set Library Access
        [HttpPut("{hubId}/lib/{libId}/access/{access}")]
        public async Task<IActionResult> SetLibraryAccess(string hubId, string libId, string access)
        {
            var result = await _mediator.Send(new SetLibraryAccessCommand(hubId, libId, access));

            if (result == null)
            {
                return NotFound();
            }

            var uri = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/library/{hubId}/lib/{result}";

            Response.Headers.Add("Location", uri);
            return StatusCode(303);
        }
    }
}