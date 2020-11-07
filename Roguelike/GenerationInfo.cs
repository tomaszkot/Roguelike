namespace Roguelike
{
  public class DebugGenerationInfo
  {
    public bool EachEnemyGivesPotion { get; set; } = false;
    public bool EachEnemyGivesJewellery { get; set; } = false;
  }

  public class GenerationInfo : Dungeons.GenerationInfo
  {
    static GenerationInfo()
    {
      DefaultRevealedValue = false;
    }

    public class Generated
    {
      public int ChempionsCount = 0;
    }
    public Generated GeneratedInfo = new Generated();

    public GenerationInfo()
    {
      //TMP!!!
      NumberOfRooms = 2;
      GenerateEnemies = true;
      //ForcedNumberOfEnemiesInRoom = 1;
    }

    public static bool RevealUpperLevelsOnLoad { get; set; } = true;
    public const int MaxLevelIndex = 1;//0 - only one level, 1 - two levels,./... -1 endless
    public int ForcedNumberOfEnemiesInRoom { get; set; } = 0;//-1 means field is not used
    public static DebugGenerationInfo DebugInfo = new DebugGenerationInfo();

    public static float ChanceToGenerateEnemyFromBarrel = .15f;
    public static float ChanceToTurnOnSpecialSkillByEnemy = 0.5f;

    public const int LevelUpPoints = 5;
    public const float NextExperienceIncrease = 1.002f;

    public bool GenerateEnemies { get; set; } = true && !ForceEmpty;

    public bool GenerateLoot { get; set; } = true && !ForceEmpty;

    public bool GenerateInteractiveTiles { get; set; } = true && !ForceEmpty;

    public static int DefaultEnemyRageUsageCount = 0;
    public static int DefaultEnemyWeakenUsageCount = 0;
    public static int DefaultEnemyIronSkinUsageCount = 0;
    public static int DefaultEnemyResistAllUsageCount = 0;
    public static int DefaultEnemyInaccuracyUsageCount = 0;

    public override void MakeEmpty()
    {
      base.MakeEmpty();
      GenerateEnemies = false;
      GenerateInteractiveTiles = false;
      GenerateLoot = false;
    }
  }
}
