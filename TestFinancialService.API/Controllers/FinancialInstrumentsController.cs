using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TestFinancialService.API.Services;

namespace TestFinancialService.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class FinancialInstrumentsController(TickersService tickersService) : ControllerBase
{
    [HttpGet]
    public ActionResult<List<string>> GetAvailable()
    {
        return Ok(tickersService.Available);
    }

    [HttpGet]
    public ActionResult<double?> GetCurrentPrice([Required] string ticker)
    {
        var result = tickersService.GetPrice(ticker);
        if (result == null)
            return NotFound();

        return Ok(result);
    }
}
