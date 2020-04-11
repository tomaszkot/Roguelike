namespace Roguelike
{
  class FactorCalculator
  {
    public static int CalcFromLevel(int lvl, int baseFactor, float increase = 10.0f)
    {
      if (lvl == 0)
        return 0;

      if (lvl == 1)
        return baseFactor;

      int prev = CalcFromLevel(lvl - 1, baseFactor);

      return CalcFactor(prev, increase);
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
      return CalcFactor(prev, inc);
    }

    public static int CalcFromLevel2(int lvl, float divider = 1)
    {
      return (int)((lvl * lvl) / divider);
    }

    public static int CalcFactor(int prevValue, float inc)
    {
      return prevValue + (int)(prevValue * inc / 100f);
    }
  }
}
