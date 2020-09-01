using Newtonsoft.Json;
using Roguelike.Serialization;
using Roguelike.Settings;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike
{
  public class LootHistory
  {
    public string Name { get; set; }
    public LootKind LootKind { get; set; }
    public EquipmentKind EquipmentKind { get; set; }

    public LootHistory() { }

    public LootHistory(Loot loot)
    {
      Name = loot.Name;
      LootKind = loot.LootKind;
      if (loot is Equipment)
        EquipmentKind = (loot as Equipment).EquipmentKind;
    }

    public override bool Equals(object obj)
    {
      var other = obj as LootHistory;
      if (other == null)
        return false;
      return this.GetHashCode() == other.GetHashCode(); 
    }

    public override int GetHashCode()
    {
      return (Name + "_" + LootKind.ToString() + "_" + EquipmentKind.ToString()).GetHashCode();
    }

    static public bool operator ==(LootHistory a, LootHistory b)
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

    static public bool operator !=(LootHistory a, LootHistory b)
    {
      return !(a == b);
    }
  }

  public class History
  {
    public List<LootHistory> GeneratedLoot { get; set; }  = new List<LootHistory>();

    public int Count(LootKind lk)
    {
      return GeneratedLoot.Where(i => i.LootKind == lk).Count();
    }

    public int Count(EquipmentKind ek)
    {
      return GeneratedLoot.Where(i => i.EquipmentKind == ek).Count();
    }

    public void AddLootHistory(LootHistory lh)
    {
      if (!GeneratedLoot.Contains(lh))
        GeneratedLoot.Add(lh);
    }
  }

  public class GameState:  IPersistable
  {
    public class HeroPath
    {
      public string World { get; set; }
      public string Pit { get; set; } = "";
      public int LevelIndex { get; set; }

      public override string ToString()
      {
        return Pit.Any() ? this.World + " " + Pit + " " + LevelIndex  : World;
      }
    }
        
    public RpgGameSettings Settings { get; set; } = new RpgGameSettings();
    public HeroPath HeroPathValue { get; set; } = new HeroPath();
    public History History { get; set; } = new History();

    [JsonIgnore]
    public bool Dirty { get; set; } = true;//TODO true

    public override string ToString()
    {
      return Settings.ToString() + ";" +  HeroPathValue.ToString();
    }

  }
}
