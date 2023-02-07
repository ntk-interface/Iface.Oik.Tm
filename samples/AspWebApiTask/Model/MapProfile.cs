using AutoMapper;
using Iface.Oik.Tm.Interfaces;

namespace AspWebApi.Model
{
  public class MapProfile : Profile
  {
    public MapProfile()
    {
      CreateMap<TmStatus, TmStatusDto>();
      CreateMap<TmAnalog, TmAnalogDto>();
      CreateMap<TmAlarm, TmAlarmDto>();
    }
  }
}