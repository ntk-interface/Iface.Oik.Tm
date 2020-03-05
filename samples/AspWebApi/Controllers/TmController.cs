using System;
using System.Collections.Generic;
using System.Linq;
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


    [HttpGet]
    public async Task<IActionResult> ShowList([FromQuery(Name = "s")] string statuses,
                                              [FromQuery(Name = "a")] string analogs)
    {
      List<TmStatus> tmStatuses;
      List<TmAnalog> tmAnalogs;

      try
      {
        tmStatuses = statuses?.Split(',')
                             .Select(tmAddrString => new TmStatus(TmAddr.Parse(tmAddrString, TmType.Status)))
                             .ToList();
        tmAnalogs = analogs?.Split(',')
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
        s = _mapper.Map<IEnumerable<TmStatusDto>>(tmStatuses),
        a = _mapper.Map<IEnumerable<TmAnalogDto>>(tmAnalogs),
      });
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