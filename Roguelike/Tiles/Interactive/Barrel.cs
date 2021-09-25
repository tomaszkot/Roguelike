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
    public string OriginMap { get; set; }
    public const char BarrelSymbol = '~';
    private BarrelKind barrelKind;
    public string UnhidingMapName { get; set; }

    public bool Destroyed { get; set; }

    public BarrelKind BarrelKind
    {
      get => barrelKind;
      set
      {
        barrelKind = value;
        SetSound();
        if(barrelKind == BarrelKind.PileOfSkulls)
          tag1 = "pile_skulls";
      }
    }

    private void SetSound()
    {
      var snd = (barrelKind == BarrelKind.Barrel) ? "barrel_broken" : "bones_fall"; ;
      DestroySound = snd;
    }

    public bool CanGenerateEnemy 
    {
      get { return true; }
    }

    public Barrel(Point point) : base(BarrelSymbol)
    {
      Kind = InteractiveTileKind.Barrel;
      BarrelKind = BarrelKind.Barrel;
      Name = Kind.ToString();
      tag1 = "barrel1";
      //BarrelKind = RandHelper.GetRandomDouble() < 0.5 ? BarrelKind.Barrel : BarrelKind.PileOfSkulls;
    }

    public Barrel() : this(new Point().Invalid())
    {
      
    }

    public Point GetPoint() { return point; }

    public bool SetLevel(int level, Difficulty? diff = null)
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
