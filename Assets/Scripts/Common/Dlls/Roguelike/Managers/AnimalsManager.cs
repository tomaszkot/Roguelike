using Dungeons.Tiles;
using Roguelike.Policies;
using Roguelike.Tiles.LivingEntities;
using SimpleInjector;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Managers
{
  public class AnimalsManager : EntitiesManager
  {
    
    public AnimalsManager(GameContext context, EventsManager eventsManager, Container container, GameManager gm) :
                         base(TurnOwner.Animals, context, eventsManager, container, gm)
    {
      context.ContextSwitched += Context_ContextSwitched;
      BrutalPendingForAllIdleFalseMode = true;
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
      var anim = entity as Animal;
      
      if (anim.LastHitCoolDown > 0)
      {
        var empties = new List<Tile>();
        anim.LastHitCoolDown--;
        var neibs = this.gameManager.CurrentNode.GetEmptyNeighborhoodTiles(anim, false);
        foreach (var neibFar in neibs)
        {
          var neibsFar = this.gameManager.CurrentNode.GetEmptyNeighborhoodTiles(neibFar, false);
          empties.AddRange(neibsFar);
        }
        if (empties.Any())
        {
          var target = empties.OrderByDescending(i => i.DistanceFrom(gameManager.Hero)).FirstOrDefault();
          if (target != null)
          {
            MakeMoveOnPath(anim, target.point, false, true);
            return;
          }
        }
      }

      if (entity.CanMakeRandomMove())
        base.MakeRandomMove(entity);
    }

    //TODO
    protected override bool ShalLReportTurnOwner(Policy policy)
    {
      return false;
    }
  }
}
