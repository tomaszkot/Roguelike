using NUnit.Framework;
using Roguelike.Tiles;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class EnemiesTests : TestBase
  {
    [Test]
    public void TestLevel()
    {
      var game = CreateGame();
      Assert.AreEqual(game.Hero.Level, 1);

      //TODO add to node during generation
      var enemy = new Enemy();
      Assert.AreEqual(enemy.Level, 1);
      var chemp = new Enemy();
      chemp.SetNonPlain(false);
      Assert.AreEqual(chemp.Level, 1);

      var boss = new Enemy();
      boss.SetNonPlain(true);
      Assert.AreEqual(boss.Level, 1);
    }

    float GetHitAttackValue(Enemy en)
    {
      return en.GetCurrentValue(Roguelike.Attributes.EntityStatKind.Attack);
    }

    [Test]
    public void TestPower()
    {
      var game = CreateGame();

      var enemy = new Enemy();
      var chemp = new Enemy();
      chemp.SetNonPlain(false);
      var boss = new Enemy();
      boss.SetNonPlain(true);

      var enemyHit = GetHitAttackValue(enemy);
      Assert.Greater(GetHitAttackValue(chemp), enemyHit);
      Assert.Greater(GetHitAttackValue(boss), GetHitAttackValue(chemp));

      enemy.SetLevel(enemy.Level+1);
      Assert.Greater(GetHitAttackValue(enemy), enemyHit);
    }
  }
}
