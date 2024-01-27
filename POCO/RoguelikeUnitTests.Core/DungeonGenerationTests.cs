﻿using Dungeons.TileContainers;
using Dungeons.Tiles;
using NUnit.Framework;
using Roguelike;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class DungeonGenerationTests : TestBase
  {
    [Test]
    [Repeat(1)]
    public void TestCustomInteriorGen()
    {
      var gi = new Dungeons.GenerationInfo();
      gi.ForcedDungeonLayouterKind = Dungeons.DungeonLayouterKind.Default;
      var game = CreateGame(false);
      Assert.Null(game.Hero);
      Assert.AreEqual(game.Level, null);

      game.LevelGenerator.CustomNodeCreator = (int nodeIndex, Dungeons.GenerationInfo gi) =>
      {
        //TODO typed CreateDungeonNodeInstance 
        var dungeon = game.LevelGenerator.CreateDungeonNodeInstance() as Roguelike.Generators.TileContainers.DungeonNode;
        dungeon.Create(10, 10, gi);

        var li = game.LevelGenerator.LevelIndex + 1;
        dungeon.SetTileAtRandomPosition<Enemy>(li, new Enemy(Container));
        dungeon.SetTileAtRandomPosition<Barrel>(li, new Barrel(Container));
        dungeon.SetTileAtRandomPosition<Barrel>(li, new Barrel(Container));
        return dungeon;
      };

      var info = new Roguelike.Generators.GenerationInfo();
      info.NumberOfRooms = 1;
      info.MakeEmpty();
      info.GenerateOuterWalls = false;
      info.ForcedDungeonLayouterKind = Dungeons.DungeonLayouterKind.Default;
      var level = game.LevelGenerator.Generate(0, info);

      Assert.AreEqual(level.Nodes.Count, 1);
      Assert.AreEqual(level.GetTiles<Wall>().Count, 0);
      Assert.AreEqual(level.GetTiles<Enemy>().Count, 1);
      Assert.AreEqual(level.GetTiles<Barrel>().Count, 2);
    }

    [Test]
    public void NewGameTest()
    {
      //game can have 1-n levels (sub-dungeons)
      var game = CreateGame(false);
      Assert.Null(game.Hero);

      game.SetMaxLevelIndex(1);//there will be level0, level1
      var levelZero = game.GenerateLevel(0);

      Assert.NotNull(levelZero);
      Assert.AreEqual(levelZero.Index, 0);//1st level has index 0

      Assert.AreEqual(levelZero, game.Level);
      Assert.NotNull(levelZero.GetTiles<Hero>().SingleOrDefault());

      Assert.NotNull(levelZero.GetTiles().Where(i => i.IsEmpty).FirstOrDefault());
      levelZero.DoGridAction((int col, int row) =>
      {
        if (levelZero.Tiles[row, col] == null)
        {
          //it can be out of walls
          //var tile = levelZero.GetTile(new Point(col, row));
          //Assert.NotNull(tile);
          //Assert.True(tile is Loot);
        }
      });

      //1st level0 has only stairs down
      Assert.AreEqual(levelZero.GetTiles<Stairs>().Count, 1);
      Assert.AreEqual(levelZero.GetTiles<Stairs>()[0].StairsKind, StairsKind.LevelDown);

      var level1 = game.GenerateLevel(1);
      Assert.AreNotEqual(levelZero, level1);

      //last level has NOT stairs down, but shall have up ones
      Assert.AreEqual(level1.GetTiles<Stairs>().Count, 1);
      Assert.AreEqual(level1.GetTiles<Stairs>()[0].StairsKind, StairsKind.LevelUp);
    }

    [Test]
    public void AllLevelsTest()
    {
      var game = CreateGame(false);
      Assert.Null(game.Hero);
      Assert.Null(game.Level);

      int maxLevelIndex = 10;
      game.SetMaxLevelIndex(maxLevelIndex - 1);
      Hero hero = null;

      for (int i = 0; i < maxLevelIndex; i++)
      {
        var level = game.GenerateLevel(i);
        if (i == 0)
        {
          hero = game.Hero;
          Assert.NotNull(level.GetTiles<Hero>().SingleOrDefault());
        }
        else
          Assert.Null(level.GetTiles<Hero>().SingleOrDefault());//he is at 0

        Assert.NotNull(game.Hero);
        Assert.AreEqual(hero, game.Hero);
      }

      for (int levelIndex = 0; levelIndex < maxLevelIndex; levelIndex++)
      {
        Assert.AreEqual(game.Level.Index, levelIndex);
        var stairsDown = game.Level.GetTiles<Stairs>().Where(i => i.StairsKind == StairsKind.LevelDown).SingleOrDefault();
        Assert.True(stairsDown != null || levelIndex == maxLevelIndex - 1);
        if (stairsDown != null)
        {
          game.GameManager.InteractHeroWith(stairsDown);
        }
      }
    }


    [Test]
    public void ChempionsCount()
    {
      //game can have 1-n levels (sub-dungeons)
      var gi = new Roguelike.Generators.GenerationInfo();
      gi.ForcedDungeonLayouterKind = Dungeons.DungeonLayouterKind.Default;
      var game = CreateGame(false);
      Assert.Null(game.Hero);
      //var gi = new Roguelike.Generators.GenerationInfo();
      Assert.Greater(gi.NumberOfRooms, 3);
      Assert.Greater(gi.ForcedNumberOfEnemiesInRoom, 2);
      //gi.NumberOfRooms = 5;
      //gi.ForcedNumberOfEnemiesInRoom = 4;

      var level_0 = game.GenerateLevel(0, gi);

      Assert.AreEqual(level_0.Nodes.Count, 6);
      var enemies = level_0.GetTiles<Enemy>();
      Assert.Greater(enemies.Count, 5);
      var chemps = enemies.Where(i => i.PowerKind == EnemyPowerKind.Champion).Count();
      Assert.Greater(chemps, 1);
      Assert.LessOrEqual(chemps, gi.NumberOfRooms+2);

    }

    [Test]
    [Repeat(1)]
    public void FixedRoomSize()
    {
      //game can have 1-n levels (sub-dungeons)
      var game = CreateGame(false);
      Assert.Null(game.Hero);
      var gi = new Roguelike.Generators.GenerationInfo();
      gi.NumberOfRooms = 1;//tmp
      gi.MaxNodeSize = new System.Drawing.Size(11, 11);
      gi.MinNodeSize = gi.MaxNodeSize;
      gi.PreventSecretRoomGeneration = true;
      gi.ChildIslandAllowed = false;
      gi.ForcedDungeonLayouterKind = Dungeons.DungeonLayouterKind.Default;
      var level0 = game.GenerateLevel(0, gi);
      Assert.NotNull(level0);

      Assert.AreEqual(level0.GeneratorNodes.Count, 1);
      Assert.AreEqual(level0.GeneratorNodes[0].Width, gi.MaxNodeSize.Width);
      Assert.AreEqual(level0.GeneratorNodes[0].Height, gi.MaxNodeSize.Height);
    }

    [TestCase(2)]
    [TestCase(3)]
    public void AppendMazeTest(int numEnemies)
    {
      var level1 = GenRoomWithEnemies(numEnemies);
      Assert.AreEqual(level1.GetTiles<Enemy>().Count(), numEnemies);
      Assert.Greater(level1.GetTiles().Where(i => i.IsEmpty).Count(), numEnemies + 1);

      var level2 = GenRoomWithEnemies(numEnemies);
      Assert.AreEqual(level2.GetTiles<Enemy>().Count(), numEnemies);

      var res = level2.Merge(level1.GetTiles().Where(i => i is Enemy).ToList(), new System.Drawing.Point(0, 0),
        (Dungeons.Tiles.Tile tile) => { return tile is Enemy; });
      Assert.True(res);

      Assert.AreEqual(level2.GetTiles<Enemy>().Count(), numEnemies + numEnemies);
    }

    private DungeonLevel GenRoomWithEnemies(int numEnemies)
    {
      var generator = Container.GetInstance<Dungeons.IDungeonGenerator>();
      var info = new Roguelike.Generators.GenerationInfo();
      info.NumberOfRooms = 1;
      info.GenerateEnemies = false;
      info.MinNodeSize = new Size(8, 8);
      info.MaxNodeSize = info.MinNodeSize;
      info.ChildIslandAllowed = false;
      var level = generator.Generate(0, info);
      Assert.Greater(level.GetTiles().Where(i => i.IsEmpty).Count(), 0);

      int y = 2;
      for (int i = 0; i < numEnemies; i++)
      {
        var en = SpawnEnemy();
        var pt = new Point(2, y++);
        var set = level.SetTile(en, pt);
        Assert.True(set);

        //en = SpawnEnemy();
        //pt = new Point(2, 3);
        //set = level.SetTile(en, pt);
        //Assert.True(set);
      }

      return level;
    }

    //void Log(string log)
    //{
    //  Debug.WriteLine(log);
    //}

    [Test]
    [Repeat(1)]
    public void TestLootRevealFlagBasic()
    {
      Log("TestLootRevealFlagBasic start");
       var info = new Roguelike.Generators.GenerationInfo();
      info.NumberOfRooms = 1;
      info.MinNodeSize = new Size(15, 15);
      info.MaxNodeSize = new Size(30, 30);
      info.ForceChildIslandInterior = true;
      info.PreventSecretRoomGeneration = true;
      info.ForcedNumberOfEnemiesInRoom = 10;
      info.ForcedDungeonLayouterKind = Dungeons.DungeonLayouterKind.Default;
      var game = CreateGame(gi: info);

      var level = game.Level;
      Assert.GreaterOrEqual(level.Width, info.MinNodeSize.Width);
      Assert.GreaterOrEqual(level.Height, info.MinNodeSize.Height);
      Assert.AreEqual(level.Nodes.Count, 1);
      var normalRoom = level.Nodes[0];
      Assert.AreEqual(normalRoom.ChildIslands.Count, 1);

      Assert.True(normalRoom.Revealed);
      var island = normalRoom.ChildIslands[0];
      Assert.False(island.Revealed);

      var islandTiles = level.GetTiles().Where(i => i.DungeonNodeIndex == island.NodeIndex).ToList();
      Assert.Greater(islandTiles.Count, 0);

      var en = level.GetTiles().Where(i => i is Enemy).ToList();
      //var enNormal = normalRoom.GetTiles<Enemy>();
      //var enIsland = normalRoom.GetTiles<Enemy>();
      //Assert.AreEqual(en.Count, enNormal.Count + enIsland.Count);

      var normalRoomEnemies = en.Where(i => i.DungeonNodeIndex == normalRoom.NodeIndex).ToList();
      var islandRoomEnemies = en.Where(i => i.DungeonNodeIndex == island.NodeIndex).ToList();
      Assert.AreEqual(en.Count, normalRoomEnemies.Count + islandRoomEnemies.Count);
      Assert.Greater(normalRoomEnemies.Count, 0);
      //Assert.Greater(islandRoomEnemies.Count, 0);TODO
      Log("TestLootRevealFlagBasic end");
    }

    [Test]
    public void TestLootRevealFlagAdv()
    {
      var info = new Roguelike.Generators.GenerationInfo();
      info.NumberOfRooms = 2;
      info.MinNodeSize = new Size(15, 15);
      info.MaxNodeSize = new Size(30, 30);
      info.ForceChildIslandInterior = true;
      info.PreventSecretRoomGeneration = true;
      info.ForcedDungeonLayouterKind = Dungeons.DungeonLayouterKind.Default;
      var game = CreateGame(gi: info);

      var level = game.Level;
      Assert.GreaterOrEqual(level.Width, info.MinNodeSize.Width);
      Assert.GreaterOrEqual(level.Height, info.MinNodeSize.Height);
      Assert.AreEqual(level.Nodes.Count, 2);
      Assert.AreEqual(level.Nodes[0].ChildIslands.Count, 1);
      Assert.AreEqual(level.Nodes[1].ChildIslands.Count, 1);

      Assert.True(level.Nodes[0].Revealed);
      Assert.False(level.Nodes[1].Revealed);

      var chidIsl1 = level.Nodes[0].ChildIslands[0];
      Assert.False(chidIsl1.Revealed);
      var chidIsl2 = level.Nodes[1].ChildIslands[0];
      Assert.False(chidIsl2.Revealed);
      Assert.Greater(level.GetTiles().Where(i => i.DungeonNodeIndex == chidIsl1.NodeIndex).Count(), 1);
      Assert.Greater(level.GetTiles().Where(i => i.DungeonNodeIndex == chidIsl2.NodeIndex).Count(), 1);

      //Assert.AreEqual(level.Height, info.MaxNodeSize);
      //var en = level.GetTiles().Where(i => i is Enemy).ToList();
      //var lootGrouped = level.GetTiles().Where(i=> i is Loot).ToList().GroupBy(i=>i.DungeonNodeIndex).ToList();
      //var zeroIndexCount = tiles.Where(i => i.DungeonNodeIndex == 0).Count();

      //var nonZero = level.GetTiles().Where(i => i.DungeonNodeIndex != 0).ToList();
      //Assert.AreEqual(zeroIndexCount, tiles.Count);
    }

    [Test]
    public void TestHeroTile()
    {
      var game = CreateGame();
      Assert.NotNull(game.Hero);
      Assert.AreEqual(game.Hero.DungeonNodeIndex, game.Level.Nodes.Where(i => !i.Secret).First().NodeIndex);
    }

    [Test]
    public void EnemiesNames()
    {
      var game = CreateGame(false);
      game.SetMaxLevelIndex(1);//there will be level0, level1
      game.GenerateLevel(0);
      var enemies = game.Level.GetTiles<Enemy>();
      enemies.ForEach(i =>
      {
        Assert.True(i.Name.Any());
        Assert.True(i.Name.ToLower() != "enemy");
      });
      { 
        var enemy = CreateEnemy();
        AssertEnemyName(enemy, true);
        enemy.Symbol = EnemySymbols.SkeletonSymbol;
        AssertEnemyName(enemy, false, "Skeleton");
      }
      {
        var enemy = CreateEnemy();
        AssertEnemyName(enemy, true);
        enemy.tag1 = "drowned_man";
        enemy.SetNameFromTag1();
        AssertEnemyName(enemy, false, "Drowned man");
      }
      {
        var enemy = CreateEnemy();
        enemy.tag1 = "bandit3";
        AssertEnemyName(enemy, true);
        enemy.SetNameFromTag1();
        AssertEnemyName(enemy, false, "Bandit");
      }
    }

    private static void AssertEnemyName(Enemy enemy, bool empty, string name = "")
    {
      if (empty)
      {
        Assert.True(!enemy.Name.Any() || enemy.Name == "Enemy");
        Assert.True(!enemy.DisplayedName.Any() || enemy.DisplayedName == "Enemy");
      }
      else
      {
        Assert.True(enemy.Name == name);
        Assert.True(enemy.DisplayedName == name);
      }
    }

    //[Test]
    [TestCase(SurfaceKind.ShallowWater)]
    [TestCase(SurfaceKind.Oil)]
    public void SurfaceTest(SurfaceKind surfaceKind)
    {
      var game = CreateGame();
      Assert.NotNull(game.Hero);
      var surfCount = GetSurfaces(surfaceKind, game).Count;
      var sur = new Surface() { Kind = surfaceKind };
      var placement = SetCloseToHero(sur);
      Assert.Greater(GetSurfaces(surfaceKind, game).Count, surfCount);
      game.GameManager.HandleHeroShift(placement.Item2);
      Assert.AreEqual(game.Hero.point, sur.point);
      Assert.True(game.Level.GetSurfaceKindsUnderLivingEntity(game.Hero).Contains(sur.Kind));
    }

    private static bool LevelHasSurfaceKind(SurfaceKind surfaceKind, RoguelikeGame game)
    {
      return GetSurfaces(surfaceKind, game).Any();
    }

    private static System.Collections.Generic.Dictionary<Point, Surface> GetSurfaces(SurfaceKind surfaceKind, RoguelikeGame game)
    {
      return game.Level.SurfaceSets.GetKind(surfaceKind).Tiles;
    }

    [TestCase(SurfaceKind.Lava, true)]
    [TestCase(SurfaceKind.Oil, true)]
    [TestCase(SurfaceKind.Oil, false)]
    public void SurfaceHurtTestForHero(SurfaceKind surfaceKind, bool isBurning)
    {
      var game = CreateGame();
      Assert.NotNull(game.Hero);
      var surfCount = GetSurfaces(surfaceKind, game).Count;
      var sur = new Surface() { Kind = surfaceKind };
      sur.IsBurning = isBurning;
      var placement = SetCloseToHero(sur);
      Assert.Greater(GetSurfaces(surfaceKind, game).Count, surfCount);

      AssertLavaEffects(game.Hero, sur, placement);
    }

    [TestCase(SurfaceKind.Oil)]
    [Repeat(1)]
    public void OilSurfaceBurnsOut(SurfaceKind surfaceKind)
    {
      var game = CreateGame();
      Assert.NotNull(game.Hero);
      var surfCount = GetSurfaces(surfaceKind, game).Count;
      var sur = new Surface() { Kind = surfaceKind };
      sur.IsBurning = true;
      var set = game.Level.SetTile(sur, game.Level.GetEmptyTiles().First().point);
      Assert.True(set);
      Assert.Greater(GetSurfaces(surfaceKind, game).Count, surfCount);
      WaitForFireEnd();
      Assert.AreEqual(GetSurfaces(surfaceKind, game).Count, surfCount);
    }

    private void WaitForFireEnd()
    {
      for (int i = 0; i < 10; i++)
      {
        GotoNextHeroTurn();
      }
    }

    private void AssertLavaEffects(LivingEntity le, Surface sur, System.Tuple<Point, Dungeons.TileNeighborhood> placement)
    {
      Assert.True(le.Alive);
      var health = le.Stats.Health;
      var set = game.Level.SetTile(le, placement.Item1);
      Assert.True(set);
      GotoNextHeroTurn();
      //Assert.AreEqual(le.point, sur.point);
      //Assert.AreEqual(game.Level.GetSurfaceKindUnderLivingEntity(le), sur.Kind);
      Assert.True(le.Alive);
      if (sur.IsBurning)
      {
        Assert.True(le.HasLastingEffect(Roguelike.Effects.EffectType.Firing));
        Assert.Less(le.Stats.Health, health);
      }
    }

    [TestCase(SurfaceKind.Lava)]
    [TestCase(SurfaceKind.Oil)]
    public void SurfaceHurtTestForEnemy(SurfaceKind surfaceKind)
    {
      var game = CreateGame(true, 1);
      var enemy = AllEnemies.First();
      PlaceCloseToHero(enemy);

      var surfCount = GetSurfaces(surfaceKind, game).Count;
      var sur = new Surface() { Kind = surfaceKind };
      sur.IsBurning = true;
      var placement = SetCloseToLivingEntity(sur, enemy);
      Assert.Greater(GetSurfaces(surfaceKind, game).Count, surfCount);
      AssertLavaEffects(enemy, sur, placement);
    }


    [TestCase(SurfaceKind.Oil)]
    public void SurfaceSpread(SurfaceKind surfaceKind)
    {
      var game = CreateGame(true, 1);
      var surfCount = GetSurfaces(surfaceKind, game).Count;
      game.GameManager.SpreadOil(game.Hero);
      Assert.Greater(GetSurfaces(surfaceKind, game).Count, surfCount);
    }

    [Test]
    public void OilSpreadFire()
    {
      var game = CreateGame(true, 1);
      GetSurfaces(SurfaceKind.Oil, game).Clear();
      game.GameManager.SpreadOil(game.Hero);
      var oilTiles = GetSurfaces(SurfaceKind.Oil, game).Values;
      Assert.Greater(oilTiles.Count, 1);
      foreach (var sur in oilTiles)
        Assert.False(sur.IsBurning);

      UseFireBallSpellSource(game.Hero, oilTiles.ElementAt(0) as HitableSurface, true);
      foreach (var sur in oilTiles)
        Assert.True(sur.IsBurning);

      Assert.True(game.Hero.HasLastingEffect(Roguelike.Effects.EffectType.Firing));
      WaitForFireEnd();
      oilTiles = GetSurfaces(SurfaceKind.Oil, game).Values;
      Assert.AreEqual(oilTiles.Count, 0);
    }

    [Test]
    public void OilSpreadFromBarrel()
    {
      var game = CreateGame(true, 1);
      var barrel = new Barrel(Container) { BarrelKind = BarrelKind.OilBarrel };
      SetCloseToHero(barrel);
      var oilTilesCount = GetSurfaces(SurfaceKind.Oil, game).Values.Count;
      
      game.GameManager.InteractHeroWith(barrel);

      var oilTiles = GetSurfaces(SurfaceKind.Oil, game).Values;
      Assert.Greater(oilTiles.Count, oilTilesCount);
      foreach (var sur in oilTiles)
        Assert.False(sur.IsBurning);
    }
    [Test]
    public void SpreadOilTest()
    {
      var game = CreateGame(false);
      Assert.Null(game.Hero);

      game.SetMaxLevelIndex(0);//there will be level0
      var levelZero = game.GenerateLevel(0);
      Assert.True(game.Level.SurfaceSets.GetKind(SurfaceKind.Oil).Tiles.Values.Any());
    }

    }
}
