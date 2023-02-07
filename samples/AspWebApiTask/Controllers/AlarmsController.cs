using System.Collections.Generic;
using System.Threading.Tasks;
using AspWebApi.Model;
using AutoMapper;
using Iface.Oik.Tm.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AspWebApiTask.Controllers
{
  [Route("api/alarms")]
  [ApiController]
  public class AlarmsController : Controller
  {
    private readonly IOikDataApi _api;
    private readonly IMapper     _mapper;


    public AlarmsController(IOikDataApi api, IMapper mapper)
    {
      _api    = api;
      _mapper = mapper;
    }


    [HttpGet]
    public async Task<IActionResult> Index()
    {
      var alarms = await _api.GetPresentAlarms();
      
      return Ok(_mapper.Map<IEnumerable<TmAlarmDto>>(alarms));
    }
  }
}