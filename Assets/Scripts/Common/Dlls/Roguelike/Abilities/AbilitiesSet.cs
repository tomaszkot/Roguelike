﻿using Roguelike.Attributes;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Abilities
{
  public class AbilitiesSet
  {
    List<PassiveAbility> passiveAbilities = new List<PassiveAbility>();
    List<ActiveAbility> activeAbilities = new List<ActiveAbility>();
        
    public AbilitiesSet()
    {
      EnsureItems();
    }

    public static AbilitiesSet CreateEmpty()
    {
      var set = new AbilitiesSet();
      set.passiveAbilities.Clear();
      set.activeAbilities.Clear();
      return set;
      
    }

    public bool IsActive(AbilityKind kind)
    {
      return activeAbilities.Where(i => i.Kind == kind).Any();
    }

    public void EnsureItems()
    {
      {
        var kinds = Enum.GetValues(typeof(AbilityKind)).Cast<AbilityKind>().ToList();
        foreach (var kind in kinds)
        {

          if (passiveAbilities.Any(i => i.Kind == kind) || kind == AbilityKind.Unset)
            continue;

          if (activeAbilities.Any(i => i.Kind == kind) || kind == AbilityKind.Unset)
            continue;

          Ability ab = null;
          if (kind == AbilityKind.ExplosiveCocktail ||
              kind == AbilityKind.ThrowingStone ||
              kind == AbilityKind.ThrowingKnife ||
              kind == AbilityKind.ThrowingTorch ||
              kind == AbilityKind.HunterTrap ||
              kind == AbilityKind.PoisonCocktail ||
              kind == AbilityKind.Stride ||
              kind == AbilityKind.OpenWound ||
              kind == AbilityKind.Rage ||
              kind == AbilityKind.WeightedNet ||
              kind == AbilityKind.Smoke ||
              kind == AbilityKind.ArrowVolley ||
              kind == AbilityKind.PiercingArrow ||
              kind == AbilityKind.PerfectHit ||
              kind == AbilityKind.Cannon ||
              kind == AbilityKind.IronSkin ||
              kind == AbilityKind.ElementalVengeance ||
              kind == AbilityKind.ZealAttack
           )
          {
            ab = new ActiveAbility() { Kind = kind };
            activeAbilities.Add(ab as ActiveAbility);
          }
          else
          {
            if (kind == AbilityKind.LootingMastering)
              ab = new LootAbility(true) { Kind = AbilityKind.LootingMastering };
            else
              ab = new PassiveAbility() { Kind = kind };

            passiveAbilities.Add(ab as PassiveAbility);
          }
        }
      }

    }

    public Ability GetByEntityStatKind(EntityStatKind esk, bool primary)
    {
      if (primary)
        return PassiveItems.Where(i => i.PrimaryStat.Kind == esk).FirstOrDefault();

      return PassiveItems.Where(i => i.AuxStat.Kind == esk).FirstOrDefault();
    }
        
    public Ability GetAbility(AbilityKind kind)
    {
      Ability ab = passiveAbilities.Where(i => i.Kind == kind).FirstOrDefault();
      if(ab == null)
        ab = activeAbilities.Where(i => i.Kind == kind).FirstOrDefault();
      return ab;
    }

    public List<PassiveAbility> PassiveItems
    {
      get
      {
        //can not called it here - deserialization doubles items!
        //if (!abilities.Any())
        //  EnsureAbilities();
        return passiveAbilities;
      }

      set
      {
        passiveAbilities = value;//for serialization
      }
    }

    public List<ActiveAbility> ActiveItems
    {
      get
      {
        return activeAbilities;
      }

      set
      {
        activeAbilities = value;//for serialization
      }
    }
  }
}
