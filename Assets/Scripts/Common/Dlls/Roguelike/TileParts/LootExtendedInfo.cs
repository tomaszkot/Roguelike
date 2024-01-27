using Roguelike.Attributes;
using System.Linq;

namespace Roguelike.TileParts
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
  }
}
