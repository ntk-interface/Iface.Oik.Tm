using System.Threading.Tasks;
using AspWebApi.Model;
using AutoMapper;
using Iface.Oik.Tm.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AspWebApi.Controllers
{
  [Route("api/tm")]
  [ApiController]
  public class TmController : Controller
  {
    private readonly IOikDataApi _api;
    private readonly IMapper     _mapper;


    public TmController(IOikDataApi api, IMapper mapper)
    {
      _api    = api;
      _mapper = mapper;
    }


    [HttpGet("ts/{id}")]
    public async Task<IActionResult> ShowTs(string id)
    {
      if (!TmAddr.TryParse(id, out var tmAddr, TmType.Status))
      {
        return NotFound();
      }
      var tmStatus = new TmStatus(tmAddr);
      await Task.WhenAll(_api.UpdateTagPropertiesAndClassData(tmStatus),
                         _api.UpdateStatus(tmStatus));

      return Ok(_mapper.Map<TmStatusDto>(tmStatus));
    }


    [HttpGet("ti/{id}")]
    public async Task<IActionResult> ShowTi(string id)
    {
      if (!TmAddr.TryParse(id, out var tmAddr, TmType.Analog))
      {
        return NotFound();
      }
      var tmAnalog = new TmAnalog(tmAddr);
      await Task.WhenAll(_api.UpdateTagPropertiesAndClassData(tmAnalog),
                         _api.UpdateAnalog(tmAnalog));

      return Ok(_mapper.Map<TmAnalogDto>(tmAnalog));
    }
  }
}