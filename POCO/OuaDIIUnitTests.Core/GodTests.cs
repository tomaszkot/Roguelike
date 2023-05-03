using NUnit.Framework;
using OuaDII.Managers;
using OuaDII.Tiles;
using OuaDII.Tiles.Looting;
using Roguelike.Attributes;
using System.Collections.Generic;

namespace OuaDIIUnitTests
{
  [TestFixture]
  class GodTests : TestBase
  {
    [Test]
    public void GodStatuesProps()
    {
      var lm = new LootManager();
      foreach (var gk in lm.GodKinds)
      {
        CreateStatue(gk.Value);
      }
    }

    [Test]
    public void EquipGodStatues()
    {
      TestGodEquip(OuaDII.Tiles.GodKind.Wales, new List<EntityStatKind>() { EntityStatKind.Defense, EntityStatKind.Health });
      TestGodEquip(OuaDII.Tiles.GodKind.Jarowit, new List<EntityStatKind>() { EntityStatKind.ChanceToBulkAttack, EntityStatKind.ChanceToStrikeBack });
      TestGodEquip(OuaDII.Tiles.GodKind.Perun, new List<EntityStatKind>() { EntityStatKind.LightingAttack, EntityStatKind.ResistLighting });

      //TestGodEquip(OuaDII.Tiles.GodKind.Dziewanna, new List<EntityStatKind>() { EntityStatKind.LifeStealing, EntityStatKind.ManaStealing });
      //TestGodEquip(OuaDII.Tiles.GodKind.Swarog, new List<EntityStatKind>() { EntityStatKind.ChanceToStrikeBack, EntityStatKind.ChanceToCauseStunning });
      //TestGodEquip(OuaDII.Tiles.GodKind.Swiatowit, new List<EntityStatKind>() { EntityStatKind.ChanceToEvadeMagicAttack, EntityStatKind.ChanceToEvadeMeleeAttack,
      // EntityStatKind.Health});
    }

    private void TestGodEquip(OuaDII.Tiles.GodKind godKind, List<EntityStatKind> influencedStats)
    {
      CreateWorld();

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      var god = CreateStatue(godKind);
      hero.Inventory.Add(god);

      Dictionary<EntityStatKind, float> influencedStatsBefore = new Dictionary<EntityStatKind, float>();
      foreach (var esk in influencedStats)
        influencedStatsBefore[esk] = hero.Stats.GetStat(esk).Value.CurrentValue;

      SetHeroEquipment(god);
      foreach (var esk in influencedStats)
        Assert.Greater(hero.Stats.GetStat(esk).Value.CurrentValue, influencedStatsBefore[esk]);
    }

    private static GodStatue CreateStatue(OuaDII.Tiles.GodKind kind)
    {
      var stat = new GodStatue();
      stat.GodKind = kind;
      Assert.AreEqual(stat.Class, Roguelike.Tiles.EquipmentClass.Unique);

      Assert.Greater(stat.ExtendedInfo.Stats.Values().Count, 0);
      Assert.Less(stat.ExtendedInfo.Stats.Values().Count, 6);

      return stat;
    }
  }
}
