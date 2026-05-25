using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.Tm.Api;

public partial class OikDataApi
{
  public async Task<IReadOnlyCollection<TmAlert>> GetAlerts(PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Sql,
                         null,
                         () => _sql.GetAlerts())
            .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<TmAlert>> GetAlertsWithAnalogMicroSeries(PreferApi prefer = PreferApi.Auto)
  {
    if (!_serverFeatures.AreMicroSeriesEnabled)
    {
      return await GetAlerts(prefer).ConfigureAwait(false);
    }
    return await Execute(prefer,
                         PreferApi.Sql,
                         null,
                         () => _sql.GetAlertsWithAnalogMicroSeries())
            .ConfigureAwait(false);
  }


  public async Task<bool> RemoveAlert(TmAlert alert, PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.RemoveAlert(alert),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<bool> RemoveAlert(byte[] alertId, PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.RemoveAlert(alertId),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<bool> RemoveAlerts(IEnumerable<TmAlert> alerts, PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.RemoveAlerts(alerts),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<TmStatus>> GetPresentAps(PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Sql,
                         () => _tms.GetPresentAps(),
                         () => _sql.GetPresentAps())
            .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<TmStatus>> GetUnackedAps(PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Sql,
                         null,
                         () => _sql.GetUnackedAps())
            .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<TmStatus>> GetAbnormalStatuses(PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Sql,
                         null,
                         () => _sql.GetAbnormalStatuses())
            .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<TmAlarm>> GetPresentAlarms(PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Sql,
                         null,
                         () => _sql.GetPresentAlarms())
            .ConfigureAwait(false);
  }


  public async Task<bool> HasPresentAps(PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Sql,
                         null,
                         () => _sql.HasPresentAps())
            .ConfigureAwait(false);
  }


  public async Task<bool> HasPresentAlarms(PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Sql,
                         null,
                         () => _sql.HasPresentAlarms())
            .ConfigureAwait(false);
  }
}