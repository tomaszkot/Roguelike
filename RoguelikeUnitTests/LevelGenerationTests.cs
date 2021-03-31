using NUnit.Framework;
using Roguelike.Tiles.Interactive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class LevelGenerationTests : TestBase
  {
    [Test]
    public void TestOneRoom()
    {
      var game = CreateGame(false);
      Assert.Null(game.Hero);

      var info = new Roguelike.Generators.GenerationInfo();
      info.NumberOfRooms = 1;
      info.ChildIslandAllowed = false;

      var level = game.LevelGenerator.Generate(0, info);
      Assert.AreEqual(level.Parts.Count, 1);
      level.Nodes[0].GetTiles().All(i => i.DungeonNodeIndex == 0);
      Assert.AreEqual(level.Nodes[0].GetTiles<Door>().Count, 0);
    }

    [TestCase(0)]
    [TestCase(1)]
    public void OneRoomOneSecret(int secretRooIndex)
    {
      var game = CreateGame(false);
      Assert.Null(game.Hero);

      var info = new Roguelike.Generators.GenerationInfo();
      info.NumberOfRooms = 2;
      info.SecretRooIndex = secretRooIndex;
      info.ChildIslandAllowed = false;

      var level = game.LevelGenerator.Generate(0, info);
      Assert.AreEqual(level.Parts.Count, 1);
      level.Nodes[0].GetTiles().All(i => i.DungeonNodeIndex == 0);

      var doors = level.Nodes[0].GetTiles<Door>();
      Assert.AreEqual(doors.Count, 1);
      if(secretRooIndex == 1)
        Assert.AreEqual(doors[0].DungeonNodeIndex, 0);
      else
        Assert.AreEqual(doors[0].DungeonNodeIndex, 1);

      Assert.AreEqual(level.Nodes[secretRooIndex].Secret, true);
      level.Nodes[1].GetTiles().All(i => i.DungeonNodeIndex == 1);
      doors = level.Nodes[1].GetTiles<Door>();
      Assert.AreEqual(doors.Count, 0);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void TestTwoRoomsOneSecret(int secretRooIndex)
    {
      var game = CreateGame(false);
      Assert.Null(game.Hero);

      var info = new Roguelike.Generators.GenerationInfo();
      info.NumberOfRooms = 3;
      info.SecretRooIndex = secretRooIndex;
      info.ChildIslandAllowed = false;

      var level = game.LevelGenerator.Generate(0, info);
      Assert.AreEqual(level.Parts.Count, 1);
      AssertNodeIndex(level, 0);

      for (var i = 0; i < level.Nodes.Count; i++)
      {
        Assert.AreEqual(level.Nodes[i].Secret, secretRooIndex == i);
      }

      var doors = level.Nodes[0].GetTiles<Door>();
      
      if (secretRooIndex == 0)
        Assert.AreEqual(doors.Count, 1);
      else
        Assert.Greater(doors.Count, 4);

      if (secretRooIndex == 0)
        Assert.AreEqual(doors[0].DungeonNodeIndex, 1);
      else
        Assert.AreEqual(doors[0].DungeonNodeIndex, 0);

      AssertNodeIndex(level, 1);
      doors = level.Nodes[1].GetTiles<Door>();
      if(secretRooIndex == 0)
        Assert.Greater(doors.Count, 4);
      else if (secretRooIndex == 1)
        Assert.AreEqual(doors.Count, 0);
      else if (secretRooIndex == 2)
        Assert.AreEqual(doors.Count, 1);

      AssertNodeIndex(level, 2);
      doors = level.Nodes[2].GetTiles<Door>();
      Assert.AreEqual(doors.Count, 0);
    }

    private static bool AssertNodeIndex(Dungeons.TileContainers.DungeonLevel level, int index)
    {
      return level.Nodes[index].GetTiles().All(i => i.DungeonNodeIndex == index);
    }
  }
}
