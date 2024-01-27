using Dungeons.Core;
using NUnit.Framework;
using Roguelike.Managers;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using System.Linq;
using static Dungeons.TileContainers.DungeonNode;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class DungeonInteractionTests : TestBase
  {
    [Test]
    public void IsTileEmpty()
    {
      var game = CreateGame();
      var emp = game.Level.GetEmptyTiles(emptyCheckContext : EmptyCheckContext.DropLoot).First();
      var emp1 = game.Level.GetEmptyTiles(emptyCheckContext: EmptyCheckContext.DropLoot).First();
      Assert.AreEqual(emp, emp1);
      Assert.AreEqual(emp.Symbol, Dungeons.Tiles.Constants.SymbolBackground);
      var loot = new Food(FoodKind.Plum);
      game.Level.SetTile(loot, emp.point);
      var ret = game.Level.GetTile(emp.point);
      Assert.AreEqual(ret, loot);
      var emp2 = game.Level.GetEmptyTiles(emptyCheckContext: EmptyCheckContext.DropLoot).First();
      Assert.AreNotEqual(emp, emp2);
    }

    [Test]
    public void GetNeighborTiles()
    {
      var game = CreateGame();
      var neibs = game.GameManager.CurrentNode.GetNeighborTiles(game.Hero, true);
      Assert.AreEqual(neibs.Count, 8);

    }

    [Test]
    public void ConsumeFood()
    {
      var game = CreateGame();
      var loot = new Food(FoodKind.Plum);

      CollectLoot(game, loot);
      Assert.True(game.Hero.Inventory.Contains(loot));
      //game.Hero.ReduceHealth(1);
      Assert.False(game.Hero.Consume(loot));
      Assert.True(game.Hero.Inventory.Contains(loot));//hero not hurt
    }


    [Test]
    public void DestroyBarrel()
    {
      var game = CreateGame();
      var barrels = game.Level.GetTiles<Barrel>();
      Assert.Greater(barrels.Count, 0);
      var pt = barrels.First().point;
      Assert.AreEqual(game.Level.GetTile(pt), barrels.First());

      var res = game.GameManager.InteractHeroWith(barrels.First());
      Assert.AreEqual(res, InteractionResult.Attacked);
      var barrels1 = game.Level.GetTiles<Barrel>();
      Assert.AreEqual(barrels1.Count, barrels.Count - 1);
      Assert.AreNotEqual(game.Level.GetTile(pt), barrels.First());
      // var tile = game.Level.GetTile(pt);
      // Assert.True(tile.IsEmpty);
    }

    private static void CollectLoot(Roguelike.RoguelikeGame game, Loot loot)
    {
      var freeTile = game.Level.GetFirstEmptyPoint().Value;
      Assert.True(game.Level.SetTile(loot, freeTile));

      Assert.True(game.Level.SetTile(game.Hero, freeTile));

      Assert.True(game.GameManager.CollectLootOnHeroPosition());
      Assert.True(game.Hero.Inventory.Contains(loot));
    }

    [Test]
    [Repeat(1)]//tried 100 but passed :/
    public void StairsTest()
    {
      var game = CreateGame(logLevel: LogLevel.Info);
      game.SetMaxLevelIndex(1);
      
      Assert.AreEqual(game.Level.Index, 0);
      var levelZero = game.Level;
      var stairs = game.Level.GetTiles<Stairs>().ToList();
      var down = stairs.Where(i => i.StairsKind == StairsKind.LevelDown).Single();

      //hero shall be on the level
      Assert.NotNull(game.Level.GetTiles<Hero>().SingleOrDefault());

      var result = game.GameManager.InteractHeroWith(down);
      Assert.AreEqual(result, InteractionResult.ContextSwitched);
      Assert.AreNotEqual(levelZero, game.Level);
      Assert.AreEqual(game.Level.Index, 1);
      Assert.NotNull(game.Level.GetTiles<Hero>().SingleOrDefault());
      Assert.Null(levelZero.GetTiles<Hero>().SingleOrDefault());//old level shall not have hero

      down = game.Level.GetTiles<Stairs>().Where(i => i.StairsKind == StairsKind.LevelDown).SingleOrDefault();
      Assert.Null(down);//max level 1
      var up = game.Level.GetTiles<Stairs>().Where(i => i.StairsKind == StairsKind.LevelUp).Single();
      result = game.GameManager.InteractHeroWith(up);
      Assert.AreEqual(result, InteractionResult.ContextSwitched);

      Assert.NotNull(game.Level.GetTiles<Hero>().SingleOrDefault());
      Assert.AreEqual(game.Level, levelZero);
    }

  }
}
