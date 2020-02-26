using System;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace AspWebApi
{
  public class Program
  {
    public static void Main(string[] args)
    {
      // требуется для работы с кодировкой Win-1251
      Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
      
      // устанавливаем соединение с сервером ОИК
      try
      {
        TmStartup.Connect();
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        Environment.Exit(-1);
      }
      
      // .NET Generic Host
      CreateHostBuilder(args).Build().Run();
    }


    public static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
          .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
  }
}