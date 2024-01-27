using Dungeons.Tiles;
using Newtonsoft.Json;
using Roguelike.Abilities;
using Roguelike.Attributes;
using Roguelike.Calculated;
using Roguelike.Extensions;
using Roguelike.Tiles.LivingEntities;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Tiles.Looting
{
  public class ProjectileFightItem : FightItem, 
    Roguelike.Abstract.Projectiles.IProjectile,
    IEquipment
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
    public AttackDescription AttackDescription 
    { 
      get; 
      set; 
    }

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

    public bool CausesFire { get {
        return FightItemKind == FightItemKind.ExplosiveCocktail ||
           FightItemKind == FightItemKind.ThrowingTorch;
      } }

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
        if (value == base.FightItemKind)
          return;
        base.FightItemKind = value;

        if (value == FightItemKind.ExplosiveCocktail || value == FightItemKind.Stone || value == FightItemKind.PoisonCocktail
          || value == FightItemKind.ThrowingTorch || value == FightItemKind.CannonBall)
          DiesOnHit = true;

        if (value == FightItemKind.ThrowingTorch)
          EquipmentKind = EquipmentKind.Shield;

        Range = CalcRange(value);
      }
    }

    public static int CalcRange(FightItemKind kind) 
    {
      int DefaultMaxRangeAddition = 0;
      if (kind.IsBowAmmoKind())
        DefaultMaxRangeAddition = 2;
      else if (kind.IsCrossBowAmmoKind())
        DefaultMaxRangeAddition = 1;

      var range = DefaultMaxRange + DefaultMaxRangeAddition;
      if (kind == FightItemKind.CannonBall)
        range += 3;
      return range;
    }

    public int Range { get; set; } = DefaultMaxRange;

    public string HitSound => HitTargetSound;

    [JsonIgnore]
    public int MaxVictimsCount { get; set; }
    public EquipmentKind EquipmentKind { get; set; }
    public AnimalKind MatchingAnimalKind { get; set; }
    public bool IsIdentified { get; set; } = true;

    EntityStats entityStats = new EntityStats();
    public EntityStats GetStats()
    {
      return entityStats;
    }
    public void PrepareForSave() { }
    public int RequiredLevel { get; set; }

    public List<EntityStat> GetEffectiveRequiredStats() { return GetStats().GetStats().Values.ToList(); }

    public string Tag1 { get { return tag1; } }

    public bool MissedTarget { get; set ; }

    public bool IsBetter(IEquipment currentEq) 
    {
      return this.Price > currentEq.Price;
    }

    public float GetReqStatValue(EntityStat es)
    {
      return es.Value.TotalValue;
    }

    //public bool Countable { get; set; } = true;
  }
}
