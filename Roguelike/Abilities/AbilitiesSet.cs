using Roguelike.Attributes;
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
        var kinds = Enum.GetValues(typeof(PassiveAbilityKind)).Cast<PassiveAbilityKind>().ToList();
        foreach (var kind in kinds)
        {
          if (passiveAbilities.Any(i => i.Kind == kind) || kind == PassiveAbilityKind.Unset)
            continue;
          if (kind == PassiveAbilityKind.LootingMastering)
            passiveAbilities.Add(new LootAbility(true) { Kind = PassiveAbilityKind.LootingMastering });
          else
            passiveAbilities.Add(new PassiveAbility() { Kind = kind });

          allItems.Add(passiveAbilities.Last());
        }
      }
      var kindsAct = Enum.GetValues(typeof(ActiveAbilityKind)).Cast<ActiveAbilityKind>().ToList();
      foreach (var kind in kindsAct)
      {
        if (activeAbilities.Any(i => i.Kind == kind) || kind == ActiveAbilityKind.Unset)
          continue;
        activeAbilities.Add(new ActiveAbility() { Kind = kind });
        allItems.Add(passiveAbilities.Last());
      }
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
