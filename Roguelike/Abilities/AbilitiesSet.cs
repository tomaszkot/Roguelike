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
    List<Ability> abilities = new List<Ability>();
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

      var kinds = Enum.GetValues(typeof(AbilityKind)).Cast<AbilityKind>().ToList();
      foreach (var kind in kinds)
      {
        if (abilities.Any(i => i.Kind == kind) || kind == AbilityKind.Unknown)
          continue;
        if (kind == AbilityKind.LootingMastering)
          abilities.Add(new LootAbility(true) { Kind = AbilityKind.LootingMastering });
        else
          abilities.Add(new Ability() { Kind = kind });
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

    public List<Ability> GetItems()
    {
      //EnsureAbilities(false);
      return abilities;
    }

    public Ability GetByEntityStatKind(EntityStatKind esk, bool primary)
    {
      if (primary)
        return Items.Where(i => i.PrimaryStat.Kind == esk).FirstOrDefault();

      return Items.Where(i => i.AuxStat.Kind == esk).FirstOrDefault();
    }

    public List<Ability> Items
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
