using System;

namespace Iface.Oik.Tm.IntegrationTest.Util;

public static class Log
{
  private static int _totalErrorsCount;


  public static void Condition(bool isOk, string message)
  {
    if (isOk)
    {
      Message(message, "OK");
    }
    else
    {
      Error(message);
    }
  }


  public static void Message(string message, string state = "")
  {
    Console.WriteLine($"{message,-64} {state}");
  }


  public static void Error(string message, string state = "ERROR")
  {
    Console.WriteLine($"{message,-64} {state}");
    _totalErrorsCount++;
  }


  public static void ExitMessage()
  {
    Condition(_totalErrorsCount == 0, $"Total errors: {_totalErrorsCount}");
  }
}