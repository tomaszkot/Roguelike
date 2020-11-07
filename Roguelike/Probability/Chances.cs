
using Dungeons.Core;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public Dictionary<T, float> ValuesCopy()
    {
      return values.ToDictionary(entry => entry.Key,
      entry => entry.Value);
    }

    public T RollDice(T unset, T[] skip)
    {
      var rand = RandHelper.GetRandomDouble();
      var matches = GetMatchesBeneathThreashold(rand);
      if (matches.Any())
        return RandHelper.GetRandomElem<T>(matches, skip);
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
}
