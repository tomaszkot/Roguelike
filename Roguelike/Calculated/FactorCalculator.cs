﻿using System;

namespace Roguelike.Calculated
{
  public class FactorCalculator
  {
    public static int CalcFromLevel(int lvl, int baseFactor, float incPercentage = 10.0f)
    {
      if (lvl == 0)
        return 0;

      if (lvl == 1)
        return baseFactor;

      int prev = CalcFromLevel(lvl - 1, baseFactor);

      return AddFactor(prev, incPercentage);
    }

    public static int CalcFromLevel1(int lvl, float increase, float levelMult = 1)
    {
      if (lvl == 0)
        return 0;

      if (lvl == 1)
        return (int)increase;

      // int prev = lvl == 1 ? (int)increase : CalcFromLevel(lvl - 1, baseFactor);
      int prev = CalcFromLevel1(lvl - 1, increase, levelMult);
      var inc = increase;
      if (levelMult != 1)
      {
        inc += levelMult * lvl;
      }
      return AddFactor(prev, inc);
    }

    public static int CalcFromLevel2(int lvl, float divider = 1)
    {
      return (int)((lvl * lvl) / divider);
    }

    public static int CalcFromLevel3(int lvl, int baseValue, float incPercentage = 10.0f)
    {
      if (lvl == 0)
        return 0;

      //if (lvl == 1)
      //  return baseValue;

      float res = baseValue*10;
      for (int i = 1; i < lvl; i++)
      {
        res = (float) Math.Ceiling((double)AddFactor(res, incPercentage));
      }
      return (int)res/10;
    }

    public static int AddFactor(int prevValue, float incPercentage)
    {
      return prevValue + (int)(prevValue * incPercentage / 100f);
    }

    public static float AddFactor(float prevValue, float incPercentage)
    {
      return prevValue + (float)(prevValue * incPercentage / 100f);
    }
  }
}
