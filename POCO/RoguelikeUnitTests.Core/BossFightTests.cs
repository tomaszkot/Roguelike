using NUnit.Framework;
using Roguelike.Events;
using System.Diagnostics;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class BossFightTests : TestBase
  {
    [Test]
    [Repeat(1)]
    public void NonPlainEnemyUsesEffects()
    {
      var game = CreateGame(genNumOfEnemies: 1, numberOfRooms: 1);
      var hero = game.Hero;

      var spellAttackDone = false;
      game.GameManager.EventsManager.EventAppended += (object sender, Roguelike.Events.GameEvent e) =>
      {
        if (e is LivingEntityAction lea)
        {
          Debug.WriteLine("lea.Info: "+ lea.Info);
          if (lea.Info.Contains("Ball") ||
              lea.Info.Contains("Casting Projectile"))
            spellAttackDone = true;
        }
      };

      hero.Stats.SetNominal(Roguelike.Attributes.EntityStatKind.Health, 255);
      var enemy = AllEnemies.First();
      enemy.SetNonPlain(true);
      Assert.NotNull(enemy.SelectedManaPoweredSpellSource);
      var closeHero = game.Level.GetClosestEmpty(hero);
      game.Level.SetTile(enemy, closeHero.point);
      var enemyMana = enemy.Stats.Mana;

      for (int i = 0; i < 20; i++)
      {
        game.GameManager.EnemiesManager.AttackIfPossible(enemy, hero);//TODO
        if (enemy.Stats.Mana < enemyMana)
          break;
      }
      Assert.True(spellAttackDone);
    }
  }
}
