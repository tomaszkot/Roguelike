using Dungeons;
using Dungeons.Tiles;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Roguelike.Managers
{
  public class EnemiesManager : EntitiesManager
  {
    //GameContext context;
    List<LivingEntity> enemies;
    public List<LivingEntity> Enemies { get => enemies; set => enemies = value; }
   

    public EnemiesManager(GameContext context, EventsManager eventsManager) :
      base(context, eventsManager)
    {
      this.context = context;
      context.TurnOwnerChanged += OnTurnOwnerChanged;
      context.ContextSwitched += Context_ContextSwitched;
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
        var target = Hero;
        if (AttackIfPossible(enemy, target))
        {
          continue;
        }
        //context.Logger.LogInfo("!AttackIfPossible ...");
        bool makeRandMove = false;
        if (ShallChaseTarget(enemy, target))
        {
          makeRandMove = !MakeMoveOnPath(enemy, target);
        }
        else
          makeRandMove = true;
        if (makeRandMove)
        {
          MakeRandomMove(enemy);
        }
      }

    }

    private bool MakeMoveOnPath(LivingEntity enemy, Hero target)
    {
      bool forHeroAlly = false;
      bool moved = false;
      enemy.PathToTarget = Node.FindPath(enemy.Point, target.Point, forHeroAlly, true);
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

    private bool AttackIfPossible(LivingEntity enemy, Hero hero)
    {
      var victim = GetPhysicalAttackVictim(enemy, hero);
      if (victim != null)
      {
        var enemyAttackPollicy = AttackPolicy(enemy, hero);
        enemyAttackPollicy.OnApplied += (s,e)=>OnPolicyApplied(e);
        enemyAttackPollicy.Apply();
        //if (enCasted != null)
        //  gm.AppendAction(new EnemyAction() { KindValue = EnemyAction.Kind.AttackingHero, Enemy = enCasted })/*;*/

        return true;
      }

      return false;
    }

    protected override bool MoveEntity(LivingEntity entity, Point newPos)
    {
      var moved = base.MoveEntity(entity, newPos);
      return moved;
    }

    protected override void OnPolicyAppliedAllIdle()
    {
      if (context.TurnOwner == TurnOwner.Enemies)//for ASCII/UT
      {
        context.IncreaseActions(TurnOwner.Enemies);
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
