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
      
      var level = game.GenerateLevel(0, gi);

      Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Hero);
      var movement = GetMovementDirections();
      Assert.True(movement.Item1 > 0 || movement.Item2 > 0);
      var heroPrevPos = game.Hero.Point;
      game.GameManager.HandleHeroShift(movement.Item1, movement.Item2);
      Assert.AreNotEqual(heroPrevPos, game.Hero.Point);

      Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Allies);
      movement = GetMovementDirections();
      Assert.True(movement.Item1 > 0 || movement.Item2 > 0);
      heroPrevPos = game.Hero.Point;
      game.GameManager.HandleHeroShift(movement.Item1, movement.Item2);
      Assert.AreEqual(heroPrevPos, game.Hero.Point);//shall not move as already did in turn
      
      Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Allies);
    }

    [Test]
    public void TestMovesNumberCustomTurnOwner()
    {
      var game = CreateGame(false);
      Assert.Null(game.Hero);
      var gi = new Roguelike.GenerationInfo();
      gi.MakeEmpty();

      var level = game.GenerateLevel(0, gi);
      //do not change turn owner automatically!
      game.GameManager.Context.AutoTurnManagement = false;

      Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Hero);
      var movement = GetMovementDirections();
      Assert.True(movement.Item1 > 0 || movement.Item2 > 0);
      var heroPos = game.Hero.Point;
      game.GameManager.HandleHeroShift(movement.Item1, movement.Item2);
      Assert.AreNotEqual(heroPos, game.Hero.Point);
      Assert.AreEqual(game.GameManager.Context.TurnActionsCount[TurnOwner.Hero], 1);

      heroPos = game.Hero.Point;
      TryToMoveHero(game);
      Assert.AreEqual(heroPos, game.Hero.Point);//hero shall not move as it already move this turn
      Assert.AreEqual(Game.GameManager.Context.GetActionsCount(), 1);

      Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Hero);

      MoveToHeroTurn(game);

      //try move agin
      TryToMoveHero(game);
      Assert.AreNotEqual(heroPos, game.Hero.Point);//now shall move!
      Assert.AreEqual(Game.GameManager.Context.GetActionsCount(), 1);
    }

    private void MoveToHeroTurn(RoguelikeGame game)
    {
      game.GameManager.Context.DoMoveToNextTurnOwner();
      Assert.AreEqual(Game.GameManager.Context.TurnCounts[TurnOwner.Hero], 1);
      Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Allies);
      game.GameManager.Context.DoMoveToNextTurnOwner();
      Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Enemies);
      game.GameManager.Context.DoMoveToNextTurnOwner();
      Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Hero);
      Assert.AreEqual(Game.GameManager.Context.GetActionsCount(), 0);
    }

    private void TryToMoveHero(RoguelikeGame game)//, out Tuple<int, int> movement, out Point heroPos)
    {
      Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Hero);
      var movement = GetMovementDirections();
      Assert.True(movement.Item1 > 0 || movement.Item2 > 0);
      game.GameManager.HandleHeroShift(movement.Item1, movement.Item2);
    }

    [Test]
    public void TestFigthHitNumberCustomTurnOwner()
    {
      var game = CreateGame(false);
      Assert.Null(game.Hero);
      var gi = new Roguelike.GenerationInfo();
      gi.MakeEmpty();

      var level = game.GenerateLevel(0, gi);
      game.GameManager.Context.AutoTurnManagement = false;

      Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Hero);
      var movement = GetMovementDirections();
      Assert.True(movement.Item1 > 0 || movement.Item2 > 0);
      var heroPos = game.Hero.Point;

      //
      var en = new Enemy();
      var enHealth = en.GetCurrentValue(Roguelike.Attributes.EntityStatKind.Health);
      var pos = game.GameManager.GetNewPositionFromMove(heroPos, movement.Item1, movement.Item2).Point;
      var set = level.SetTile(en, pos);
      Assert.True(set);

      game.GameManager.HandleHeroShift(movement.Item1, movement.Item2);
      Assert.AreEqual(heroPos, game.Hero.Point);
      Assert.Less(en.GetCurrentValue(Roguelike.Attributes.EntityStatKind.Health), enHealth);
      Assert.AreEqual(Game.GameManager.Context.GetActionsCount(), 1);

      enHealth = en.GetCurrentValue(Roguelike.Attributes.EntityStatKind.Health);
      game.GameManager.HandleHeroShift(movement.Item1, movement.Item2);
      Assert.AreEqual(heroPos, game.Hero.Point);

      //hit not done as hero already hit in this turn
      Assert.AreEqual(en.GetCurrentValue(Roguelike.Attributes.EntityStatKind.Health), enHealth);
      Assert.AreEqual(Game.GameManager.Context.GetActionsCount(), 1);

      MoveToHeroTurn(game);
      game.GameManager.HandleHeroShift(movement.Item1, movement.Item2);
      Assert.AreEqual(heroPos, game.Hero.Point);

      //hit done 
      Assert.Less(en.GetCurrentValue(Roguelike.Attributes.EntityStatKind.Health), enHealth);
      Assert.AreEqual(Game.GameManager.Context.GetActionsCount(), 1);
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