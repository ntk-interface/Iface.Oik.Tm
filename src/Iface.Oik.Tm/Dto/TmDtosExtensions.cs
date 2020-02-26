namespace Iface.Oik.Tm.Dto
{
  public static class TmDtosExtensions
  {
    public static TmStatusDto AdaptToTmStatusDto(this TmStatusTmTreeDto dto)
    {
      return new TmStatusDto
      {
        VCode      = dto.VCode,
        Flags      = dto.Flags,
        VS2        = dto.VS2,
        ChangeTime = dto.ChangeTime,
      };
    }


    public static TmStatusPropertiesDto AdaptToTmStatusPropertiesDto(this TmStatusTmTreeDto dto)
    {
      return new TmStatusPropertiesDto
      {
        Name         = dto.Name,
        VImportance  = dto.VImportance,
        VNormalState = dto.VNormalState,
        ClassId      = dto.ClassId,
        ClText0      = dto.ClText0,
        ClText1      = dto.ClText1,
        ClBreakText  = dto.ClBreakText,
        ClMalfunText = dto.ClMalfunText,
        ClFlAName    = dto.ClFlAName,
        ClFlBName    = dto.ClFlBName,
        ClFlCName    = dto.ClFlCName,
        ClFlDName    = dto.ClFlDName,
        ClFlAText0   = dto.ClFlAText0,
        ClFlAText1   = dto.ClFlAText1,
        ClFlBText0   = dto.ClFlBText0,
        ClFlBText1   = dto.ClFlBText1,
        ClFlCText0   = dto.ClFlCText0,
        ClFlCText1   = dto.ClFlCText1,
        ClFlDText0   = dto.ClFlDText0,
        ClFlDText1   = dto.ClFlDText1,
      };
    }


    public static TmAnalogDto AdaptToTmAnalogDto(this TmAnalogTmTreeDto dto)
    {
      return new TmAnalogDto
      {
        VVal       = dto.VVal,
        Flags      = dto.Flags,
        ChangeTime = dto.ChangeTime,
      };
    }


    public static TmAnalogPropertiesDto AdaptToTmAnalogPropertiesDto(this TmAnalogTmTreeDto dto)
    {
      return new TmAnalogPropertiesDto
      {
        Name     = dto.Name,
        VUnit    = dto.VUnit,
        VFormat  = dto.VFormat,
        ClassId  = dto.ClassId,
        Provider = dto.Provider,
      };
    }
  }
}