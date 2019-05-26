﻿#define ASCII_BUILD  
using Roguelike.LootContainers;
using Dungeons.Core;
using System.Drawing;
using System;
using System.Collections.Generic;
using Roguelike.TileParts;
using Roguelike.Utils;
using System.Linq;
using Roguelike.Serialization;
using Newtonsoft.Json;

namespace Roguelike.Tiles
{
  public class Hero : LivingEntity, IPersistable
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

      Dirty = true;//TODO
#if ASCII_BUILD
      color = ConsoleColor.Yellow;
#endif
    }

    [JsonIgnore]
    public bool Dirty { get; set; }

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
        ac = new LootAction(eq) { Info = Name + " put on " + eq, KindValue = LootAction.Kind.PutOn, EquipmentKind = eq.EquipmentKind };
      else
        ac = new LootAction(null) { Info = Name + " took off " + kind, KindValue = LootAction.Kind.TookOff, EquipmentKind = kind };
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
            var stats = eq.GetStats();
            //var att = stats.Stats[EntityStatKind.Attack];
            Stats.AccumulateFactors(stats, positive);
          }
        }
      }
    }

    

  }
}
