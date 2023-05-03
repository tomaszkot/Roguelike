using Dungeons.TileContainers;
using NUnit.Framework;
using OuaDII.Managers;
using OuaDII.TileContainers;
using OuaDII.Tiles.Interactive;
using Roguelike.Spells;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.Looting;
using System.Linq;

namespace OuaDIIUnitTests
{
  [TestFixture]
  class ScrollTests : TestBase
  {
    [Test]
    public void TeleportTest()
    {
      CreateWorld();

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      GameManager.LootGenerator.GetLootByAsset("teleport_scroll");
    }

    [Test]
    public void PortalWithAlly()
    {
      CreateWorld();

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      var am = GameManager.AlliesManager;

      //portal
      OuaDII.Tiles.Interactive.Portal portal = AddPortal(hero);

      GotoNextHeroTurn();

      //ally
      var scroll = new Scroll(SpellKind.Skeleton);
      hero.Inventory.Add(scroll);
      Assert.NotNull(GameManager.SpellManager.ApplySpell(hero, scroll));
      var ally = am.AllAllies.Last() as Roguelike.Tiles.LivingEntities.Ally;
      Assert.Less(ally.DistanceFrom(hero), 5);
      var orgAllypos = ally.Point;
      var orgHeroPos = hero.point;

      //teleport
      GameManager.InteractHeroWith(portal);
      Assert.AreNotEqual(hero.point, orgHeroPos);
      Assert.AreNotEqual(ally.Point, orgAllypos);

    }

    [Test]
    [Repeat(1)]
    public void PortalWorldToWorldTest()
    {
      //for (int k = 0; k < 3; k++)
      {
        CreateWorld();

        var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
        var heroOriginalPoint = hero.point;

        var camp = this.GameManager.World.WorldSpecialTiles.GroundPortals.SingleOrDefault(i => i.GroundPortalKind == GroundPortalKind.Camp);
        Assert.Greater(hero.DistanceFrom(camp), 4);

        OuaDII.Tiles.Interactive.Portal portal = AddPortal(hero);
        GameManager.InteractHeroWith(portal);

        //GameManager.PortalManager.UsePortal(portal, GroundPortalKind.RatPit);
        Assert.Greater(hero.DistanceFrom(heroOriginalPoint), 4);
        //var destPortals = GameManager.CurrentNode.GetNeighborTiles<OuaDII.Tiles.Interactive.Portal>(hero, true);
        var destPortals = GameManager.CurrentNode.GetTiles<OuaDII.Tiles.Interactive.Portal>().Where(i=>i.PortalKind == PortalDirection.Dest).ToList();
        Assert.AreEqual(destPortals.Count, 1);
        Assert.Less(destPortals[0].DistanceFrom(hero), 4);
        Assert.AreEqual(destPortals[0].PortalKind, PortalDirection.Dest);
        Assert.AreEqual(GameManager.CurrentNode.GetTiles<OuaDII.Tiles.Interactive.Portal>().Count, 2);

        //go back
        //GameManager.PortalManager.UsePortal(destPortals[0], GroundPortalKind.Unset);
        GameManager.InteractHeroWith(destPortals[0]);
        Assert.AreEqual(GameManager.CurrentNode, GameManager.World);
        destPortals = GameManager.CurrentNode.GetNeighborTiles<OuaDII.Tiles.Interactive.Portal>(hero);
        Assert.AreEqual(destPortals.Count, 0);
        Assert.Less(hero.DistanceFrom(heroOriginalPoint), 4);
        Assert.AreEqual(GameManager.CurrentNode.GetTiles<OuaDII.Tiles.Interactive.Portal>().Count, 0);
      }
    }

    private OuaDII.Tiles.Interactive.Portal AddPortal(OuaDII.Tiles.LivingEntities.Hero hero)
    {
      ActivatePortalScroll(hero);

      var portalPH = GameManager.CurrentNode.GetClosestEmpty(hero);
      var portal = GameManager.PortalManager.AppendPortal(PortalDirection.Src, portalPH.point, GroundPortalKind.Unset);
      Assert.NotNull(portal);
      return portal;
    }

    [Test]
    [Repeat(1)]
    public void PortalDungeonToWorldTest()
    {
      var world = CreateWorld();

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      var heroOriginalPoint = hero.point;

      ActivatePortalScroll(hero);

      Assert.AreEqual(GameManager.CurrentNode, GameManager.World);
      AssertPortalsCount(GameManager.CurrentNode, 0);

      //go to dungeon
      DungeonPit pit = GotoNonQuestPit(world);
      Assert.AreNotEqual(GameManager.CurrentNode, GameManager.World);
      AssertPortalsCount(GameManager.CurrentNode, 0);

      var portalPH = GameManager.CurrentNode.GetClosestEmpty(hero);
      var portal = GameManager.PortalManager.AppendPortal(PortalDirection.Src, portalPH.point, GroundPortalKind.Unset);
      AssertPortalsCount(GameManager.CurrentNode, 1);

      //go back to world
      GameManager.PortalManager.UsePortal(portal, GroundPortalKind.BatPit);
      Assert.AreNotEqual(hero.point, heroOriginalPoint);
      Assert.AreEqual(GameManager.CurrentNode, GameManager.World);
      AssertHeroInNode(GameManager.CurrentNode, hero);

      var portals = GameManager.CurrentNode.GetTiles<OuaDII.Tiles.Interactive.Portal>();
      Assert.AreEqual(portals.Count, 1);
      //var portalsClose = portals[0];//GameManager.CurrentNode.GetNeighborTiles<OuaDII.Tiles.Interactive.Portal>(hero, true);
      Assert.Less(portals[0].DistanceFrom(GameManager.Hero), 5);//barrels, chests
      Assert.AreEqual(portals[0].PortalKind, PortalDirection.Dest);
      Assert.AreEqual(GameManager.CurrentNode.GetTiles<OuaDII.Tiles.Interactive.Portal>().Count, 1);

      //go back
      GameManager.PortalManager.UsePortal(portals[0], GroundPortalKind.Unset);
      Assert.AreNotEqual(GameManager.CurrentNode, GameManager.World);

      //no portal in dungeon
      AssertPortalsCount(GameManager.CurrentNode, 0);

      //no portal in world
      AssertPortalsCount(GameManager.World, 0);
    }

    private void ActivatePortalScroll(OuaDII.Tiles.LivingEntities.Hero hero)
    {
      var portalScroll = GameManager.LootGenerator.GetLootByAsset("portal_scroll") as Scroll;
      hero.Inventory.Add(portalScroll);
      hero.ShortcutsBar.ActiveItemDigit = hero.ShortcutsBar.GetItemDigit(portalScroll);
      Assert.AreEqual(GameManager.Hero.ActiveSpellSource, portalScroll);
    }

    private void AssertHeroInNode(DungeonNode node, Roguelike.Tiles.LivingEntities.Hero hero)
    {
      var heros = GameManager.CurrentNode.GetTiles<OuaDII.Tiles.LivingEntities.Hero>();
      if (heros.Count == 1)
        return;
      var atHeroPos = GameManager.CurrentNode.GetTiles().Where(i => i.point == hero.point).FirstOrDefault();
      if(atHeroPos != null)
        return;//TODO that is not that nice
      Assert.True(false);
    }

    private void AssertPortalsCount(DungeonNode node, int count)
    {
      Assert.AreEqual(node.GetTiles<OuaDII.Tiles.Interactive.Portal>().Count, count);
    }
  }
}
