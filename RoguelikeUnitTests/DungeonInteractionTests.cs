using NUnit.Framework;
using Roguelike.Managers;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
      var game = CreateGame();
      game.SetMaxLevelIndex(1);

      Assert.AreEqual(game.Level.Index, 0);
      var level0 = game.Level;
      var down = game.Level.GetTiles<Stairs>().Where(i=> i.StairsKind == StairsKind.LevelDown).Single();

      //hero shall be on the level
      Assert.NotNull(game.Level.GetTiles<Hero>().SingleOrDefault());

      var result = game.GameManager.InteractHeroWith(down);
      Assert.AreEqual(result, InteractionResult.ContextSwitched);
      Assert.AreNotEqual(level0, game.Level);
      Assert.AreEqual(game.Level.Index, 1);
      Assert.NotNull(game.Level.GetTiles<Hero>().SingleOrDefault());
      Assert.Null(level0.GetTiles<Hero>().SingleOrDefault());//old level shall not have hero

      down = game.Level.GetTiles<Stairs>().Where(i => i.StairsKind == StairsKind.LevelDown).SingleOrDefault();
      Assert.Null(down);//max level 1
      var up = game.Level.GetTiles<Stairs>().Where(i => i.StairsKind == StairsKind.LevelUp).Single();
      result = game.GameManager.InteractHeroWith(up);
      Assert.AreEqual(result, InteractionResult.ContextSwitched);

      Assert.NotNull(game.Level.GetTiles<Hero>().SingleOrDefault());
      Assert.AreEqual(game.Level, level0);
    }

  }
}
