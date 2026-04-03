namespace Iface.Oik.Tm.Native.Dto;

public class TagPropsAndClassData
{
  public string Name      { get; set; } = string.Empty;
  public string ClassName { get; init; } = string.Empty;
} 

public class StatusPropsAndClassData : TagPropsAndClassData
{
  public string CaptionOn          { get; init; } = string.Empty;
  public string CaptionOff         { get; init; } = string.Empty;
  public string CaptionBreak       { get; init; } = string.Empty;
  public string CaptionMalfunction { get; init; } = string.Empty;
}