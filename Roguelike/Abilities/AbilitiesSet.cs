using Roguelike.Attributes;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Abilities
{
  public class AbilitiesSet
  {
    List<PassiveAbility> abilities = new List<PassiveAbility>();
    //ExplosiveCocktail explosiveCocktailPropsProvider = new ExplosiveCocktail();
    //ThrowingKnife throwingKnifePropsProvider = new ThrowingKnife();
    //Trap trapPropsProvider = new Trap();
    Dictionary<FightItemKind, FightItem> fightItemsProps = new Dictionary<FightItemKind, FightItem>();

    public AbilitiesSet()
    {
      EnsureItems();
    }

    public void EnsureItems()
    {

      var kinds = Enum.GetValues(typeof(PassiveAbilityKind)).Cast<PassiveAbilityKind>().ToList();
      foreach (var kind in kinds)
      {
        if (abilities.Any(i => i.Kind == kind) || kind == PassiveAbilityKind.Unset)
          continue;
        if (kind == PassiveAbilityKind.LootingMastering)
          abilities.Add(new LootAbility(true) { Kind = PassiveAbilityKind.LootingMastering });
        else
          abilities.Add(new PassiveAbility() { Kind = kind });
      }
      EnsureProps();
    }

    private void EnsureProps()
    {
      if (!fightItemsProps.Any())
      {
        //fightItemsProps[FightItemKind.ExplodePotion] = explosiveCocktailPropsProvider;
        //fightItemsProps[FightItemKind.Knife] = throwingKnifePropsProvider;
        //fightItemsProps[FightItemKind.Trap] = trapPropsProvider;
      }
    }

    public FightItem GetFightItem(FightItemKind kind)
    {
      EnsureProps();
      return fightItemsProps[kind];
    }

    public List<PassiveAbility> GetItems()
    {
      //EnsureAbilities(false);
      return abilities;
    }

    public PassiveAbility GetByEntityStatKind(EntityStatKind esk, bool primary)
    {
      if (primary)
        return Items.Where(i => i.PrimaryStat.Kind == esk).FirstOrDefault();

      return Items.Where(i => i.AuxStat.Kind == esk).FirstOrDefault();
    }

    public List<PassiveAbility> Items
    {
      get
      {
        //can not called it here - deserialization doubles items!
        //if (!abilities.Any())
        //  EnsureAbilities();
        return abilities;
      }

      set
      {
        abilities = value;//for serialization
      }
    }
  }
}
