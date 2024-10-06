using Microsoft.AspNetCore.Mvc;

namespace RestServerSprint_2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Controller : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "Hello World";
        }
    }
}