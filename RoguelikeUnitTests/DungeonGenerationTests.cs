using NUnit.Framework;
using Roguelike.Generators;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class DungeonGenerationTests : TestBase
  {
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
      var gi = new Dungeons.GenerationInfo();
      gi.NumberOfRooms = 1;//tmp
      gi.MaxNodeSize = 5;
      gi.MinNodeSize = 5;
      var level0 = game.GenerateLevel(0, gi);
      Assert.NotNull(level0);

      Assert.AreEqual(level0.Nodes.Count, 1);
      Assert.AreEqual(level0.Nodes[0].Width, 5);
      Assert.AreEqual(level0.Nodes[0].Height, 5);
    }

    [Test]
    public void TestLootRevealFlag()
    {
      var generator = Container.GetInstance<Dungeons.IDungeonGenerator>();
      var info = new Dungeons.GenerationInfo();
      //info.NumberOfRooms = 1;
      //info.MinNodeSize = 5;
      //info.MaxNodeSize = 5;
      //info.RevealTiles = true;

      var level = generator.Generate(0, info);
      //Assert.AreEqual(level.Width, info.MaxNodeSize);
      //Assert.AreEqual(level.Height, info.MaxNodeSize);
      var en = level.GetTiles().Where(i => i is Enemy).ToList();
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
