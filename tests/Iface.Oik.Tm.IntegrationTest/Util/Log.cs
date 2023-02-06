using System;

namespace Iface.Oik.Tm.IntegrationTest.Util;

public static class Log
{
  private static int _totalErrorsCount;


  public static void Message(string message)
  {
    Console.WriteLine(message);
  }


  public static void Condition(bool isOk, string message)
  {
    if (isOk)
    {
      Console.WriteLine($"{message}     OK");
    }
    else
    {
      Console.WriteLine($"{message}     ERROR");
      _totalErrorsCount++;
    }
  }


  public static void ExitMessage()
  {
    Condition(_totalErrorsCount == 0, $"Total errors: {_totalErrorsCount}");
  }
}