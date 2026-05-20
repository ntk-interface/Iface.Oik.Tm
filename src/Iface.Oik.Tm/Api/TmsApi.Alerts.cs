using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Api;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Api;

public partial class TmsApi
{
  public async Task<bool> RemoveAlert(TmAlert alert)
  {
    if (alert == null) return false;

    return await RemoveAlert(alert.Id).ConfigureAwait(false);
  }


  public async Task<bool> RemoveAlert(byte[] alertId)
  {
    if (alertId.IsNullOrEmpty()) return false;

    return await Task.Run(() => TmNative.tmcAlertListRemove(_cid,
                                                            new[]
                                                            {
                                                              new TmNativeDefs.TAlertListId { IData = alertId }
                                                            }))
                     .ConfigureAwait(false);
  }


  public async Task<bool> RemoveAlerts(IEnumerable<TmAlert> alerts)
  {
    if (alerts == null) return false;

    var nativeAlertList = alerts.Select(a => new TmNativeDefs.TAlertListId { IData = a.Id })
                                .ToArray();
    if (nativeAlertList.Length == 0)
    {
      return false;
    }

    return await Task.Run(() => TmNative.tmcAlertListRemove(_cid, nativeAlertList))
                     .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<TmStatus>> GetPresentAps()
  {
    var adrTmList = await Task.Run(() => TmNativeApi.GetPresentAps(_cid))
                              .ConfigureAwait(false);

    var tags = adrTmList.Select(adrTm => new TmStatus(adrTm.Ch, adrTm.Rtu, adrTm.Point))
                        .ToList();

    await UpdateTagsPropertiesAndClassData(tags).ConfigureAwait(false);
    await UpdateStatuses(tags).ConfigureAwait(false);

    return tags;
  }
}