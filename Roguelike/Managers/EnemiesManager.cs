using Dungeons.Tiles;
using Roguelike.Policies;
using Roguelike.Strategy;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Roguelike.Managers
{
  public class EnemiesManager : EntitiesManager
  {
    //List<LivingEntity> enemies;
    //public List<LivingEntity> Enemies { get => entities; set => entities = value; }
    AttackStrategy attackStrategy;

    public EnemiesManager(GameContext context, EventsManager eventsManager, Container container) :
      base(TurnOwner.Enemies, context, eventsManager, container)
    {
      this.context = context;
      attackStrategy = new AttackStrategy(context);
      attackStrategy.OnPolicyApplied = (Policy pol)=>{ OnPolicyApplied(pol); };

      context.TurnOwnerChanged += OnTurnOwnerChanged;
      context.ContextSwitched += Context_ContextSwitched;
    }
        
    private void Context_ContextSwitched(object sender, ContextSwitch e)
    {
      var entities = Context.CurrentNode.GetTiles<LivingEntity>().Where(i=> i is Enemy).ToList();
      base.SetEntities(entities);
    }

    private void OnTurnOwnerChanged(object sender, TurnOwner turnOwner)
    {
      if (turnOwner == TurnOwner.Enemies)
      { 
      
      }
    }

    public void MakeMove(LivingEntity enemy)
    {
    }
        
    public override void MakeTurn(LivingEntity entity)
    {
      var target = Hero;
      if (AttackIfPossible(entity, target))
        return;

      bool makeRandMove = false;
      if (ShallChaseTarget(entity, target))
      {
        makeRandMove = !MakeMoveOnPath(entity, target.Point);
      }
      else
        makeRandMove = true;
      if (makeRandMove)
      {
        MakeRandomMove(entity);
      }
    }

    public override void MakeRandomMove(LivingEntity entity)
    {
      if (entity.InitialPoint != LivingEntity.DefaultInitialPoint)
      {
        if (entity.MoveKind != EntityMoveKind.ReturningHome)
        {
          var distFromInitPoint = entity.DistanceFrom(entity.InitialPoint);
          if (distFromInitPoint > 5)
          {
            entity.MoveKind = EntityMoveKind.ReturningHome;
          }
        }

        if (entity.MoveKind == EntityMoveKind.ReturningHome)
        {
          if (MakeMoveOnPath(entity, entity.InitialPoint))
          {
            if(entity.Point == entity.InitialPoint)
              entity.MoveKind = EntityMoveKind.Freestyle;

            return;
          }
        }
      }
      base.MakeRandomMove(entity);
    }

    private bool MakeMoveOnPath(LivingEntity enemy, Point target)
    {
      bool forHeroAlly = false;
      bool moved = false;
      enemy.PathToTarget = Node.FindPath(enemy.Point, target, forHeroAlly, true);
      if (enemy.PathToTarget != null && enemy.PathToTarget.Count > 1)
      {
        var node = enemy.PathToTarget[1];
        var pt = new Point(node.Y, node.X);
        if (Node.GetTile(pt) is LivingEntity)
        {
          //gm.Assert(false, "Level.GetTile(pt) is LivingEntity "+ enemy + " "+pt);
          return false;
        }
        MoveEntity(enemy, pt);
        moved = true;
      }

      return moved;
    }

    private bool ShallChaseTarget(LivingEntity enemy, Hero target)
    {
      if (enemy.WasEverHitBy(target))
      {
        enemy.MoveKind = EntityMoveKind.FollowingHero;
        return true;
      }

      var dist = enemy.DistanceFrom(target);
      if(dist < 5)
        return true;
      
      return false;
    }

    public bool AttackIfPossible(LivingEntity enemy, Hero hero)
    {
      return attackStrategy.AttackIfPossible(enemy, hero);
    }

    protected override bool MoveEntity(LivingEntity entity, Point newPos)
    {
      if (entity.InitialPoint == LivingEntity.DefaultInitialPoint)
      {
        entity.InitialPoint = entity.Point;
      }

      var moved = base.MoveEntity(entity, newPos);
      return moved;
    }

    protected override void OnPolicyAppliedAllIdle()
    {
      if (context.TurnOwner == TurnOwner.Enemies)//for ASCII/UT
      {
        context.IncreaseActions(TurnOwner.Enemies);
        //Context.Logger.LogInfo("Enemies OnPolicyAppliedAllIdle");
        base.OnPolicyAppliedAllIdle();
      }
    }

    
  }
}
