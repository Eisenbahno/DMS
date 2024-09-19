using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace RestAPI_Sprint_1.Controller
{
    [ApiController]
    [Route("/")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetRoot()
        {
            var response = new { message = "Welcome to the REST API!", statusCode = (int)HttpStatusCode.OK };
            return StatusCode((int)HttpStatusCode.OK, response);
        }
    }
}