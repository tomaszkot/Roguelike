using Dungeons.Core;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Probability
{
  public class Chances<T>
  {
    protected Dictionary<T, float> values;

    public Chances(Dictionary<T, float> dict)
    {
      this.values = dict;
      var allValues = Enum.GetValues(typeof(T)).Cast<T>().ToList();
      foreach (var v in allValues)
      {
        this.values[v] = 0;
      }
    }

    public List<float> Values() { return values.Values.ToList(); }

    public T RollDice(T unset)
    {
      var rand = RandHelper.GetRandomDouble();
      var matches = GetMatchesBeneathThreashold(rand);
      if (matches.Any())
        return RandHelper.GetRandomElem<T>(matches);
      return unset;
    }
    
    public List<T> GetMatchesBeneathThreashold(double rand)
    {
      var chances = values.OrderBy(i => i.Value).ToList();
      var matches = new List<T>();
      foreach (var ch in chances)
      {
        if (rand <= ch.Value)
        {
          matches.Add(ch.Key);
        }
      }
      return matches;
    }
  }

  public class EquipmentClassChances : Chances<EquipmentClass>
  {
    public EquipmentClassChances() : base(new Dictionary<EquipmentClass, float>())
    {
    }

    public void SetValue(EquipmentClass equipmentClass, float value)
    {
      values[equipmentClass] = value;
    }

    public float GetValue(EquipmentClass equipmentClass)
    {
      return values[equipmentClass];
    }

    public void Reset()
    {
      foreach (var kv in values)
      {
        values[kv.Key] = 0;
      }
    }

    public EquipmentClassChances Clone(float factor)
    {
      var clone = new EquipmentClassChances();
      clone.SetValue(EquipmentClass.Magic, GetValue(EquipmentClass.Magic) * factor);
      clone.SetValue(EquipmentClass.MagicSecLevel, GetValue(EquipmentClass.MagicSecLevel) * factor);
      clone.SetValue(EquipmentClass.Unique, GetValue(EquipmentClass.Unique) * factor);
      clone.SetValue(EquipmentClass.Plain, GetValue(EquipmentClass.Plain) * factor);

      return clone;
    }
  }

  public class LootKindChances : Chances<LootKind>
  {
    public LootKindChances() : base(new Dictionary<LootKind, float>())
    {
      var lootKinds = Enum.GetValues(typeof(LootKind));
      foreach (var lootKind in lootKinds.Cast<LootKind>())
      {
        SetChance(lootKind, 0);
      }
    }

    public void SetChance(LootKind lootKind, float chance)
    {
      values[lootKind] = chance;
    }
  
  };

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
      var lk  = chance.RollDice(LootKind.Unset);
      if (lk != LootKind.Gold && lk != LootKind.Equipment && lk != LootKind.Unset)
      {
        int k = 0;
        k++;
      }
      return lk;
    }

    public EquipmentClass RollDice(LootSourceKind lsk)
    {
      return equipmentClassChances[lsk].RollDice(EquipmentClass.Unset);
    }

  }
}
