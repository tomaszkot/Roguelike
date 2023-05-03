using Dungeons.Core;
using Dungeons.TileContainers;
using Roguelike;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System.Collections.Generic;
using System.Linq;

namespace OuaDII.Generators
{
  public class RoomContentGenerator : Roguelike.Generators.RoomContentGenerator
  {
    Quests.QuestKind questKind;
    string pitName;

    public RoomContentGenerator(Container c) : base(c)
    {
    }

    static Dictionary<Quests.QuestKind, List<string>> EnemiesForQuests = new Dictionary<Quests.QuestKind, List<string>>()
    {
      { Quests.QuestKind.CrazyMiller, new List<string>(){ "rat", "bat"} },
      { Quests.QuestKind.StonesMine, new List<string>(){ "rat", "stone_golem" } }
    };

    //TODO remove it!!!
    static Dictionary<Quests.QuestKind, List<string>> GeneratedEnemiesForQuests = new Dictionary<Quests.QuestKind, List<string>>()
    {
      {Quests.QuestKind.CrazyMiller, new List<string>(){ }},
      {Quests.QuestKind.StonesMine, new List<string>(){ }},
    };

    public void Run
    (
      Dungeons.TileContainers.DungeonNode node, int levelIndex, int nodeIndex,
      int enemiesStartLevel, Roguelike.Generators.GenerationInfo gi, Quests.QuestKind questKind,
      string pitName)
    {
      this.questKind = questKind;
      this.pitName = pitName;
      base.Run(node, levelIndex, nodeIndex, enemiesStartLevel, gi);

    }

    protected override void GenerateInteractive()
    {
      base.GenerateInteractive();
    }

    private bool RoomWithoutRandomStuff()
    {
      return this.questKind == Quests.QuestKind.Smiths || this.questKind == Quests.QuestKind.Malbork;
    }

    private bool ShallGenerateEnemies()
    {
      return !RoomWithoutRandomStuff();
    }

    protected override void GenerateLoot()
    {
      if (RoomWithoutRandomStuff())
        return;

      base.GenerateLoot();
      //for(int i=0;i<1;i++)
      if(RandHelper.GetRandomDouble() > .5)
        node.SetTileAtRandomPosition(new MinedLoot(MinedLootKind.Sulfur) { Count = RandHelper.GetRandomInt(4) });
      //var wpn = this.lootGen.GetRandomEquipment(EquipmentKind.Weapon);
      //var set = node.SetTile(wpn, node.GetFirstEmptyPoint().Value);
    }

    int championsCount = 0;

    public KeyPuzzle KeyPuzzle { get; internal set; }

    protected override bool ShallGenerateChampion(string enemy, int packIndex)
    {
      if (questKind == Quests.QuestKind.StonesMine)
      {
        return true;
      }
      if (questKind == Quests.QuestKind.CrazyMiller)
      {
        if (RandHelper.GetRandomDouble() > 0.5f || (node.NodeIndex == 1 && championsCount == 0))
        {
          return true;
        }

        if (node.IsChildIsland)
        {
          //if (levelIndex == 0 || championsCount == 0)
          //{
          //  championsCount++;
          //  return true;
          //}
          //else
          return false;
        }
      }

      return base.ShallGenerateChampion(enemy, packIndex);
    }

    protected override List<string> GetEnemyNames(DungeonNode node)
    {
      List<string> enemyNames = null;

      if (EnemiesForQuests.ContainsKey(questKind))
      {
        var possibleEnemies = EnemiesForQuests[questKind].ToList();
        possibleEnemies.RemoveAll(i => GeneratedEnemiesForQuests[questKind].Contains(i));
        if (!possibleEnemies.Any())
          possibleEnemies = EnemiesForQuests[questKind].ToList();

        enemyNames = Filter(possibleEnemies);
        GeneratedEnemiesForQuests[questKind].AddRange(enemyNames);
      }
      else
      {
        enemyNames = base.GetEnemyNames(node);
        //enemyNames = possibleEnemyNames;// RandHelper.GetRandomElem<string>();
      }

      return enemyNames;
    }

    protected override void AddPlainChestAtRandomLoc()
    {
      if (RoomWithoutRandomStuff())
        return;
      base.AddPlainChestAtRandomLoc();
    }

    protected override void GenerateEnemies()
    {
      if (this.gi != null && !gi.GenerateEnemies)
        return;

      Roguelike.Tiles.LivingEntities.LivingEntity ent = null;

      if (ShallGenerateEnemies())
      {
        base.GenerateEnemies();

        if (this.questKind == Quests.QuestKind.CrazyMiller)
        {
          if (LevelIndex == 1 && node.IsChildIsland)
          {
            ent = CreateBoss("Miller Bratomir", EnemySymbols.QuestBoss, false);
          }
        }
        else if (this.questKind == Quests.QuestKind.StonesMine)
        {
          if (LevelIndex == 1 && node.IsChildIsland)
          {
            ent = CreateBoss("stone_golem", EnemySymbols.QuestBoss, true);
          }
        }
        //else if (this.questKind == Quests.QuestKind.GatheringEntry)
        //{
        //  if (LevelIndex == 0)
        //  {
        //    ent = CreateBoss("fallen_one", EnemySymbols.QuestBoss, true);
        //  }
        //}
      }
      if (this.questKind == Quests.QuestKind.Smiths)
      {
        if (LevelIndex == 0)
        {
          ent = new OuaDII.Tiles.LivingEntities.Merchant(container);
          ent.tag1 = "ally_smith_Ziemowit";
          ent.Name = Discussions.DiscussionFactory.MerchantZiemowitName;
        }
      }
      if (ent != null)
      {
        var empty = node.GetRandomEmptyTile(DungeonNode.EmptyCheckContext.Unset);
        if (empty != null)
          node.SetTile(ent, empty.point);
      }
      if (ShallGenerateEnemies())
        EnsureBoss();
    }

    protected override void SetBarrelKind(Barrel barrel)
    {
      if (RoomWithoutRandomStuff())
        allowSkullPiles = false;
      base.SetBarrelKind(barrel);
    }
  }
}
