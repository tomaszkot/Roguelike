using Newtonsoft.Json;
using Roguelike.Abilities;
using Roguelike.Tiles.LivingEntities;

namespace Roguelike.Abstract.Projectiles
{
  public interface IProjectile : Dungeons.Tiles.Abstract.IProjectile
  {
    [JsonIgnore]
    LivingEntity Caller { get; set; }

    bool DiesOnHit { get; set; }

    AbilityKind ActiveAbilitySrc { get; set; }

    int MaxVictimsCount { get; set; }
    int Count { get; set; }
  }
}
