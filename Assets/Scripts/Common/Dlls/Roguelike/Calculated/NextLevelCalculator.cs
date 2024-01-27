using Roguelike.Generators;
using Roguelike.Tiles.LivingEntities;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Calculated
{
  internal class NextLevelCalculator
  {
    Dictionary<int, double> levelToExpThreshold = new Dictionary<int, double>();

    public double NextLevelExperience { get; private set; }
    public double PrevLevelExperience { get; private set; }
    public int LevelUpPoints { get; private set; }
    AdvancedLivingEntity entity;

    public NextLevelCalculator(AdvancedLivingEntity entity)
    {
      this.entity = entity;
      levelToExpThreshold[1] = GenerationInfo.FirstNextLevelExperienceThreshold;
      double nextLevelExp = levelToExpThreshold[1];
      for (int i = 2; i < 100; i++)
      {
        levelToExpThreshold[i] = CalcNextLevel(nextLevelExp, i);
        nextLevelExp = levelToExpThreshold[i];
      }
    }

    double CalcNextLevel(double nextLevelExperience, int nextLevel)
    {
      double nextExp = FactorCalculator.AddFactor((int)nextLevelExperience, 60);
      
      if (nextLevel <= 5)
      {
        nextExp = nextExp * 1.3;
      }
      return nextExp;
    }

    public int GetNextLevel(int currentLevel)
    {
      var prevLevel = levelToExpThreshold.First();
      foreach (var item in levelToExpThreshold)
      {
        if (entity.Experience < item.Value)
        {
          NextLevelExperience = item.Value;
          PrevLevelExperience = prevLevel.Value;
          LevelUpPoints = (item.Key - currentLevel)*GenerationInfo.LevelUpPoints;
          return item.Key;
        }
        prevLevel = item;
      }
      return 100;
    }
  }
}
