using Dungeons.Core;
using Dungeons.Tiles;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
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
    protected Dungeons.TileContainers.DungeonNode node;
    protected GenerationInfo gi;
    Container container;
    protected LootGenerator lootGen;

    public virtual void Run(Dungeons.TileContainers.DungeonNode node, int levelIndex, int nodeIndex, GenerationInfo gi, Container container)
    {
      this.node = node;
      this.container = container;
      this.levelIndex = levelIndex;
      this.gi = gi;
      this.logger = container.GetInstance<ILogger>();
      lootGen = container.GetInstance<LootGenerator>();

      GenerateLoot();
      GenerateInteractive();
      GenerateEnemies();
    }

    protected virtual void GenerateInteractive()
    {
      if (this.gi != null && !gi.GenerateInteractiveTiles)
        return;

      int lootNumber = RandHelper.GetRandomInt(4);//TODO
      lootNumber++;//at least one
      for (int i = 0; i < lootNumber; i++)
      {
        node.SetTileAtRandomPosition<Barrel>();
      }
    }


    protected virtual void GenerateLoot()
    {
      if (this.gi != null && !gi.GenerateLoot)
        return;

      int lootNumber = RandHelper.GetRandomInt(4);//TODO
      lootNumber++;//at least one
      for (int i = 0; i < lootNumber; i++)
      {
        var mush = node.SetTileAtRandomPosition<Mushroom>();
        //var loot = new Mushroom();
        //loot.tag1 = "mash3";
        //loot.SetKind(MushroomKind.Boletus);
        //bool set = node.SetTile(loot, node.GetFirstEmptyPoint().Value);
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
      if(res)
        logger.LogInfo("placed " + enemy + " at :" + enemy.Point);
      else
        logger.LogError("not placed ! " + enemy + " at :" + enemy.Point);
      return res;
    }

    protected virtual bool ShallGenerateChampion(string enemy, int packIndex)
    {
      var rand = RandHelper.GetRandomDouble();
      if(rand >= 0.5f)
        return true;
      if (gi.GeneratedChempionsCount == 0 && this.node.NodeIndex > gi.NumberOfRooms/2)
        return true;

      if (gi.GeneratedChempionsCount == 1 && this.node.NodeIndex == gi.NumberOfRooms-1)
        return true;

      return false;
    }

    protected virtual List<Enemy> CreateEnemiesPack(string enemyName, int packIndex)
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
        var packEnemies = CreateEnemiesPack(enemyName, packIndex);
        
        if (packEnemies.Any() && ShallGenerateChampion(enemyName, packIndex))
        {
          RandHelper.GetRandomElem<Enemy>(packEnemies).SetNonPlain(false);
          gi.GeneratedChempionsCount++;
        }

        SetPower(packEnemies);

        PlaceEnemiesPack(packEnemies);
      }

      logger.LogInfo("room totsl enemies: " + node.GetTiles<Enemy>().Count);
    }

    private void SetPower(List<Enemy> packEnemies)
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
  }
}
