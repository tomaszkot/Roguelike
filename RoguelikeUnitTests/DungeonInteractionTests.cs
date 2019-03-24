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
    public void LootCollect()
    {
      var game = CreateGame();
      var loot = new Loot();

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
      game.SetMaxLevelindex(1);

      var level0 = game.Level;
      var down = game.Level.GetTiles<Stairs>().Where(i=> i.Kind == StairsKind.LevelDown).Single();

      //hero shall be on the level
      Assert.NotNull(game.Level.GetTiles<Hero>().SingleOrDefault());

      var result = game.GameManager.InteractHeroWith(down);
      Assert.AreEqual(result, InteractionResult.ContextSwitched);
      Assert.AreNotEqual(result, game.Level);
      Assert.NotNull(game.Level.GetTiles<Hero>().SingleOrDefault());
      Assert.Null(level0.GetTiles<Hero>().SingleOrDefault());//old level shall not have hero

      down = game.Level.GetTiles<Stairs>().Where(i => i.Kind == StairsKind.LevelDown).SingleOrDefault();
      Assert.Null(down);//max level 1
      var up = game.Level.GetTiles<Stairs>().Where(i => i.Kind == StairsKind.LevelUp).Single();
      result = game.GameManager.InteractHeroWith(up);
      Assert.AreEqual(result, InteractionResult.ContextSwitched);

      Assert.NotNull(game.Level.GetTiles<Hero>().SingleOrDefault());
      Assert.AreEqual(game.Level, level0);
    }

  }
}
