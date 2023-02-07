using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AspWebApiTask.Controllers
{
  [Route("api/time")]
  [ApiController]
  public class TimeController : Controller
  {
    private readonly IOikDataApi _api;


    public TimeController(IOikDataApi api)
    {
      _api = api;
    }


    [HttpGet]
    public async Task<IActionResult> Index()
    {
      var time = await _api.GetSystemTimeString();
      if (string.IsNullOrEmpty(time))
      {
        return StatusCode(StatusCodes.Status503ServiceUnavailable);
      }
      return Ok(time);
    } 
  }
}