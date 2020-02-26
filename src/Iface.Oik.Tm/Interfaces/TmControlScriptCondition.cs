namespace Iface.Oik.Tm.Interfaces
{
  public struct TmControlScriptCondition
  {
    public bool   IsConditionMet { get; }
    public string Text           { get; }


    public TmControlScriptCondition(bool isConditionMet, string text)
    {
      IsConditionMet = isConditionMet;
      Text           = text;
    }
  }
}