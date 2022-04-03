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

    AbilityKind ActiveAbilityKind { get; set; }

    int MaxVictimsCount { get; set; }
  }
}
