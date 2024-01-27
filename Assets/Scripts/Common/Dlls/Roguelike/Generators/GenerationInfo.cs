//#define TEST_ON
using Dungeons;
using Roguelike.Attributes;
using Roguelike.Effects;
using Roguelike.Tiles.Looting;

namespace Roguelike.Generators
{
  public class DebugGenerationInfo
  {
    public bool EachEnemyGivesPotion { get; set; } = false;
    public bool EachEnemyGivesJewellery { get; set; } = false;

    public string ForcedEnemyName = "";//"wolf_skeleton";
                                               //
    public bool GenerateEnemies { get; set; } = true;

    public EffectType ForcedEffectType { get; set; } = EffectType.Unset;

    public EntityStatKind[] HackedStats = new EntityStatKind[] {
    };
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
      NumberOfRooms = 6;//6 -> 5 normal + 1 secret

#if TEST_ON
      //MakeEmpty();
      //ForcedNumberOfEnemiesInRoom = 1;
      //GenerateEnemies = false;
      NumberOfRooms = 2;
      //MaxLevelIndex = 0;
      DefaultForcedDungeonLayouterKind = DungeonLayouterKind.Default;
      ForcedKeyPuzzle = KeyPuzzle.LeverSet;

#endif
    }

    public KeyPuzzle ForcedKeyPuzzle { get; set; }
    public string ForcedEnemyName = "";
    public KeyPuzzle KeyPuzzle { get; set; }
    public static int MaxEnemyPackCount = 5;
    public static bool RevealUpperLevelsOnLoad { get; set; } = true;
    public static int DefaultMaxLevelIndex = 1;//0 - only one level, 1 - two levels,./... -1 endless
    public int ForcedNumberOfEnemiesInRoom { get; set; } = 5;//5 - normal, -1 means field is not used
    public static DebugGenerationInfo DebugInfo = new DebugGenerationInfo();

    public static DungeonLayouterKind DefaultForcedDungeonLayouterKind = DungeonLayouterKind.Unset;
    public static float ChanceToGenerateEnemyFromBarrel = .15f;
    public static float ChanceToGenerateEnemyFromGrave = .4f;
    public static float ChanceToTurnOnSpecialSkillByEnemy = 0.5f;

    public const int LevelUpPoints = 5;
    public const float NextExperienceIncrease = .5f;
    public const int FirstNextLevelExperienceThreshold = 250;
    public const int AbilityPointLevelUpIncrease = 3;
    public const float EnemyStatsIncreasePerLevel = .15f;
    public bool GenerateEnemies { get; set; } = DebugInfo.GenerateEnemies && !ForceEmpty;

    public bool GenerateLoot { get; set; } = true && !ForceEmpty;

    public bool GenerateInteractiveTiles { get; set; } = true && !ForceEmpty;

    public static int DefaultEnemyRageUsageCount = 0;
    public static int DefaultEnemyWeakenUsageCount = 0;
    public static int DefaultEnemyIronSkinUsageCount = 0;
    public static int DefaultEnemyResistAllUsageCount = 0;
    public static int DefaultEnemyInaccuracyUsageCount = 0;
    
    public int MaxBarrelsPerRoom = 5;
    public int MaxLootPerRoom = 2;
    public static int MaxMerchantMagicDust = 8;
    public static int MaxMerchantHooch = 8;
    public static float ChangeToGetEnchantableItem = 0.2f;
    public static float MaxMagicAttackDistance = 6;
    public static Difficulty Difficulty = Difficulty.Normal;

    public override void MakeEmpty()
    {
      base.MakeEmpty();
      GenerateEnemies = false;
      GenerateInteractiveTiles = false;
      GenerateLoot = false;
    }
  }
}
