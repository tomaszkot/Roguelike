using Dungeons.Core;
using Dungeons.Tiles;
using Roguelike.Abstract;
using Roguelike.Attributes;
using Roguelike.Effects;
using Roguelike.Policies;
using Roguelike.Spells;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Roguelike
{
  namespace Strategy
  {
    class AttackStrategy
    {
      GameContext context;
      public Action<Policy> OnPolicyApplied;
      public AbstractGameLevel Node { get => context.CurrentNode; }
      
      public AttackStrategy(GameContext context)
      {
        this.context = context;
      }

      public GameContext Context { get => context; set => context = value; }

      public bool AttackIfPossible(LivingEntity attacker, LivingEntity target)
      {
        if (!attacker.CanAttack)
          return false;

        if (MakeNonPhysicalMove(attacker, target))
          return true;

        var enemyCasted = attacker as Enemy;
        if (enemyCasted != null)
        {
          if (enemyCasted.PrefferedFightStyle == PrefferedFightStyle.Magic)
          {
            if (attacker.DistanceFrom(target) < 8)//TODO
            {
              var scroll = new Scroll(Spells.SpellKind.FireBall);
              Context.ApplySpellAttackPolicy(attacker, target, scroll, null,
                (p) => { OnPolicyApplied(p); }
              );

              return true;
            }
          }
        }

        var victim = GetPhysicalAttackVictim(attacker, target);
        if (victim != null)
        {
          var enCasted = attacker as Enemy;
          if (enCasted != null)
          {
            if (TurnOnSpecialSkill(enCasted, victim))
            {
              OnPolicyApplied(new Policy() { Kind = PolicyKind.Generic});
              return true;
            }

          }

          if (Context != null)
          {
            Context.ApplyPhysicalAttackPolicy(attacker, target, (pol) =>
            {
              OnPolicyApplied(pol);
            }
            );
          }
          return true;
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
        var rage = enemy.LastingEffects.Any(i => i.Type == EffectType.Rage);
        var ironSkin = enemy.LastingEffects.Any(i => i.Type == EffectType.IronSkin);
        var resistAll = enemy.LastingEffects.Any(i => i.Type == EffectType.ResistAll);
        if (rage)
          specialEffCounter++;
        if (ironSkin)
          specialEffCounter++;
        if (resistAll)
          specialEffCounter++;
        if (specialEffCounter > 0)//do not accumulate effects
          return true;

        return false;
      }

      bool MakeNonPhysicalMove(LivingEntity enemy, LivingEntity hero)
      {
        //TODO

        if (enemy.ActiveScrollCoolDownCounter > 0)
        {
          var en = enemy as Enemy;

          bool decreaseCoolDown = RandHelper.Random.NextDouble() > .3f;
          if (en.PowerKind == EnemyPowerKind.Boss)
            decreaseCoolDown = RandHelper.Random.NextDouble() > .5f;
          if (decreaseCoolDown)
            enemy.ActiveScrollCoolDownCounter--;
        }
        if (enemy.ActiveScroll != null && enemy.ActiveScrollCoolDownCounter == 0)
        {
          var level = Context.CurrentNode;
          enemy.PathToTarget = level.FindPath(enemy.Point, hero.Point, false, true);
          if (enemy.PathToTarget != null)
          {
            var path = enemy.PathToTarget.GetRange(0, enemy.PathToTarget.Count - 1);
            if (path.Any())
            {
              var clearPath = path.All(i =>
                level.GetTile(new System.Drawing.Point(i.Y, i.X)) == enemy ||
                level.GetTile(new System.Drawing.Point(i.Y, i.X)).IsEmpty
              );

              if (clearPath)
              {
                var first = path.FirstOrDefault();
                var straithPath = path.All(i => i.X == first.X || i.Y == first.Y);
                if (straithPath)
                {
                  if (enemy.DistanceFrom(hero) < 5 || (enemy.Point.Y == hero.Point.Y && enemy.DistanceFrom(hero) < 7)) //|| VisibleFromCamera TODO
                  {
                    //var spell = enemy.ActiveScroll.CreateSpell(enemy);
                    //if (spell is OffensiveSpell)
                    //  enemy.DamageApplier.ApplySpellDamage(enemy, hero, spell as AttackingSpell);
                    //else
                    //  hero.AddLastingEffect(EffectType.BushTrap, 3, 3);
                    //enemy.ActiveScrollCoolDownCounter = GetCoolDown(enemy as Enemy);
                    //return true;
                    return false;//TODO
                  }
                }
              }
            }
          }
        }

        bool resistOn = TurnOnResistAll(enemy, hero);
        if (resistOn)
        {
          return true;
        }

        return false;
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
