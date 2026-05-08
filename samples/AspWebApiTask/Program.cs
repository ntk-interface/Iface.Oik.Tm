using System;
using Iface.Oik.Tm.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AspWebApiTask
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var app = Host.CreateDefaultBuilder(args)
                    .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
                    .Build();
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
      
      app.Run();
    }
  }
}