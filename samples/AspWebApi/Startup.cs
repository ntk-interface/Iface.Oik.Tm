using AutoMapper;
using Iface.Oik.Tm.Api;
using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Api;
using Iface.Oik.Tm.Native.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AspWebApi
{
  public class Startup
  {
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddControllers();
      
      services.AddAutoMapper(typeof(MapperProfile));

      // регистрация зависимостей ОИК
      services.AddSingleton<ITmNative, TmNative>();
      services.AddSingleton<ITmsApi, TmsApi>();
      services.AddSingleton<IOikSqlApi, OikSqlApi>();
      services.AddSingleton<IOikDataApi, OikDataApi>();
      services.AddSingleton<ICommonInfrastructure, CommonInfrastructure>();
      services.AddSingleton<ServerService>();
      services.AddSingleton<ICommonServerService>(provider => provider.GetService<ServerService>());
      
      // регистрация фоновых служб
      services.AddHostedService<TmStartup>();
      services.AddSingleton<IHostedService>(provider => provider.GetService<ServerService>());
    }


    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      app.UseStatusCodePages();
      app.UseRouting();
      app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
  }
}