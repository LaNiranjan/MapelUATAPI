using MapelRestAPI.Requests;
using MapelRestAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace MapelRestAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserAuthController : Controller
    {
        private readonly UserAuthHandler _handler;

        public UserAuthController(UserAuthHandler handler)
        {
            _handler = handler;
        }

        [HttpPost("get-by-email")]
        public async Task<IActionResult> GetByEmail([FromBody] EntraAuthExtensionRequest request)
        {
            if (request?.email == null)
                return BadRequest("Email is required.");

            var result = await _handler.HandleAsync(request.email);

            // Return in Entra expected format:
            return Ok(new
            {
                action = "Continue",
                extension_userAppData = result // Example attribute
            });

            //var result = await _handler.HandleAsync(request.Email);
            //return Ok(result);
        }
    }
}
