using System.Collections.Generic;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.Tm.Api;

public partial class OikDataApi
{
  public async Task<IReadOnlyCollection<string>> GetFilesInDirectory(string    path,
                                                                     PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.GetFilesInDirectory(path),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<bool> DownloadFile(string    remotePath,
                                       string    localPath,
                                       PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.DownloadFile(remotePath, localPath),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<string>> GetComtradeDays(PreferApi prefer = PreferApi.Auto)
  {
    if (!_serverFeatures.IsComtradeEnabled)
    {
      return null;
    }

    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.GetComtradeDays(),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<string>> GetComtradeFilesByDay(string    day,
                                                                       PreferApi prefer = PreferApi.Auto)
  {
    if (!_serverFeatures.IsComtradeEnabled)
    {
      return null;
    }

    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.GetComtradeFilesByDay(day),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<bool> DownloadComtradeFile(string    filename,
                                               string    localPath,
                                               PreferApi prefer = PreferApi.Auto)
  {
    if (!_serverFeatures.IsComtradeEnabled)
    {
      return false;
    }

    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.DownloadComtradeFile(filename, localPath),
                         null)
            .ConfigureAwait(false);
  }
}