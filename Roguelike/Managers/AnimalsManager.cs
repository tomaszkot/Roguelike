using Roguelike.Tiles.LivingEntities;
using SimpleInjector;

namespace Roguelike.Managers
{
  public class AnimalsManager : EntitiesManager
  {
    public AnimalsManager(GameContext context, EventsManager eventsManager, Container container, GameManager gm) :
                         base(TurnOwner.Animals, context, eventsManager, container, gm)
    {
      context.ContextSwitched += Context_ContextSwitched;
    }

    private void Context_ContextSwitched(object sender, ContextSwitch e)
    {
      var entities = Context.CurrentNode.GetTiles<Animal>();
      base.AllEntities.Clear();
      entities.ForEach(i=> base.AllEntities.Add(i));
      entitiesSet = true;
    }

    public override void MakeRandomMove(LivingEntity entity)
    {
      if(entity.CanMakeRandomMove())
        base.MakeRandomMove(entity);
    }
  }
}
