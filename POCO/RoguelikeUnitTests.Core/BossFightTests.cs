using NUnit.Framework;
using Roguelike.Events;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class BossFightTests : TestBase
  {
    [Test]
    public void NonPlainEnemyUsesEffects()
    {
      var game = CreateGame(genNumOfEnemies: 1, numberOfRooms: 1);
      var hero = game.Hero;

      var spellAttackDone = false;
      game.GameManager.EventsManager.EventAppended += (object sender, Roguelike.Events.GameEvent e) =>
      {
        if (e is LivingEntityAction lea)
        {
          if (lea.Info.Contains("Ball"))
            spellAttackDone = true;
        }
      };

      hero.Stats.SetNominal(Roguelike.Attributes.EntityStatKind.Health, 255);
      var enemy = AllEnemies.First();
      enemy.SetNonPlain(true);
      Assert.NotNull(enemy.ActiveManaPoweredSpellSource);
      var closeHero = game.Level.GetClosestEmpty(hero);
      game.Level.SetTile(enemy, closeHero.point);
      var enemyMana = enemy.Stats.Mana;

      for (int i = 0; i < 10; i++)
      {
        game.GameManager.EnemiesManager.AttackIfPossible(enemy, hero);//TODO
        if (enemy.Stats.Mana < enemyMana)
          break;
      }
      Assert.True(spellAttackDone);
    }
  }
}
