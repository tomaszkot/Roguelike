using Dungeons.TileContainers;
using Dungeons.Tiles;
using NUnit.Framework;
using Roguelike.Generators;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
      //var info = new Roguelike.GenerationInfo();
      //info.MakeEmpty();
      //info.NumberOfRooms = 1;

      game.LevelGenerator.CustomNodeCreator = (int nodeIndex, Dungeons.GenerationInfo gi) => {
        var dungeon = game.LevelGenerator.CreateDungeonNodeInstance(); 
        return dungeon;
      };
      var level = game.LevelGenerator.Generate(0);

      var walls = level.GetTiles<Wall>();
      Assert.AreEqual(walls.Count, 0);
      var tiles = level.GetTiles();
      Assert.IsTrue(tiles.All(i=> i.IsEmpty));
      //Assert.True(walls.All(i=> i.IsSide) && false);
    }

    [Test]
    public void NewGameTest()
    {
      //game can have 1-n levels (sub-dungeons)
      var game = CreateGame(false);
      Assert.Null(game.Hero);

      game.SetMaxLevelIndex(1);//there will be level0, level1
      var level0 = game.GenerateLevel(0);

      Assert.NotNull(level0);
      Assert.AreEqual(level0.Index, 0);//1st level has index 0

      Assert.AreEqual(level0, game.Level);
      Assert.NotNull(level0.GetTiles<Hero>().SingleOrDefault());

      //1st level0 has only stairs down
      Assert.AreEqual(level0.GetTiles<Stairs>().Count, 1);
      Assert.AreEqual(level0.GetTiles<Stairs>()[0].StairsKind, StairsKind.LevelDown);

      var level1 = game.GenerateLevel(1);
      Assert.AreNotEqual(level0, level1);

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
      game.SetMaxLevelIndex(maxLevelIndex-1);
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
        Assert.True(stairsDown !=null || levelIndex == maxLevelIndex-1);
        if (stairsDown != null)
        {
          game.GameManager.InteractHeroWith(stairsDown);
        }
      }
    }

    //[Test]
    //public void AllLevelsTest()
    //{
    //}

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

      var level2 = GenRoomWithEnemies();
      Assert.AreEqual(level2.GetTiles<Enemy>().Count(), 2);
            
      level2.Merge(level1.GetTiles().Where(i=>i is Enemy).ToList(), new System.Drawing.Point(0, 0),
        (Dungeons.Tiles.Tile tile) => { return tile is Enemy; });

      Assert.AreEqual(level2.GetTiles<Enemy>().Count(), 4);
    }

    private DungeonLevel GenRoomWithEnemies()
    {
      var generator = Container.GetInstance<Dungeons.IDungeonGenerator>();
      var info = new Roguelike.GenerationInfo();
      info.NumberOfRooms = 1;
      info.GenerateEnemies = false;
      info.MinNodeSize = new System.Drawing.Size(5, 5);
      info.MaxNodeSize = new System.Drawing.Size(5, 5);
      var level = generator.Generate(0, info);
      Assert.Greater(level.GetTiles().Where(i => i.IsEmpty).Count(), 0);

      var en = new Enemy();
      var pt = new Point(2,2);// level.GetFirstEmptyPoint();
      var set = level.SetTile(en, pt);
      Assert.True(set);

      en = new Enemy();
      pt = new Point(2, 3);
      set = level.SetTile(en, pt);
      Assert.True(set);
      return level;

      
    }

    [Test]
    public void TestLootRevealFlagBasic()
    {
      var generator = Container.GetInstance<Dungeons.IDungeonGenerator>();
      var info = new Roguelike.GenerationInfo();
      info.NumberOfRooms = 1;
      info.MinNodeSize = new System.Drawing.Size(15,15);
      info.MaxNodeSize = new System.Drawing.Size(30, 30);
      info.ForceChildIslandInterior = true;
      //info.RevealTiles = true;

      var level = generator.Generate(0, info);
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
      var generator = Container.GetInstance<Dungeons.IDungeonGenerator>();
      var info = new Roguelike.GenerationInfo();
      info.NumberOfRooms = 2;
      info.MinNodeSize = new System.Drawing.Size(15, 15);
      info.MaxNodeSize = new System.Drawing.Size(30, 30);
      info.ForceChildIslandInterior = true;
      //info.RevealTiles = true;

      var level = generator.Generate(0, info);
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
      //var tiles = level.GetTiles().Where(i=> i is Loot).ToList().GroupBy(i=>i.DungeonNodeIndex).ToList();
      //var zeroIndexCount = tiles.Where(i => i.DungeonNodeIndex == 0).Count();

      //var nonZero = level.GetTiles().Where(i => i.DungeonNodeIndex != 0).ToList();
      int k = 0;
      k++;
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


  }
}
