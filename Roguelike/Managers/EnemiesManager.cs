using Dungeons.Core;
using Dungeons.Tiles;
using Roguelike.Events;
using Roguelike.Policies;
using Roguelike.Strategy;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
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
    AlliesManager alliesManager;

    public AlliesManager AlliesManager { get => alliesManager; set => alliesManager = value; }

    public EnemiesManager(GameContext context, EventsManager eventsManager, Container container, AlliesManager alliesManager, GameManager gm) :
      base(TurnOwner.Enemies, context, eventsManager, container, gm)
    {
      this.AlliesManager = alliesManager;
      this.context = context;

      context.TurnOwnerChanged += OnTurnOwnerChanged;
      context.ContextSwitched += Context_ContextSwitched;
    }

    public virtual List<Enemy> GetActiveEnemies()
    {
      return this.AllEntities.Where(i => i.Revealed && i.Alive).Cast<Enemy>().ToList();
    }

    public virtual List<Enemy> GetEnemies()
    {
      return this.AllEntities.Cast<Enemy>().ToList();
    }

    private void Context_ContextSwitched(object sender, ContextSwitch e)
    {
      var entities = Context.CurrentNode.GetTiles<LivingEntity>().Where(i=> i is Enemy && !i.HeroAlly).ToList();
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

    public void MoveAwayFromHero(LivingEntity enemy)
    {
      var possibleMoves = Level.GetNeighborTiles(enemy).Where(i => i.IsEmpty).ToList();
      if (possibleMoves.Any())
      {
        var md = possibleMoves.Max(i => i.DistanceFrom(Hero));
        var tile = possibleMoves.Where(i => i.DistanceFrom(Hero) == md).FirstOrDefault();
        if (tile != null)
          MoveEntity(enemy, tile.point);
      }
    }

    public override void MakeTurn(LivingEntity enemy)
    {
      var target = Hero;

      if (enemy.LastingEffects.Any(i => i.Type ==  Effects.EffectType.Frighten))
      {
        MoveAwayFromHero(enemy);
        return;
      }

      if (AttackAlly(enemy))
        return;

      if (enemy.DistanceFrom(Hero) > 10)
        return;

      if (!target.IsTransformed())
      {
        //if (CastEffectsForAllies(entity))
        //  return;
        if(MakeEmergencyTeleport(enemy as Enemy))
          return;

        if (AttackIfPossible(enemy, target))
          return;

        if (enemy.HasRelocateSkill)
        {
          //if (TryRelocate(enemy))
          //  return;
        }
      }

      bool makeRandMove = false;
      if (ShallChaseTarget(enemy, target))
      {
        makeRandMove = !MakeMoveOnPath(enemy, target.point, false);
      }
      else
        makeRandMove = true;
      if (makeRandMove)
      {
        MakeRandomMove(enemy);
      }
    }

    //bool TryRelocate(LivingEntity li)
    //{
    //  Enemy enemy = li as Enemy;
    //  if (enemy.ShallRelocate(Hero))
    //  {
    //    var firstEmpty = enemy.GetTeleportTile(Level);
    //    if (firstEmpty != null)
    //    {
    //      if (enemy.PlainSymbol == Enemy.VampireSymbol || enemy.PlainSymbol == FallenOne.FallenOneSymbol)
    //      {
    //        TeleportEnemy(enemy, firstEmpty);
    //      }
    //      else
    //      {
    //        if (enemy.Move(firstEmpty.point))
    //          enemy.Relocatting = true;
    //        if (enemy.HerdMember != null && !enemy.PendingForRellocation)
    //        {
    //          //call others
    //          var herdMembers = GetHerdMembers(enemy);
    //          //gm.AppendRedLog("herdMembers = "+ herdMembers.Count);
    //          foreach (var hm in herdMembers)
    //          {
    //            hm.PendingForRellocation = true;
    //          }
    //        }

    //        enemy.PendingForRellocation = false;
    //      }
    //      enemy.TeleportCounter++;

    //      return true;
    //    }
    //  }

    //  return false;
    //}

    public bool AttackIfPossible(LivingEntity entity, LivingEntity target)
    {
      return attackStrategy.AttackIfPossible(entity, target);
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
          if (MakeMoveOnPath(entity, entity.InitialPoint, false))
          {
            if(entity.point == entity.InitialPoint)
              entity.MoveKind = EntityMoveKind.Freestyle;

            return;
          }
        }
      }
      base.MakeRandomMove(entity);
    }
            
    protected override bool MoveEntity(LivingEntity entity, Point newPos, List<Point> fullPath = null)
    {
      if (entity.InitialPoint == LivingEntity.DefaultInitialPoint)
      {
        entity.InitialPoint = entity.point;
      }

      var moved = base.MoveEntity(entity, newPos, fullPath);
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

    bool AttackAlly(LivingEntity enemy)
    {
      var ally = AlliesManager.AllEntities.Where(i=>i.DistanceFrom(enemy) < 2).FirstOrDefault();
      if (ally != null)
      {
        if (RandHelper.Random.NextDouble() < 0.3f)
          return AttackIfPossible(enemy, ally);
      }
      return false;
    }

    private bool MakeEmergencyTeleport(Enemy enemy)
    {
      if (enemy.PowerKind !=  EnemyPowerKind.Plain)
      {
        var enCasted = enemy;
        if (enCasted.Stats.HealthBelow(.6f)
          && enCasted.NumberOfEmergencyTeleports > 0
          && !Level.GetNeighborTiles(enCasted).Any(i => i is Hero)
          )
        {
          var free = gameManager.CurrentNode.GetClosestEmpty(Hero);
          if (free != null && !gameManager.CurrentNode.GetNeighborTiles(enemy).Any(i => i == free))//does not make sense to teleport to next cell
          {
            TeleportEnemy(enCasted, free);
            enCasted.NumberOfEmergencyTeleports--;
            return true;
          }
        }
      }

      return false;
    }

    private TileContainers.AbstractGameLevel Level => gameManager.CurrentNode;

    public void TeleportEnemy(Enemy enemy, Tile firstEmpty)
    {
      Level.SetTile(enemy, firstEmpty.point);
      gameManager.AppendAction(new EnemyAction() { Info = enemy.Name + " has teleported", Enemy = enemy as Enemy, Kind = EnemyActionKind.Teleported });
      gameManager.SoundManager.PlaySound("teleport");
    }

    //private bool CastEffectsForAllies(LivingEntity enemy)
    //{
    //  var enCasted = enemy as Enemy;
    //  if (enCasted == null || enCasted.PowerKind != EnemyPowerKind.Champion)
    //    return false;
    //  if (enCasted.NumberOfCastedEffectsForAllies > 0)
    //    return false;

    //  if (enCasted.HerdMember == null)
    //    return false;

    //  if (enCasted.DistanceFrom(gm.Hero) > 4)
    //    return false;

    //  var members = GetHerdMembers(enCasted);
    //  if (members.Count(i => i.DistanceFrom(gm.Hero) < 2) == 0)
    //  {
    //    return false;
    //  }

    //  if (members.Any())
    //  {
    //    bool castRage = (RandHelper.Random.NextDouble() > .5);
    //    foreach (var mem in members)
    //    {
    //      if (castRage)
    //      {
    //        var spell = new RageSpell(enCasted);
    //        mem.AddLastingEffect(EffectType.Rage, spell.TourLasting, spell.Damage);
    //      }
    //      else
    //      {
    //        var spell = new IronSkinSpell(enCasted);
    //        mem.AddLastingEffect(EffectType.IronSkin, spell.TourLasting, EntityStatKind.Defence, spell.Factor);
    //      }
    //    }
    //    enCasted.NumberOfCastedEffectsForAllies++;
    //    return true;
    //  }
    //  return false;
    //}
  }
}
