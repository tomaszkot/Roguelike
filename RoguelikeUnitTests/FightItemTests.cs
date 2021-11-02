using NUnit.Framework;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class FightItemTests : TestBase
  {
    [Test]
    public void ArrowFightItemTest()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var fi = AddFightItemToHero(FightItemKind.PlainArrow, hero);

      var enemy = ActiveEnemies.First();
      var enemyHealth = enemy.Stats.Health;
      enemy.Stats.SetNominal(Roguelike.Attributes.EntityStatKind.Defense, 10);
      var mana = hero.Stats.Mana;

      Assert.True(game.GameManager.HeroTurn);
      var bow = GenerateEquipment<Weapon>("Bow");
      Assert.True(hero.SetEquipment(bow));

      var tile = game.GameManager.CurrentNode.GetClosestEmpty(hero);
      game.GameManager.CurrentNode.SetTile(enemy, tile.point);

      Assert.True(UseFightItem(hero, enemy, fi));

      Assert.Greater(enemyHealth, enemy.Stats.Health);
      Assert.AreEqual(mana, hero.Stats.Mana);
      Assert.False(game.GameManager.HeroTurn);
    }

    [Test]
    public void StoneFightItemTest()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var fi = AddFightItemToHero(FightItemKind.Stone, hero);
      var enemy = ActiveEnemies.First();
      var enemyHealth = enemy.Stats.Health;
      var mana = hero.Stats.Mana;

      Assert.True(game.GameManager.HeroTurn);
      UseFightItem(hero, enemy, fi);

      Assert.Greater(enemyHealth, enemy.Stats.Health);
      Assert.AreEqual(mana, hero.Stats.Mana);
      Assert.False(game.GameManager.HeroTurn);
    }

    private bool UseFightItem(Roguelike.Tiles.LivingEntities.Hero hero, Roguelike.Tiles.LivingEntities.Enemy enemy, ProjectileFightItem fi)
    {
      hero.Inventory.Add(fi);
      if (fi.FightItemKind == FightItemKind.Stone)
        return game.GameManager.ApplyAttackPolicy(hero, enemy, fi);
      
      return game.GameManager.TryApplyAttackPolicy(fi, enemy);
    }
  }
}
