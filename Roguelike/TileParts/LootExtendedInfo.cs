﻿using Roguelike.Attributes;
using System.Linq;

namespace Roguelike
{
  namespace TileParts
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

      //[JsonIgnoreAttribute]
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

      //public object Clone()
      //{
      //  var clone = this.MemberwiseClone() as LootExtendedInfo;
      //  clone.Stats = this.Stats.Clone() as EntityStats;
      //  return clone;
      //}

    }
  }
}