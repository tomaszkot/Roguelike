////#define ASCII_BUILD  
using Dungeons.Core;
using Roguelike.Abstract.Spells;
using Roguelike.Tiles.Abstract;
using System.Drawing;

namespace Roguelike.Tiles.Interactive
{

  public enum BarrelKind { Barrel, PileOfSkulls }

  public class Barrel : InteractiveTile, IDestroyable
  {
    public const char BarrelSymbol = '~';
    private BarrelKind barrelKind;

    public bool Destroyed { get; set; }

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

    public Point GetPoint() { return point; }

    public bool SetLevel(int level)
    {
      Level = level;
      return true;
    }

    public override bool OnHitBy(ISpell damager)
    {
      Destroyed = true;
      return base.OnHitBy(damager);
    }
  }


}
