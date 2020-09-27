using NUnit.Framework;
using Roguelike.Attributes;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class PotionTests : TestBase
  {
    [Test]
    public void TestNames()
    {
      var hp = new Potion();
      hp.SetKind(Roguelike.Tiles.Looting.PotionKind.Health);
      Assert.Greater(hp.PrimaryStatDescription.Length, 5);

      var mana = new Potion();
      mana.SetKind(Roguelike.Tiles.Looting.PotionKind.Mana);
      Assert.Greater(mana.PrimaryStatDescription.Length, 5);

      var poison = new Potion();
      poison.SetKind(Roguelike.Tiles.Looting.PotionKind.Poison);
      Assert.Greater(poison.PrimaryStatDescription.Length, 5);
    }

    [Test]
    public void TestHealthPotionConsume()
    {
      var game = CreateGame();
      var hero = game.Hero;

      Assert.Greater(game.GameManager.EnemiesManager.Enemies.Count, 0);
      var heroHealth = hero.Stats.Health;
      var halfHealth = heroHealth / 2;

      while (hero.Stats.Health > halfHealth)
        hero.OnPhysicalHit(game.GameManager.EnemiesManager.Enemies.First());
      Assert.Greater(heroHealth, hero.Stats.Health);
      heroHealth = hero.Stats.Health;

      var hp = Helper.AddTile<Potion>();
      hp.SetKind(PotionKind.Health);
      AddItemToInv(hp);

      hero.Consume(hp);
      Assert.Greater(hero.Stats.Health, heroHealth);
    }

    [Test]
    public void TestManaPotionConsume()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var heroMana = hero.Stats.Mana;

      hero.Stats.SetNominal(Roguelike.Attributes.EntityStatKind.Magic, 15);//TODO
      var spell = new Spell(hero);
      Assert.Greater(heroMana, hero.Stats.Mana);

      heroMana = hero.Stats.Mana;

      var pot = Helper.AddTile<Potion>();
      pot.SetKind(PotionKind.Mana);
      AddItemToInv(pot);
      hero.Consume(pot);
      Assert.Greater(hero.Stats.Mana, heroMana);
    }

    [Test]
    public void TestStrengthPotionConsume()
    {
      var game = CreateGame();
      var hero = game.Hero;

      TestSpecialPotion(hero, EntityStatKind.Strength, SpecialPotionKind.Strength);
    }

    [Test]
    public void TestMagicPotionConsume()
    {
      var game = CreateGame();
      var hero = game.Hero;

      TestSpecialPotion(hero, EntityStatKind.Magic, SpecialPotionKind.Magic);
    }

    private void TestSpecialPotion(Hero hero, EntityStatKind esk, SpecialPotionKind spk)
    {
      var statValue = hero.Stats[esk].Nominal;
      var pot = Helper.AddTile<SpecialPotion>();
      pot.SpecialPotionKind = spk;
      AddItemToInv(pot);
      hero.Consume(pot);
      Assert.AreEqual(hero.Stats[esk].Nominal, statValue + 1);
    }
  }
}

