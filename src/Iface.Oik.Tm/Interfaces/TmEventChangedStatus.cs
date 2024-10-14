namespace Iface.Oik.Tm.Interfaces
{
  public class TmEventChangedStatus
  {
    public short     Status  { get; }
    public TmFlags   Flags   { get; }
    public TmS2Flags S2Flags { get; }
    
    public bool IsOn         => Status > 0;
    public bool IsOff        => Status == 0;
    public bool IsUnreliable => Flags.HasFlag(TmFlags.Unreliable);
    public bool IsInvalid    => Flags.HasFlag(TmFlags.Invalid);
    
    public bool IsIntermediate => S2Flags.HasFlag(TmS2Flags.Intermediate);
    public bool IsBreak        => S2Flags.HasFlag(TmS2Flags.Break) && !IsIntermediate;
    public bool IsMalfunction  => S2Flags.HasFlag(TmS2Flags.Malfunction);
    public bool IsS2Failure    => IsBreak || IsMalfunction || IsIntermediate;
    
    public bool HasProblems => Status < 0 || IsUnreliable || IsInvalid || IsS2Failure;
    
    
    private TmEventChangedStatus(short status, TmFlags flags, TmS2Flags s2Flags)
    {
      Status  = status;
      Flags   = flags;
      S2Flags = s2Flags;
    }


    public static TmEventChangedStatus CreateFromDto(short? code, int? flags, short? s2Flags)
    {
      return new TmEventChangedStatus(code ?? -1,
                                      (TmFlags)((flags ?? 0) & 0x0000_FFFF), // 3 и 4 байты флагов - служебные, не для клиента
                                      (TmS2Flags)(s2Flags ?? 0));
    }
  }
}