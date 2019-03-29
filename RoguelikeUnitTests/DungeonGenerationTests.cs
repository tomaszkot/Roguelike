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
      var game = CreateGame(false);
      Assert.Null(game.Hero);

      game.SetMaxLevelindex(1);
      var level0 = game.GenerateLevel(0);

      Assert.NotNull(level0);
      Assert.AreEqual(level0.Index, 0);//1st level has index 0

      Assert.AreEqual(level0, game.Level);
      Assert.NotNull(level0.GetTiles<Hero>().Single());

      //1st level0 has only stairs down
      Assert.AreEqual(level0.GetTiles<Stairs>().Count, 1);
      Assert.AreEqual(level0.GetTiles<Stairs>()[0].Kind, StairsKind.LevelDown);

      var level1 = game.GenerateLevel(1);
      Assert.AreNotEqual(level0, level1);

      //last level has NOT stairs down, but shall have up ones
      Assert.AreEqual(level1.GetTiles<Stairs>().Count, 1);
      Assert.AreEqual(level1.GetTiles<Stairs>()[0].Kind, StairsKind.LevelUp);
    }

    [Test]
    public void TestLootRevealFlag()
    {
      var generator = Container.GetInstance<Dungeons.IDungeonGenerator>();
      var info = new Dungeons.GenerationInfo();
      info.NumberOfNodes = 1;
      info.MinNodeSize = 4;
      info.MaxNodeSize = 4;
      info.RevealTiles = true;

      var level = generator.Generate(0, info);
      Assert.AreEqual(level.Width, info.MaxNodeSize);
      Assert.AreEqual(level.Height, info.MaxNodeSize);
      var zeroIndexCount = level.GetTiles().Where(i => i.DungeonNodeIndex == 0).Count();
      Assert.AreEqual(zeroIndexCount, info.MinNodeSize* info.MaxNodeSize);
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
