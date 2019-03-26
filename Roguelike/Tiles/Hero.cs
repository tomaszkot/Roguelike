#define ASCII_BUILD  
using Roguelike.LootContainers;
using Dungeons.Core;
using System.Drawing;
using System;
using System.Collections.Generic;
using Roguelike.TileParts;
using Roguelike.Utils;
using System.Linq;

namespace Roguelike.Tiles
{
  public class Hero : LivingEntity
  {
    Inventory inventory = null;

    public Inventory Inventory { get => inventory; set => inventory = value; }
    internal CurrentInventory CurrentInventory { get => currentInventory; set => currentInventory = value; }

    CurrentInventory currentInventory = new CurrentInventory();

    public Hero():base(new Point().Invalid(), '@')
    {
      Stats.SetNominal(EntityStatKind.Health, 100);//level up +2
                                                                                     // Character.Mana = 40;
      Stats.SetNominal(EntityStatKind.Strength, 15);//15
      Stats.SetNominal(EntityStatKind.Magic, 10);
      Stats.SetNominal(EntityStatKind.Mana, 40);
      Stats.SetNominal(EntityStatKind.Defence, 10);

      CreateInventory();

#if ASCII_BUILD
      color = ConsoleColor.Yellow;
#endif
    }

    public override string ToString()
    {
      return base.ToString();// + Data.AssetName;
    }

    public bool PrimaryEquipmentActive
    {
      get;
      set;
    }

    protected virtual Inventory CreateInventory()
    {
      inventory = new Inventory();
      return inventory;
    }

    public SerializableDictionary<EquipmentKind, Equipment> CurrentEquipment
    {
      get
      {
        return PrimaryEquipmentActive ? currentInventory.PrimaryEquipment : currentInventory.SecondaryEquipment;
      }
      set
      {
        if (PrimaryEquipmentActive)
          currentInventory.PrimaryEquipment = value;
        else
          currentInventory.SecondaryEquipment = value;
      }
    }

    public SerializableDictionary<EquipmentKind, Equipment> PrimaryEquipment
    {
      get
      {
        return currentInventory.PrimaryEquipment;
      }
      set { currentInventory.PrimaryEquipment = value; }
    }

    public SerializableDictionary<EquipmentKind, Equipment> SecondaryEquipment
    {
      get
      {
        return currentInventory.SecondaryEquipment;
      }
      set { currentInventory.SecondaryEquipment = value; }
    }

    public void SetEquipment(EquipmentKind kind, Equipment eq)
    {
      CurrentEquipment[kind] = eq;
      RecalculateStatFactors(false);

      LootAction ac = null;
      if (eq != null)
        ac = new LootAction() { Info = Name + " put on " + eq, Loot = eq, KindValue = LootAction.Kind.PutOn, EquipmentKind = eq.EquipmentKind };
      else
        ac = new LootAction() { Info = Name + " took off " + kind, Loot = null, KindValue = LootAction.Kind.TookOff, EquipmentKind = kind };
      AppendAction(ac);
    }

    

    public void RecalculateStatFactors(bool fromLoad)
    {
      Stats.ResetStatFactors();
      if (fromLoad)//this shall not be affected by any after load
      {
        Stats.Stats[EntityStatKind.ChanceToHit].SetSubtraction(0);
        Stats.Stats[EntityStatKind.Defence].SetSubtraction(0);
        Stats.Stats[EntityStatKind.Attack].SetSubtraction(0);
      }

     
      //accumulate positive factors
      AccumulateEqFactors(true);

      //var abs = Abilities.GetItems();
      //foreach (var ab in abs)
      //{
      //  if (!ab.BeginTurnApply)
      //  {
      //    if (ab.PrimaryStat.Kind != EntityStatKind.Unknown)
      //    {
      //      Stats.AccumulateFactor(ab.PrimaryStat.Kind, ab.PrimaryStat.Factor);
      //      AddAuxStat(ab);
      //    }
      //  }
      //}

      AccumulateEqFactors(false);
    }

    private void AccumulateEqFactors(bool positive)
    {
      var eqipKinds = Enum.GetValues(typeof(EquipmentKind)).Cast<EquipmentKind>();

      foreach (EquipmentKind et in eqipKinds)
      {
        if (CurrentEquipment.ContainsKey(et))//old game save ?
        {
          var eq = CurrentEquipment[et];
          if (eq != null)
          {
            var stats = GetStats(eq);
            //var att = stats.Stats[EntityStatKind.Attack];
            Stats.AccumulateFactors(stats, positive);
          }
        }
      }
    }

    //TODO make it method of Equipment
    EntityStats GetStats(Equipment eq)
    {
      EntityStats stats = new EntityStats();
      if (eq == null)
        return stats;
      if (eq is Weapon)
      {
        stats.Stats[EntityStatKind.Attack].Factor += eq.PrimaryStatValue;
      }
      else if (eq is Armor)
      {
        stats.Stats[EntityStatKind.Defence].Factor += eq.PrimaryStatValue;
      }
      else if (eq is Jewellery)
      {
        var juw = eq as Jewellery;
        stats.Stats[juw.PrimaryStat].Factor += juw.PrimaryStatValue;
      }
      if (!eq.IsPlain())
      {
        stats.Accumulate(eq.ExtendedInfo.Stats);
      }
      return stats;
    }

  }
}
