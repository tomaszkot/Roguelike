namespace Roguelike.Tiles.Abstract
{
  public interface IDestroyable : ILootSource, IObstacle
  {
    //Point Position { get; }
    bool Destroyed { get; set; }
  }
}
