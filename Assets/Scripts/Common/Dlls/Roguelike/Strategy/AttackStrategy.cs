using Dungeons.Core;
using Dungeons.Tiles;
using Roguelike.Attributes;
using Roguelike.Managers.Policies;
using Roguelike.Effects;
using Roguelike.Events;
using Roguelike.Factors;
using Roguelike.Generators;
using Roguelike.Managers;
using Roguelike.Policies;
using Roguelike.Spells;
using Roguelike.TileContainers;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

namespace Roguelike
{
    namespace Strategy
    {
        public interface ITilesAtPathProvider
    {
      List<Dungeons.Tiles.IObstacle> GetTilesAtPath(System.Drawing.Point from, System.Drawing.Point to);
    }

    public class TilesAtPathProvider : ITilesAtPathProvider
    {
      public List<Dungeons.Tiles.IObstacle> GetTilesAtPath(Point from, Point to)
      {
        return new List<Dungeons.Tiles.IObstacle>();
      }
    }

    public class AttackStrategy
    {
      GameContext context;
      public GameManager GameManager { get; set; }
      public Action<Policy> OnPolicyApplied;
      public AbstractGameLevel Node { get => context.CurrentNode; }
      public ITilesAtPathProvider TilesAtPathProvider { get; set; }

      public AttackStrategy(GameContext context, GameManager gm)
      {
        this.context = context;
        this.GameManager = gm;
        TilesAtPathProvider = context.Container.GetInstance<ITilesAtPathProvider>();
      }
            
      public bool AttackIfPossible(LivingEntity attacker, LivingEntity target)
      {
        if(target == null)
          return false;
        if (!attacker.CanAttack)
          return false;

        var enemyCasted = attacker as Enemy;
        if (enemyCasted != null)
        {
          if(enemyCasted.RessurectOrderCooldown > 0)
            enemyCasted.RessurectOrderCooldown--;

          bool resistOn = TurnOnResistAll(enemyCasted, target);
          if (resistOn)
            return true;

          if (TryUseMagicAttack(attacker, target))
            return true;

          if (TryUseSpecialSpell(attacker, target))
            return true;

          if (TryUseProjectileAttack(attacker, target))
            return true;
          else
            attacker.LastAttackWasProjectile = false;
        }

        var victim = GetPhysicalAttackVictim(attacker, target);
        if (victim != null)
        {
          var enCasted = attacker as Enemy;
          if (enCasted != null)
          {
            if (TurnOnSpecialSkill(enCasted, victim))
            {
              OnPolicyApplied(new GenericPolicy() { Kind = PolicyKind.Generic });
              return true;
            }

          }
                    
          GameManager.ApplyPhysicalAttackPolicy(attacker, target, (policy) =>
          {
            OnPolicyApplied(policy);
          }, EntityStatKind.Unset);
          
          return true;
        }

        return false;
      }


      private bool TryUseSpecialSpell(LivingEntity attacker, LivingEntity target)
      {
        if (attacker is Enemy en && attacker.Name.Contains("Skeleton") && en.PowerKind != EnemyPowerKind.Plain)
        {
          var ressurectTargets = GameManager.GetRessurectTargets(en);
          if (ressurectTargets.Any() && en.RessurectOrderCooldown == 0 && en.RessurectOrdersCounter < 3)
          {
            if (ressurectTargets.Count > 1 || en.Stats.HealthBelow(0.5f))
            {
              en.SendCommand(EntityCommandKind.RaiseMyFriends);
              return true;
            }
          }
        }
        return false;
      }

      bool IsClearPath(LivingEntity attacker, LivingEntity target)
      {
        var isClearPath = false;
        //is target next to attacker
        isClearPath = context.CurrentNode.GetNeighborTiles(attacker, true).Contains(target);
        if (!isClearPath)
        {
          if (TilesAtPathProvider != null)
          {
            var tiles = TilesAtPathProvider.GetTilesAtPath(attacker.point, target.point);
            var clear = !tiles.Any(i => i is Dungeons.Tiles.IObstacle);
            if (!clear && tiles.Count == 1 && tiles[0] is IDestroyable && RandHelper.GetRandomDouble() > 0.65)
            {
              clear = true;
            }
            if (clear)
              isClearPath = true;
          }
          else
          {
            var pathToTarget = FindPathForEnemy(attacker, target, 1, true);
            if (pathToTarget != null)
            {
              var obstacles = pathToTarget.Where(i => context.CurrentNode.GetTile(new System.Drawing.Point(i.Y, i.X)) is Dungeons.Tiles.IObstacle).ToList();
              if (!obstacles.Any())
              {
                isClearPath = true;
              }
            }
          }
        }

        return isClearPath;
      }
            
      private bool UseMagicAttack(LivingEntity attacker, LivingEntity target)
      {
        //if (attacker.IsInProjectileReach(pfi, target.Position))
        var isSmoked = this.GameManager.CurrentNode.IsAtSmoke(attacker) && attacker.DistanceFrom(target) > 1;
        if (isSmoked)
          return false;

        if (attacker.DistanceFrom(target) < attacker.MaxMagicAttackDistance)
        {
          var useMagic = IsClearPath(attacker, target);
          if (useMagic)
          {
            var applied = GameManager.SpellManager.ApplyAttackPolicy(attacker, target, attacker.ActiveManaPoweredSpellSource, null, (p) => { OnPolicyApplied(p); });

            return applied == ApplyAttackPolicyResult.OK;
          }
        }

        return false;
      }
            

      bool TurnOnSpecialSkill(Enemy enemy, LivingEntity victim)
      {

        if (enemy.Stats.HealthBelow(0.2f))
          return false;
        

        if (enemy.EntityKind == EntityKind.Animal)
        {
          if (enemy.GetLastingEffectCounter(EffectType.WildRage) == 0)
          {
            bool add = IsUnderPreassure(enemy);

            if (add)
            {
              enemy.LastingEffectsSet.AddLastingEffect(50, EntityStatKind.Strength, 4);
              return true;
            }
          }
        }
        if (enemy.EntityKind == EntityKind.Undead)
        {
          if (enemy.GetLastingEffectCounter(EffectType.Hooch) == 0)
          {
            bool add = IsUnderPreassure(enemy);

            if (add)
            {
              enemy.Consume(new Hooch());
              return true;
            }
          }
        }
        else if (enemy.tag1.StartsWith("druid"))
        {
          if (!enemy.EverSummonedChild && IsUnderPreassure(enemy))
          {
            var enemyChild = GameManager.CurrentNode.SpawnEnemy(enemy.Level, GameManager.GameState.CoreInfo.Difficulty);
            var ept = GameManager.CurrentNode.GetClosestEmpty(enemy);
            this.GameManager.AppendEnemy(enemyChild, ept.point, enemy.Level);
            GameManager.AppendAction(new LivingEntityAction()
            {
              Kind = LivingEntityActionKind.SummonedBuddy,
              InvolvedEntity = enemy,
              Info = enemy.Name + " has summoned a buddy " + enemyChild.Name
            });
            enemy.EverSummonedChild = true;
          }
        }

        var cast = RandHelper.Random.NextDouble() <= GenerationInfo.ChanceToTurnOnSpecialSkillByEnemy;
        if (cast && enemy.HasAnyEffectToUse())//normally boss and chemp have these
        {
          var canCastWeaken = enemy.HasEffectToUse(EffectType.Weaken) && !victim.LastingEffects.Any(i => i.Type == EffectType.Weaken
          );
          var canCastInaccuracy = enemy.HasEffectToUse(EffectType.Inaccuracy) && !victim.LastingEffects.Any(i => i.Type == EffectType.Inaccuracy
          );
          int specialEffCounter = 0;
          var hasSpecialEff = HasSpecialEffectOn(enemy, out specialEffCounter);
          if (specialEffCounter == 0 || canCastWeaken || canCastInaccuracy || (specialEffCounter == 1 && enemy.Stats.HealthBelow(0.4f)))
          {
            EffectType effectToUse = EffectType.Unset;
            if (specialEffCounter > 0)
            {
              if (canCastWeaken)
              {
                effectToUse = EffectType.Weaken;
              }
              else
                return false;
            }
            else
              effectToUse = enemy.GetRandomEffectToUse(canCastWeaken, canCastInaccuracy);

            if (effectToUse == EffectType.Unset)
              return false;

            if (victim.LastingEffects.Any(i => i.Type == effectToUse))
            {
              //GameManager.Instance.AppendUnityLog(enemy.Name + " victim.LastingEffects.Any(i => i.Type == effectToUse) " + effectToUse);
              return false;//
            }

            var spellKind = SpellConverter.SpellKindFromEffectType(effectToUse);

            if (spellKind != SpellKind.Unset)
            {
              LivingEntity lastingEffectTarget = enemy;
              if (spellKind == SpellKind.Weaken || spellKind == SpellKind.Inaccuracy)
                lastingEffectTarget = victim;

              lastingEffectTarget.AddLastingEffectFromSpell(spellKind, effectToUse);
              enemy.ReduceEffectToUse(effectToUse);

              return true;
            }
          }
        }

        return false;
      }

      private static bool IsUnderPreassure(Enemy enemy)
      {
        bool add = false;
        if (enemy.Stats.HealthBelow(0.5f) && RandHelper.GetRandomDouble() > 0.5f)
          add = true;
        if (enemy.Stats.HealthBelow(0.3f) && RandHelper.GetRandomDouble() > 0.2f)
          add = true;
        return add;
      }

      bool HasSpecialEffectOn(Enemy enemy, out int specialEffCounter)
      {
        specialEffCounter = 0;
        //var rage = enemy.LastingEffects.Any(i => i.Type == EffectType.Rage);
        var ironSkin = enemy.LastingEffects.Any(i => i.Type == EffectType.IronSkin);
        var resistAll = enemy.LastingEffects.Any(i => i.Type == EffectType.ResistAll);
        //if (rage)
        //  specialEffCounter++;
        if (ironSkin)
          specialEffCounter++;
        if (resistAll)
          specialEffCounter++;
        if (specialEffCounter > 0)//do not accumulate effects
          return true;

        return false;
      }

      //List<LivingEntity> everUsedProjectile = new List<LivingEntity>();

      private bool TryUseProjectileAttack(LivingEntity attacker, LivingEntity target)
      {
        var nameLower = attacker.Name.ToLower();
        var allow = nameLower.Contains("bandit") || nameLower.StartsWith("skeleton") || nameLower.Contains("druid") ||
          nameLower.Contains("drowned") || nameLower.Contains("lava el");
        if (!allow)
          return false;

        var isSmoked = this.GameManager.CurrentNode.IsAtSmoke(attacker) && attacker.DistanceFrom(target) > 1;
        if (isSmoked)
          return false;

        if (attacker.LastAttackWasProjectile && RandHelper.GetRandomDouble() > 0.1)
          return false;

        var skip = true;
        if (attacker.Stats.HealthBelow(0.5f) && !attacker.EverUsedFightItem)
          skip = false;

        if (skip && RandHelper.GetRandomDouble() < 0.5)
          skip = false;

        if (skip)
          return false;
        var enemy = attacker as Enemy;
        var fi = enemy.ActiveFightItem;
        if (fi != null && fi.Count > 0)
        {
          var pfi = fi as ProjectileFightItem;
          //if (pfi.FightItemKind == FightItemKind.Stone ||
          //    pfi.FightItemKind == FightItemKind.ThrowingKnife)
          {
            if(enemy.DistanceFrom(target) <=1.5)
              return false;
          }
          if (attacker.IsInProjectileReach(pfi, target.Position))
          {
            var useProjectile = IsClearPath(attacker, target);
            if (useProjectile)
            {
              if (GameManager.ProjectileFightItemPolicyManager.ApplyAttackPolicy(enemy, target, pfi, null, (p) => { OnPolicyApplied(p); }))
              {
                //GameManager.Logger.LogInfo(enemy +" used pfi " + pfi + " count:" + pfi.Count);
                enemy.LastAttackWasProjectile = true;
                return true;
              }
              else
                GameManager.Assert(false);
            }
          }
        }

        return false;
      }

      bool TryUseMagicAttack(LivingEntity enemy, LivingEntity victim)
      {
        var en = enemy as Enemy;
        if (enemy.ActiveScrollCoolDownCounter > 0)
        {
          bool decreaseCoolDown = RandHelper.Random.NextDouble() > .3f;
          if (en.PowerKind == EnemyPowerKind.Boss)
            decreaseCoolDown = RandHelper.Random.NextDouble() > .5f;
          if (decreaseCoolDown)
            enemy.ActiveScrollCoolDownCounter--;
        }
        if (enemy.ActiveManaPoweredSpellSource != null && enemy.ActiveScrollCoolDownCounter == 0)
        {
          if (UseMagicAttack(enemy, victim))
          {
            enemy.ActiveScrollCoolDownCounter = GetCoolDown(enemy as Enemy);
            return true;
          }
        }
        return false;
      }


      public List<Algorithms.PathFinderNode> FindPathForEnemy(LivingEntity enemy, LivingEntity target, int startIndex = 0, bool forEnemyProjectile = false)
      {
        var pathToTarget = context.CurrentNode.FindPath(enemy.point, target.point, false, true, forEnemyProjectile, enemy);
        if (pathToTarget != null && pathToTarget.Any())
        {
          return pathToTarget.GetRange(startIndex, pathToTarget.Count - 1);
        }

        return pathToTarget;
      }

      private int GetCoolDown(Enemy enemy)
      {
        //if (enemy.Symbol == Enemy.GolemSymbol || enemy.PlainSymbol == Enemy.GolemSymbol)
        //  return 3;
        //else if (enemy.Symbol == Enemy.GardenQueenSymbol || enemy.PlainSymbol == Enemy.GardenQueenSymbol)
        //  return 5;
        return 2;
      }

      private bool TurnOnResistAll(LivingEntity enemy, LivingEntity target)
      {
        //TODO
        //Enemy enCasted = enemy as Enemy;
        //if (enCasted != null && enCasted.lastHitBySpell)
        //{
        //  if (enCasted.Kind != Enemy.PowerKind.Plain)
        //  {
        //    int specialEffCounter;
        //    var resist = enemy.LastingEffects.Any(i => i.Type == LivingEntity.EffectType.ResistAll);
        //    if (resist)
        //      return false;
        //    HasSpecialEffectOn(enCasted, out specialEffCounter);
        //    if (specialEffCounter < 2)
        //    {
        //      if (enCasted.GetEffectUseCount(EffectType.ResistAll) > 0)
        //      {
        //        var spell = new ResistAllSpell(enemy);
        //        //TODO EntityStatKind.ResistCold, whatever we send here is OK, later all are aplied
        //        enemy.AddLastingEffect(EffectType.ResistAll, spell.TourLasting, EntityStatKind.ResistCold, spell.Factor);
        //        enCasted.ReduceEffectToUse(EffectType.ResistAll);
        //        return true;
        //      }
        //    }
        //  }
        //}

        return false;
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
}
