using System.Collections.Generic;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.Tm.Services
{
  public class TmTelecontrolService : ITmTelecontrolService
  {
    private readonly ICommonInfrastructure _infr;
    private readonly IOikDataApi           _api;


    public TmTelecontrolService(ICommonInfrastructure infr,
                                IOikDataApi           api)
    {
      _api  = api;
      _infr = infr;
    }


    public TmTelecontrolValidationResult Validate(TmStatus tmStatus)
    {
      if (!tmStatus.HasTelecontrol)
      {
        return TmTelecontrolValidationResult.StatusHasNoTelecontrol;
      }
      if (!_infr.TmUserInfo.HasAccess(TmUserPermissions.Telecontrol))
      {
        return TmTelecontrolValidationResult.Forbidden;
      }
      if (tmStatus.IsUnreliable ||
          tmStatus.IsInvalid    ||
          tmStatus.IsS2Failure  ||
          tmStatus.IsManuallyBlocked)
      {
        return TmTelecontrolValidationResult.StatusIsUnreliable;
      }

      return TmTelecontrolValidationResult.Ok;
    }


    public async Task<(bool, IReadOnlyCollection<TmControlScriptCondition>)> CheckScript(TmStatus tmStatus)
    {
      return await _api.CheckTelecontrolScript(tmStatus);
    }


    public async Task<(bool, IReadOnlyCollection<TmControlScriptCondition>)> CheckScriptExplicitly(TmStatus tmStatus,
                                                                                                   int
                                                                                                     explicitNewStatus)
    {
      return await _api.CheckTelecontrolScriptExplicitly(tmStatus, explicitNewStatus);
    }


    public bool CanOverrideScript()
    {
      return _infr.TmUserInfo.HasAccess(TmUserPermissions.OverrideControlScript);
    }


    public async Task<TmTelecontrolResult> Execute(TmStatus tmStatus, bool overrideScript = false)
    {
      if (overrideScript)
      {
        if (!CanOverrideScript())
        {
          return TmTelecontrolResult.ScriptError;
        }
        await _api.OverrideTelecontrolScript();
      }
      return await _api.Telecontrol(tmStatus);
    }


    public async Task<TmTelecontrolResult> ExecuteExplicitly(TmStatus tmStatus,
                                                             int      explicitNewStatus,
                                                             bool     overrideScript = false)
    {
      if (overrideScript)
      {
        if (!CanOverrideScript())
        {
          return TmTelecontrolResult.ScriptError;
        }
        await _api.OverrideTelecontrolScript();
      }
      return await _api.TelecontrolExplicitly(tmStatus, explicitNewStatus);
    }
  }
}