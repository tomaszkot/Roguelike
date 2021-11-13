using Dungeons.Tiles;
using Dungeons.Tiles.Abstract;
using Newtonsoft.Json;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles.Looting
{
  public class ProjectileFightItem : FightItem, Roguelike.Abstract.Projectiles.IProjectile
  {
    public const int DefaultMaxRange = 5;

    public ProjectileFightItem() : this(FightItemKind.Unset, null)
    {
    }

    public ProjectileFightItem(FightItemKind kind, LivingEntity caller = null) : base(kind)
    {
      Caller = caller;
      DiesOnHit = false;
      if (kind == FightItemKind.ExplosiveCocktail)
        DiesOnHit = true;
    }

    [JsonIgnore]
    public Tile Target { get; set; }

    public override bool IsCollectable
    {
      get
      {
        if (this.FightItemKind != FightItemKind.HunterTrap)
          return true;

        return FightItemState == FightItemState.Deactivated ||
               FightItemState == FightItemState.Unset;
      }
    }

    public bool DiesOnHit 
    {
      get; 
      set; 
    }
    public bool AlwaysHit { get; set; }

    public override FightItemKind FightItemKind 
    { 
      get => base.FightItemKind; 
      set
      {
        base.FightItemKind = value;
        if (value == FightItemKind.ExplosiveCocktail)
          DiesOnHit = true;

        if (value == FightItemKind.PlainArrow)
          Range = DefaultMaxRange + 2;
        else if (value == FightItemKind.PlainBolt)
          Range = DefaultMaxRange + 1;
      }
    }

    public int Range { get; internal set; } = DefaultMaxRange;
  }
}
