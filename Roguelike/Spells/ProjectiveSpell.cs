using Newtonsoft.Json;
using Roguelike.Abstract.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Spells
{
  public class ProjectiveSpell : OffensiveSpell, IProjectileSpell
  {
    public const int DefaultMaxRange = 5;

    public bool SourceOfDamage = true;

    [JsonIgnore]
    public Dungeons.Tiles.Tile Target { get; set; }

    public ProjectiveSpell(LivingEntity caller, Weapon weapon, bool withVariation = true) : base(caller, weapon, withVariation)
    {
      EntityRequired = true;
      EnemyRequired = true;
      this.Caller = caller;
    }

    [JsonIgnore]
    public bool DiesOnHit { get; set; } = true;

    public string HitSound 
    {
      get { return GetHitSound(); }
    }

    public int Range { get; set; } = DefaultMaxRange;
  }
}
