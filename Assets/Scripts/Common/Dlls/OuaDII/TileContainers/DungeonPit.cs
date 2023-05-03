using Dungeons;
using Dungeons.Core;
using Newtonsoft.Json;
using OuaDII.Generators;
using OuaDII.Managers;
using OuaDII.Quests;
using OuaDII.Tiles;
using Roguelike.Serialization;
using Roguelike.TileContainers;
using Roguelike.Tiles.Interactive;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII.TileContainers
{
  //set of levels, entrance is from a World
  public class DungeonPit : IPersistable
  {
    string name;
    public string Name
    {
      get { return name; }
      set
      {
        name = value;
        SetQuestKind();
      }
    }

    public List<GameLevel> Levels { get; set; } = new List<GameLevel>();

    public bool Dirty { get; set; }

    OuaDII.Generators.LevelGenerator generator;

    //level of enemies at the first level of the dungeon. At the end it can be bigger
    public int StartEnemiesLevel { get; set; } = 1;

    //each level of the dungeon will increase enemies power
    public float EnemiesPowerIncreasePerLevel { get; set; } = 1.1f;

    [JsonIgnore]
    public Dictionary<int, Roguelike.Generators.GenerationInfo> GenerationInfos { get; set; }

    [JsonIgnore]
    public LevelGenerator LevelGenerator
    {
      get => generator;
      set => generator = value;
    }
    public QuestKind QuestKind { get; set; }

    [JsonIgnore]//TODO static
    public static IQuestRoomCreatorOwner QuestRoomCreatorOwner { get; set; }

    public string GetLastLevelBossName()
    {
      return GetLastLevelBossNameFromPitName(Name);
    }

    public static string GetPitDisplayName(Stairs stairs)
    {
      if (stairs.PitName == GameManager.GameOnePitDown)
      {
        return "Buried Dungeon";
      }
      var name = DungeonPit.GetPitDisplayName(stairs.PitName);
      //var name = "Pit";
      //var boss = DungeonPit.GetLastLevelBossNameFromPitName(stairs.PitName);
      //if (!string.IsNullOrEmpty(boss))
      //  name = boss + "'s Pit";

      name = name.ToUpperFirstLetter();
      return name;
    }

    public static string GetLastLevelBossNameFromPitName(string pitName)
    {
      if (pitName.Contains("GatheringEntry"))
      {
        return "fallen_one";
      }
      var boss = pitName.Replace(PitDownPreffix, "");
      if (boss.StartsWith("_"))
        boss = boss.Substring(1);

      if (boss.EndsWith("s"))//TODO
        boss = boss.Substring(0, boss.Length - 1);
      return boss;
    }

    //public int MaxLevels { get; internal set; }

    public const string PitDownPreffix = "pit_down";
    public const string PitUpSuffix = "pit_up";
    public const string PitGathering = "GatheringEntry";

    public DungeonPit()
    {
      name = "";
      GenerationInfos = new Dictionary<int, Roguelike.Generators.GenerationInfo>();
    }

    public static string GetPitQuestName(QuestKind questKind)
    {
      return GetFullPitName(questKind.ToString());
    }

    public static string GetFullPitName(string pitName)
    {
      return PitDownPreffix + "_" + pitName;
    }

    //TODO use GetPitQuestName
    public static string GetPitDisplayName(string pitName)
    {
      if (WorldGenerator.PitRats == pitName)
        return "Rats Pit";
      if (WorldGenerator.PitBats == pitName)
        return "Bats Pit";
      if (WorldGenerator.PitSpiders == pitName)
        return "Spiders Pit";
      if (WorldGenerator.PitSkeletons == pitName)
        return "Skeletons Pit";
      if (WorldGenerator.PitSnakes == pitName)
        return "Snakes Pit";

      if (WorldGenerator.PitWorms == pitName)
        return "Worms Pit";

      if (pitName == "pit_down_CrazyMiller")
        return "Crazy Miller's Pit";
      else if (pitName == "pit_down_Smiths")
        return "Smiths";
      else if (pitName == "pit_down_StonesMine")
        return "Stones Mine";
      else if (pitName == "pit_down_GatheringEntry")
        return "Gathering Entry";
      
      return pitName;
    }

    public override string ToString()
    {
      return Name;
    }

    //public static string GetPitNameForQuest(string tileName)
    //{
    //  var pitName = "";
    //  var quests = Enum.GetValues(typeof(QuestKind)).Cast<QuestKind>();
    //  foreach (var quest in quests)
    //  {
    //    if (tileName.Contains(quest.ToString()))
    //    {
    //      pitName = DungeonPit.GetPitName(quest);
    //      break;
    //    }
    //  }

    //  return pitName;
    //}

    void SetQuestKind()
    {
      this.QuestKind = GetQuestKind(name);
    }

    public static QuestKind GetQuestKind(string pitName)
    {
      var questKind = QuestKind.Unset;
      var quests = Enum.GetValues(typeof(QuestKind)).Cast<QuestKind>();
      foreach (var quest in quests)
      {
        if (pitName.Contains(GetPitQuestName(quest)))
        {
          questKind = quest;
          break;
        }
      }

      return questKind;
    }

    public void AddLevel(GameLevel lvl)
    {
      Levels.Add(null);//add empty
      SetLevel(Levels.Count - 1, lvl);//set it
    }

    public void SetLevel(int index, GameLevel lvl)
    {
      if (Levels.Count > index)
      {
        lvl.PitName = Name;
        Levels[index] = lvl;
      }
      else
        Debug.Assert(false);
    }

    public void SetGenInfoAtLevelIndex(int index, Roguelike.Generators.GenerationInfo info)
    {
      GenerationInfos[index] = info;
    }

    public Roguelike.Generators.GenerationInfo GetGenInfoAt(int index)
    {
      return GenerationInfos.ContainsKey(index) ? GenerationInfos[index] : null;
    }
  }
}
