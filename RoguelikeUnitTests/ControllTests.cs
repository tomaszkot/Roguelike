using Dungeons.Core;
using NUnit.Framework;
using Roguelike;
using Roguelike.Tiles;
using System;
using System.Drawing;
using System.Linq;
using static Dungeons.TileContainers.DungeonNode;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class ControllTests : TestBase
  {
    [Test]
    public void TestEntityManagers()
    {
      var game = CreateGame();
      var enemies = game.GameManager.EnemiesManager.AllEntities;
      Assert.Greater(enemies.Count, 0);
      Assert.AreEqual(enemies.Count, game.Level.GetTiles<Enemy>().Count);
      Assert.False(game.GameManager.AlliesManager.Contains(game.Hero));
    }

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

    [Test]
    public void TestMovesNumber()
    {
      var game = CreateGame(false);
      Assert.Null(game.Hero);
      var gi = new Roguelike.GenerationInfo();
      gi.MakeEmpty();
      
      var level = game.GenerateLevel(0, gi);

      Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Hero);

      var heroPrevPos = game.Hero.Point;

      //var emptyHeroNeib = level.GetEmptyNeighborhoodPoint(game.Hero, Dungeons.TileContainers.DungeonNode.EmptyNeighborhoodCallContext.Move);
      //game.GameManager.HandleHeroShift(emptyHeroNeib.Item2);
      TryToMoveHero();

      Assert.AreNotEqual(heroPrevPos, game.Hero.Point);

      Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Allies);
      heroPrevPos = game.Hero.Point;

      var emptyHeroNeib = level.GetEmptyNeighborhoodPoint(game.Hero, EmptyNeighborhoodCallContext.Move);
      game.GameManager.HandleHeroShift(emptyHeroNeib.Item2);
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

      //var emptyHeroNeib = level.GetEmptyNeighborhoodPoint(game.Hero, Dungeons.TileContainers.DungeonNode.EmptyNeighborhoodCallContext.Move);
      var heroPos = game.Hero.Point;
      //game.GameManager.HandleHeroShift(emptyHeroNeib.Item2);
      TryToMoveHero();

      Assert.AreNotEqual(heroPos, game.Hero.Point);
      Assert.AreEqual(game.GameManager.Context.TurnActionsCount[TurnOwner.Hero], 1);

      heroPos = game.Hero.Point;
      TryToMoveHero();
      Assert.AreEqual(heroPos, game.Hero.Point);//hero shall not move as it already move this turn
      Assert.AreEqual(Game.GameManager.Context.GetActionsCount(), 1);

      Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Hero);

      MoveToHeroTurn(game);

      //try move agin
      TryToMoveHero();
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
        
    [Test]
    public void TestFightHitNumberCustomTurnOwner()
    {
      var game = CreateGame(false);
      Assert.Null(game.Hero);
      
      var gi = new Roguelike.GenerationInfo();
      gi.MakeEmpty();

      var level = game.GenerateLevel(0, gi);
      game.GameManager.Context.AutoTurnManagement = false;
      game.Hero.Stats.SetNominal(Roguelike.Attributes.EntityStatKind.ChanceToHit, 100);

      Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Hero);
      var heroPos = game.Hero.Point;

      var en = SpawnEnemy();
      var enHealth = en.Stats.Health;

      var emptyHeroNeib = SetClose(en);
      var neib = emptyHeroNeib.Item2;

      game.GameManager.HandleHeroShift(neib);
      Assert.AreEqual(heroPos, game.Hero.Point);

      Assert.Less(en.Stats.Health, enHealth);
      Assert.AreEqual(Game.GameManager.Context.GetActionsCount(), 1);

      enHealth = en.Stats.Health;
      game.GameManager.HandleHeroShift(neib);
      Assert.AreEqual(heroPos, game.Hero.Point);

      //hit not done as hero already hit in this turn
      Assert.AreEqual(en.Stats.Health, enHealth);
      Assert.AreEqual(Game.GameManager.Context.GetActionsCount(), 1);

      MoveToHeroTurn(game);
      game.GameManager.HandleHeroShift(neib);
      Assert.AreEqual(heroPos, game.Hero.Point);

      //hit done 
      Assert.Less(en.Stats.Health, enHealth);
      Assert.AreEqual(Game.GameManager.Context.GetActionsCount(), 1);

      heroPos = game.Hero.Point;
      TryToMoveHero();
      
      Assert.AreEqual(heroPos, game.Hero.Point);//hero shall not move as it already made action this turn
    }


    [Test]
    public void TestFight()
    {
      for (int i = 0; i < 2; i++)
      {
        var game = CreateGame(false);
        Assert.Null(game.Hero);
        var gi = new Roguelike.GenerationInfo();
        gi.MakeEmpty();

        var level = game.GenerateLevel(0, gi);
        game.Hero.Stats.SetNominal(Roguelike.Attributes.EntityStatKind.ChanceToHit, 100);
        var heroPos = game.Hero.Point;
        Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Hero);

        var en = SpawnEnemy();
        var enHealth = en.Stats.Health;

        var emptyHeroNeib = SetClose(en);

        //move hero toward enemy - hit it
        var res = game.GameManager.HandleHeroShift(emptyHeroNeib.Item2);
        Assert.AreEqual(heroPos, game.Hero.Point);
        Assert.Less(en.Stats.Health, enHealth);

        Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Allies);
      }
    }
       
    
    private Tuple<Point, Dungeons.TileNeighborhood> SetClose(Enemy en)
    {
      var level = game.Level;
      var emptyHeroNeib = level.GetEmptyNeighborhoodPoint(game.Hero, Dungeons.TileContainers.DungeonNode.EmptyNeighborhoodCallContext.Move);
      Assert.AreNotEqual(GenerationConstraints.InvalidPoint, emptyHeroNeib);
      level.Logger.LogInfo("emptyHeroNeib = " + emptyHeroNeib);
      var set = level.SetTile(en, emptyHeroNeib.Item1);
      Assert.True(set);
      return emptyHeroNeib;
    }
  }
}