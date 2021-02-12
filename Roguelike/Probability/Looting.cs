using Dungeons.Core;
using Roguelike.Abilities;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Probability
{
  public class Looting
  {
    Dictionary<LootSourceKind, EquipmentClassChances> equipmentClassChances = new Dictionary<LootSourceKind, EquipmentClassChances>();
    Dictionary<LootSourceKind, LootKindChances> lootKindChances = new Dictionary<LootSourceKind, LootKindChances>();

    public Dictionary<LootSourceKind, EquipmentClassChances> EquipmentClassChances { get => equipmentClassChances;  }
    public Dictionary<LootSourceKind, LootKindChances> LootKindChances { get => lootKindChances; }

    public Looting()
    {
      var lootSourceKinds = Enum.GetValues(typeof(LootSourceKind)).Cast<LootSourceKind>();
      foreach (var lootSource in lootSourceKinds)
      {
        SetLootingChance(lootSource, new EquipmentClassChances());

        var lch = new LootKindChances();
        SetLootingChance(lootSource, lch);
        //Debug.Assert(looting.ContainsKey(lootSource));
      }
    }

    public void SetLootingChance(LootSourceKind lsk, LootKind lootKind, float chance)
    {
      lootKindChances[lsk].SetChance(lootKind, chance);
    }

    public void SetLootingChance(LootSourceKind lsk, LootKindChances lootingChance)
    {
      lootKindChances[lsk] = lootingChance;
    }

    public void SetLootingChance(LootSourceKind lootSource, EquipmentClassChances lc)
    {
      equipmentClassChances[lootSource] = lc;
    }

    public LootKind RollDiceForKind(LootSourceKind lsk)
    {
      var chance = lootKindChances[lsk];
      //LootKind.Other
      var lk  = chance.RollDice(LootKind.Unset, new LootKind[] {LootKind.Seal, LootKind.SealPart });
      if (lk != LootKind.Gold && lk != LootKind.Equipment && lk != LootKind.Unset)
      {
        int k = 0;
        k++;
      }
      if (lk == LootKind.Gem)
      {
        int k = 0;
        k++;
      }

      return lk;
    }

    public EquipmentClass RollDice(LootSourceKind lsk, LootAbility ab)
    {
      var chance = equipmentClassChances[lsk];
      var chanceClone = chance.Clone(1);
      chanceClone.SetValue(EquipmentClass.Magic, chanceClone.GetValue(EquipmentClass.Magic) + ab.ExtraChanceToGetMagicLoot);
      var eqClass = chanceClone.RollDice(EquipmentClass.Unset, new EquipmentClass[] { });
      return eqClass;
      //ab.ExtraChanceToGetMagicLoot
    }

  }
}
