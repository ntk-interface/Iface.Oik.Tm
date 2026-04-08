using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.Tm.Api;

public partial class OikDataApi
{
  public async Task<IReadOnlyCollection<TmAlert>> GetAlerts(PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: false, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      throw new NotImplementedException();
    }
    else if (api == ApiSelection.Sql)
    {
      return await _sql.GetAlerts().ConfigureAwait(false);
    }
    else
    {
      return null;
    }
  }


  public async Task<IReadOnlyCollection<TmAlert>> GetAlertsWithAnalogMicroSeries(PreferApi prefer = PreferApi.Auto)
  {
    if (!_serverFeatures.AreMicroSeriesEnabled)
    {
      return await GetAlerts(prefer).ConfigureAwait(false);
    }

    var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: false, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      throw new NotImplementedException();
    }
    else if (api == ApiSelection.Sql)
    {
      return await _sql.GetAlertsWithAnalogMicroSeries().ConfigureAwait(false);
    }
    else
    {
      return null;
    }
  }


  public async Task<bool> RemoveAlert(TmAlert alert, PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.RemoveAlert(alert).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return false;
    }
  }


  public async Task<bool> RemoveAlert(byte[] alertId, PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.RemoveAlert(alertId).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return false;
    }
  }


  public async Task<bool> RemoveAlerts(IEnumerable<TmAlert> alerts, PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.RemoveAlerts(alerts).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return false;
    }
  }


  public async Task<IReadOnlyCollection<TmStatus>> GetPresentAps(PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: false, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      return await _tms.GetPresentAps().ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      return await _sql.GetPresentAps().ConfigureAwait(false);
    }
    else
    {
      return null;
    }
  }


  public async Task<IReadOnlyCollection<TmStatus>> GetUnackedAps(PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: false, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      throw new NotImplementedException();
    }
    else if (api == ApiSelection.Sql)
    {
      return await _sql.GetUnackedAps().ConfigureAwait(false);
    }
    else
    {
      return null;
    }
  }


  public async Task<IReadOnlyCollection<TmStatus>> GetAbnormalStatuses(PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: false, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      throw new NotImplementedException();
    }
    else if (api == ApiSelection.Sql)
    {
      return await _sql.GetAbnormalStatuses().ConfigureAwait(false);
    }
    else
    {
      return null;
    }
  }


  public async Task<IReadOnlyCollection<TmAlarm>> GetPresentAlarms(PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: false, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      throw new NotImplementedException();
    }
    else if (api == ApiSelection.Sql)
    {
      return await _sql.GetPresentAlarms().ConfigureAwait(false);
    }
    else
    {
      return null;
    }
  }


  public async Task<bool> HasPresentAps(PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: false, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      throw new NotImplementedException();
    }
    else if (api == ApiSelection.Sql)
    {
      return await _sql.HasPresentAps().ConfigureAwait(false);
    }
    else
    {
      return false;
    }
  }


  public async Task<bool> HasPresentAlarms(PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: false, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      throw new NotImplementedException();
    }
    else if (api == ApiSelection.Sql)
    {
      return await _sql.HasPresentAlarms().ConfigureAwait(false);
    }
    else
    {
      return false;
    }
  }
}