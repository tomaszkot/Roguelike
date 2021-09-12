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
  public class RoomContentGenerator
  {
    private int levelIndex = 0;
    protected int enemiesStartLevel = 0;
    protected ILogger logger;
    protected TileContainers.DungeonNode node;
    protected GenerationInfo gi;
    protected Container container;
    protected LootGenerator lootGen;
    public string LevelBossName { get; set; }
    public int LevelIndex { get => levelIndex; set => levelIndex = value; }
    Difficulty difficulty;

    public RoomContentGenerator(Container container)
    {
      this.container = container;
    }

    public virtual void Run
    (
      Dungeons.TileContainers.DungeonNode node, int levelIndex, int nodeIndex, int enemiesStartLevel, GenerationInfo gi
    )
    {
      this.difficulty = Roguelike.Generators.GenerationInfo.Difficulty;
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
      for (int i = 0; i < repeat; i++)
      {
        GenerateLoot();
        GenerateInteractive();
        GenerateEnemies();
      }
      node.ContentGenerated = true;
    }

    protected Enemy CreateBoss(string name, char symbol)
    {
      var enemy = CreateEnemyInstance(name);
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
      if (RandHelper.GetRandomDouble() < 0.5)
      { 
        chestsNumber++;
      }

      if (node.Width > 15 || node.Height > 15)
      {
        barrelsNumber += 2;
        if (!node.IsChildIsland)
          chestsNumber++;
      }
      for (int i = 0; i < barrelsNumber; i++)
      {
        var barrel = node.SetTileAtRandomPosition<Barrel>(levelIndex);
        SetBarrelKind(barrel);
      }
      for (int i = 0; i < chestsNumber; i++)
      {
        AddPlainChestAtRandomLoc();
      }

      node.GetTiles<Barrel>().ForEach(i => SetILootSourceLevel(i));
      node.GetTiles<Chest>().ForEach(i => SetILootSourceLevel(i));
    }

    public void AddExtraEnemy(Dungeons.TileContainers.DungeonNode node, string enemy)
    {
      var tilePH = node.GetRandomEmptyTile();
      if (tilePH != null)
      {
        var en = CreateEnemyInstance(enemy);
        node.SetTile(en, tilePH.point);
      }
    }

    protected bool allowSkullPiles = true;
    protected virtual void SetBarrelKind(Barrel barrel)
    {
      var kind = BarrelKind.Barrel;
      if (allowSkullPiles && RandHelper.GetRandomDouble() > 0.75)
        kind = BarrelKind.PileOfSkulls;
      barrel.BarrelKind = kind;
    }

    protected virtual void AddPlainChestAtRandomLoc()
    {
      var chest = new Chest() { ChestKind = ChestKind.Plain };
      node.SetTileAtRandomPosition(chest);
      
    }

    protected virtual void GenerateLoot()
    {
      
      if (this.gi != null && !gi.GenerateLoot)
        return;

      int lootNumber = RandHelper.GetRandomInt(gi.MaxLootPerRoom);
      if (lootNumber == 0)
        lootNumber++;//at least one

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
        node.SetTileAtRandomPosition(new Chest() { ChestKind = ChestKind.Gold });

      if (RandHelper.GetRandomDouble() > .5)
        node.SetTileAtRandomPosition(new ProjectileFightItem(FightItemKind.Stone) { Count = RandHelper.GetRandomInt(4) });
      if (RandHelper.GetRandomDouble() > .5)
        node.SetTileAtRandomPosition(new ProjectileFightItem(FightItemKind.ThrowingKnife) { Count = RandHelper.GetRandomInt(4) });

      //node.SetTileAtRandomPosition(new Hooch() { Count = RandHelper.GetRandomInt(4) });
    }

    public virtual Enemy CreateEnemyInstance(string enemyName)
    {
      var enemy = container.GetInstance<Enemy>();

      enemy.Container = this.container;
      enemy.tag1 = enemyName;
      enemy.Name = enemyName;
      SetILootSourceLevel(enemy);
      if (EnemySymbols.EnemiesToSymbols.ContainsKey(enemy.Name))
        enemy.Symbol = EnemySymbols.EnemiesToSymbols[enemy.Name];
      return enemy;
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

    protected virtual List<Enemy> CreateEnemiesPack(string enemyName)
    {
      List<Enemy> enemiesPack = new List<Enemy>();

      var packSize = gi.ForcedNumberOfEnemiesInRoom;
      if (packSize == -1)
        packSize = RandHelper.GetRandomElem<int>(new List<int>() { 3, 4, 5 });
      for (int enIndex = 0; enIndex < packSize; enIndex++)
      {
        var enemy = CreateEnemyInstance(enemyName);
        enemiesPack.Add(enemy);
      }

      return enemiesPack;
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
      }

      //logger.LogInfo("room totsl enemies: " + node.GetTiles<Enemy>().Count);
    }

    void Log(string log)
    {
      Debug.WriteLine(log);
    }

    protected void CreateEnemiesPack(int packIndex, string enemyName, bool addBoss = false)
    {
      Log("CreateEnemiesPack packIndex: " + packIndex + ", ChildIsland: " + gi.ChildIsland + ", NodeIndex:  " + node.NodeIndex);
      var packEnemies = CreateEnemiesPack(enemyName);

      if (packEnemies.Any() && ShallGenerateChampion(enemyName, packIndex))
      {
        RandHelper.GetRandomElem<Enemy>(packEnemies).SetNonPlain(false);
        gi.GeneratedInfo.ChempionsCount++;
      }

      if (addBoss)
      {
        //var en = packEnemies.Where(i => i.PowerKind == EnemyPowerKind.Plain).FirstOrDefault();
        //if(en)
        var bossSymbol = EnemySymbols.GetSymbolFromName(LevelBossName);
        if (bossSymbol == '\0')
          bossSymbol = EnemySymbols.QuestBoss;
        packEnemies.Add(CreateBoss(enemyName, bossSymbol));

        var chest = node.SetTileAtRandomPosition<Chest>(levelIndex);
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
    }

    void SetILootSourceLevel(ILootSource src)
    {
      var esl = enemiesStartLevel;
      if (esl > 0)
        esl--;

      //esl = 0;

      src.SetLevel(esl + levelIndex + 1, difficulty);
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
        Point? enemyPoint = null;
        if (node.NodeIndex == 0 && levelIndex == 0)
        {
          emptyCells = emptyCells.Where(i => i.DistanceFrom(emptyCells.First()) > (node.Width / 2 + node.Height / 2) / 2).ToList();
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

    protected virtual List<string> GetEnemyNames(Dungeons.TileContainers.DungeonNode node)
    {
      List<string> enemyNames = EnemySymbols.EnemiesToSymbols.Keys.ToList();
      return Filter(enemyNames);
    }

    protected List<string> Filter(List<string> enemyNames)
    {
      var chosen = RandHelper.GetRandomElem<string>(enemyNames);
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
