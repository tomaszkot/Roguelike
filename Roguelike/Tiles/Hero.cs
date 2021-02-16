#define ASCII_BUILD  
using Dungeons.Core;
using System.Drawing;
using System;
using Roguelike.Attributes;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System.Linq;
using Roguelike.LootContainers;
using System.Collections.Generic;
using Roguelike.Calculated;

namespace Roguelike.Tiles
{
  public class Hero : AdvancedLivingEntity
  {
    
    public static int FirstNextLevelExperienceThreshold = 50;
    public const int StartStrength = 15;//15;50
    
    public LootContainers.Crafting Crafting { get; set; }
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
      Stats.SetNominal(EntityStatKind.Health, 40);//level up +2 // 40 -> 140
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
        if (eq.Identify())
        {
          Inventory.Remove(scroll);
          if (CurrentEquipment.PrimaryEquipment.Values.Contains(eq) ||
            CurrentEquipment.SpareEquipment.Values.Contains(eq))
            RecalculateStatFactors(false);
          return true;
        }
      }
      return false;
    }

    public override string ToString()
    {
      return base.ToString();
    }

    public virtual void OnContextSwitched(Container container)
    {
      this.Container = container;

      Inventory.Owner = "Hero.Inv";
      Crafting.InvItems.Owner = "Crafting.InvItems";
      Crafting.Recipes.Owner = "Crafting.Recipes";
    }

    protected override float GetStrengthIncrease()
    {
      return Stats.GetCurrentValue(EntityStatKind.Strength) - StartStrength;
    }


    public Tuple<int, int> GetTotalAttackValues()
    {
      Tuple<int, int> res;
      var attack = GetHitAttackValue(false);

      var nonPhysicals = GetNonPhysicalDamages();
      foreach (var stat in nonPhysicals)
        attack += stat.Value;

      var intAttack = (int)attack;
      var variation = (int)GetAttackVariation();
      if (variation != 0)
        res = new Tuple<int, int>(intAttack- variation, intAttack+ variation);
      else
        res = new Tuple<int, int>(intAttack, intAttack);

      return res;
    }

    //for UI
    public string GetTotalAttackValue()
    {
      var attack = GetTotalAttackValues();
      var value = attack.Item1.ToString();
      if (attack.Item1 != attack.Item2)
      {
        value = attack.Item1.ToString() + "-" + attack.Item2.ToString();
      }
      return value;
    }

    public override string GetFormattedStatValue(EntityStatKind kind)
    {
      var value = base.GetFormattedStatValue(kind);
      if (kind == EntityStatKind.Attack)
      {
        value = GetTotalAttackValue();
      }
      return value;
    }

    public override float GetHitAttackValue(bool withVariation)
    {
      var ad = new AttackDescription(this);
      var att = ad.CurrentPhysical;
      if (withVariation)//GUI is not meant to have it changed on character panel
      {
        var variation = GetAttackVariation();
        var sign = RandHelper.Random.NextDouble() > .5f ? -1 : 1;

        att += sign * variation * (float)RandHelper.Random.NextDouble();
      }
      return att;
    }

    public override float GetAttackVariation()
    {
      var currentEquipment = GetActiveEquipment();
      if (currentEquipment[CurrentEquipmentKind.Weapon] != null)
      {
        var wpn = currentEquipment[CurrentEquipmentKind.Weapon] as Weapon;
        return wpn.GetPrimaryDamageVariation();
      }

      return 0;
    }

    internal void PrepareForSave()
    {
      Inventory.GetItems<Equipment>().ToList().ForEach(i => i.PrepareForSave());
      CurrentEquipment.PrimaryEquipment.ToList().ForEach
      (
        i => {
          if(i.Value!=null)
            i.Value.PrepareForSave(); 
        }
      );
    }

    public override bool CanCauseBleeding()
    {
      return CurrentEquipment.GetWeapon() != null;
    }
  }
}
