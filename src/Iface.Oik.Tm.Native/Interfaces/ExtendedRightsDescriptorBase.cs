using System.Collections.Generic;

namespace Iface.Oik.Tm.Native.Interfaces;

public abstract class ExtendedRightsDescriptorBase<T> 
  where T : ExtendedRightBase
{
  public byte    DoUserId   { get; set; }
  public uint    MaxUserID  { get; set; }
  public byte    DoGroup    { get; set; }
  public byte    DoKeyId    { get; set; }
  public byte    DoUserNick { get; set; }
  public byte    DoUserPwd  { get; set; }
  public List<T> Rights     { get; set; } = new();
}

public abstract class ExtendedRightBase
{
  public bool                       IsHeader    { get; set; }
  public byte                       ByteIndex   { get; set; }
  public Dictionary<string, string> Description { get; set; } = new();
}