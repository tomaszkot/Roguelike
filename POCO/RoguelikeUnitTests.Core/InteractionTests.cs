using Dungeons;
using NUnit.Framework;
using Roguelike.Attributes;
using Roguelike.Managers;
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

    [TestCase(AttackKind.Melee)]
    [TestCase(AttackKind.WeaponElementalProjectile)]
    [TestCase(AttackKind.SpellElementalProjectile)]
    [TestCase(AttackKind.PhysicalProjectile)]
    public void CrackedStoneDestroyTest(AttackKind ak)
    {
      var gi = new Roguelike.Generators.GenerationInfo();
      gi.MakeEmpty();
      gi.NumberOfRooms = 1;
      gi.ForcedNumberOfEnemiesInRoom = 1;
      var game = CreateGame(gi: gi);
      var hero = game.Hero;
      hero.AlwaysHit[ak] = true;//TODO

      var stonePh = hero.Position;
      stonePh.X += 1;
      Assert.True(game.Level.SetEmptyTile(stonePh));

      PassiveSpell spell;
      var scroll = PrepareScroll(hero, SpellKind.CrackedStone);
      spell = game.GameManager.SpellManager.ApplyPassiveSpell<PassiveSpell>(hero, scroll, stonePh) as CrackedStoneSpell;
      Assert.NotNull(spell);
      var stone = game.Level.GetTile(stonePh) as CrackedStone;
      Assert.NotNull(stone);
      Assert.False(stone.Damaged);
      Assert.Greater(stone.Durability, 0);

      int destrAfterHits = 0;
      ProjectileFightItem fi = null;
      int destrAfterHitsExp = 1;
      if (ak == AttackKind.PhysicalProjectile)
      {
        fi = ActivateFightItem(FightItemKind.ThrowingKnife, hero, 10);
        
      }
      else if (ak == AttackKind.SpellElementalProjectile)
      {
        
      }

      
      for (int ind = 0; ind < 10; ind++)
      {
        game.GameManager.RecentlyHit = null;
        GotoNextHeroTurn();
        if (ak == AttackKind.Melee)
          game.GameManager.InteractHeroWith(stone);
        else if (ak == AttackKind.PhysicalProjectile)
          Assert.True(UseFightItem(hero, stone, fi));
        else if (ak == AttackKind.WeaponElementalProjectile)
          HeroUseWeaponElementalProjectile(stone);
        else if (ak == AttackKind.SpellElementalProjectile)
        {
          hero.Stats.SetNominal(EntityStatKind.Mana, 100);
          Assert.True(UseFireBallSpellSource(hero, stone, true, SpellKind.FireBall));
        }
        Assert.True(stone.Damaged);
        Assert.AreEqual(game.GameManager.RecentlyHit, stone);
        var tileAt = game.Level.GetTile(stone.point);
        Assert.True(!stone.Destroyed || tileAt.IsEmpty);
        if (tileAt.IsEmpty)
        {
          //Assert.True(stone.Destroyed);
          break;
        }
        destrAfterHits++;
      }
      Assert.Greater(destrAfterHits, destrAfterHitsExp);
      Assert.Less(destrAfterHits, 7);
    }

  }

}
