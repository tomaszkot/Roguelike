////#define ASCII_BUILD  
using Dungeons.Core;
using System.Drawing;

namespace Roguelike.Tiles
{
  public enum BarrelKind { Barrel, PileOfSkulls }

  public class Barrel : InteractiveTile
  {
    public const char BarrelSymbol = '~';
    public BarrelKind BarrelKind { get; set; }

    public Barrel(Point point) : base(BarrelSymbol)
    {
      Kind = InteractiveTileKind.Barrel;
      Name = Kind.ToString();
      BarrelKind = BarrelKind.Barrel;
    }

    public Barrel() : this(new Point().Invalid())
    {

    }

    //public override bool OnHitBy(IMovingDamager spell)
    //{
    //  GameManager.Instance.InputManager.HandleBarrelOrChest(this);
    //  return base.OnHitBy(spell);
    //}

  }


}
