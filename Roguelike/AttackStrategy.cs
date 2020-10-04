using Dungeons.Core;
using Dungeons.Tiles;
using Roguelike.Attributes;
using Roguelike.Policies;
using Roguelike.Spells;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Linq;

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

      public bool AttackIfPossible(LivingEntity enemy, Hero hero)
      {
        if (!enemy.CanAttack)
          return false;

        if (MakeNonPhysicalMove(enemy, hero))
          return true;

        var enemyCasted = enemy as Enemy;
        if (enemyCasted.PrefferedFightStyle == PrefferedFightStyle.Magic)
        {
          if (enemyCasted.DistanceFrom(hero) < 8)//TODO
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
          var enCasted = enemy as Enemy;
          if (enCasted != null)
          {
            if (TurnOnSpecialSkill(enCasted, victim))
              return true;

          }

          Context.ApplyPhysicalAttackPolicy(enemy, hero, (pol) => OnPolicyApplied(pol));
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
        var cast = RandHelper.Random.NextDouble() < GenerationInfo.ChanceToTurnOnSpecialSkillByEnemy;

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

            if (effectToUse == EffectType.Rage)
            {
              var spell = new RageSpell(enemy);
              enemy.AddLastingEffect(effectToUse, spell.TourLasting, EntityStatKind.Attack, spell.Factor);
            }
            else if (effectToUse == EffectType.Weaken)
            {
              var spell = new WeakenSpell(enemy);
              victim.AddLastingEffect(effectToUse, spell.TourLasting, EntityStatKind.Defence, spell.Factor);
            }
            else if (effectToUse == EffectType.Inaccuracy)
            {
              var spell = new InaccuracySpell(enemy);
              victim.AddLastingEffect(effectToUse, spell.TourLasting, EntityStatKind.ChanceToHit, spell.Factor);
            }
            else if (effectToUse == EffectType.IronSkin)
            {
              var spell = new IronSkinSpell(enemy);
              enemy.AddLastingEffect(effectToUse, spell.TourLasting, EntityStatKind.Defence, spell.Factor);
            }
            else //if (effectToUse == EffectType.ResistAll)
            {
              var spell = new ResistAllSpell(enemy);
              enemy.AddLastingEffect(effectToUse, spell.TourLasting, EntityStatKind.ResistCold, spell.Factor);//TODO EntityStatKind.ResistCold, whatever we send here is OK, later all are aplied
            }
            
            enemy.ReduceEffectToUse(effectToUse);
            return true;
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

      bool MakeNonPhysicalMove(LivingEntity enemy, LivingEntity target)
      {
        //TODO

        //if (enemy.ActiveScrollCoolDownCounter > 0)
        //{
        //  var en = enemy as Enemy;

        //  bool decreaseCoolDown = CommonRandHelper.Random.NextDouble() > .3f;
        //  if (en.Kind == Enemy.PowerKind.Boss)
        //    decreaseCoolDown = CommonRandHelper.Random.NextDouble() > .5f;
        //  if (decreaseCoolDown)
        //    enemy.ActiveScrollCoolDownCounter--;
        //}
        //if (enemy.ActiveScroll != null && enemy.ActiveScrollCoolDownCounter == 0)
        //{
        //  enemy.PathToTarget = Level.FindPath(enemy.point, target.point, false, true);
        //  if (enemy.PathToTarget != null)
        //  {
        //    var path = enemy.PathToTarget.GetRange(0, enemy.PathToTarget.Count - 1);
        //    if (path.Any())
        //    {
        //      var clearPath = path.All(i =>
        //        gm.Level.GetTile(new Point(i.Y, i.X)) == enemy ||
        //        gm.Level.GetTile(new Point(i.Y, i.X)).IsEmpty
        //      );

        //      if (clearPath)
        //      {
        //        var first = path.FirstOrDefault();
        //        var straithPath = path.All(i => i.X == first.X || i.Y == first.Y);
        //        if (straithPath)
        //        {
        //          if (enemy.DistanceFrom(gm.Hero) < 5 || (enemy.point.y == gm.Hero.point.y && enemy.DistanceFrom(gm.Hero) < 7)) //|| VisibleFromCamera TODO
        //          {
        //            var spell = enemy.ActiveScroll.CreateSpell(enemy);
        //            if (spell is AttackingSpell)
        //              enemy.DamageApplier.ApplySpellDamage(enemy, target, spell as AttackingSpell);
        //            else
        //              target.AddLastingEffect(EffectType.BushTrap, 3, 3);
        //            enemy.ActiveScrollCoolDownCounter = GetCoolDown(enemy as Enemy);
        //            return true;
        //          }
        //        }
        //      }
        //    }
        //  }
        //}

        //bool resistOn = TurnOnResistAll(enemy, target);
        //if (resistOn)
        //{
        //  return true;
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
