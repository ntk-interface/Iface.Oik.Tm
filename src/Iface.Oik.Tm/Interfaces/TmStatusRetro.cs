using System;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
    public class TmStatusRetro
    {
        public short    Status { get; }
        public TmFlags  Flags  { get; }
        public DateTime Time   { get; }
        
        public TmStatusRetro(short status,
                             short flags,
                             long  timestamp)
        {
            Status = status;
            Flags  = (TmFlags)flags;
            Time   = DateUtil.GetDateTimeFromTimestamp(timestamp);
        }
    }
}