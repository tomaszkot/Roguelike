using Dungeons;
using NUnit.Framework;
using Roguelike.Spells;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.Looting;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class InteractionTests : TestBase
  {
    [Test]
    public void TestTorchSlot()
    {
      var game = CreateGame(true);
      Assert.True(game.GameManager.HeroTurn);
      var interactive = new TorchSlot(Container);
      Assert.False(interactive.IsLooted);
      var loot = new ProjectileFightItem() { FightItemKind = FightItemKind.ThrowingTorch };
      Assert.AreEqual(game.Hero.GetStackedCountForHotBar(loot), 0);

      InteractHeroWith(interactive);
      Assert.True(interactive.IsLooted);
      Assert.AreEqual(game.Hero.GetStackedCountForHotBar(loot), 1);

      InteractHeroWith(interactive);
      Assert.False(interactive.IsLooted);
      Assert.AreEqual(game.Hero.GetStackedCountForHotBar(loot), 0);
    }

    [Test]
    public void TestBarrelsAndPlainChests()
    {
      var game = CreateGame(true);
      var gi = new GenerationInfo();
      Assert.Greater(gi.NumberOfRooms, 3);

      TestInteraction<Barrel>(game, true);
      TestInteraction<Chest>(game, false);
    }

    private void TestInteraction<T>(Roguelike.RoguelikeGame game, bool interShallBeDestroyed) where T : Roguelike.Tiles.Interactive.InteractiveTile
    {
      var inters = game.Level.GetTiles<T>();
      var intersCount = inters.Count;
      Assert.GreaterOrEqual(intersCount, 5);
      foreach (var inter in inters)
      {
        InteractHeroWith(inter);
      }

      inters = game.Level.GetTiles<T>();

      Assert.AreEqual(inters.Count, interShallBeDestroyed ? 0 : intersCount);
    }
    [Test]
    public void TestChestsDestroy()
    {
      var game = CreateGame(true);
      var chest = game.Level.GetTiles<Chest>().Where(i => i.ChestKind == ChestKind.Plain).First();
      Assert.True(chest.Closed);
      Assert.False(chest.Destroyed);
      InteractHeroWith(chest);
      Assert.False(chest.Closed);
      Assert.False(chest.Destroyed);
      Assert.True(game.Level.GetTiles<Chest>().Where(i => i == chest).Any());

      for (int i=0;i<10;i++)
        InteractHeroWith(chest);

      Assert.False(chest.Closed);
      Assert.True(chest.Destroyed);
      Assert.False(game.Level.GetTiles<Chest>().Where(i => i == chest).Any());
    }

    [Test]
    public void CrackedStoneDestroyTest()
    {
      var gi = new Roguelike.Generators.GenerationInfo();
      gi.MakeEmpty();
      gi.NumberOfRooms = 1;
      gi.ForcedNumberOfEnemiesInRoom = 1;
      var game = CreateGame(gi: gi);
      var hero = game.Hero;

      var stonePh = hero.Position;
      stonePh.X += 1;
      Assert.True(game.Level.SetEmptyTile(stonePh));

      PassiveSpell spell;
      var scroll = PrepareScroll(hero, SpellKind.CrackedStone);
      spell = game.GameManager.SpellManager.ApplyPassiveSpell<PassiveSpell>(hero, scroll, stonePh) as PassiveSpell;
      Assert.NotNull(spell);
      var stone = game.Level.GetTile(stonePh);
      Assert.True(stone is CrackedStone);

      int destrAfterHits = 0;
      for (int i = 0; i < 10; i++)
      {
        GotoNextHeroTurn();
        game.GameManager.InteractHeroWith(stone);

        var tileAt = game.Level.GetTile(stonePh);
        if (tileAt.IsEmpty)
          break;
        destrAfterHits++;
      }
      Assert.Greater(destrAfterHits, 1);
      Assert.Less(destrAfterHits, 6);
    }

  }

}
