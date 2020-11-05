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
    public static int FirstNextLevelExperienceThreshold = 50;
    public const int StartStrength = 15;//15;50
    
    public Roguelike.LootContainers.Crafting Crafting { get; set; }
    public override Container Container 
    { 
      get => base.Container; 
      set 
      {
        base.Container = value;
        Inventory.Container = value;
        Crafting.Container = value;
      }
    }

    public Hero(): base(new Point().Invalid(), '@')
    {
      canAdvanceInExp = true;
      Stats.SetNominal(EntityStatKind.Health, 240);//level up +2 // 40 -> 140
      // Character.Mana = 40;
      Stats.SetNominal(EntityStatKind.Strength, StartStrength);
      Stats.SetNominal(EntityStatKind.Attack, StartStrength);
      Stats.SetNominal(EntityStatKind.Magic, 10);
      Stats.SetNominal(EntityStatKind.Mana, 40);
      Stats.SetNominal(EntityStatKind.Defense, 10);
      Stats.SetNominal(EntityStatKind.Dexterity, 10);

      bool debugMode = false;
      if (debugMode)
      {
        Stats.SetNominal(EntityStatKind.Health, 140);
        Stats.SetNominal(EntityStatKind.Strength, 50);
      }

      NextLevelExperience = FirstNextLevelExperienceThreshold;

      CreateInventory(null);
      Inventory.InvOwner = InvOwner.Hero;
      Inventory.InvBasketKind = InvBasketKind.Hero;

      Crafting = new Roguelike.LootContainers.Crafting(null);
      
      Dirty = true;//TODO
      Revealed = true;
      
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
      this.Container = container;

      Inventory.Owner = "Hero.Inv";
      Crafting.InvItems.Owner = "Crafting.InvItems";
      Crafting.Recipes.Owner = "Crafting.Recipes";
    }

    //protected override float GetCurrentAttack()
    //{
    //  var att = base.GetCurrentAttack();
    //  return att + this.GetCurrentValue(EntityStatKind.Strength - Hero.StartStrength);
    //}

    protected override float GetStrengthIncrease()
    {
      return Stats.GetCurrentValue(EntityStatKind.Strength) - StartStrength;
    }

    //public void RestoreState(GameState gameState)
    //{
    //  if (gameState.Settings.CoreInfo.RestoreHeroToSafePointAfterLoad)
    //  {
    //    if (gameState.HeroInitGamePosition != new Point().Invalid())
    //      Point = gameState.HeroInitGamePosition;
    //  }
    //}
  }
}
