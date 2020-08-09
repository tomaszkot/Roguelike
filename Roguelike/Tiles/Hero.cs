#define ASCII_BUILD  
using Dungeons.Core;
using System.Drawing;
using System;
using Roguelike.Events;
using Roguelike.Attributes;
using Roguelike.Managers;
using Dungeons;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System.Linq;
using Roguelike.LootContainers;

namespace Roguelike.Tiles
{
  public class Hero : AdvancedLivingEntity
  {
    public static int FirstNextLevelExperienceThreshold = 15;
    protected Container container;
    public Roguelike.LootContainers.Crafting Crafting { get; set; }

    public Hero(): base(new Point().Invalid(), '@')
    {
      canAdvanceInExp = true;
      Stats.SetNominal(EntityStatKind.Health, 150);//level up +2
      // Character.Mana = 40;
      var str = 15;
      Stats.SetNominal(EntityStatKind.Strength, str);//15
      Stats.SetNominal(EntityStatKind.Attack, str);
      Stats.SetNominal(EntityStatKind.Magic, 10);
      Stats.SetNominal(EntityStatKind.Mana, 40);
      Stats.SetNominal(EntityStatKind.Defence, 10);
      Stats.SetNominal(EntityStatKind.Dexterity, 10);

      NextLevelExperience = FirstNextLevelExperienceThreshold;

      CreateInventory();
      Inventory.InvOwner = InvOwner.Hero;
      Inventory.InvBasketKind = InvBasketKind.Hero;

      Crafting = new Roguelike.LootContainers.Crafting();
      
      Dirty = true;//TODO

      
#if ASCII_BUILD
      color = ConsoleColor.Yellow;
#endif
    }

    public bool Identify(Equipment eq)
    {
      var scroll = Inventory.GetItems<Scroll>().Where(i=> i.Kind == Spells.SpellKind.Identify).FirstOrDefault();
      if (scroll != null)
      {
        Inventory.Remove(scroll);
        if (eq.Identify())
        {
          if(CurrentEquipment.PrimaryEquipment.Values.Contains(eq) ||
            CurrentEquipment.SpareEquipment.Values.Contains(eq))
            RecalculateStatFactors(false);
          return true;
        }
      }
      return false;
    }

    public override string ToString()
    {
      return base.ToString();// + Data.AssetName;
    }

    public virtual void OnContextSwitched(Container container)
    {
      this.container = container;

      Inventory.Owner = "Hero.Inv";
      Inventory.Container = container;
      Crafting.Container = container;
      Crafting.InvItems.Owner = "Crafting.InvItems";
      Crafting.Recipes.Owner = "Crafting.Recipes";
    }
  }
}
