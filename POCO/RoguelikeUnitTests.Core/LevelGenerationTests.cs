using NUnit.Framework;
using Roguelike;
using Roguelike.Generators;
using Roguelike.Tiles.Interactive;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class LevelGenerationTests : TestBase
  {
    [Test]
    [Repeat(1)]
    public void TestDifficulty()
    {
      var gi = new GenerationInfo();
      gi.ForcedEnemyName = "skeleton";//avoid stroing ones like bear
      GenerationInfo.Difficulty = Difficulty.Easy;
      var gameEasy = CreateGame(true, 60, gi : gi);
      Assert.AreEqual(gameEasy.GameManager.GameState.CoreInfo.Difficulty, Difficulty.Easy);
      
      var plainEasy = PlainNormalEnemies.First();
      var chempEasy = ChampionNormalEnemies.First();
      
      GenerationInfo.Difficulty = Difficulty.Normal;
      var gameNormal = CreateGame(true);
      Assert.AreEqual(gameNormal.GameManager.GameState.CoreInfo.Difficulty, Difficulty.Normal);
      var plainNormal = PlainNormalEnemies.First();
      var chempNormal = ChampionNormalEnemies.First();

      GenerationInfo.Difficulty = Difficulty.Hard;
      var gameHard = CreateGame(true);
      Assert.AreEqual(gameHard.GameManager.GameState.CoreInfo.Difficulty, Difficulty.Hard);
      var plainHard = PlainNormalEnemies.First();
      var chempHard = ChampionNormalEnemies.First();


      Assert.Greater(plainNormal.Stats.Defense, plainEasy.Stats.Defense);
      Assert.Greater(chempNormal.Stats.Defense, chempEasy.Stats.Defense);
      Assert.Greater(chempHard.Stats.Defense, chempNormal.Stats.Defense);
    }
        

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

    //TODO
    //[TestCase(0)]
    //[TestCase(1)]
    //public void OneRoomOneSecret(int secretRoomIndex)
    //{
    //  var game = CreateGame(false);
    //  Assert.Null(game.Hero);

    //  var info = new Roguelike.Generators.GenerationInfo();
    //  info.NumberOfRooms = 2;
    //  info.SecretRoomIndex = secretRoomIndex;
    //  info.ChildIslandAllowed = false;

    //  var level = game.LevelGenerator.Generate(0, info);
    //  Assert.AreEqual(level.Parts.Count, 1);
    //  level.Nodes[0].GetTiles().All(i => i.DungeonNodeIndex == 0);

    //  var doors = level.Nodes[0].GetTiles<Door>();
    //  Assert.AreEqual(doors.Count, 1);
    //  if (secretRoomIndex == 1)
    //    Assert.AreEqual(doors[0].DungeonNodeIndex, 0);
    //  else
    //    Assert.AreEqual(doors[0].DungeonNodeIndex, 1);

    //  Assert.AreEqual(level.Nodes[secretRoomIndex].Secret, true);
    //  level.Nodes[1].GetTiles().All(i => i.DungeonNodeIndex == 1);
    //  doors = level.Nodes[1].GetTiles<Door>();
    //  Assert.AreEqual(doors.Count, 0);
    //}

    //[TestCase(0)]
    //[TestCase(1)]
    //[TestCase(2)]
    //public void TestTwoRoomsOneSecret(int secretRooIndex)
    //{
    //  var game = CreateGame(false);
    //  Assert.Null(game.Hero);

    //  var info = new Roguelike.Generators.GenerationInfo();
    //  info.NumberOfRooms = 3;
    //  info.SecretRoomIndex = secretRooIndex;
    //  info.ChildIslandAllowed = false;

    //  var level = game.LevelGenerator.Generate(0, info);
    //  Assert.AreEqual(level.Parts.Count, 1);
    //  AssertNodeIndex(level, 0);

    //  for (var i = 0; i < level.Nodes.Count; i++)
    //  {
    //    Assert.AreEqual(level.Nodes[i].Secret, secretRooIndex == i);
    //  }

    //  var doors = level.Nodes[0].GetTiles<Door>();

    //  if (secretRooIndex == 0)
    //    Assert.AreEqual(doors.Count, 1);
    //  else
    //    Assert.Greater(doors.Count, 4);

    //  if (secretRooIndex == 0)
    //    Assert.AreEqual(doors[0].DungeonNodeIndex, 1);
    //  else
    //    Assert.AreEqual(doors[0].DungeonNodeIndex, 0);

    //  AssertNodeIndex(level, 1);
    //  doors = level.Nodes[1].GetTiles<Door>();
    //  if (secretRooIndex == 0)
    //    Assert.Greater(doors.Count, 4);
    //  else if (secretRooIndex == 1)
    //    Assert.AreEqual(doors.Count, 0);
    //  else if (secretRooIndex == 2)
    //    Assert.AreEqual(doors.Count, 1);

    //  AssertNodeIndex(level, 2);
    //  doors = level.Nodes[2].GetTiles<Door>();
    //  Assert.AreEqual(doors.Count, 0);
    //}

    private static bool AssertNodeIndex(Dungeons.TileContainers.DungeonLevel level, int index)
    {
      return level.Nodes[index].GetTiles().All(i => i.DungeonNodeIndex == index);
    }
  }
}
