using System.Collections.Generic;
using System.Threading.Tasks;
using AspWebApi.Model;
using AutoMapper;
using Iface.Oik.Tm.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AspWebApi.Controllers
{
  [Route("api/aps")]
  [ApiController]
  public class ApsController : Controller
  {
    private readonly IOikDataApi _api;
    private readonly IMapper     _mapper;


    public ApsController(IOikDataApi api, IMapper mapper)
    {
      _api    = api;
      _mapper = mapper;
    }


    [HttpGet]
    public async Task<IActionResult> Index()
    {
      var statuses = await _api.GetPresentAps();

      return Ok(_mapper.Map<IEnumerable<TmStatusDto>>(statuses));
    }
  }
}