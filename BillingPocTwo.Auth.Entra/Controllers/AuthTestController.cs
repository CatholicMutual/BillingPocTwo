using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillingPocTwo.Auth.Entra.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpGet("test")]
        [Authorize]
        public IActionResult Test()
        {
            return Ok("Authentication successful!");
        }
    }
}
