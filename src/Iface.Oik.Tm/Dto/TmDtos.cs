using System;

namespace Iface.Oik.Tm.Dto
{
  public class TmChannelDto
  {
    public int    ChannelId { get; set; }
    public string Name      { get; set; }
  }


  public class TmRtuDto
  {
    public int    ChannelId { get; set; }
    public int    RtuId     { get; set; }
    public string Name      { get; set; }
  }


  public class TmStatusDto
  {
    public short     VCode      { get; set; }
    public int       Flags      { get; set; }
    public short     VS2        { get; set; }
    public DateTime? ChangeTime { get; set; }
  }


  public class TmStatusPropertiesDto
  {
    public string Name         { get; set; }
    public short  VImportance  { get; set; }
    public short  VNormalState { get; set; }
    public short  ClassId      { get; set; }
    public string ClText0      { get; set; }
    public string ClText1      { get; set; }
    public string ClBreakText  { get; set; }
    public string ClMalfunText { get; set; }
    public string ClFlAName    { get; set; }
    public string ClFlBName    { get; set; }
    public string ClFlCName    { get; set; }
    public string ClFlDName    { get; set; }
    public string ClFlAText0   { get; set; }
    public string ClFlAText1   { get; set; }
    public string ClFlBText0   { get; set; }
    public string ClFlBText1   { get; set; }
    public string ClFlCText0   { get; set; }
    public string ClFlCText1   { get; set; }
    public string ClFlDText0   { get; set; }
    public string ClFlDText1   { get; set; }
  }


  public class TmStatusTmTreeDto
  {
    public short     Ch           { get; set; }
    public short     Rtu          { get; set; }
    public short     Point        { get; set; }
    public string    Name         { get; set; }
    public short     VImportance  { get; set; }
    public short     VNormalState { get; set; }
    public short     ClassId      { get; set; }
    public string    ClText0      { get; set; }
    public string    ClText1      { get; set; }
    public string    ClBreakText  { get; set; }
    public string    ClMalfunText { get; set; }
    public string    ClFlAName    { get; set; }
    public string    ClFlBName    { get; set; }
    public string    ClFlCName    { get; set; }
    public string    ClFlDName    { get; set; }
    public string    ClFlAText0   { get; set; }
    public string    ClFlAText1   { get; set; }
    public string    ClFlBText0   { get; set; }
    public string    ClFlBText1   { get; set; }
    public string    ClFlCText0   { get; set; }
    public string    ClFlCText1   { get; set; }
    public string    ClFlDText0   { get; set; }
    public string    ClFlDText1   { get; set; }
    public short     VCode        { get; set; }
    public int       Flags        { get; set; }
    public short     VS2          { get; set; }
    public DateTime? ChangeTime   { get; set; }
  }


  public class TmAnalogDto
  {
    public float     VVal       { get; set; }
    public int       Flags      { get; set; }
    public DateTime? ChangeTime { get; set; }
  }


  public class TmAnalogPropertiesDto
  {
    public string Name     { get; set; }
    public string VUnit    { get; set; }
    public string VFormat  { get; set; }
    public short  ClassId  { get; set; }
    public string Provider { get; set; }
  }


  public class TmAnalogTmTreeDto
  {
    public short     Ch         { get; set; }
    public short     Rtu        { get; set; }
    public short     Point      { get; set; }
    public string    Name       { get; set; }
    public string    VUnit      { get; set; }
    public string    VFormat    { get; set; }
    public short     ClassId    { get; set; }
    public string    Provider   { get; set; }
    public float     VVal       { get; set; }
    public int       Flags      { get; set; }
    public DateTime? ChangeTime { get; set; }
  }


  public class TmAlarmDto
  {
    public short  AlarmId    { get; set; }
    public string AlarmName  { get; set; }
    public float  CmpVal     { get; set; }
    public short  CmpSign    { get; set; }
    public short  Importance { get; set; }
    public short  InUse      { get; set; }
    public bool   Active     { get; set; }
    public int    Tma        { get; set; }
  }


  public class TmEventDto
  {
    public byte[]    Elix         { get; set; }
    public DateTime? UpdateTime   { get; set; }
    public string    RecText      { get; set; }
    public string    Name         { get; set; }
    public string    RecStateText { get; set; }
    public short     RecType      { get; set; }
    public string    RecTypeName  { get; set; }
    public string    UserName     { get; set; }
    public short     Importance   { get; set; }
    public int       Tma          { get; set; }
    public string    TmaStr       { get; set; }
    public short?    TmType       { get; set; }
    public string    TmTypeName   { get; set; }
    public short?    ClassId      { get; set; }
    public DateTime? AckTime      { get; set; }
    public string    AckUser      { get; set; }
  }


  public class TmAlertDto
  {
    public byte[]    AlertId    { get; set; }
    public short     Importance { get; set; }
    public bool      Active     { get; set; }
    public bool      Unack      { get; set; }
    public DateTime? OnTime     { get; set; }
    public DateTime? OffTime    { get; set; }
    public string    TypeName   { get; set; }
    public string    Name       { get; set; }
    public string    ValueText  { get; set; }
    public DateTime? CurTime    { get; set; }
    public float     CurValue   { get; set; }
    public int       Tma        { get; set; }
    public short?    TmType     { get; set; }
    public short?    ClassId    { get; set; }
    
    public float[]    MsValues { get; set; }
    public DateTime[] MsTimes  { get; set; }
    public short[]    MsFlags  { get; set; }
  }


  public class TmAnalogMicroSeriesDto
  {
    public float[]    Values { get; set; }
    public DateTime[] Times  { get; set; }
    public short[]    Flags  { get; set; }
  }
}