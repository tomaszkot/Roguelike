using Dungeons.Core;
using Dungeons.Tiles;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Roguelike.Generators
{
  public class RoomContentGenerator : BaseTilesGenerator
  {
    private int levelIndex = 0;
    int enemiesStartLevel = 0;
    protected ILogger logger;
    protected TileContainers.DungeonNode node;
    protected GenerationInfo gi;
    protected Container container;
    protected LootGenerator lootGen;
    public string LevelBossName { get; set; }
    public int LevelIndex { get => levelIndex; set => levelIndex = value; }
    public int EnemiesStartLevel { get => enemiesStartLevel; set => enemiesStartLevel = value; }

    

    public RoomContentGenerator(Container container) : base(container)
    {
      this.container = container;
    }

    public virtual void Run
    (
      Dungeons.TileContainers.DungeonNode node,//room
      int levelIndex, 
      int nodeIndex, 
      int enemiesStartLevel,
      GenerationInfo gi
    )
    {
      
      this.enemiesStartLevel = enemiesStartLevel;
      this.node = node as TileContainers.DungeonNode;
      this.levelIndex = levelIndex;
      this.gi = gi;
      this.logger = container.GetInstance<ILogger>();
      lootGen = container.GetInstance<LootGenerator>();
      lootGen.LevelIndex = levelIndex;

      if (node.ContentGenerated)
      {
        EnsureBoss();//TODO what is the point of it, needed after load ?
        return;
      }

      int repeat = CalcNumberOfDynamicOnes(1);
      try
      {
        for (int i = 0; i < repeat; i++)
        {
          GenerateLoot();
          GenerateInteractive();
          GenerateEnemies();
        }

        SetILootSourceLevel(node, null);
      }
      catch (System.Exception ex)
      {
        //TODO in corridor mode sometimes there is not enough room :/
        logger.LogError(ex);
      }


      node.ContentGenerated = true;
    }

    protected Enemy CreateBoss(string name, char symbol, bool addBossToTag1)
    {
      var enemy = CreateEnemyInstance(container, name, false, true);
      if(addBossToTag1)
        enemy.tag1 += "_boss";
      enemy.Symbol = symbol;// EnemySymbols.QuestBoss;
      enemy.SetNonPlain(true);

      return enemy;
    }

    int CalcNumberOfDynamicOnes(int startNum)
    {
      float mult = (float)this.node.Width/ (float)Dungeons.GenerationInfo.MaxRoomSideSize;
      if (mult < 1)
        mult = 1;

      var fac = (int)System.Math.Ceiling(mult);
      int num = startNum * fac;
      return num;
    }

    protected virtual void GenerateInteractive()
    {
      if (this.gi != null && !gi.GenerateInteractiveTiles)
        return;

      int barrelsNumber = RandHelper.GetRandomInt(gi.MaxBarrelsPerRoom) + 1;
      int chestsNumber = 1;
      
      if (node.IsChildIsland)
      {
        barrelsNumber = 2;
        chestsNumber = 1;
      }

      if (RandHelper.GetRandomDouble() < 0.5)
      {
        barrelsNumber++;
      }
      if (RandHelper.GetRandomDouble() < 0.2)
      {
        chestsNumber++;
      }
      else
        barrelsNumber++;

      if (gi.MinimalContent)
      {
        barrelsNumber /= 2;
        chestsNumber = 1;
      }
      else if (node.Width > 15 || node.Height > 15)
      {
        barrelsNumber += 2;
        if (!node.IsChildIsland)
        {
          if (RandHelper.GetRandomDouble() < 0.15)//too many chests
            chestsNumber++;
        }
      }
      for (int i = 0; i < barrelsNumber; i++)
      {
        var barrel = node.SetTileAtRandomPosition(levelIndex, new Barrel(node.Container));
        if(barrel!=null)
          SetBarrelKind(barrel);
      }
      //var barrelO = node.SetTileAtRandomPosition<Barrel>(levelIndex);
      //if (barrelO != null)
      //  barrelO.BarrelKind = BarrelKind.OilBarrel;

      if (node.Corridor)
        chestsNumber = 1;
      for (int i = 0; i < chestsNumber; i++)
      {
        AddPlainChestAtRandomLoc();
      }
      //var chest = new DeadBody(); 
      //node.SetTileAtRandomPosition(chest);
      
    }

    public void AddExtraEnemy(Dungeons.TileContainers.DungeonNode node, string enemy)
    {
      var tilePH = node.GetRandomEmptyTile(Dungeons.TileContainers.DungeonNode.EmptyCheckContext.Unset);
      if (tilePH != null)
      {
        var en = CreateEnemyInstance(container, enemy, false, true);
        node.SetTile(en, tilePH.point);
      }
    }

    protected bool allowSkullPiles = true;
    protected virtual void SetBarrelKind(Barrel barrel)
    {
      var kind = BarrelKind.Barrel;
      if (allowSkullPiles && RandHelper.GetRandomDouble() > 0.5)
        kind = BarrelKind.PileOfSkulls;
      else if(RandHelper.GetRandomDouble() > 0.6)
        kind = BarrelKind.OilBarrel;
      barrel.BarrelKind = kind;
    }

    protected virtual void AddPlainChestAtRandomLoc()
    {
      var chest = new Chest(node.Container) { ChestKind = ChestKind.Plain };
      node.SetTileAtRandomPosition(chest);
      chest.ChestVisualKind = RandHelper.GetRandomDouble() > 0.6 ? ChestVisualKind.Grave : ChestVisualKind.Chest;
    }

    protected virtual void GenerateLoot()
    {
      if (this.gi != null && !gi.GenerateLoot)
        return;

      int lootNumber = RandHelper.GetRandomInt(gi.MaxLootPerRoom);
      if (lootNumber == 0)
        lootNumber++;//at least one

      if (gi.MinimalContent)
        lootNumber = 1;

      for (int i = 0; i < lootNumber; i++)
      {
        var loot = lootGen.GetRandomLoot(levelIndex + 1);
        node.SetTileAtRandomPosition(loot);
      }
      float magicDustThreshold = .1f;
      if (RandHelper.GetRandomDouble() > magicDustThreshold)
      {
        node.SetTileAtRandomPosition(new MagicDust());
      }

      if (node.Secret)
        node.SetTileAtRandomPosition(new Chest(node.Container) { ChestKind = ChestKind.Gold });

      if (RandHelper.GetRandomDouble() > .5)
        node.SetTileAtRandomPosition(new ProjectileFightItem(FightItemKind.Stone) { Count = RandHelper.GetRandomInt(4) });
      if (RandHelper.GetRandomDouble() > .9)
        node.SetTileAtRandomPosition(new ProjectileFightItem(FightItemKind.Stone) { Count = RandHelper.GetRandomInt(4) });
      if (RandHelper.GetRandomDouble() > .5)
        node.SetTileAtRandomPosition(new ProjectileFightItem(FightItemKind.ThrowingKnife) { Count = RandHelper.GetRandomInt(4) });
      if (RandHelper.GetRandomDouble() > .6)
        node.SetTileAtRandomPosition(new ProjectileFightItem(FightItemKind.ThrowingTorch) { Count = RandHelper.GetRandomInt(4) });
      if (RandHelper.GetRandomDouble() > .9)
        node.SetTileAtRandomPosition(new ProjectileFightItem(FightItemKind.CannonBall) { Count = RandHelper.GetRandomInt(4) });


      //node.SetTileAtRandomPosition(new Hooch() { Count = RandHelper.GetRandomInt(4) });
    }
        
    protected virtual bool PlaceEnemy(Enemy enemy, Dungeons.TileContainers.DungeonNode node, Tile keepDistFrom)
    {
      var pt = GetRandomEmptyPoint(node, keepDistFrom);
      if (pt != null)
        return PlaceEnemy(enemy, node, pt.Value);

      return false;
    }

    private Point? GetRandomEmptyPoint(Dungeons.TileContainers.DungeonNode node, Tile keepDistFrom)
    {
      var emptyTiles = node.GetEmptyTiles();
      if (keepDistFrom != null)
      {
        var emptyTilesDist = emptyTiles.Where(i => i.DistanceFrom(keepDistFrom) >= node.Width / 2).ToList();
        if (emptyTilesDist.Any())
          emptyTiles = emptyTilesDist;
      }

      var tile = node.GetRandomEmptyTile(emptyTiles, nodeIndex: node.NodeIndex);
      if (tile != null)
        return tile.point;

      return null;
    }

    protected virtual bool PlaceEnemy(Enemy enemy, Dungeons.TileContainers.DungeonNode node, Point pt)
    {
      var res = node.SetTile(enemy, pt);
      if (res)
      {
        // logger.LogInfo("placed " + enemy + " at :" + enemy.Point);
      }
      else
        logger.LogError("not placed ! " + enemy + " at :" + enemy.point);
      return res;
    }

    protected virtual bool ShallGenerateChampion(string enemy, int packIndex)
    {
      if (node.Secret)
        return true;
      if (gi.GeneratedInfo.ChempionsCount == gi.NumberOfRooms - 1)
        return false;
      var rand = RandHelper.GetRandomDouble();
      if (rand >= 0.4f)
        return true;
      if (gi.GeneratedInfo.ChempionsCount == 0 && this.node.NodeIndex > gi.NumberOfRooms / 2)
        return true;

      if (gi.GeneratedInfo.ChempionsCount == 1 && this.node.NodeIndex == gi.NumberOfRooms - 1)
        return true;

      return false;
      //return true;
    }

    

    protected virtual void GenerateEnemies()
    {
      if (this.gi != null && !gi.GenerateEnemies)
        return;
      var enemyNames = GetEnemyNames(node);
      
      for (int packIndex = 0; packIndex < enemyNames.Count; packIndex++)
      {
        var enemyName = enemyNames[packIndex];
        CreateEnemiesPack(packIndex, enemyName);
        if (gi.MinimalContent)
          break;
      }

      //logger.LogInfo("room totsl enemies: " + node.GetTiles<Enemy>().Count);
    }

    void Log(string log)
    {
      //Debug.WriteLine(log);
    }

    protected void CreateEnemiesPack(int packIndex, string enemyName, bool addBoss = false)
    {
      Log("CreateEnemiesPack start packIndex: " + packIndex + ", ChildIsland: " + gi.ChildIsland + ", NodeIndex:  "
        + node.NodeIndex + " ChempsCount: "+ gi.GeneratedInfo.ChempionsCount);
      var packEnemies = CreateEnemiesPack(gi, enemyName);
      var shallCh = ShallGenerateChampion(enemyName, packIndex);
      if (packEnemies.Any() && shallCh)
      {
        RandHelper.GetRandomElem<Enemy>(packEnemies).SetNonPlain(false);
        gi.GeneratedInfo.ChempionsCount++;
        Log("ChempionsCount added!");
      }
      else
      {
        Log("ChempionsCount not inc! NodeIndex:" + this.node.NodeIndex + " ChempionsCount: "+ gi.GeneratedInfo.ChempionsCount);
      }

      if (addBoss)
      {
        //var en = packEnemies.Where(i => i.PowerKind == EnemyPowerKind.Plain).FirstOrDefault();
        //if(en)
        var bossSymbol = EnemySymbols.GetSymbolFromName(LevelBossName);
        if (bossSymbol == '\0')
          bossSymbol = EnemySymbols.QuestBoss;
        packEnemies.Add(CreateBoss(enemyName, bossSymbol, true));

        var chest = node.SetTileAtRandomPosition(levelIndex, new Chest(node.Container));
        chest.ChestKind = ChestKind.GoldDeluxe;
      }

      //SetPowerFromLevel(packEnemies);

      PlaceEnemiesPack(packEnemies);
      foreach (var en in packEnemies)
      {
        Log("enemy: "+en);
        if (en.DungeonNodeIndex != node.NodeIndex)
        {
          Log("en.DungeonNodeIndex != node.NodeIndex!");
        }
      }

      Log("CreateEnemiesPack end packIndex: " + packIndex + ", ChildIsland: " + gi.ChildIsland + ", NodeIndex:  " +
        node.NodeIndex + " ChempsCount: " + gi.GeneratedInfo.ChempionsCount);
    }

    protected override void SetILootSourceLevel(ILootSource src, Hero hero)
    {
      var esl = enemiesStartLevel;
      if (esl > 0)
        esl--;

      src.SetLevel(esl + levelIndex + 1, Difficulty);
    }

    bool placeEnemiesPackClosely = true;
    protected virtual void PlaceEnemiesPack(List<Enemy> packEnemies, bool keepDistFromHero = true)
    {
      Tile stairs = null;
      if (keepDistFromHero)
      {
        stairs = node.GetTiles<Stairs>().FirstOrDefault(i => i.StairsKind == StairsKind.LevelUp ||
                                                        i.StairsKind == StairsKind.PitUp);
        if (stairs != null)
        {

        }
      }

      if (placeEnemiesPackClosely)
      {
        var emptyCells = node.GetEmptyTiles().Where(i=>i.DungeonNodeIndex == node.NodeIndex).ToList();
        if (!emptyCells.Any())
          return;//corridor ?

        Point? enemyPoint = null;
        if (node.NodeIndex == 0 && levelIndex == 0)
        {
          emptyCells = emptyCells.Where(i => i.DistanceFrom(emptyCells.First()) > (node.Width / 2 + node.Height / 2) / 2).ToList();
          if (!emptyCells.Any())
            return;//corridor ?
          enemyPoint = emptyCells.GetRandomElem<Tile>().point;
        }
        else
        {
          enemyPoint = GetRandomEmptyPoint(node, stairs);
          if (enemyPoint == null)
          {
            logger.LogError("GetRandomEmptyPoint(node) failed " + node);
            return;
          }
        }

        emptyCells.RemoveAll(i => i.point == enemyPoint);
        foreach (var en in packEnemies)
        {
          PlaceEnemy(en, node, enemyPoint.Value);
          var empty = node.GetClosestEmpty(en, emptyCells);
          if (empty == null)
          {
            empty = emptyCells.FirstOrDefault();
          }

          if (empty != null)
          {
            enemyPoint = empty.point;
            emptyCells.Remove(empty);
          }
          else
          {
            logger.LogError("node.GetClosestEmpty failed " + node);
            return;
          }
        }
      }
      else
      {
        foreach (var en in packEnemies)
        {
          PlaceEnemy(en, node, stairs);
        }
      }
    }

    List<string> enemyNames = new List<string>
      {
      "bat",
      "rat",
      "spider",
      "skeleton",
      "snake",
      "worm",
      "stone_golem",
      "wolf_skeleton"
      };

    protected override List<string> GetEnemyNames(Dungeons.TileContainers.DungeonNode node)
    {
      //List<string> enemyNames = //EnemySymbols.EnemiesToSymbols.Keys.ToList();
      return Filter(enemyNames);
    }

    protected override List<string> Filter(List<string> enemyNames)
    {
      var chosen = RandHelper.GetRandomElem<string>(enemyNames);
      if (GenerationInfo.DebugInfo.ForcedEnemyName.Any())
        chosen = GenerationInfo.DebugInfo.ForcedEnemyName;
      if(gi.ForcedEnemyName.Any())
        chosen = gi.ForcedEnemyName;
      var selEnemyNames = new List<string>();
      selEnemyNames.Add(chosen);
      return selEnemyNames;
    }

    public void EnsureBoss()
    {
      if (!string.IsNullOrEmpty(LevelBossName))
      {
        CreateEnemiesPack(-1, LevelBossName, true);
        //return CreateBoss(LevelBossName, bossSymbol);
      }

      return;
    }
  }
}
