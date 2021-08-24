using Newtonsoft.Json;
using Roguelike.Tiles.LivingEntities;

namespace Roguelike.Abstract.Projectiles
{
  public interface IProjectile : Dungeons.Tiles.Abstract.IProjectile
  {
    [JsonIgnore]
    LivingEntity Caller { get; set; }
  }
}
