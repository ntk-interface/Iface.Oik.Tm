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
      return await _api.CheckTelecontrolScript(tmStatus).ConfigureAwait(false);
    }


    public async Task<(bool, IReadOnlyCollection<TmControlScriptCondition>)> CheckScriptExplicitly(TmStatus tmStatus,
                                                                                                   int
                                                                                                     explicitNewStatus)
    {
      return await _api.CheckTelecontrolScriptExplicitly(tmStatus, explicitNewStatus).ConfigureAwait(false);
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
        await _api.OverrideTelecontrolScript().ConfigureAwait(false);
      }
      return await _api.Telecontrol(tmStatus).ConfigureAwait(false);
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
        await _api.OverrideTelecontrolScript().ConfigureAwait(false);
      }
      return await _api.TelecontrolExplicitly(tmStatus, explicitNewStatus).ConfigureAwait(false);
    }
    
    
    public TmTeleregulateValidationResult Validate(TmAnalog tmAnalog)
    {
      if (!tmAnalog.HasTeleregulation)
      {
        return TmTeleregulateValidationResult.AnalogHasNoTeleregulation;
      }
      if (!_infr.TmUserInfo.HasAccess(TmUserPermissions.Telecontrol))
      {
        return TmTeleregulateValidationResult.Forbidden;
      }
      if (tmAnalog.IsUnreliable ||
          tmAnalog.IsInvalid    ||
          tmAnalog.IsManuallyBlocked)
      {
        return TmTeleregulateValidationResult.AnalogIsUnreliable;
      }

      return TmTeleregulateValidationResult.Ok;
    }


    public async Task<(bool, IReadOnlyCollection<TmControlScriptCondition>)> CheckScript(TmAnalog tmAnalog)
    {
      return await _api.CheckTeleregulationScript(tmAnalog).ConfigureAwait(false);
    }


    public async Task<TmTelecontrolResult> TeleregulateByStepUp(TmAnalog analog, bool overrideScript = false)
    {
      if (overrideScript)
      {
        if (!CanOverrideScript())
        {
          return TmTelecontrolResult.ScriptError;
        }
        await _api.OverrideTelecontrolScript().ConfigureAwait(false);
      }
      return await _api.TeleregulateByStepUp(analog).ConfigureAwait(false);
    }


    public async Task<TmTelecontrolResult> TeleregulateByStepDown(TmAnalog analog, bool overrideScript = false)
    {
      if (overrideScript)
      {
        if (!CanOverrideScript())
        {
          return TmTelecontrolResult.ScriptError;
        }
        await _api.OverrideTelecontrolScript().ConfigureAwait(false);
      }
      return await _api.TeleregulateByStepDown(analog).ConfigureAwait(false);
    }


    public async Task<TmTelecontrolResult> TeleregulateByCode(TmAnalog analog, int code, bool overrideScript = false)
    {
      if (overrideScript)
      {
        if (!CanOverrideScript())
        {
          return TmTelecontrolResult.ScriptError;
        }
        await _api.OverrideTelecontrolScript().ConfigureAwait(false);
      }
      return await _api.TeleregulateByCode(analog, code).ConfigureAwait(false);
    }


    public async Task<TmTelecontrolResult> TeleregulateByValue(TmAnalog analog, float value, bool overrideScript = false)
    {
      if (overrideScript)
      {
        if (!CanOverrideScript())
        {
          return TmTelecontrolResult.ScriptError;
        }
        await _api.OverrideTelecontrolScript().ConfigureAwait(false);
      }
      return await _api.TeleregulateByValue(analog, value).ConfigureAwait(false);
    }
  }
}