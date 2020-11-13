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

    static Dictionary<Weapon.WeaponKind, EntityStatKind> weapons2Esk = new Dictionary<Weapon.WeaponKind, EntityStatKind>()
    {
      {Weapon.WeaponKind.Axe,  EntityStatKind.AxeExtraDamage},
      { Weapon.WeaponKind.Sword, EntityStatKind.SwordExtraDamage},
      { Weapon.WeaponKind.Bashing, EntityStatKind.BashingExtraDamage},
      { Weapon.WeaponKind.Dagger, EntityStatKind.DaggerExtraDamage}
    };

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

    public override string GetFormattedStatValue(EntityStatKind kind)
    {
      var value = base.GetFormattedStatValue(kind);
      if (kind == EntityStatKind.Attack)
      {
        var variation = GetAttackVariation();
        if (variation != 0)
        {
          var cv = GetCurrentValue(kind);
          value = (cv - variation) + "-" + (cv + variation);
        }
      }
      return value;
    }

    public override float GetHitAttackValue(bool withVariation)
    {
      //return base.GetHitAttackValue(withVariation);
      var att = base.GetHitAttackValue(withVariation);
      var wpn = GetActiveEquipment()[CurrentEquipmentKind.Weapon] as Weapon;
      if (wpn != null)
      {
        if (weapons2Esk.ContainsKey(wpn.Kind))
        {
          var esk = weapons2Esk[wpn.Kind];
          //var ab = Abilities.GetByEntityStatKind(esk, false);
          //if (ab != null)
          //{
          //  att += (att * ab.AuxStat.Factor / 100f);
          //}
        }
        //Abilities.GetFightItem
        //if (ab.AuxStat.Kind == EntityStatKind.AxeExtraDamage && wpn.Kind == Weapon.WeaponKind.Axe
        //    || ab.AuxStat.Kind == EntityStatKind.SwordExtraDamage && wpn.Kind == Weapon.WeaponKind.Sword
        //    || ab.AuxStat.Kind == EntityStatKind.BashingExtraDamage && wpn.Kind == Weapon.WeaponKind.Bashing
        //    || ab.AuxStat.Kind == EntityStatKind.DaggerExtraDamage && wpn.Kind == Weapon.WeaponKind.Dagger)
        //{
        //Stats.AccumulateFactor(EntityStatKind.Attack, ab.AuxStat.Factor);
        //}
      }

      if (withVariation)//GUI is not meant to have it changed on character panel
      {
        var variation = GetAttackVariation();
        var sign = RandHelper.Random.NextDouble() > .5f ? -1 : 1;

        att += sign * variation * (float)RandHelper.Random.NextDouble();
      }
      return att;
    }

    public float GetAttackVariation()
    {
      var currentEquipment = GetActiveEquipment();
      if (currentEquipment[CurrentEquipmentKind.Weapon] != null)
      {
        var wpn = currentEquipment[CurrentEquipmentKind.Weapon] as Weapon;
        return wpn.GetPrimaryDamageVariation();
      }

      return 0;
    }
  }
}
