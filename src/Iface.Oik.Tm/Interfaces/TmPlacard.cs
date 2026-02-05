using System;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmPlacard
  {
    public Guid              Id          { get; }
    public int               TypeId      { get; set; }
    public TmPlacardType     Type        { get; set; }
    public DateTime          CreatedTime { get; set; }
    public string            Operator    { get; set; }
    public int?              Index       { get; set; }
    public string            Comment     { get; set; }
    public Tob               Tob         { get; set; }
    public float             Scale       { get; set; } = 1.0f;
    public TmPlacardPosition Position    { get; set; } = TmPlacardPosition.Center;
    public int               OffsetX     { get; set; }
    public int               OffsetY     { get; set; }


    public bool IsPseudoPlacardPermittedGround { get; set; }


    public TmPlacard(Guid id)
    {
      Id = id;
    }


    public TmPlacard()
      : this(Guid.NewGuid())
    {
    }


    public override bool Equals(object obj)
    {
      return Equals(obj as TmPlacard);
    }


    public bool Equals(TmPlacard comparison)
    {
      if (ReferenceEquals(comparison, null))
      {
        return false;
      }
      if (ReferenceEquals(this, comparison))
      {
        return true;
      }

      return Id       == comparison.Id       &&
             TypeId   == comparison.TypeId   &&
             Index    == comparison.Index    &&
             Comment  == comparison.Comment  &&
             Position == comparison.Position &&
             OffsetX  == comparison.OffsetX  &&
             OffsetY  == comparison.OffsetY  &&
             Scale.Equals(comparison.Scale);
    }


    public static bool operator ==(TmPlacard left, TmPlacard right)
    {
      if (ReferenceEquals(left, null))
      {
        return ReferenceEquals(right, null);
      }
      return left.Equals(right);
    }


    public static bool operator !=(TmPlacard left, TmPlacard right)
    {
      return !(left == right);
    }


    public override int GetHashCode()
    {
      return Id.GetHashCode();
    }
  }


  public class TmPlacardType
  {
    public static readonly int GroundId = 1;
    public static readonly int GapId    = 2;


    public int             Id      { get; set; }
    public string          Name    { get; set; }
    public TmPlacardAction Action  { get; set; }
    public byte[]          Icon    { get; set; } // SVG
    public byte[]          PngIcon { get; set; } // BitmapImage


    public bool IsGround => Id == GroundId;
    public bool IsGap    => Id == GapId;
  }


  public enum TmPlacardPosition
  {
    Center      = 0,
    TopLeft     = 1,
    Top         = 2,
    TopRight    = 3,
    Left        = 4,
    Right       = 5,
    BottomLeft  = 6,
    Bottom      = 7,
    BottomRight = 8,
  }
}