using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Api;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Api;

public partial class TmsApi
{
  public async Task<IReadOnlyCollection<TmChannel>> GetTmTreeChannels()
  {
    var result = new List<TmChannel>();

    await Task.Run(() =>
    {
      var itemsIndexes = new ushort[255];
      var count = TmNative.tmcEnumObjects(_cid, (ushort)TmNativeDefs.TmDataTypes.Channel, 255,
                                          itemsIndexes, 0, 0, 0);

      for (int i = 0; i < count; i++)
      {
        var channelId = itemsIndexes[i];
        result.Add(new TmChannel(channelId, GetChannelNameSync(channelId)));
      }
    }).ConfigureAwait(false);

    return result;
  }


  public async Task<IReadOnlyCollection<TmRtu>> GetTmTreeRtus(int channelId)
  {
    if (channelId < 0 || channelId > 254) return null;

    var result = new List<TmRtu>();

    await Task.Run(() =>
    {
      var itemsIndexes = new ushort[255];
      var count = TmNative.tmcEnumObjects(_cid, (ushort)TmNativeDefs.TmDataTypes.Rtu, 255,
                                          itemsIndexes, (short)channelId, 0, 0);

      for (int i = 0; i < count; i++)
      {
        var rtuId = itemsIndexes[i];
        result.Add(new TmRtu(channelId,
                             rtuId,
                             GetRtuNameSync(channelId, rtuId)));
      }
    }).ConfigureAwait(false);

    return result;
  }


  public async Task<IReadOnlyCollection<TmStatus>> GetTmTreeStatuses(int channelId, int rtuId)
  {
    if (channelId < 0 || channelId > 254 ||
        rtuId     < 1 || rtuId     > 255)
    {
      return null;
    }

    var result     = new List<TmStatus>();
    var startIndex = 0;
    while (true)
    {
      var currentStartIndex = (short)startIndex;
      var itemsIndexes      = new ushort[255];
      var count = await Task.Run(() => TmNative.tmcEnumObjects(_cid,
                                                               (ushort)TmNativeDefs.TmDataTypes.Status,
                                                               255,
                                                               itemsIndexes,
                                                               (short)channelId,
                                                               (short)rtuId,
                                                               currentStartIndex))
                            .ConfigureAwait(false);
      if (count == 0)
      {
        break;
      }

      for (var i = 0; i < count; i++)
      {
        result.Add(new TmStatus(channelId, rtuId, itemsIndexes[i]));
      }

      startIndex = itemsIndexes[count - 1] + 1;
      // todo name, properties?
    }

    return result;
  }


  public async Task<IReadOnlyCollection<TmAnalog>> GetTmTreeAnalogs(int channelId, int rtuId)
  {
    if (channelId < 0 || channelId > 254 ||
        rtuId     < 1 || rtuId     > 255)
    {
      return null;
    }

    var result     = new List<TmAnalog>();
    var startIndex = 0;
    while (true)
    {
      var currentStartIndex = (short)startIndex;
      var itemsIndexes      = new ushort[255];
      var count = await Task.Run(() => TmNative.tmcEnumObjects(_cid,
                                                               (ushort)TmNativeDefs.TmDataTypes.Analog,
                                                               255,
                                                               itemsIndexes,
                                                               (short)channelId,
                                                               (short)rtuId,
                                                               currentStartIndex))
                            .ConfigureAwait(false);
      if (count == 0)
      {
        break;
      }

      for (var i = 0; i < count; i++)
      {
        result.Add(new TmAnalog(channelId, rtuId, itemsIndexes[i]));
      }

      startIndex = itemsIndexes[count - 1] + 1;
      // todo name, properties?
    }

    return result;
  }


  public async Task<IReadOnlyCollection<TmAccum>> GetTmTreeAccums(int channelId, int rtuId)
  {
    if (channelId < 0 || channelId > 254 ||
        rtuId     < 1 || rtuId     > 255)
    {
      return null;
    }

    var   result     = new List<TmAccum>();
    var startIndex = 0;
    while (true)
    {
      var currentStartIndex = (short)startIndex;
      var itemsIndexes      = new ushort[255];
      var count = await Task.Run(() => TmNative.tmcEnumObjects(_cid,
                                                               (ushort)TmNativeDefs.TmDataTypes.Accum,
                                                               255,
                                                               itemsIndexes,
                                                               (short)channelId,
                                                               (short)rtuId,
                                                               currentStartIndex))
                            .ConfigureAwait(false);
      if (count == 0)
      {
        break;
      }

      for (var i = 0; i < count; i++)
      {
        result.Add(new TmAccum(channelId, rtuId, itemsIndexes[i]));
      }

      startIndex = itemsIndexes[count - 1] + 1;
      // todo name, properties?
    }

    return result;
  }


  public async Task<string> GetChannelName(int channelId)
  {
    return await Task.Run(() => GetChannelNameSync(channelId)).ConfigureAwait(false);
  }


  private string GetChannelNameSync(int channelId)
  {
    if (channelId < 0 || channelId > 254) return null;

    Span<byte> buf = stackalloc byte[1024];
    TmNative.tmcGetObjectName(_cid, (ushort)TmNativeDefs.TmDataTypes.Channel, (short)channelId, 0, 0,
                              buf, buf.Length);

    return EncodingUtil.BytesToString(buf);
  }


  public async Task<string> GetRtuName(int channelId, int rtuId)
  {
    return await Task.Run(() => GetRtuNameSync(channelId, rtuId)).ConfigureAwait(false);
  }


  private string GetRtuNameSync(int channelId, int rtuId)
  {
    if (channelId < 0 || channelId > 254 ||
        rtuId     < 1 || rtuId     > 255)
    {
      return null;
    }

    Span<byte> buf = stackalloc byte[1024];
    TmNative.tmcGetObjectName(_cid, (ushort)TmNativeDefs.TmDataTypes.Rtu, (short)channelId, (short)rtuId, 0,
                              buf, buf.Length);

    return EncodingUtil.BytesToString(buf);
  }
  
  
  public async Task<IReadOnlyCollection<TmClassStatus>> GetStatusesClasses()
    {
      const int tmcAddrsLimit = 127;

      var tmClasses = new List<TmClassStatus>();

      var tmcAddrs = Enumerable.Range(1, tmcAddrsLimit).Select(x => new TmNativeDefs.TAdrTm
      {
        Ch    = -1,
        RTU   = -1,
        Point = (short)x
      }).ToArray();

      var classDataPtr = await Task.Run(() => TmNative.tmcGetStatusClassData(_cid, tmcAddrsLimit, tmcAddrs))
                                   .ConfigureAwait(false);

      var singleClassDataPtr = Marshal.PtrToStructure<IntPtr>(classDataPtr);

      if (singleClassDataPtr == IntPtr.Zero)
      {
        return Array.Empty<TmClassStatus>();
      }

      for (var i = 0; i <= tmcAddrsLimit; i++)
      {
        var tmcClassDataStr = TmNativeUtil.GetCStringFromIntPtr(singleClassDataPtr);

        if (tmcClassDataStr == string.Empty)
        {
          singleClassDataPtr = IntPtr.Add(singleClassDataPtr, 1);
          continue;
        }

        var tmClassId    = 0;
        var tmClassName  = "";
        var tmClassFlags = 0;

        tmcClassDataStr.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                       .ForEach(property =>
                                {
                                  var kvp = property.Split('=');
                                  if (kvp.Length != 2)
                                  {
                                    return;
                                  }

                                  if (kvp[0] == "ClassNumber")
                                  {
                                    int.TryParse(kvp[1], out tmClassId);
                                  }

                                  if (kvp[0] == "ClassName")
                                  {
                                    tmClassName = kvp[1];
                                  }

                                  if (kvp[0] == "ClassFlags")
                                  {
                                    int.TryParse(kvp[1], NumberStyles.HexNumber, null, out tmClassFlags);
                                  }
                                });

        if (tmClassId != 0)
        {
          tmClasses.Add(new TmClassStatus(tmClassId, tmClassName, tmClassFlags));
        }

        singleClassDataPtr = IntPtr.Add(singleClassDataPtr, tmcClassDataStr.Length + 1);
      }

      return tmClasses;
    }


    public async Task<IReadOnlyCollection<TmClassAnalog>> GetAnalogsClasses()
    {
      const int tmcAddrsLimit = 127;
      var       tmAnalogs     = new List<TmClassAnalog>();

      var tmcAddrs = Enumerable.Range(1, tmcAddrsLimit).Select(x => new TmNativeDefs.TAdrTm
      {
        Ch    = -1,
        RTU   = -1,
        Point = (short)x
      }).ToArray();

      var classDataPtr = await Task.Run(() => TmNative.tmcGetAnalogClassData(_cid, tmcAddrsLimit, tmcAddrs))
                                   .ConfigureAwait(false);
      var singleClassDataPtr = Marshal.PtrToStructure<IntPtr>(classDataPtr);

      if (singleClassDataPtr == IntPtr.Zero)
      {
        return Array.Empty<TmClassAnalog>();
      }

      for (var i = 0; i <= tmcAddrsLimit; i++)
      {
        var tmcClassDataStr = TmNativeUtil.GetCStringFromIntPtr(singleClassDataPtr);

        if (tmcClassDataStr == string.Empty)
        {
          singleClassDataPtr = IntPtr.Add(singleClassDataPtr, 1);
          continue;
        }

        var tmClassId   = 0;
        var tmClassName = "";

        tmcClassDataStr.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                       .ForEach(property =>
                                {
                                  var kvp = property.Split('=');
                                  if (kvp.Length != 2)
                                  {
                                    return;
                                  }

                                  if (kvp[0] == "ClassNumber")
                                  {
                                    int.TryParse(kvp[1], out tmClassId);
                                  }

                                  if (kvp[0] == "ClassName")
                                  {
                                    tmClassName = kvp[1];
                                  }
                                });
        if (tmClassId != 0)
        {
          tmAnalogs.Add(new TmClassAnalog(tmClassId, tmClassName));
        }

        singleClassDataPtr = IntPtr.Add(singleClassDataPtr, tmcClassDataStr.Length + 1);
      }

      return tmAnalogs;
    }
}