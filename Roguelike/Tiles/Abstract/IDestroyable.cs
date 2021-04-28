namespace Roguelike.Tiles.Abstract
{
  public interface IDestroyable : ILootSource, IObstacle
  {
    //bool OnHitBy(Dungeons.Tiles.Abstract.ISpell md);
    //Point Position { get; }
    bool Destroyed { get; set; }
  }
}
