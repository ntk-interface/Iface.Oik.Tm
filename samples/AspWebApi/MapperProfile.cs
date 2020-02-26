using AspWebApi.Model;
using AutoMapper;
using Iface.Oik.Tm.Interfaces;

namespace AspWebApi
{
  public class MapperProfile : Profile
  {
    public MapperProfile()
    {
      CreateMap<TmStatus, TmStatusDto>();
      CreateMap<TmAnalog, TmAnalogDto>();
      CreateMap<TmAlarm, TmAlarmDto>();
    }
  }
}