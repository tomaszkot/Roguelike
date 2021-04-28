namespace Roguelike.Abstract.Projectiles
{
  public interface IProjectile
  {
    Dungeons.Tiles.IObstacle Target { get; set; }
  }
}
