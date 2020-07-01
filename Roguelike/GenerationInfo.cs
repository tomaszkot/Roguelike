namespace Roguelike
{
  public class DebugGenerationInfo
  {
    public bool EachEnemyGivesPotion { get; set; } = false;
    public bool EachEnemyGivesJewellery { get; set; } = false;
  }

  public class GenerationInfo : Dungeons.GenerationInfo
  {
    public class Generated
    {
      public int ChempionsCount = 0;
    }
    public Generated GeneratedInfo = new Generated();

    public GenerationInfo()
    {
      //TMP!!!
      //NumberOfRooms = 1;
      //GenerateEnemies = false;
      //ForcedNumberOfEnemiesInRoom = 1;
    }
    public const int MaxLevelIndex = 0;//0 - only one level, 1 - two levels,./... -1 endless
    public int ForcedNumberOfEnemiesInRoom { get; set; } = 5;//-1 means field is not used
    public static DebugGenerationInfo DebugInfo = new DebugGenerationInfo();

    public const int LevelUpPoints = 5;
    public const float NextExperienceIncrease = 1.002f;

    public bool GenerateEnemies { get; set; } = true && !ForceEmpty;

    public bool GenerateLoot { get; set; } = true && !ForceEmpty;

    public bool GenerateInteractiveTiles { get; set; } = true && !ForceEmpty;
        

    public override void MakeEmpty()
    {
      base.MakeEmpty();
      GenerateEnemies = false;
      GenerateInteractiveTiles = false;
      GenerateLoot = false;
    }
  }
}
