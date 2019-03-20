using Dungeons.Tiles;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike
{
  

  public class LootExtendedInfo
  {
    string description;
    EntityStats stats = new EntityStats();

    public LootExtendedInfo()
    {
    }

    public string Description
    {
      get
      {
        return description;
      }

      set
      {
        description = value;
      }
    }

    public EntityStats Stats
    {
      get
      {
        return stats;
      }

      set
      {
        stats = value;
      }
    }

    public override string ToString()
    {
      var res = "";
      if (stats.GetActiveStatsDescription().Any())
      {
        res += "Magic Item";
        res += " " + stats.GetActiveStatsDescription();
      }
      return res;
    }
    public object Clone()
    {
      var clone = this.MemberwiseClone() as LootExtendedInfo;
      clone.Stats = this.Stats.Clone() as EntityStats;
      return clone;
    }

  }
}
