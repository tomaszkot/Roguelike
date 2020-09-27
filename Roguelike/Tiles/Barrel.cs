////#define ASCII_BUILD  
using Dungeons.Core;
using System.Drawing;

namespace Roguelike.Tiles
{
  
  public enum BarrelKind { Barrel, PileOfSkulls }

  public class Barrel : InteractiveTile, ILootSource
  {
    public const char BarrelSymbol = '~';
    public BarrelKind BarrelKind { get; set; }

    public Barrel(Point point) : base(BarrelSymbol)
    {
      Kind = InteractiveTileKind.Barrel;
      Name = Kind.ToString();
      DestroySound = "barrel_broken";
      //BarrelKind = RandHelper.GetRandomDouble() < 0.5 ? BarrelKind.Barrel : BarrelKind.PileOfSkulls;
    }

    public Barrel() : this(new Point().Invalid())
    {

    }

    public Point GetPoint() { return Point; }

    public void SetLevel(int level) { Level = level; }

    //public override bool OnHitBy(IMovingDamager spell)
    //{
    //  GameManager.Instance.InputManager.HandleBarrelOrChest(this);
    //  return base.OnHitBy(spell);
    //}

  }


}
