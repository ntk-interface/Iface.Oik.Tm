namespace Iface.Oik.Tm.Native.Dto;

public record struct TmImpulseArchiveDto(byte   Type, // ImpulseArchiveFlags
                                         uint   UnixTime,
                                         ushort Milliseconds,
                                         uint   Flags, // TmFlags
                                         float  Value);