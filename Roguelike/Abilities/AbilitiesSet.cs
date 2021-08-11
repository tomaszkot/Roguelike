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
    List<Ability> allItems = new List<Ability>();
    //Dictionary<FightItemKind, FightItem> fightItemsProps = new Dictionary<FightItemKind, FightItem>();

    public AbilitiesSet()
    {
      EnsureItems();
    }

    public void EnsureItems()
    {
      {
        var kinds = Enum.GetValues(typeof(AbilityKind)).Cast<AbilityKind>().ToList();
        foreach (var kind in kinds)
        {

          if (passiveAbilities.Any(i => i.Kind == kind) || kind == AbilityKind.Unset)
            continue;

          Ability ab = null;
          if (kind == AbilityKind.ExplosiveMastering ||
              kind == AbilityKind.ThrowingStoneMastering ||
              kind == AbilityKind.ThrowingKnifeMastering
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
          allItems.Add(ab);
        }
      }
      //var kindsAct = Enum.GetValues(typeof(AbilityKind)).Cast<ActiveAbilityKind>().ToList();
      //foreach (var kind in kindsAct)
      //{
      //  if (activeAbilities.Any(i => i.Kind == kind) || kind == ActiveAbilityKind.Unset)
      //    continue;
      //  activeAbilities.Add(new ActiveAbility() { Kind = kind });
      //  allItems.Add(passiveAbilities.Last());
      //}
      //EnsureProps();
    }

    //private void EnsureProps()
    //{
    //  if (!fightItemsProps.Any())
    //  {
    //    //fightItemsProps[FightItemKind.ExplodePotion] = explosiveCocktailPropsProvider;
    //    //fightItemsProps[FightItemKind.Knife] = throwingKnifePropsProvider;
    //    //fightItemsProps[FightItemKind.Trap] = trapPropsProvider;
    //  }
    //}

    //public FightItem GetFightItem(FightItemKind kind)
    //{
    //  EnsureProps();
    //  return fightItemsProps[kind];
    //}

    public Ability GetByEntityStatKind(EntityStatKind esk, bool primary)
    {
      if (primary)
        return PassiveItems.Where(i => i.PrimaryStat.Kind == esk).FirstOrDefault();

      return PassiveItems.Where(i => i.AuxStat.Kind == esk).FirstOrDefault();
    }

    public List<Ability> AllItems
    {
      get
      {
        //can not called it here - deserialization doubles items!
        //if (!abilities.Any())
        return allItems;
      }
    }

    public Ability GetAbility(AbilityKind kind)
    {
      return allItems.Where(i => i.Kind == kind).FirstOrDefault();
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
