using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Npgsql;

namespace Iface.Oik.Tm.Utils
{
  public static class NpgsqlExtensions
  {
    public static NpgsqlDataReader ExecuteReaderSeq(this NpgsqlCommand cmd)
    {
      return cmd.ExecuteReader(CommandBehavior.SequentialAccess);
    }


    public static async Task<DbDataReader> ExecuteReaderSeqAsync(this NpgsqlCommand cmd)
    {
      return await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess)
                      .ConfigureAwait(false);
    }


    public static string GetStringOrDefault(this DbDataReader reader,
                                            int               ordinal,
                                            string            defaultValue = null)
    {
      if (reader.IsDBNull(ordinal))
      {
        return defaultValue;
      }
      return reader.GetString(ordinal);
    }


    public static DateTime? GetDateTimeWithEpochCheckOrDefault(this DbDataReader reader,
                                                               int               ordinal,
                                                               DateTime?         defaultValue = null)
    {
      if (reader.IsDBNull(ordinal))
      {
        return defaultValue;
      }
      var dt = reader.GetDateTime(ordinal);
      if (dt.IsEpoch())
      {
        return defaultValue;
      }
      return dt;
    }


    public static int? GetInt32OrDefault(this DbDataReader reader,
                                         int               ordinal,
                                         int?              defaultValue = null)
    {
      if (reader.IsDBNull(ordinal))
      {
        return defaultValue;
      }
      return reader.GetInt32(ordinal);
    }


    public static short? GetInt16OrDefault(this DbDataReader reader,
                                           int               ordinal,
                                           short?            defaultValue = null)
    {
      if (reader.IsDBNull(ordinal))
      {
        return defaultValue;
      }
      return reader.GetInt16(ordinal);
    }
  }
}