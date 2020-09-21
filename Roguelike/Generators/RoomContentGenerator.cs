using Dungeons.Core;
using Dungeons.Tiles;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Generators
{
  public class RoomContentGenerator
  {
    protected int levelIndex;
    protected ILogger logger;
    protected TileContainers.DungeonNode node;
    protected GenerationInfo gi;
    Container container;
    protected LootGenerator lootGen;
    public string LevelBossName { get; set; }

    public virtual void Run(Dungeons.TileContainers.DungeonNode node, int levelIndex, int nodeIndex, GenerationInfo gi, Container container)
    {
      this.node = node as TileContainers.DungeonNode;
      this.container = container;
      this.levelIndex = levelIndex;
      this.gi = gi;
      this.logger = container.GetInstance<ILogger>();
      lootGen = container.GetInstance<LootGenerator>();
      lootGen.LevelIndex = levelIndex;

      if (node.ContentGenerated)
      {
        EnsureBoss();
        return;
      }
            
      GenerateLoot();
      GenerateInteractive();
      GenerateEnemies();
      node.ContentGenerated = true;
    }

    protected Enemy CreateBoss(string name, char symbol)
    {
      var enemy = CreateEnemyInstance(name);
      //var enemy = new Tiles.Enemy();
      //enemy.tag1 = name;// "Miller";
      //enemy.Name = name;// "Miller";
      enemy.Symbol = symbol;// EnemySymbols.QuestBoss;
      enemy.SetNonPlain(true);
      //var empty = node.GetRandomEmptyTile();
      //if (empty != null)
      //  node.SetTile(enemy, empty.Point);
      //else
      //{
      //  this.logger.LogError("no room for boss!!!");
      //}

      return enemy;
    }
        
    protected virtual void GenerateInteractive()
    {
      if (this.gi != null && !gi.GenerateInteractiveTiles)
        return;

      int barrelsNumber = RandHelper.GetRandomInt(5);//TODO
      if (node.IsChildIsland)
      {
        barrelsNumber = 2;
        AddPlainChest();
        node.SetTileAtRandomPosition<Barrel>(levelIndex);
      }
      barrelsNumber++;//at least one
      if (RandHelper.GetRandomDouble() < 0.5)
        barrelsNumber++;

      if (node.Width > 15 || node.Height > 15)
      {
        barrelsNumber += 2;
        if(!node.IsChildIsland)
          AddPlainChest();
      }
      for (int i = 0; i < barrelsNumber; i++)
      {
        var barrel = node.SetTileAtRandomPosition<Barrel>(levelIndex);
        barrel.BarrelKind = RandHelper.GetRandomDouble() < 0.75 ? BarrelKind.Barrel : BarrelKind.PileOfSkulls;
      }

      node.GetTiles<Barrel>().ForEach(i => i.SetLevel(levelIndex + 1));
      node.GetTiles<Chest>().ForEach(i => i.SetLevel(levelIndex + 1));
    }

    private void AddPlainChest()
    {
      var chest = new Chest() { ChestKind = ChestKind.Plain };
      node.SetTileAtRandomPosition(chest);
    }

    protected virtual void GenerateLoot()
    {
      if (this.gi != null && !gi.GenerateLoot)
        return;

      int lootNumber = RandHelper.GetRandomInt(2);//TODO
      if(lootNumber == 0)
        lootNumber++;//at least one
            
      for (int i = 0; i < lootNumber; i++)
      {
        var loot = lootGen.GetRandomLoot(levelIndex+1);
        node.SetTileAtRandomPosition(loot);
      }
      float magicDustThreshold = .1f;
      if (RandHelper.GetRandomDouble() > magicDustThreshold)
      {
        node.SetTileAtRandomPosition(new MagicDust());
      }
    }

    protected virtual Enemy CreateEnemyInstance(string enemyName)
    {
      var enemy = container.GetInstance<Enemy>();
      
      enemy.tag1 = enemyName;
      enemy.Name = enemyName;
      if (EnemySymbols.EnemiesToSymbols.ContainsKey(enemy.Name))
        enemy.Symbol = EnemySymbols.EnemiesToSymbols[enemy.Name];
      return enemy;
    }

    protected virtual bool PlaceEnemy(Enemy enemy, Dungeons.TileContainers.DungeonNode node, Tile keepDistFrom)
    {
      var pt = GetRandomEmptyPoint(node, keepDistFrom);
      if(pt!=null)
        return PlaceEnemy(enemy, node, pt.Value);

      return false;
    }

    private Point? GetRandomEmptyPoint(Dungeons.TileContainers.DungeonNode node, Tile keepDistFrom)
    {
      var emptyTiles = node.GetEmptyTiles();
      if (keepDistFrom != null)
      {
        var emptyTilesDist = emptyTiles.Where(i => i.DistanceFrom(keepDistFrom) >= node.Width/2).ToList();
        if (emptyTilesDist.Any())
          emptyTiles = emptyTilesDist;
      }
      
      var tile = node.GetRandomEmptyTile(emptyTiles, nodeIndex: node.NodeIndex);
      if (tile != null)
        return tile.Point;

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
        logger.LogError("not placed ! " + enemy + " at :" + enemy.Point);
      return res;
    }

    protected virtual bool ShallGenerateChampion(string enemy, int packIndex)
    {
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

      logger.LogInfo("room totsl enemies: " + node.GetTiles<Enemy>().Count);
    }

    //protected void CreateEnemiesPack(string enemyName)
    //{
    //  CreateEnemiesPack(-1, enemyName);
    //}

    protected void CreateEnemiesPack(int packIndex, string enemyName, bool addBoss = false)
    {
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

      SetPowerFromLevel(packEnemies);

      PlaceEnemiesPack(packEnemies);
    }

    private void SetPowerFromLevel(List<Enemy> packEnemies)
    {
      foreach (var en in packEnemies)
      {
        en.SetLevel(levelIndex);
      }
    }

    bool placeEnemiesPackClosely = true;
    protected virtual void PlaceEnemiesPack(List<Enemy> packEnemies, bool keepDistFromHero = true)
    {
      Tile stairs = null;
      if (keepDistFromHero)
      {
        stairs = node.GetTiles<Stairs>().FirstOrDefault(i=>i.StairsKind == StairsKind.LevelUp ||
                                                        i.StairsKind == StairsKind.PitUp);
        if (stairs!=null)
        {

        }
      }
      
      if (placeEnemiesPackClosely)
      {
        var emptyCells = node.GetEmptyTiles();
        var pt = GetRandomEmptyPoint(node, stairs);
        if (pt == null)
        {
          logger.LogError("GetRandomEmptyPoint(node) failed " + node);
          return;
        }
        
        emptyCells.RemoveAll(i=> i.Point == pt);
        foreach (var en in packEnemies)
        {
          PlaceEnemy(en, node, pt.Value);
          var empty = node.GetClosestEmpty(en, emptyCells);
          if (empty == null)
          {
            empty = emptyCells.FirstOrDefault();
          }

          if (empty != null)
          {
            pt = empty.Point;
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

      return ;
    }
  }
}
