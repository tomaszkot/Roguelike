using Newtonsoft.Json;
using Roguelike.Abilities;
using Roguelike.Abstract.Spells;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;

namespace Roguelike.Spells
{
  public class ProjectiveSpell : OffensiveSpell, IProjectileSpell
  {
    public const int DefaultMaxRange = 5;

    public bool SourceOfDamage = true;

    [JsonIgnore]
    public Dungeons.Tiles.Tile Target { get; set; }

    public ProjectiveSpell(LivingEntity caller, SpellKind sk, Weapon weapon, bool withVariation = true) : base(caller, sk, weapon, withVariation)
    {
      EntityRequired = true;
      EnemyRequired = true;
      this.Caller = caller;
    }

    [JsonIgnore]
    public bool DiesOnHit { get; set; } = true;

    public AbilityKind ActiveAbilitySrc { get; set; }
    public override string HitSound 
    {
      get { return GetHitSound(); }
    }

    public int Range { get; set; } = DefaultMaxRange;

    [JsonIgnore]
    public int MaxVictimsCount { get; set; }

    [JsonIgnore]
    public int Count { get; set; }

    public bool MissedTarget
    {
      get;
      set;
    }
  }
}
