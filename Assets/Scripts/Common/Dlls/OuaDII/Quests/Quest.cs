//#define TEST_QUEST_ON 
using Roguelike.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII.Quests
{
  public enum QuestKind { 
    Unset = 0, 
    CrazyMiller = 1, Smiths = 2,

    IronOreForSmith = 3, SilesiaCoalForSmith = 4, HourGlassForMiller = 5, 
    WarewolfHarassingVillage = 6,

    GatheringEntry = 7,//depricated
    ToadstoolsForWanda = 8, Hornets = 9, CreatureInPond = 10, FernForDobromila = 11
    , StonesMine = 12, Malbork = 13, RescueJurantDaughter = 14, KillBoar = 15, SacksOfBarley = 16
  }

  public class QuantityQuestRequirement : QuestRequirement
  {
    protected string EntityName { get; set; }
    public int EntityQuantity { get; protected set; }

    public QuestKind QuestKind { get; set; }
  }

  public class EnemyQuestRequirement : QuantityQuestRequirement
  {
    public EnemyQuestRequirement() { }//for json ser/deser
    public EnemyQuestRequirement(string enemyName, int enemyQuantity, string enemyHerd) 
    {
      EnemyName = enemyName;
      EnemyQuantity = enemyQuantity;
      EnemyHerd = enemyHerd;
    }

    public string EnemyName
    {
      get { return EntityName; }
      set { EntityName = value; }
    }

    public int EnemyQuantity
    {
      get { return EntityQuantity; }
      set { EntityQuantity = value; }
    }

    public string EnemyHerd { get; set; }

    public static int QuestEnemyQuantity(QuestKind kind)
    {
      switch (kind)
      {
        case QuestKind.Unset:
          break;
        case QuestKind.WarewolfHarassingVillage:
          break;
        case QuestKind.Hornets:
          return 2;//4
        case QuestKind.CreatureInPond:
          return 1;
        default:
          break;
      }
      return 0;

    }

    public static int QuestCheatingPunishmentForQuantity(QuestKind kind)
    {
      switch (kind)
      {
        case QuestKind.Unset:
        case QuestKind.CrazyMiller:
        case QuestKind.Smiths:
          break;
        default:
          break;
      }
      return 0;

    }
  }

  public class LootQuestRequirement : QuantityQuestRequirement
  {
    public string LootName 
    {
      get { return EntityName; }
      set { EntityName = value; } 
    }
    
    public int LootQuantity 
    {
      get { return EntityQuantity; }
      set { EntityQuantity = value; } 
    }

    public static int QuestLootQuantity(QuestKind kind)
    {
      switch (kind)
      {
        case QuestKind.Unset:
        case QuestKind.CrazyMiller:
        case QuestKind.Smiths:
          break;
        case QuestKind.IronOreForSmith:
#if TEST_QUEST_ON
         return 2;//10
#else
         return 10;
#endif
        case QuestKind.SilesiaCoalForSmith:
#if TEST_QUEST_ON
          return 2;//10
#else
         return 10;
#endif
        case QuestKind.ToadstoolsForWanda:
#if TEST_QUEST_ON
          return 2;
#else
         return 12;
#endif
        case QuestKind.HourGlassForMiller:
          break;
        case QuestKind.WarewolfHarassingVillage:
          break;
        case QuestKind.Hornets:
          break;
        case QuestKind.FernForDobromila:
          return 1;
        case QuestKind.SacksOfBarley:
          return 5;
        default:
          break;
      }
      return 0;

    }

    public static int QuestCheatingPunishmentLootQuantity(QuestKind kind)
    {
      switch (kind)
      {
        case QuestKind.Unset:
          break;
        case QuestKind.CrazyMiller:
          break;
        case QuestKind.Smiths:
          break;
        case QuestKind.IronOreForSmith:
          return 10;
        case QuestKind.SilesiaCoalForSmith:
          return 10;
        case QuestKind.HourGlassForMiller:
          break;
        case QuestKind.WarewolfHarassingVillage:
          break;
        default:
          break;
      }
      return 0;

    }

    public override string ToString()
    {
      return base.ToString() + "LootQuantity: " + LootQuantity;
    }
  }

  
}
