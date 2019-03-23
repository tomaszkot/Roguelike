using NUnit.Framework;
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
    public void EquipmentPutOnHero()
    {
      var gameNode = CreateNewDungeon();
      var hero = Hero;

      var heroAttack = hero.Stats.Attack;

      Equipment wpn = GameManager.GenerateRandomEquipment(EquipmentKind.Weapon);
      hero.SetEquipment(EquipmentKind.Weapon, wpn);

      Assert.Greater(hero.Stats.Attack, heroAttack);
    }
  }
}
