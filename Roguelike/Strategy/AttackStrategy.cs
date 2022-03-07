using Dungeons.Core;
using Dungeons.Tiles;
using Roguelike.Effects;
using Roguelike.Generators;
using Roguelike.Managers;
using Roguelike.Policies;
using Roguelike.Spells;
using Roguelike.TileContainers;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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

      //public GameContext Context { get => context; set => context = value; }

      public bool AttackIfPossible(LivingEntity attacker, LivingEntity target)
      {
        if (!attacker.CanAttack)
          return false;

        var enemyCasted = attacker as Enemy;
        if (enemyCasted != null)
        {
          bool resistOn = TurnOnResistAll(enemyCasted, target);
          if (resistOn)
            return true;

          if (TryUseMagicAttack(attacker, target))
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
              OnPolicyApplied(new Policy() { Kind = PolicyKind.Generic });
              return true;
            }

          }
                    
          GameManager.ApplyPhysicalAttackPolicy(attacker, target, (policy) =>
          {
            OnPolicyApplied(policy);
          });
          
          return true;
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
            if (!tiles.Any(i => i is Dungeons.Tiles.IObstacle))
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
        if (attacker.DistanceFrom(target) < attacker.MaxMagicAttackDistance)
        {
          var useMagic = IsClearPath(attacker, target);
          if (useMagic)
          {
            GameManager.SpellManager.ApplyAttackPolicy(attacker, target, attacker.ActiveManaPoweredSpellSource, null, (p) => { OnPolicyApplied(p); });

            return true;
          }
        }

        return false;
      }

      bool TurnOnSpecialSkill(Enemy enemy, LivingEntity victim)
      {
        if (/*victim.HeroAlly ||*/ victim is CrackedStone)
          return false;
        if (enemy.Stats.HealthBelow(0.2f))
          return false;
        var cast = RandHelper.Random.NextDouble() <= GenerationInfo.ChanceToTurnOnSpecialSkillByEnemy;

        //if (enemy.PowerKind == EnemyPowerKind.Plain && !enemy.EverCastedHooch)
        //{
        //  if (enemy.GetLastingEffect(EffectType.Hooch) == null)
        //  {

        //    bool addHooch = enemy.RoomKind == RoomKind.PuzzleRoom;
        //    if (!addHooch)
        //      addHooch = enemy.RoomKind == RoomKind.Island;// && CommonRandHelper.GetRandomDouble() > 0.5f;
        //    if (addHooch)
        //    {

        //      //little cheat
        //      //var he = enemy.GetCurrentValue(EntityStatKind.Health);
        //      //enemy.ReduceHealth(-he * 0.33f);
        //      var nh = enemy.Stats.GetNominal(EntityStatKind.Health);
        //      enemy.Stats.SetNominal(EntityStatKind.Health, nh * 1.33f);
        //      var he1 = enemy.GetCurrentValue(EntityStatKind.Health);

        //      enemy.AddLastingEffect(EffectType.Hooch, 7, 0);
        //      enemy.EverCastedHooch = true;

        //      return true;
        //    }
        //  }
        //}
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
        var allow = nameLower.Contains("bandit") || nameLower.Contains("skeleton") || nameLower.Contains("druid") ||
          nameLower.Contains("drowned");
        if (!allow)
          return false;

        if(attacker.LastAttackWasProjectile && RandHelper.GetRandomDouble() > 0.1)
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
          if (pfi.FightItemKind == FightItemKind.Stone ||
              pfi.FightItemKind == FightItemKind.ThrowingKnife)
          {
            if(enemy.DistanceFrom(target) <=1.5)
              return false;
          }
          if (attacker.IsInProjectileReach(pfi, target.Position))
          {
            var useProjectile = IsClearPath(attacker, target);
            if (useProjectile)
            {
              if (GameManager.ApplyAttackPolicy(enemy, target, pfi, null, (p) => { OnPolicyApplied(p); }))
              {
                enemy.RemoveFightItem(fi);
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
        var pathToTarget = context.CurrentNode.FindPath(enemy.point, target.point, false, true, forEnemyProjectile);
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
