using Roguelike.Managers;

namespace Roguelike.Abstract.Managers
{
  public interface IGameManagerProvider
  {
    GameManager GameManager
    {
      get;
      //set;
    }
  }
}
