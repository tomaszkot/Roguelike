using Dungeons.Core;
using NUnit.Framework;
using Roguelike.Attributes;
using Roguelike.Spells;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using System;
using System.Diagnostics;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class SpellsTests : TestBase
  {
    public const int BaseFactor = 30;

    [TestCase(true)]
    [TestCase(false)]
    public void HitBarrelTest(bool scroll)
    {
      var game = CreateGame();
      var hero = game.Hero;

      Assert.True(game.GameManager.HeroTurn);
      var barrel = game.Level.GetTiles<Barrel>().First();
      Assert.AreEqual(game.Level.GetTile(barrel.point), barrel);

      Assert.AreEqual(game.GameManager.Context.TurnOwner, Roguelike.TurnOwner.Hero);
      Assert.True(UseFireBallSpellSource(hero, barrel, scroll));
      Assert.AreNotEqual(game.Level.GetTile(barrel.point), barrel);
      Assert.AreEqual(game.GameManager.Context.TurnOwner, Roguelike.TurnOwner.Allies);
    }

    [Test]
    public void TestDescriptions()
    {
      var game = CreateGame();
      {
        var scroll = new Scroll(SpellKind.Weaken);
        var spell = scroll.CreateSpell(game.Hero);
        var castedSpell = spell as PassiveSpell;
        var features = castedSpell.CreateSpellStatsDescription(true);
        Assert.NotNull(features);

        Assert.AreEqual(Math.Round(castedSpell.StatKindEffective.Value, 3), 3.1);
        Assert.AreEqual(castedSpell.StatKindPercentage.Value, 31);
      }
      {
        var scroll = new SwiatowitScroll();
        Assert.AreEqual(scroll.Name, "Swiatowit Scroll");
      }
      {
        var spell = new CrackedStoneSpell(game.Hero);
        var currentLevelDesc = spell.CreateSpellStatsDescription(true);
        Assert.AreEqual(currentLevelDesc.Duration, 0);
      }

      //Assert.AreEqual(features[1], "Defense: +" + (BaseFactor + 1) + "%");
    }

    [Test]
    public void TestCrackedStoneDescription()
    {
      var game = CreateGame();
      {
        var spell = new CrackedStoneSpell(game.Hero);
        var currentLevelDesc = spell.CreateSpellStatsDescription(true);
        Assert.AreEqual(currentLevelDesc.Duration, 0);//CrackedStone would have a Health not a Duration
        Assert.AreEqual(currentLevelDesc.Range, 1);//how far can set create it

        var nextLevelDesc = spell.CreateSpellStatsDescription(false);
        Assert.AreEqual(nextLevelDesc.Duration, 0);//CrackedStone would have a Health not a Duration
        Assert.AreEqual(nextLevelDesc.Range, 2);//how far can set create it
      }

      //Assert.AreEqual(features[1], "Defense: +" + (BaseFactor + 1) + "%");
    }

    [Test]
    public void TestPrices()
    {
      var game = CreateGame();
      var scroll = new Scroll(SpellKind.Dziewanna);
      Assert.Less(scroll.Price, 100);
    }
    [Test]
    public void SpellPowerTest()
    {
      var game = CreateGame();
      IncMagic(game, 50);
      var scroll = new Scroll(SpellKind.FireBall);
      var spell = scroll.CreateSpell(game.Hero) as FireBallSpell;
      Assert.AreEqual(spell.CurrentLevel, 1);

      float startDamage = 2.3f;
      Assert.GreaterOrEqual(spell.Damage, startDamage);
      Assert.Less(spell.Damage, startDamage * 2);

      var spellState = game.Hero.Spells.GetState(SpellKind.FireBall);
      int ind = 0;
      float expDamage = 0;
      while (spell.CurrentLevel < spellState.MaxLevel)
      {
        Assert.True(game.Hero.IncreaseSpell(SpellKind.FireBall));
        var nextLevelMagicNeeded = spell.NextLevelMagicNeeded;
        spell = scroll.CreateSpell(game.Hero) as FireBallSpell;
        Assert.AreEqual(spell.CurrentLevel, ind + 2);
        expDamage = startDamage + (ind + 1);
        Assert.GreaterOrEqual(spell.Damage, expDamage);
        Assert.Less(spell.Damage, expDamage*2);
        Debug.WriteLine(ind+ " spell.Damage: " + spell.Damage + " mag lev reg: "+ nextLevelMagicNeeded);
        ind++;
      }
    }

    [Test]
    public void SpellPowerTestEnemy()
    {
      var game = CreateGame(false);

      int maxLevelIndex = 10;
      game.SetMaxLevelIndex(maxLevelIndex - 1);
     // Hero hero = null;
      float lastDamagePlain = 0;
      //float lastDamageChemp = 0;
      for (int dungeonLevel = 0; dungeonLevel < maxLevelIndex; dungeonLevel++)
      {
        var level = game.GenerateLevel(dungeonLevel);
        //game.Level.GEt //
        var enPlain = level.GetTiles<Enemy>().Where(i=>i.PowerKind == EnemyPowerKind.Plain).First();
        var scroll = new Scroll(SpellKind.FireBall);
        var spellPlain = scroll.CreateSpell(enPlain) as FireBallSpell;
        var enChemp = level.GetTiles<Enemy>().Where(i => i.PowerKind == EnemyPowerKind.Champion).First();
        var enBoss = level.GetTiles<Enemy>().Where(i => i.PowerKind == EnemyPowerKind.Plain).Last();
        enBoss.SetNonPlain(true);
        var spellChemp = scroll.CreateSpell(enChemp) as FireBallSpell;
        var spellBoss = scroll.CreateSpell(enBoss) as FireBallSpell;
        //var spellBoss = scroll.CreateSpell(enBoss) as FireBallSpell;
        Assert.GreaterOrEqual(spellPlain.CurrentLevel, dungeonLevel + 1);
        Assert.Less(spellPlain.CurrentLevel, (dungeonLevel + 1) * 2);
        if (dungeonLevel == 0)
        {
          lastDamagePlain = spellPlain.Damage;
          //lastDamageChemp = spellPlain.Damage;
        }
        else
        {
          Assert.Greater(spellPlain.Damage, lastDamagePlain);
          Assert.Less(spellPlain.Damage, lastDamagePlain*2);

          Assert.Greater(spellChemp.Damage, spellPlain.Damage);
          Assert.Greater(spellBoss.Damage, spellChemp.Damage);

          lastDamagePlain = spellPlain.Damage;
        }

      }
    }

    //[Test]
    //public void TargetRequirementTests()
    //{
    //  var game = CreateGame();
    //  var spellKinds = EnumHelper.GetEnumValues<SpellKind>(true);
    //  foreach (var spellKind in spellKinds)
    //  {
    //    var scroll = new Scroll(spellKind);
    //    var spell = scroll.CreateSpell(game.Hero);

    //    //Assert.AreEqual(scroll.TargetRequired, spell is OffensiveSpell);//TODO
    //  }
    //}


    [Test]
    public void DurationTest()
    {
      var game = CreateGame();
      IncMagic(game);
      {
        var scroll = new Scroll(SpellKind.ManaShield);
        var spell = scroll.CreateSpell(game.Hero) as ManaShieldSpell;
        Assert.AreEqual(spell.Duration, 5);
        Assert.True(game.Hero.IncreaseSpell(SpellKind.ManaShield));
        spell = scroll.CreateSpell(game.Hero) as ManaShieldSpell;
        Assert.AreEqual(spell.Duration, 6);
      }
      {
        var sks = new[] { SpellKind.Dziewanna, SpellKind.Swarog, SpellKind.Swiatowit, SpellKind.CrackedStone, SpellKind.SwapPosition };
        //var sks = EnumHelper.GetEnumValues<SpellKind>(true);
        foreach (var sk in sks)
        {
          var scroll = new Scroll(sk);
          var spell = scroll.CreateSpell(game.Hero);
          if (spell is PassiveSpell ps)
          {
            Assert.AreEqual(ps.Duration, 0, "tested " + ps.ToString());
          }
        }
      }

    }

    [Test]
    public void NextLevelMagicNeededTest()
    {
      var game = CreateGame();
      var scroll = new Scroll(SpellKind.ManaShield);
      var spell = scroll.CreateSpell(game.Hero) as ManaShieldSpell;
      Assert.AreEqual(spell.NextLevelMagicNeeded, 12);
      Assert.AreEqual(game.Hero.Stats.Magic, 10);

      Assert.False(game.Hero.IncreaseSpell(SpellKind.ManaShield));
      IncMagic(game);
      Assert.AreEqual(game.Hero.Stats.Magic, 12);
      Assert.True(game.Hero.IncreaseSpell(SpellKind.ManaShield));

      spell = scroll.CreateSpell(game.Hero) as ManaShieldSpell;
      Assert.AreEqual(spell.NextLevelMagicNeeded, 14);
    }

    private static void IncMagic(Roguelike.RoguelikeGame game, int magicStatInc = 2)
    {
      game.Hero.LevelUpPoints = magicStatInc;
      for(int i=0;i< magicStatInc; i++)
        game.Hero.IncreaseStatByLevelUpPoint(Roguelike.Attributes.EntityStatKind.Magic);

    }

    [Test]
    public void TeleportDescTest()
    {
      var game = CreateGame();
      var scroll = new Scroll(SpellKind.Teleport);
      var spell = scroll.CreateSpell<TeleportSpell>(game.Hero);
      Assert.AreEqual(spell.Duration, 0);
      Assert.Greater(spell.Range, 0);
      Assert.Greater(spell.ManaCost, 0);

      CheckPassiveSpell(spell, false, EntityStatKind.TeleportExtraRange, 4);
      CheckPassiveSpell(spell,true, EntityStatKind.TeleportExtraRange, 5);
    }

    [Test]
    public void SwapPosDescTest()
    {
      var game = CreateGame();
      var scroll = new Scroll(SpellKind.SwapPosition);
      var spell = scroll.CreateSpell<SwapPositionSpell>(game.Hero);
      Assert.AreEqual(spell.Duration, 0);
      Assert.Greater(spell.Range, 0);
      Assert.Greater(spell.ManaCost, 0);

      CheckPassiveSpell(spell, false, EntityStatKind.SwapPositionExtraRange, 4);
      CheckPassiveSpell(spell, true, EntityStatKind.SwapPositionExtraRange, 5);
    }

    private static void CheckPassiveSpell(PassiveSpell spell, bool nextLevel, EntityStatKind range, int expRange)
    {
      var descNext = spell.CreateSpellStatsDescription(!nextLevel).GetEntityStats();
      Assert.AreEqual(descNext.Count(), 2);
      Assert.AreEqual(descNext[0].Kind, EntityStatKind.Mana);
      Assert.AreEqual(descNext[1].Kind, range);
      Assert.AreEqual(descNext[1].Value.TotalValue, expRange);
    }

    [TestCase(AttackKind.Melee, "punch")]
    [TestCase(AttackKind.PhysicalProjectile, "arrow_hit_body")]
    [TestCase(AttackKind.SpellElementalProjectile, "fireball_hit")]
    [TestCase(AttackKind.WeaponElementalProjectile, "fireball_hit")]
    public void CrackedStoneTest(AttackKind ak, string expSound)
    {
      var gi = new Roguelike.Generators.GenerationInfo();
      gi.MakeEmpty();
      var game = CreateGame(gi: gi);
      var hero = game.Hero;
      var sndPlayed = "";
      game.GameManager.SoundManager.PlayedSound += (object s, string snd) =>
      {
        sndPlayed = snd;
      };

      var stonePh = hero.Position;
      stonePh.X += 1;
      Assert.True(game.Level.SetEmptyTile(stonePh));
      var enemyPh = stonePh;
      enemyPh.X += 1;
      PassiveSpell spell;
      var scroll = PrepareScroll(hero, SpellKind.CrackedStone);
      spell = game.GameManager.SpellManager.ApplyPassiveSpell<PassiveSpell>(hero, scroll, stonePh) as PassiveSpell;
      Assert.NotNull(spell);
      var crackedStone = game.Level.GetTile(stonePh) as CrackedStone;
      Assert.NotNull(crackedStone);
      var crackedStoneHealth = crackedStone.Durability;
      Assert.Greater(crackedStone.Durability, 0);

      GotoNextHeroTurn();
      if (ak == AttackKind.Melee)
        game.GameManager.InteractHeroWith(crackedStone);
      else if (ak == AttackKind.PhysicalProjectile)
      {
        var fi = ActivateFightItem(FightItemKind.ThrowingKnife, hero, 20);
        Assert.True(UseFightItem(hero, crackedStone, hero.SelectedProjectileFightItem));
      }
      else if (ak == AttackKind.SpellElementalProjectile)
      {
        var sk = SpellKind.FireBall;
        Assert.True(UseFireBallSpellSource(hero, crackedStone, true, sk));
      }
      else if (ak == AttackKind.WeaponElementalProjectile)
      {
        //var sk = SpellKind.FireBall;
        HeroUseWeaponElementalProjectile(crackedStone);
      }

      Assert.Less(crackedStone.Durability, crackedStoneHealth);
      Assert.AreEqual(expSound, sndPlayed);
    }
  }
}
