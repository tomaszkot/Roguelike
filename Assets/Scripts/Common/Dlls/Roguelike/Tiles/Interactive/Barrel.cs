//#define ASCII_BUILD  
using Dungeons.Core;
using Dungeons.Core.Policy;
using Dungeons.Fight;
using Dungeons.Tiles.Abstract;
using Newtonsoft.Json;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System.Drawing;

namespace Roguelike.Tiles.Interactive
{

  public enum BarrelKind { Barrel, PileOfSkulls, OilBarrel }

  public class Barrel : InteractiveTile, ILootSource, IDestroyable
  {
    public string OriginMap { get; set; }
    public const char BarrelSymbol = '~';
    private BarrelKind barrelKind;
    public string UnhidingMapName { get; set; }

    [JsonIgnore]
    public bool RewardGenerated { get; set; }
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
        else if (barrelKind == BarrelKind.OilBarrel)
          tag1 = "barrel_oil";
        else if (barrelKind == BarrelKind.Barrel)
          tag1 = "barrel1";
      }
    }

    private void SetSound()
    {
      var snd = (barrelKind != BarrelKind.PileOfSkulls) ? "barrel_broken" : "bones_fall"; ;
      DestroySound = snd;
    }

    public bool CanGenerateEnemy 
    {
      get { return true; }
    }

    public Barrel(Container cont, Point point) : base(cont, BarrelSymbol)
    {
      Kind = InteractiveTileKind.Barrel;
      BarrelKind = BarrelKind.Barrel;
      Name = Kind.ToString();
      tag1 = "barrel1";
      //BarrelKind = RandHelper.GetRandomDouble() < 0.5 ? BarrelKind.Barrel : BarrelKind.PileOfSkulls;
    }

    public Barrel(Container cont) : this(cont, new Point().Invalid())
    { 
      
    }

    public bool LevelSet { get; set; }
    public Loot ForcedReward { get; set; }

    public Point GetPoint() { return point; }

    public bool SetLevel(int level, Difficulty? diff = null)
    {
      Level = level;
      LevelSet = true;
      return true;
    }

    protected override void EmitInteraction()
    {
      Destroyed = true;
      base.EmitInteraction();
    }

    
  }


}
