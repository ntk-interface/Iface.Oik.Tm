using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AspMinimalWebApiTask;
using AspMinimalWebApiTask.Model;
using Iface.Oik.Tm.Api;
using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Api;
using Iface.Oik.Tm.Native.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // требуется для работы с кодировкой Win-1251

var builder = WebApplication.CreateBuilder(args);
// регистрация зависимостей ОИК
builder.Services.AddSingleton<ITmNative, TmNative>();
builder.Services.AddSingleton<ITmsApi, TmsApi>();
builder.Services.AddSingleton<IOikSqlApi, OikSqlApi>();
builder.Services.AddSingleton<IOikDataApi, OikDataApi>();
builder.Services.AddSingleton<ICommonInfrastructure, CommonInfrastructure>();
builder.Services.AddSingleton<ServerService>();
builder.Services.AddSingleton<ICommonServerService>(provider => provider.GetRequiredService<ServerService>());
builder.Services.AddSingleton<TmStartup>();
// регистрация фоновых служб
builder.Services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<TmStartup>());
builder.Services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<ServerService>());

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
  try
  {
    scope.ServiceProvider.GetRequiredService<TmStartup>().TryConnect();
  }
  catch (Exception ex)
  {
    Tms.PrintError(ex.Message);
    Environment.Exit(-1);
  }
}

app.MapGet("/",
           () => "Hello World!");
app.MapGet("/api/time",
           async (IOikDataApi api) => await api.GetSystemTimeString());
app.MapGet("/api/user",
           (ICommonInfrastructure infr) => infr.TmUserInfo.Adapt<TmUserInfoDto>());
app.MapGet("/api/alerts",
           async (IOikDataApi api) => (await api.GetAlerts()).Adapt<List<TmAlertDto>>());
app.MapGet("/api/tm/s/{tmAddrString}",
           async (IOikDataApi api, string tmAddrString) =>
           {
             if (!TmAddr.TryParse(tmAddrString, out var tmAddr, TmType.Status)) return Results.NotFound();
             var tmStatus = new TmStatus(tmAddr);
             await Task.WhenAll(api.UpdateTagPropertiesAndClassData(tmStatus), api.UpdateStatus(tmStatus));
             return Results.Ok(tmStatus.Adapt<TmStatusDto>());
           });
app.MapGet("/api/tm/a/{tmAddrString}",
           async (IOikDataApi api, string tmAddrString) =>
           {
             if (!TmAddr.TryParse(tmAddrString, out var tmAddr, TmType.Analog)) return Results.NotFound();
             var tmAnalog = new TmAnalog(tmAddr);
             await Task.WhenAll(api.UpdateTagPropertiesAndClassData(tmAnalog), api.UpdateAnalog(tmAnalog));
             return Results.Ok(tmAnalog.Adapt<TmAnalogDto>());
           });

app.Run();