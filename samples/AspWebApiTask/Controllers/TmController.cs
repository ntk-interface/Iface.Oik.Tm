using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspWebApi.Model;
using Iface.Oik.Tm.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace AspWebApiTask.Controllers
{
  [Route("api/tm")]
  [ApiController]
  public class TmController : Controller
  {
    private readonly IOikDataApi _api;


    public TmController(IOikDataApi api)
    {
      _api = api;
    }


    [HttpGet]
    public async Task<IActionResult> ShowList([FromQuery(Name = "s")] string statuses,
                                              [FromQuery(Name = "a")] string analogs)
    {
      List<TmStatus> tmStatuses;
      List<TmAnalog> tmAnalogs;

      try
      {
        tmStatuses = statuses.Split(',')
                             .Select(tmAddrString => new TmStatus(TmAddr.Parse(tmAddrString, TmType.Status)))
                             .ToList();
        tmAnalogs = analogs.Split(',')
                           .Select(tmAddrString => new TmAnalog(TmAddr.Parse(tmAddrString, TmType.Analog)))
                           .ToList();
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }

      await Task.WhenAll(_api.UpdateTagsPropertiesAndClassData(tmAnalogs),
                         _api.UpdateAnalogs(tmAnalogs),
                         _api.UpdateTagsPropertiesAndClassData(tmStatuses),
                         _api.UpdateStatuses(tmStatuses));

      return Ok(new
      {
        s = tmStatuses.Adapt<List<TmStatusDto>>(),
        a = tmAnalogs.Adapt<List<TmAnalogDto>>(),
      });
    }


    [HttpGet("ts/{status}")]
    public async Task<IActionResult> ShowTs(string status)
    {
      if (!TmAddr.TryParse(status, out var tmAddr, TmType.Status))
      {
        return NotFound();
      }
      var tmStatus = new TmStatus(tmAddr);
      await Task.WhenAll(_api.UpdateTagPropertiesAndClassData(tmStatus),
                         _api.UpdateStatus(tmStatus));

      return Ok(tmStatus.Adapt<TmStatusDto>());
    }


    [HttpGet("ti/{analog}")]
    public async Task<IActionResult> ShowTi(string analog)
    {
      if (!TmAddr.TryParse(analog, out var tmAddr, TmType.Analog))
      {
        return NotFound();
      }
      var tmAnalog = new TmAnalog(tmAddr);
      await Task.WhenAll(_api.UpdateTagPropertiesAndClassData(tmAnalog),
                         _api.UpdateAnalog(tmAnalog));

      return Ok(tmAnalog.Adapt<TmAnalogDto>());
    }
  }
}