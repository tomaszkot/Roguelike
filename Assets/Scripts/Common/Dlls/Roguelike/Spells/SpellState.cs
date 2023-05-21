using Newtonsoft.Json;
using Roguelike.Spells;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Roguelike.Spells
{
  public class SpellState
  {
    public SpellKind Kind { get; set; }
    public int Level { get; set; } = 1;

    public int CoolDownCounter { get; set; } = 0;

    internal bool IsCoolingDown()
    {
      return CoolDownCounter > 0;
    }

    [JsonIgnore]
    public string LastIncError { get; set; }

    public int MaxLevel = 10;

    public bool IncreaseLevel(IAdvancedEntity entity)
    {

      //if (abilityLevelToPlayerLevel.ContainsKey(Level + 1))
      //{
      //  var lev = abilityLevelToPlayerLevel[Level + 1];
      //  if (lev > entity.Level)
      //  {
      //    LastIncError = "Required character level for ability increase: " + lev;
      //    return false;
      //  }
      //}
      if (CanIncLevel(entity))
      {
        Level++;
        //SetStatsForLevel();
        return true;
      }
      return false;
    }

    public bool CanIncLevel(IAdvancedEntity entity)
    {
      LastIncError = "";
      if (Level == MaxLevel)
      {
        LastIncError = "Max level of the spell reached";
        return false;
      }

      var scroll = new Scroll(Kind);
      var le = entity as LivingEntity;
      var spell = scroll.CreateSpell(le);
      var canInc = le.Stats.Magic >= spell.NextLevelMagicNeeded;
      if (!canInc)
      {
        LastIncError = "Magic level too low";
        return false;
      }
      return true;
    }
  }
}
