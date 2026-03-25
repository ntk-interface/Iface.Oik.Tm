using System.Collections.Generic;
using System.Threading.Tasks;
using AspWebApi.Model;
using Iface.Oik.Tm.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace AspWebApiTask.Controllers
{
  [Route("api/alarms")]
  [ApiController]
  public class AlarmsController : Controller
  {
    private readonly IOikDataApi _api;


    public AlarmsController(IOikDataApi api)
    {
      _api = api;
    }


    [HttpGet]
    public async Task<IActionResult> Index()
    {
      var alarms = await _api.GetPresentAlarms();
      
      return Ok(alarms.Adapt<List<TmAlarmDto>>());
    }
  }
}