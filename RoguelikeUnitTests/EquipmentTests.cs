using NUnit.Framework;
using Roguelike;
using Roguelike.Attributes;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class EquipmentTests : TestBase
  {
    [Test]
    public void EquipmentProps()
    {
      var game = CreateGame();
      var hero = game.Hero;

      Equipment wpn = game.GameManager.GenerateRandomEquipment(EquipmentKind.Weapon);
      Assert.AreEqual(wpn.PrimaryStatKind, EntityStatKind.Attack);
      Assert.Greater(wpn.PrimaryStatValue, 0);
      var stats = wpn.GetStats();
      Assert.AreEqual(stats.Attack, wpn.PrimaryStatValue);
    }

    [Test]
    public void EquipmentPutOnHero()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var heroAttack = hero.Stats.Attack;

      Equipment wpn = game.GameManager.GenerateRandomEquipment(EquipmentKind.Weapon);
      Assert.AreEqual(wpn.PrimaryStatKind, EntityStatKind.Attack);
      Assert.Greater(wpn.PrimaryStatValue, 0);
      hero.SetEquipment(EquipmentKind.Weapon, wpn);

      Assert.Greater(hero.Stats.Attack, heroAttack);
    }
  }
}
