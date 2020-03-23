using NUnit.Framework;
using Roguelike;
using Roguelike.Tiles;
using System;
using System.Drawing;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class ControllTests : TestBase
  {
    [Test]
    public void TestTurnOwner()
    {
      var game = CreateGame();
      Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Hero);
      game.GameManager.SkipHeroTurn();
      Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Allies);
      game.GameManager.MakeGameTick();
      Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Enemies);
      game.GameManager.MakeGameTick();
      Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Hero);
    }

    Tuple<int, int> GetMovementDirections()
    {
      var east = game.Level.GetNeighborTile(game.Hero, Dungeons.TileNeighborhood.East);
      if (east.IsEmpty)
        return new Tuple<int, int>(1, 0);
      var south = game.Level.GetNeighborTile(game.Hero, Dungeons.TileNeighborhood.South);
      if (south.IsEmpty)
        return new Tuple<int, int>(0, 1);

      return new Tuple<int, int>(0, 0);
    }

    [Test]
    public void TestMovesNumber()
    {
      var game = CreateGame(false);
      Assert.Null(game.Hero);
      var gi = new Roguelike.GenerationInfo();
      gi.MakeEmpty();
      
      var level0 = game.GenerateLevel(0, gi);

      Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Hero);
      var movement = GetMovementDirections();
      Assert.True(movement.Item1 > 0 || movement.Item2 > 0);
      var heroPos = game.Hero.Point;
      game.GameManager.HandleHeroShift(movement.Item1, movement.Item2);
      Assert.AreNotEqual(heroPos, game.Hero.Point);

      Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Allies);
      movement = GetMovementDirections();
      Assert.True(movement.Item1 > 0 || movement.Item2 > 0);
      heroPos = game.Hero.Point;
      game.GameManager.HandleHeroShift(movement.Item1, movement.Item2);
      Assert.AreEqual(heroPos, game.Hero.Point);
    }

    [Test]
    public void TestMovesNumberCustomTurnOwner()
    {
      var game = CreateGame(false);
      Assert.Null(game.Hero);
      var gi = new Roguelike.GenerationInfo();
      gi.MakeEmpty();

      var level0 = game.GenerateLevel(0, gi);
      game.GameManager.Context.AutoTurnManagement = false;

      Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Hero);
      var movement = GetMovementDirections();
      Assert.True(movement.Item1 > 0 || movement.Item2 > 0);
      var heroPos = game.Hero.Point;
      game.GameManager.HandleHeroShift(movement.Item1, movement.Item2);
      Assert.AreNotEqual(heroPos, game.Hero.Point);
      Assert.AreEqual(game.GameManager.Context.TurnActionsCount[TurnOwner.Hero], 1);

      Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Hero);
      movement = GetMovementDirections();
      Assert.True(movement.Item1 > 0 || movement.Item2 > 0);
      heroPos = game.Hero.Point;
      game.GameManager.HandleHeroShift(movement.Item1, movement.Item2);
      Assert.AreEqual(heroPos, game.Hero.Point);//hero shall not move as it already move this turn
    }


    [Test]
    public void TestFight()
    {
      var game = CreateGame(false);
      Assert.Null(game.Hero);
      var gi = new Roguelike.GenerationInfo();
      gi.MakeEmpty();

      var level0 = game.GenerateLevel(0, gi);

      var heroPos = game.Hero.Point;
      Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Hero);
      var movement = GetMovementDirections();
      Assert.True(movement.Item1 > 0 || movement.Item2 > 0);
      var en = new Enemy();
      var enHealth = en.GetCurrentValue(Roguelike.Attributes.EntityStatKind.Health);
      var pos = game.GameManager.GetNewPositionFromMove(heroPos, movement.Item1, movement.Item2).Point;
      var set = level0.SetTile(en, pos);
      Assert.True(set);

      game.GameManager.HandleHeroShift(movement.Item1, movement.Item2);
      Assert.AreEqual(heroPos, game.Hero.Point);
      Assert.Less(en.GetCurrentValue(Roguelike.Attributes.EntityStatKind.Health), enHealth);

      Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Allies);

    }
  }
}