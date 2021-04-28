using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike
{
  namespace History
  {

    public class LootHistoryItem
    {
      public string Name { get; set; }
      public LootKind LootKind { get; set; }
      public EquipmentKind EquipmentKind { get; set; }
      public string Tag1 { get; set; }

      public LootHistoryItem() { }

      public LootHistoryItem(Loot loot)
      {
        Name = loot.Name;
        LootKind = loot.LootKind;
        Tag1 = loot.tag1;
        if (loot is Equipment)
          EquipmentKind = (loot as Equipment).EquipmentKind;
      }

      public override bool Equals(object obj)
      {
        var other = obj as LootHistoryItem;
        if (other == null)
          return false;
        return this.GetHashCode() == other.GetHashCode();
      }

      public override int GetHashCode()
      {
        return (Name + "_" + LootKind.ToString() + "_" + EquipmentKind.ToString()).GetHashCode();
      }

      static public bool operator ==(LootHistoryItem a, LootHistoryItem b)
      {
        if (Object.ReferenceEquals(a, b))
        {
          return true;
        }
        // If one is null, but not both, return false.
        if (((object)a == null) || ((object)b == null))
        {
          return false;
        }
        return a.Equals(b);
      }

      static public bool operator !=(LootHistoryItem a, LootHistoryItem b)
      {
        return !(a == b);
      }
    }

    public class LootHistory
    {
      public List<LootHistoryItem> GeneratedLoot { get; set; } = new List<LootHistoryItem>();

      public int Count(LootKind lk)
      {
        return GeneratedLoot.Where(i => i.LootKind == lk).Count();
      }

      public int Count(string tag1)
      {
        return GeneratedLoot.Where(i => i.Tag1 == tag1).Count();
      }

      public int Count(EquipmentKind ek)
      {
        return GeneratedLoot.Where(i => i.EquipmentKind == ek).Count();
      }

      public void AddLootHistory(LootHistoryItem lh)
      {
        if (!GeneratedLoot.Contains(lh))
          GeneratedLoot.Add(lh);
      }
    }
  }
}
