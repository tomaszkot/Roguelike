using NUnit.Framework;
using Roguelike;
using Roguelike.Managers;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class DungeonInteractionTests : TestBase
  {
    [Test]
    public void ConsumeFood()
    {
      var game = CreateGame();
      var loot = new Food(FoodKind.Plum);

      CollectLoot(game, loot);
      Assert.True(game.Hero.Inventory.Contains(loot));
      game.Hero.Consume(loot);
      Assert.True(!game.Hero.Inventory.Contains(loot));
    }

    [Test]
    public void LootCollect()
    {
      var game = CreateGame();
      Loot loot = new Loot();
      CollectLoot(game, loot);
      
    }

    [Test]
    public void DestroyBarrel()
    {
      var game = CreateGame();
      var barrels = game.Level.GetTiles<Barrel>();
      Assert.Greater(barrels.Count, 0);
      var pt = barrels.First().Point;
      Assert.AreEqual(game.Level.GetTile(pt), barrels.First());

      var res = game.GameManager.InteractHeroWith(barrels.First());
      Assert.AreEqual(res, InteractionResult.Attacked);
      var barrels1 = game.Level.GetTiles<Barrel>();
      Assert.AreEqual(barrels1.Count, barrels.Count-1);
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
    public void StairsTest()
    {
      Assert.Greater(GenerationInfo.MaxLevelIndex, 0);
      var game = CreateGame();
      game.SetMaxLevelIndex(1);

      Assert.AreEqual(game.Level.Index, 0);
      var levelZero = game.Level;
      var stairs = game.Level.GetTiles<Stairs>().ToList();
      var down = stairs.Where(i=> i.StairsKind == StairsKind.LevelDown).Single();

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
