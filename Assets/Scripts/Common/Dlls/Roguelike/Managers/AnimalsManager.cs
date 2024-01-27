using Dungeons.Core;
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
      EmitAllIdleWhenNooneBusy = true;
    }

    public override List<LivingEntity> FindBusyOnes(BusyOnesCheckContext context)
    {
      var ents = AllEntities.Where(i => IsBusy(i)).ToList();
      ents = ents.Where(i => i.State != EntityState.Moving).ToList();
      return ents;
    }

    private void Context_ContextSwitched(object sender, ContextSwitch e)
    {
      var entities = Context.CurrentNode.GetTiles<Animal>();
      base.AllEntities.Clear();
      entities.ForEach(i => base.AllEntities.Add(i));
      entitiesSet = true;
    }

    public override void MakeRandomMove(LivingEntity entity)
    {
      var anim = entity as Animal;

      if (entity.CanMakeRandomMove())
        base.MakeRandomMove(entity);
    }

    bool RunAwayIfNeeded(LivingEntity entity)
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
            return true;
          }
        }
      }

      return false;
    }

    public override void MakeTurn()
    {
      base.MakeTurn();
    }

    public override void MakeTurn(LivingEntity entity)
    {
      if (RunAwayIfNeeded(entity))
        return;
      var randMoveThresh = 0.5f;
      if (IsZyndrams(entity) && entity.MovesCounter > ZyndramBackMovesCounter)
      {
        randMoveThresh = 0.75f;
      }
      //if (entity.State != EntityState.Idle)//maybe animation of walk is in progress?
      //  return;
      if (entity.FixedWalkTarget != null)
        FollowTarget(entity, entity.FixedWalkTarget as LivingEntity);
      else if(RandHelper.GetRandomDouble() > randMoveThresh)
        base.MakeRandomMove(entity);
    }

    //TODO
    protected override bool ShalLReportTurnOwner(Policy policy)
    {
      return false;
    }

    
    public override List<LivingEntity> GetActiveEntities()
    {
      var basicList = GetBasicActiveEntitiesList();
      var res = basicList
       .Where(i =>
       i.DistanceFrom(gameManager.Hero) < 15 ||
       (IsZyndrams(i) && i.MovesCounter < ZyndramBackMovesCounter)
       )
       .ToList();
      return res;
    }

    
  }
}
