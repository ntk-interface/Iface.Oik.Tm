using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AspWebApiTask.Controllers
{
  [Route("api/user")]
  [ApiController]
  public class UserController : Controller
  {
    private readonly ICommonInfrastructure _infr;


    public UserController(ICommonInfrastructure infr)
    {
      _infr = infr;
    }


    [HttpGet]
    public async Task<IActionResult> Index()
    {
      return Ok(_infr.TmUserInfo);
    }


    [HttpGet("name")]
    public async Task<IActionResult> ShowName()
    {
      return Ok(_infr.TmUserInfo?.Name);
    }


    [HttpGet("id")]
    public async Task<IActionResult> ShowId()
    {
      return Ok(_infr.TmUserInfo?.Id);
    }


    [HttpGet("group")]
    public async Task<IActionResult> ShowGroup()
    {
      return Ok(_infr.TmUserInfo?.GroupId);
    }
  }
}