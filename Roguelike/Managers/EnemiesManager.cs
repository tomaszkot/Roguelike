using Dungeons.Tiles;
using Roguelike.Policies;
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
    //GameContext context;
    List<LivingEntity> enemies;
    public List<LivingEntity> Enemies { get => enemies; set => enemies = value; }
    

    public EnemiesManager(GameContext context, EventsManager eventsManager, Container container) :
      base(context, eventsManager, container)
    {
      this.context = context;
      context.TurnOwnerChanged += OnTurnOwnerChanged;
      context.ContextSwitched += Context_ContextSwitched;
    }

    public List<Enemy> GetEnemies()
    {
      return Enemies.Cast<Enemy>().ToList();
    }

    private void Context_ContextSwitched(object sender, ContextSwitch e)
    {
      //Enemies = Context.CurrentNode.GetTiles<Enemy>();
      enemies = Context.CurrentNode.GetTiles<LivingEntity>().Where(i=> i is Enemy).ToList();
      base.SetEntities(enemies);
    }

    private void OnTurnOwnerChanged(object sender, TurnOwner turnOwner)
    {
      //if(turnOwner == TurnOwner.Enemies)
      // MakeEntitiesMove();
    }

    public void MakeMove(LivingEntity enemy)
    {
    }

    public override void MakeEntitiesMove(LivingEntity skip = null)
    {
      var enemies = this.Enemies.Where(i => i.Revealed && i.Alive).ToList();
      //context.Logger.LogInfo("MakeEntitiesMove "+ enemies.Count);
      if (!enemies.Any())
      {
        OnPolicyAppliedAllIdle();
        return;
      }
      foreach (var enemy in enemies)
      {
        Debug.Assert(context.CurrentNode.GetTiles<Enemy>().Any(i => i == enemy));
        if (enemy == enemies.Last())
        {
          int kk = 0;
          kk++;
        }
        var target = Hero;
        if (AttackIfPossible(enemy, target))
        {
          continue;
        }
        //context.Logger.LogInfo("!AttackIfPossible ...");
        bool makeRandMove = false;
        if (ShallChaseTarget(enemy, target))
        {
          makeRandMove = !MakeMoveOnPath(enemy, target.Point);
        }
        else
          makeRandMove = true;
        if (makeRandMove)
        {
          MakeRandomMove(enemy);
        }
      }

      var notIdle = entities.Where(i => i.State != EntityState.Idle).ToList();
      if (notIdle.Any())
        OnPolicyAppliedAllIdle();//Barrels() ut failed witout it
    }

    public override void MakeRandomMove(LivingEntity entity)
    {
      if (entity.InitialPoint != LivingEntity.DefaultInitialPoint)
      {
        var distFromInitPoint = entity.DistanceFrom(entity.InitialPoint);
        if (distFromInitPoint > 5)
        {
          if (MakeMoveOnPath(entity, entity.InitialPoint))
            return;
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
     // return false;
    }

    private bool ShallChaseTarget(LivingEntity enemy, Hero target)
    {
      if (enemy.EverHitBy.Contains(target))
        return true;

      var dist = enemy.DistanceFrom(target);
      if(dist < 5)
        return true;
      
      return false;
    }

    public bool AttackIfPossible(LivingEntity enemy, Hero hero)
    {
      var enemyCasted = enemy as Enemy;
      if (enemyCasted.PrefferedFightStyle == PrefferedFightStyle.Magic)
      {
        if(enemyCasted.DistanceFrom(hero) < 8)//TODO
        {
          var scroll = new Scroll(Spells.SpellKind.FireBall);
          Context.ApplySpellAttackPolicy(enemy, hero, scroll, null,
            (p) => { OnPolicyApplied(p); }
          );

          return true;
        }
      }

      var victim = GetPhysicalAttackVictim(enemy, hero);
      if (victim != null)
      {
        Context.ApplyPhysicalAttackPolicy(enemy, hero, (pol)=> OnPolicyApplied(pol));
        return true;
      }

      return false;
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

    private LivingEntity GetPhysicalAttackVictim(LivingEntity enemy, LivingEntity target)
    {
      var targetNeibs = Node.GetNeighborTiles(target);
      LivingEntity victim = null;
      if (CanAttackTarget(targetNeibs, enemy))
      {
        victim = target;
      }

      return victim;
     // return null;
    }

    private bool CanAttackTarget(List<Tile> neibs, LivingEntity enemy)
    {
      return neibs.Any(i => i == enemy);
    }
  }
}
