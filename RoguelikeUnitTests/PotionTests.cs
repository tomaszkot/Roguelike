using NUnit.Framework;
using Roguelike.Attributes;
using Roguelike.Effects;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class PotionTests : TestBase
  {
    [Test]
    public void TestEquality()
    {
      var hp1 = new Potion();
      hp1.SetKind(PotionKind.Health);

      var hp2 = new Potion();
      hp2.SetKind(PotionKind.Health);

#pragma warning disable 
      Assert.True(hp1 == hp1);
#pragma warning restore 
      Assert.True(hp1.Equals(hp1));
      Assert.True(hp1.Equals(hp2));

      var mp1 = new Potion();
      mp1.SetKind(PotionKind.Mana);
      Assert.False(hp1.Equals(mp1));
      Assert.False(hp1 == mp1);

      var game = CreateGame();
      game.Hero.Inventory.Add(hp1);
      Assert.AreEqual(game.Hero.Inventory.ItemsCount, 1);
      game.Hero.Inventory.Add(hp2);
      Assert.AreEqual(game.Hero.Inventory.ItemsCount, 1);

      game.Hero.Inventory.Add(mp1);
      Assert.AreEqual(game.Hero.Inventory.ItemsCount, 2);

      game.GameManager.Save();
      game.Hero.Inventory.Items.Clear();
      Assert.AreEqual(game.Hero.Inventory.ItemsCount, 0);
      Assert.False(game.Hero.Inventory.Contains(hp1));

      game.GameManager.Load(game.Hero.Name);
      Assert.AreEqual(game.Hero.Inventory.ItemsCount, 2);

      Assert.True(game.Hero.Inventory.Contains(hp1));
      Assert.True(game.Hero.Inventory.Contains(hp2));

      var hp3 = new Potion();
      hp3.SetKind(PotionKind.Health);
      Assert.True(game.Hero.Inventory.Contains(hp3));
      game.Hero.Inventory.Add(hp3);

      Assert.AreEqual(game.Hero.Inventory.ItemsCount, 2);
    }

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

      var game = CreateGame();
      for (int i = 0; i < 50; i++)
      {
        var potion = game.GameManager.LootGenerator.GetRandomLoot(LootKind.Potion, 1) as Potion;
        Assert.AreNotEqual(potion.Kind, PotionKind.Unset);
      }
    }

    [Test]
    public void TestHealthPotionConsume()
    {
      var game = CreateGame();
      var hero = game.Hero;

      Assert.Greater(ActiveEnemies.Count, 0);
      var heroHealth = hero.Stats.Health;
      var halfHealth = heroHealth / 2;

      while (hero.Stats.Health > halfHealth)
        hero.OnMelleeHitBy(ActiveEnemies.First());
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
      var scroll = new Scroll(SpellKind.Transform);
      UseScroll(hero, SpellKind.Transform);

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

    [Test]
    public void TestPoisonPotionConsume()
    {
      var game = CreateGame();
      var hero = game.Hero;
      hero.SetChanceToExperienceEffect(EffectType.Poisoned, 100);

      //make enemy poisonus
      var enemy = AllEnemies.First();
      var poisonAttack = enemy.Stats.GetStat(EntityStatKind.PoisonAttack);
      poisonAttack.Value.Nominal = 10;

      game.Hero.OnMelleeHitBy(enemy);
      var le1 = game.Hero.GetFirstLastingEffect(EffectType.Poisoned);
      Assert.NotNull(le1);

      var pot = Helper.AddTile<Potion>();
      pot.SetKind(PotionKind.Poison);
      AddItemToInv(pot);
      hero.Consume(pot);

      le1 = game.Hero.GetFirstLastingEffect(EffectType.Poisoned);
      Assert.Null(le1);


    }
  }
}

