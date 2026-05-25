using System;

namespace Iface.Oik.Tm.Utils
{
  public class TmUtil
  {
    private static ReadOnlySpan<int> RetrospectivePossibleSteps => new[]
    {
      5,
      30,
      DateUtil.Minute,
      DateUtil.Minute * 5,
      DateUtil.Minute * 10,
      DateUtil.Minute * 15,
      DateUtil.Minute * 30,
      DateUtil.Minute * 45,
      DateUtil.Hour,
      DateUtil.Hour * 2,
      DateUtil.Hour * 4,
      DateUtil.Hour * 8,
      DateUtil.Hour * 12,
    };


    public static int GetRetrospectivePreferredStep(DateTime startTime, DateTime endTime)
    {
      return GetRetrospectivePreferredStep(DateUtil.GetUtcTimestampFromDateTime(startTime),
                                           DateUtil.GetUtcTimestampFromDateTime(endTime));
    }


    public static int GetRetrospectivePreferredStep(long startTime, long endTime)
    {
      if (endTime <= startTime)
      {
        throw new ArgumentException("Дата начала ретроспективы позднее даты конца");
      }
      const int preferredPointsCount = 125;
      
      var preferredStep = (int)((endTime - startTime) / preferredPointsCount);

      var resultIndex = RetrospectivePossibleSteps.BinarySearch(preferredStep);
      if (resultIndex >= 0)
      {
        return RetrospectivePossibleSteps[resultIndex];
      }

      var nextIndex = ~resultIndex;
      
      if (nextIndex == 0)
      {
        return RetrospectivePossibleSteps[0]; // минимальное значение
      }
      if (nextIndex >= RetrospectivePossibleSteps.Length)
      {
        return RetrospectivePossibleSteps[^1]; // максимальное значение
      }

      return ChooseClosestValue(preferredStep,
                                RetrospectivePossibleSteps[nextIndex],
                                RetrospectivePossibleSteps[nextIndex - 1]);
    }


    private static int ChooseClosestValue(int preferred, int option1, int option2)
    {
      return Math.Abs(preferred - option1) < Math.Abs(preferred - option2)
               ? option1
               : option2;
    }
  }
}