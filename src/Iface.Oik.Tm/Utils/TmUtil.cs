using System;

namespace Iface.Oik.Tm.Utils
{
  public class TmUtil
  {
    public static readonly int[] RetrospectivePossibleSteps =
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


    public static int GetRetrospectivePreferredStep(long startTime, long endTime) // todo переписать, ничего не понятно
    {
      if (endTime <= startTime)
      {
        throw new ArgumentException("Дата начала ретроспективы позднее даты конца");
      }
      const int preferredPointsCount = 125;
      int       preferredStep        = (int) ((endTime - startTime) / preferredPointsCount);

      int resultIndex = Array.BinarySearch(RetrospectivePossibleSteps, preferredStep);
      if (resultIndex >= 0)
      {
        return RetrospectivePossibleSteps[resultIndex];
      }

      resultIndex = ~resultIndex;
      if (resultIndex == 0)
      {
        return RetrospectivePossibleSteps[resultIndex];
      }
      if (resultIndex == RetrospectivePossibleSteps.Length)
      {
        return RetrospectivePossibleSteps[resultIndex - 1];
      }
      int possibleResult1 = RetrospectivePossibleSteps[resultIndex];
      int possibleResult2 = RetrospectivePossibleSteps[resultIndex - 1];

      return (Math.Abs(preferredStep - possibleResult1) < Math.Abs(preferredStep - possibleResult2))
        ? possibleResult1
        : possibleResult2;
    }
  }
}