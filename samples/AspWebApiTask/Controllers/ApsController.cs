using System.Collections.Generic;
using System.Threading.Tasks;
using AspWebApi.Model;
using Iface.Oik.Tm.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace AspWebApiTask.Controllers
{
  [Route("api/aps")]
  [ApiController]
  public class ApsController : Controller
  {
    private readonly IOikDataApi _api;


    public ApsController(IOikDataApi api)
    {
      _api = api;
    }


    [HttpGet]
    public async Task<IActionResult> Index()
    {
      var statuses = await _api.GetPresentAps();

      return Ok(statuses.Adapt<List<TmStatusDto>>());
    }
  }
}