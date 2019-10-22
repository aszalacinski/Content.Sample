using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static HAS.Content.Feature.Library.AddContentToLibrary;
using static HAS.Content.Feature.Library.AddTribeToLibrary;
using static HAS.Content.Feature.Library.CreateLibraryHub;
using static HAS.Content.Feature.Library.CreateNewLibraryInHub;
using static HAS.Content.Feature.Library.DeleteLibraryFromHub;
using static HAS.Content.Feature.Library.DeleteLibraryHub;
using static HAS.Content.Feature.Library.GetHubById;
using static HAS.Content.Feature.Library.GetHubByProfileId;
using static HAS.Content.Feature.Library.GetLibraryById;
using static HAS.Content.Feature.Library.GetLibraryByName;
using static HAS.Content.Feature.Library.GetLibraryContent;
using static HAS.Content.Feature.Library.RemoveContentFromLibrary;
using static HAS.Content.Feature.Library.SetLibraryAccess;
using static HAS.Content.Feature.Library.SetLibraryDefaultTribe;

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
        [HttpPost("{profileId}", Name="Create Library Hub")]
        public async Task<IActionResult> CreateLibraryHub(string profileid)
        {
            var result = await _mediator.Send(new CreateLibraryHubCommand(profileid));

            if(string.IsNullOrEmpty(result))
            {
                return NotFound();
            }

            var uri = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/library/{result}";

            Response.Headers.Add("Location", uri);
            return StatusCode(303);
        }

        // Get Library Hub by Id
        [HttpGet("{hubId}", Name = "Get Library Hub By Id")]
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
        [HttpGet("i/{profileId}", Name = "Get Library Hub By Profile Id")]
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
        [HttpPost("{hubId}/lib", Name = "Add Library To Hub")]
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
        [HttpGet("{hubId}/lib/{libId}", Name = "Get Library By Id")]
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
        [HttpPut("{hubId}/lib/{libId}/access/{access}", Name="Set Library Access")]
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

        // Add Tribe to Library
        [HttpPut("{hubId}/lib/{libId}/tribe/{tribeId}", Name = "Add Tribe to Library")]
        public async Task<IActionResult> AddTribeToLibrary(string hubId, string libId, string tribeId)
        {
            var result = await _mediator.Send(new AddTribeToLibraryCommand(hubId, libId, tribeId));

            if (result == null)
            {
                return NotFound();
            }

            var uri = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/library/{hubId}/lib/{result}";

            Response.Headers.Add("Location", uri);
            return StatusCode(303);
        }

        // Set Library Default Tribe
        [HttpPut("{hubId}/lib/{libId}/tribe/default/{tribeId}", Name = "Set Library to Default Tribe")]
        public async Task<IActionResult> SetLibraryDefaultTribe(string hubId, string libId, string tribeId)
        {
            var result = await _mediator.Send(new SetLibraryDefaultTribeCommand(hubId, libId, tribeId));

            if (result == null)
            {
                return NotFound();
            }

            var uri = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/library/{hubId}/lib/{result}";

            Response.Headers.Add("Location", uri);
            return StatusCode(303);
        }

        // Get Library By Name
        [HttpGet("{profileId}/lib/name/{name}", Name = "Get Library By Name")]
        public async Task<IActionResult> GetLibraryByName(string profileId, string name)
        {
            var result = await _mediator.Send(new GetLibraryByNameQuery(profileId, name));

            if (result == null)
            {
                return NotFound();
            }

            var uri = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/library/{result.HubId}/lib/{result.LibraryId}";

            Response.Headers.Add("Location", uri);
            return StatusCode(303);
        }

        // Get Library Content
        [HttpGet("{hubId}/lib/{libId}/content", Name = "Get Library Content")]
        public async Task<IActionResult> GetLibraryContent(string hubId, string libId)
        {
            var result = await _mediator.Send(new GetLibraryContentQuery(hubId, libId));

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        // Add Library Content
        [HttpPut("{hubId}/lib/{libId}/content/{contentId}", Name = "Add Content To Library")]
        public async Task<IActionResult> AddContentToLibrary(string hubId, string libId, string contentId)
        {
            var result = await _mediator.Send(new AddContentToLibraryCommand(hubId, libId, contentId));

            if (!result)
            {
                return NotFound();
            }

            var uri = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/library/{hubId}/lib/{libId}/content";

            Response.Headers.Add("Location", uri);
            return StatusCode(303);
        }

        // Remove Content from Library
        [HttpDelete("{hubId}/lib/{libId}/content/{contentId}", Name = "Remove Content from Library")]
        public async Task<IActionResult> RemoveLibraryContent(string hubId, string libId, string contentId)
        {
            var result = await _mediator.Send(new RemoveContentFromLibraryCommand(hubId, libId, contentId));

            if (!result)
            {
                return NotFound();
            }

            var uri = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/library/{hubId}/lib/{libId}/content";

            Response.Headers.Add("Location", uri);
            return StatusCode(303);
        }

        // Delete Library Hub
        [HttpDelete("{instructorId}/d/{hubId}", Name = "Delete Library Hub")]
        public async Task<IActionResult> DeleteLibraryHub(string hubId, string instructorId)
        {
            var result = await _mediator.Send(new DeleteLibraryHubCommand(hubId, instructorId));

            if(result > 0)
            {
                return NoContent();
            }

            return BadRequest($"No library hubs were deleted");

        }

        // Delete Library from Hub
        [HttpDelete("{hubId}/lib/{libId}", Name = "Delete Library from Hub")]
        public async Task<IActionResult> DeleteLibraryFromHub(string hubId, string libId)
        {
            var result = await _mediator.Send(new DeleteLibraryFromHubCommand(hubId, libId));

            if (!result)
            {
                return NotFound();
            }

            return Ok(result);
        }
    }
}