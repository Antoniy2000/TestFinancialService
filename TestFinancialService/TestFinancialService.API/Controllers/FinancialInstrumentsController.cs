using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace TestFinancialService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class FinancialInstrumentsController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAvailable()
        {
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrentPrice([Required] string pair)
        {
            return Ok();
        }
    }
}
