using Dungeons.Tiles;
using Dungeons.Tiles.Abstract;
using Newtonsoft.Json;
using Roguelike.Abilities;
using Roguelike.Calculated;
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
    }

    public AbilityKind ActiveAbilitySrc { get; set; }

    [JsonIgnore]
    public Tile Target { get; set; }

    [JsonIgnore]
    public AttackDescription AttackDescription { get; set; }

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

        if (value == FightItemKind.ExplosiveCocktail || value == FightItemKind.Stone || value == FightItemKind.PoisonCocktail)
          DiesOnHit = true;

        if (value == FightItemKind.PlainArrow)
          Range = DefaultMaxRange + 2;
        else if (value == FightItemKind.PlainBolt)
          Range = DefaultMaxRange + 1;
      }
    }

    public int Range { get; set; } = DefaultMaxRange;

    public string HitSound => HitTargetSound;

    [JsonIgnore]
    public int MaxVictimsCount { get; set; }
  }
}
