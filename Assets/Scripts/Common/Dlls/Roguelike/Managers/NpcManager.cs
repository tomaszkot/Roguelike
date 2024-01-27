using Dungeons.Core;
using Roguelike.Abstract.Tiles;
using Roguelike.Events;
using Roguelike.Policies;
using Roguelike.TileContainers;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using SimpleInjector;
using System;
using System.Drawing;
using System.Linq;

namespace Roguelike.Managers
{
  public class NpcManager : EntitiesManager
  {
    public NpcManager
    (
      GameContext context,
      EventsManager eventsManager,
      Container container,
      GameManager gm
     ) :
      base(TurnOwner.Npcs, context, eventsManager, container, gm)
    {
      context.TurnOwnerChanged += Context_TurnOwnerChanged; ;
      context.ContextSwitched += Context_ContextSwitched;
      EmitAllIdleWhenNooneBusy = true;
    }

    private void Context_ContextSwitched(object sender, ContextSwitch e)
    {
      var entities = Context.CurrentNode.GetNpcs();
      base.SetEntities(entities.Cast<LivingEntity>().ToList());
    }

    private void Context_TurnOwnerChanged(object sender, TurnOwner e)
    {

    }

    bool IsNpcAtTarget(INPC npc, Point target, bool precisely)
    {
      var dist = npc.LivingEntity.DistanceFrom(target);
      if (dist == 0)
        return true;
      if (precisely)
        return false;
      if (dist <= 1 && npc.DestPointDesc.State != DestPointState.TravelingTo &&  npc.DestPointDesc.StateCounter > 10)
        return true;
      return false;

    }

    public bool HandleStayingAtTargetState(InteractiveTile target, LivingEntity le, DestPointDesc dpd)
    {
      if (target.HidesInteractedEntity)
      {
        //dpd.OriginalTargetPoint = target.point;
        var set = gameManager.CurrentNode.SetTile(le, target.point);
        Log("StayingAtTarget " + set);
        return set;
      }
      return false;
    }

    public InteractiveTile HandleTravelBackState(LivingEntity le, DestPointDesc dpd, AbstractGameLevel gl, bool setTile)
    {
      InteractiveTile target;
      var itPoint = le.point;
      bool set = false;
      if (setTile)
      {
        var emp = gl.GetClosestEmpty(le);
        set = gl.SetTile(le, emp.point);

        Log("HandleTravelBackState set le: " + set);
      }
      target = dpd.UnbusyTarget();//target is privy or fireplace
      if (target != null)
      {
        set = gl.SetTile(target, target.point);
        Log("HandleTravelBackState set target: " + set);
      }
      dpd.ActivityKind = DestPointActivityKind.Home;
      return target;
    }

    public void SetNpcDestPointState(INPC npc, DestPointState destPointState, InteractiveTile target = null)
    {
      var log = "SetNpcDestPointState " + destPointState;
      Log(log);
      npc.DestPointDesc.State = destPointState;
      if (destPointState == DestPointState.StayingAtTarget)
      {
        HandleStayingAtTargetState(target, npc.LivingEntity, npc.DestPointDesc);
      }
      else if (destPointState == DestPointState.TravelingBack)
      {
        target = HandleTravelBackState(npc.LivingEntity, npc.DestPointDesc, gameManager.CurrentNode, true);
      }
      else if (destPointState == DestPointState.Unset)
      {
        npc.DestPointDesc.ActivityKind = DestPointActivityKind.Unset;
        //npc.WalkKind = WalkKind.Unset;
      }

      if (target != null)
      {
        if (destPointState == DestPointState.StayingAtTarget)
          gameManager.AppendAction(new NpcAction(npc, npc.DestPointDesc, target) { Info = npc.Name + " interacted with a " + target.Name });
        else if (destPointState == DestPointState.TravelingBack)
          gameManager.AppendAction(new NpcAction(npc, npc.DestPointDesc, target) { Info = npc.Name + " is traveling home from " + target.Name });
        else if (destPointState == DestPointState.Unset)
          gameManager.AppendAction(new NpcAction(npc, npc.DestPointDesc, target) { Info = npc.Name + " arrived home from " + target.Name });
      }

    }
        

    private static void Log(string log)
    {
      //Debug.WriteLine(log);
    }
    public void StartWalkToInteractive(INPC npc, InteractiveTile target, DestPointActivityKind ak)
    {
      if (npc.WalkKind != WalkKind.Unset)
        return;

      var desc = new DestPointDesc()
      {
        MoveOnPathTarget = target,
        ReturnPoint = npc.LivingEntity.point,
        ActivityKind = ak
      };

      npc.DestPointDesc = desc;
      var empts = gameManager.CurrentNode.GetEmptyNeighborhoodPoint(target);
      if (empts != null)
      {
        desc.TargetPoint = empts.Item1;
        npc.LivingEntity.PathToTarget = gameManager.CurrentNode.FindPath(npc.LivingEntity, desc.TargetPoint);
        if (npc.LivingEntity.PathToTarget != null)
        {
          //npc.WalkKind = WalkKind.GoToInteractive;
          SetNpcDestPointState(npc, DestPointState.TravelingTo, target);
        }
      }
    }

    public override void MakeTurn(LivingEntity entity)
    {
      if (entity is INPC npc)
      {
        var increasedStateCounter = npc.DestPointDesc.IncreaseStateCounter();
        var log = "NpcManager MakeTurn: " + entity + ", npc.WalkKind: " + npc.WalkKind;
        if (gameManager.AlliesManager.AllAllies.Contains(npc as IAlly))
          return;

        if (npc.PointedByHeroCounter > 0)
        {
          npc.PointedByHeroCounter--;
          return;
        }
        
        var moveOnPathTarget = npc.DestPointDesc.MoveOnPathTarget;
        if (npc.WalkKind == WalkKind.GoToHome)
        {
          moveOnPathTarget = gameManager.CurrentNode.GetTile(npc.DestPointDesc.ReturnPoint);
        }
        log += ", Move Target: " + moveOnPathTarget;
        Log(log);
        if (npc.DestPointDesc.IsWalkToTargetInProcess)
        {
          HandleWalkToInteractive(npc, increasedStateCounter);
        }
        else if (npc.WalkKind == WalkKind.FollowingTarget)
        {
          FollowTarget(npc);
        }
        else 
        {
          if (npc.WalkKind == WalkKind.Unset &&  
              npc.DestPointDesc.State != DestPointState.StayingAtTarget &&
              npc.LivingEntity.DistanceFrom(npc.LivingEntity.InitialPoint) > 5 && 
              RandHelper.Random.NextDouble() > .5f)
          {
            MakeTravelingBack(npc);
          }
          if (npc.WalkKind == WalkKind.GoToHome)
          {
            HandleTravelBack(npc, npc.LivingEntity.InitialPoint);
          }
          else if (RandHelper.Random.NextDouble() > .75f)
          {
            MakeRandomMove(entity);
          }
          else if (npc.DestPointDesc.StateCounter > gameManager.GetStartWalkToInteractiveTurnsCount())
          {
            if (RandHelper.GetRandomDouble() > 0.75)
            {
              var rand = gameManager.PossibleNpcDestMoveTargets.Where
                (
                  i => i.DistanceFrom(npc.LivingEntity.point) < 12 && !i.Busy
                )
                .ToList()
                .GetRandomElem();
              if (rand != null)
                StartWalkToInteractive(npc, rand, rand.DestPointActivityKind);
            }
          }
        }
      }
    }

    public void MakeTravelingBack(INPC npc)
    {
      SetNpcDestPointState(npc, DestPointState.TravelingBack, null);
      npc.DestPointDesc.ReturnPoint = npc.LivingEntity.InitialPoint;
    }
            

    private void HandleWalkToInteractive(INPC npc, bool increasedStateCounter)
    {
      if (npc.DestPointDesc.State == DestPointState.TravelingTo)
      {
        if (IsNpcAtTarget(npc, npc.DestPointDesc.TargetPoint, false))
        {
          if (npc.DestPointDesc.MoveOnPathTarget is InteractiveTile it)
          {
            if (it.InteractWith(npc.LivingEntity))
            {
              //gameManager.AddHiddenNpc(npc, it);
              SetNpcDestPointState(npc, DestPointState.StayingAtTarget, it);
            }
          }
        }
        else
        {
          if(RandHelper.GetRandomDouble() > 0.25)//do not hurry, let player chase him
            MakeMoveOnPath(npc.LivingEntity, npc.DestPointDesc.TargetPoint, true);
        }

      }
      else if (npc.DestPointDesc.State == DestPointState.StayingAtTarget)
      {
        if (!increasedStateCounter)
        {
          SetNpcDestPointState(npc, DestPointState.TravelingBack, null);
        }
      }
      else if (npc.DestPointDesc.State == DestPointState.TravelingBack)
      {
        HandleTravelBack(npc, npc.DestPointDesc.ReturnPoint);
      }
    }

    private void HandleTravelBack(INPC npc, Point backPoint)
    {
      if (RandHelper.GetRandomDouble() > 0.25)//do not hurry
      {
        MakeMoveOnPath(npc.LivingEntity, backPoint, true);
        if (IsNpcAtTarget(npc, backPoint, true))
        {
          SetNpcDestPointState(npc, DestPointState.Unset, null);
        }
      }
    }

    //TODO
    protected override bool ShalLReportTurnOwner(Policy policy)
    {
      return false;
    }

    public override void MakeTurn()
    {
      //??
      //if ((gameManager.CurrentNode is GameLevel))//only world
      //{
      //  CheckState(true);
      //  return;
      //}
      base.MakeTurn();
    }
  }
}
