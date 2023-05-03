using Roguelike.Calculated;
using System;
using System.Collections.Generic;

namespace Roguelike.Abilities
{
  public class LootAbility : PassiveAbility
  {
    public float ExtraChanceToAnyLoot { get; set; }
    public float ExtraChanceToGetMagicLoot { get; set; }
    public float ExtraChanceToGetUniqueLoot { get; set; }
    bool primary;

    public LootAbility()
    {

    }

    public LootAbility(bool primary)
    {
      this.Primary = primary;
      if (primary)
      {
        nextLevel = new LootAbility(false);
        SetStatsForLevel();
      }
    }

    public LootAbility NextLevel
    {
      get
      {
        return nextLevel;
      }

      set
      {
        nextLevel = value;
      }
    }

    public bool Primary
    {
      get
      {
        return primary;
      }

      set
      {
        primary = value;
      }
    }

    LootAbility nextLevel;
    List<string> emp = new List<string>();

    protected override List<string> GetCustomExtraStatDescription(int level)
    {
      if (level == Level)
      {
        var customExtraStatDescription = new List<string>();
        customExtraStatDescription.Add("Finding Non-Equipment: +" + Math.Floor(ExtraChanceToAnyLoot * 100) + " %");
        customExtraStatDescription.Add("Finding Magic Equipment: +" + Math.Floor(ExtraChanceToGetMagicLoot * 100) + " %");
        customExtraStatDescription.Add("Finding Unique Equipment: +" + Math.Floor(ExtraChanceToGetUniqueLoot * 100) + " %");
        return customExtraStatDescription;
      }
      if (nextLevel != null && nextLevel.Level == level)
        return nextLevel.GetCustomExtraStatDescription(level);

      return emp;
    }

    public override void SetStatsForLevel()
    {
      base.SetStatsForLevel();
      float val = GetLevelValue(Level);
      val /= 2;
      val *= 1 / 100f;

      ExtraChanceToAnyLoot = val * 3 / 2;
      ExtraChanceToGetMagicLoot = val * 2 / 3;
      ExtraChanceToGetUniqueLoot = ExtraChanceToGetMagicLoot * 2 / 3;

      if (NextLevel != null)
      {
        NextLevel.Level = Level + 1;
        NextLevel.SetStatsForLevel();
      }
    }

    private float GetLevelValue(int level)
    {
      return level == 0 ? 0 : FactorCalculator.CalcFromLevel2(level + 1) + 1;
    }
  }
}
