using Roguelike.Managers;
using SimpleInjector;

namespace OuaDII.Managers
{
  public class EnemiesManager : Roguelike.Managers.EnemiesManager
  {
    public EnemiesManager(GameContext context, EventsManager eventsManager, Container container, AlliesManager allies, GameManager gm) :
      base(context, eventsManager, container, allies, gm)
    {

    }
  }
}
