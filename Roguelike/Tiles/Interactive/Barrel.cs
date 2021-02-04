////#define ASCII_BUILD  
using Dungeons.Core;
using System.Drawing;

namespace Roguelike.Tiles.Interactive
{
  
  public enum BarrelKind { Barrel, PileOfSkulls }

  public class Barrel : InteractiveTile, ILootSource
  {
    public const char BarrelSymbol = '~';
    private BarrelKind barrelKind;

    public BarrelKind BarrelKind 
    { 
      get => barrelKind; 
      set  
      {
        barrelKind = value;
        
        DestroySound = (barrelKind == BarrelKind.Barrel) ? "barrel_broken" : "bones_fall";
      }
    }

    public Barrel(Point point) : base(BarrelSymbol)
    {
      Kind = InteractiveTileKind.Barrel;
      BarrelKind = BarrelKind.Barrel;
      Name = Kind.ToString();
      
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
