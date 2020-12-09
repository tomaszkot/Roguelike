using Dungeons.TileContainers;
using Dungeons.Tiles;
using NUnit.Framework;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using SimpleInjector;
using System.Drawing;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class DungeonGenerationTests : TestBase
  {
    [Test]
    public void TestCustomInteriorGen()
    {
      var game = CreateGame(false);
      Assert.Null(game.Hero);
      Assert.AreEqual(game.Level, null);

      game.LevelGenerator.CustomNodeCreator = (int nodeIndex, Dungeons.GenerationInfo gi) =>
      {
        //TODO typed CreateDungeonNodeInstance 
        var dungeon = game.LevelGenerator.CreateDungeonNodeInstance() as Roguelike.Generators.TileContainers.DungeonNode;
        dungeon.Create(10, 10, gi);

        var li = game.LevelGenerator.LevelIndex + 1;
        dungeon.SetTileAtRandomPosition<Enemy>(li);
        dungeon.SetTileAtRandomPosition<Barrel>(li);
        dungeon.SetTileAtRandomPosition<Barrel>(li);
        return dungeon;
      };

      var info = new Roguelike.GenerationInfo();
      info.NumberOfRooms = 1;
      info.MakeEmpty();
      info.GenerateOuterWalls = false;
      var level = game.LevelGenerator.Generate(0, info);

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
      var game = CreateGame(false);
      Assert.Null(game.Hero);
      var gi = new Roguelike.GenerationInfo();
      Assert.Greater(gi.NumberOfRooms, 3);
      Assert.Greater(gi.ForcedNumberOfEnemiesInRoom, 2);
      //gi.NumberOfRooms = 5;
      //gi.ForcedNumberOfEnemiesInRoom = 4;

      var level_0 = game.GenerateLevel(0, gi);

      Assert.AreEqual(level_0.Nodes.Count, 5);
      var enemies = level_0.GetTiles<Enemy>();
      Assert.Greater(enemies.Count, 5);
      var chemps = enemies.Where(i => i.PowerKind == EnemyPowerKind.Champion).Count();
      Assert.Greater(chemps, 1);
      Assert.Less(chemps, gi.NumberOfRooms);

    }

    [Test]
    public void FixedRoomSize()
    {
      //game can have 1-n levels (sub-dungeons)
      var game = CreateGame(false);
      Assert.Null(game.Hero);
      var gi = new Roguelike.GenerationInfo();
      gi.NumberOfRooms = 1;//tmp
      gi.MaxNodeSize = new System.Drawing.Size(11, 11);
      gi.MinNodeSize = gi.MaxNodeSize;
      var level0 = game.GenerateLevel(0, gi);
      Assert.NotNull(level0);

      Assert.AreEqual(level0.GeneratorNodes.Count, 1);
      Assert.AreEqual(level0.GeneratorNodes[0].Width, gi.MaxNodeSize.Width);
      Assert.AreEqual(level0.GeneratorNodes[0].Height, gi.MaxNodeSize.Height);
    }

    [Test]
    public void AppendMazeTest()
    {
      var level1 = GenRoomWithEnemies();
      Assert.AreEqual(level1.GetTiles<Enemy>().Count(), 2);
      Assert.Greater(level1.GetTiles().Where(i => i.IsEmpty).Count(), 3);

      var level2 = GenRoomWithEnemies();
      Assert.AreEqual(level2.GetTiles<Enemy>().Count(), 2);

      level2.Merge(level1.GetTiles().Where(i => i is Enemy).ToList(), new System.Drawing.Point(0, 0),
        (Dungeons.Tiles.Tile tile) => { return tile is Enemy; });

      Assert.AreEqual(level2.GetTiles<Enemy>().Count(), 4);
    }

    private DungeonLevel GenRoomWithEnemies()
    {
      var generator = Container.GetInstance<Dungeons.IDungeonGenerator>();
      var info = new Roguelike.GenerationInfo();
      info.NumberOfRooms = 1;
      info.GenerateEnemies = false;
      info.MinNodeSize = new Size(8, 8);
      info.MaxNodeSize = info.MinNodeSize;
      var level = generator.Generate(0, info);
      Assert.Greater(level.GetTiles().Where(i => i.IsEmpty).Count(), 0);

      var en = SpawnEnemy();
      var pt = new Point(2, 2);
      var set = level.SetTile(en, pt);
      Assert.True(set);

      en = SpawnEnemy();
      pt = new Point(2, 3);
      set = level.SetTile(en, pt);
      Assert.True(set);
      return level;
    }

    [Test]
    public void TestLootRevealFlagBasic()
    {
      var info = new Roguelike.GenerationInfo();
      info.NumberOfRooms = 1;
      info.MinNodeSize = new Size(15, 15);
      info.MaxNodeSize = new Size(30, 30);
      info.ForceChildIslandInterior = true;
      var game = CreateGame(gi: info);

      var level = game.Level;
      Assert.GreaterOrEqual(level.Width, info.MinNodeSize.Width);
      Assert.GreaterOrEqual(level.Height, info.MinNodeSize.Height);
      Assert.AreEqual(level.Nodes.Count, 1);
      Assert.AreEqual(level.Nodes[0].ChildIslands.Count, 1);

      Assert.True(level.Nodes[0].Revealed);
      var island = level.Nodes[0].ChildIslands[0];
      Assert.False(island.Revealed);
      Assert.Greater(level.GetTiles().Where(i => i.DungeonNodeIndex == island.NodeIndex).Count(), 0);

      var en = level.GetTiles().Where(i => i is Enemy).ToList();

      Assert.Greater(en.Where(i => i.DungeonNodeIndex == level.Nodes[0].NodeIndex).Count(), 0);
      Assert.Greater(en.Where(i => i.DungeonNodeIndex == island.NodeIndex).Count(), 0);
    }

    [Test]
    public void TestLootRevealFlagAdv()
    {
      var info = new Roguelike.GenerationInfo();
      info.NumberOfRooms = 2;
      info.MinNodeSize = new Size(15, 15);
      info.MaxNodeSize = new Size(30, 30);
      info.ForceChildIslandInterior = true;
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
      Assert.AreEqual(game.Hero.DungeonNodeIndex, 0);
    }


    //[Test]
    //public void TestLootGeneration()
    //{

    //  var node = new Roguelike.Generators.TileContainers.DungeonNode(Container);
    //  node.Create(3, 3);
    //  var loot = new Loot();
    //  loot.tag = "super loot";
    //  Assert.True(node.SetTile(loot, new System.Drawing.Point(1, 2)));

    //  var level = new Roguelike.TileContainers.DungeonLevel(Container);
    //  level.Create(3, 3);
    //  var zeroIndexCount = level.GetTiles().Where(i => i.DungeonNodeIndex == 0).Count();
    //  Assert.AreEqual(zeroIndexCount, 9);
    //  Assert.True(!level.GetTiles<Loot>().Any());
    //  Assert.True(!level.Loot.Any());

    //  level.AppendMaze(node, new System.Drawing.Point(0,0));
    //  Assert.True(level.Loot.Any());

    //  var lootOnLevel = level.GetTiles<Loot>();
    //  Assert.True(!lootOnLevel.Any());//TODO
    //  //Assert.AreEqual(lootOnLevel[0].tag, "super loot");//TODO


    //  Assert.AreEqual(level.GetTile(new System.Drawing.Point(1, 2)), loot);

    //}

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
      }
      );
    }

    [Test]
    public void SurfaceTest()
    {
      var game = CreateGame();
      Assert.NotNull(game.Hero);
      //var emptyOne = game.Level.GetEmptyNeighborhoodTiles(game.Hero).FirstOrDefault();
      //Assert.NotNull(emptyOne);
      Assert.True(!game.Level.Surfaces.Any());
      //game.Level.SetTile(, emptyOne.Point);
      var sur = new Surface() { Kind = SurfaceKind.ShallowWater };
      var placement = SetCloseToHero(sur);
      Assert.True(game.Level.Surfaces.Any());
      game.GameManager.HandleHeroShift(placement.Item2);
      Assert.AreEqual(game.Hero.Point, sur.Point);
      Assert.AreEqual(game.Level.GetSurfaceKindUnderHero(game.Hero), sur.Kind);
    }
  }
}
