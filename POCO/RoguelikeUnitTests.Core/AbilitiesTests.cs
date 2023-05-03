using Dungeons.Core;
using Dungeons.Fight;
using Dungeons.Tiles;
using NUnit.Framework;
using Roguelike.Abilities;
using Roguelike.Attributes;
using Roguelike.Managers.Policies;
using Roguelike.Generators;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using RoguelikeUnitTests.Core.Utils;
using RoguelikeUnitTests.Helpers;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Roguelike.Managers.Policies;

namespace RoguelikeUnitTests
{
    [TestFixture]
  public class AbilitiesTests : TestBase
  {
    //const int MaxAbilityInc = 5;

    
    //[Test]
    //public void SkeletonMasteringTest()
    //{
    //  var game = CreateGame();
    //  var hero = game.Hero;

    //  var scroll = PrepareScroll(hero, SpellKind.Skeleton);
    //  scroll.Count = 10;
    //  var gm = game.GameManager;
    //  Assert.AreEqual(gm.AlliesManager.AllAllies.Count(), 0);
    //  var spell = gm.SpellManager.ApplySpell(hero, scroll) as SkeletonSpell;
    //  Assert.NotNull(spell.Ally);
    //  Assert.Greater(spell.Ally.Stats.Strength, 0);
    //  var ally1 = spell.Ally;
    //  Assert.AreEqual(gm.AlliesManager.AllAllies.Count(), 1);

    //  var spell1 = gm.SpellManager.ApplySpell(hero, scroll) as SkeletonSpell;
    //  Assert.AreEqual(gm.AlliesManager.AllAllies.Count(), 1);//shall fail as so far 1 skeleton allowed

    //  var ab = game.GameManager.Hero.GetPassiveAbility(AbilityKind.SkeletonMastering);
    //  ab.IncreaseLevel(game.Hero);
    //  //game.GameManager.AlliesManager.AllEntities.Clear();

    //  GotoNextHeroTurn();
    //  spell1 = gm.SpellManager.ApplySpell(hero, scroll) as SkeletonSpell;
    //  Assert.AreEqual(gm.AlliesManager.AllAllies.Count(), 2);
    //  Assert.NotNull(spell1.Ally);
    //  Assert.AreNotEqual(spell.Ally, spell1.Ally);
    //  Assert.Greater(spell1.Ally.Stats.Strength, ally1.Stats.Strength);
    //  var ratio = spell1.Ally.Stats.Strength / ally1.Stats.Strength;
    //  Assert.That(ratio, Is.EqualTo(1).Within(.15));
    //}

    static Dictionary<AbilityKind, SpellKind> AbilityKind2SpellKind = new Dictionary<AbilityKind, SpellKind>()
    {
      {AbilityKind.FireBallMastering, SpellKind.FireBall},
      {AbilityKind.IceBallMastering, SpellKind.IceBall},
      {AbilityKind.PoisonBallMastering, SpellKind.PoisonBall},
    };

    [TestCase(AbilityKind.FireBallMastering)]
    [TestCase(AbilityKind.IceBallMastering)]
    [TestCase(AbilityKind.PoisonBallMastering)]
    public void MagicProjectileEnemyHitTest(AbilityKind ak)
    {
      var game = CreateGame();
      var ab = game.GameManager.Hero.GetPassiveAbility(ak);
      var enemy = PlainEnemies.First();
      enemy.ImmuneOnEffects = true;
      var enemyBeginHealth = enemy.Stats.Health;
      UseSpellSource(game.Hero, enemy, true, AbilityKind2SpellKind[ak]);
      Assert.AreEqual(enemy.LastingEffects.Count, 0);
      Assert.Less(enemy.Stats.Health, enemyBeginHealth);
      var diff1 = enemyBeginHealth - enemy.Stats.Health;
      enemyBeginHealth = enemy.Stats.Health;
      
      for(int i=0;i< ab.MaxLevel; i++)
        ab.IncreaseLevel(game.Hero);

      GotoNextHeroTurn();

      UseSpellSource(game.Hero, enemy, true, AbilityKind2SpellKind[ak]);
      Assert.Less(enemy.Stats.Health, enemyBeginHealth);
      var diff2 = enemyBeginHealth - enemy.Stats.Health;
      Assert.Greater(diff2, diff1);
    }

    [Test]
    public void TestBulkAttackForced()
    {
      var game = CreateGame();
      var empOnes = game.GameManager.CurrentNode.GetNeighborTiles(game.GameManager.Hero, false);
      Assert.AreEqual(empOnes.Count, 4);
      var ems = AllEnemies.Where(i => i.DistanceFrom(game.GameManager.Hero) > 1).Take(4).ToList();

      float en1Health = ems[0].Stats.Health;
      float en2Health = ems[1].Stats.Health;
      float en3Health = ems[2].Stats.Health;
      float en4Health = ems[3].Stats.Health;
      for (int i = 0; i < 4; i++)
      {
        game.GameManager.CurrentNode.SetTile(ems[i], empOnes[i].point);
        ems[i].Name += i+1;
      }
      var ab = game.GameManager.Hero.GetPassiveAbility(AbilityKind.BulkAttack);
      ab.PrimaryStat.Value.Factor = 100;
      game.Hero.AlwaysHit[AttackKind.Melee] = true;
      game.Hero.RecalculateStatFactors(false);


      Assert.True(game.GameManager.HeroTurn);
      game.GameManager.InteractHeroWith(ems[0]);
      Assert.False(game.GameManager.HeroTurn);
      Assert.Greater(en1Health, ems[0].Stats.Health);
      Assert.Greater(en2Health, ems[1].Stats.Health);
      Assert.Greater(en3Health, ems[2].Stats.Health);
      Assert.Greater(en4Health, ems[3].Stats.Health);
    }

    [Test]
    [Repeat(1)]
    public void TestBulkAttackReal()
    {
      Debug.WriteLine("TestBulkAttackReal start\n\n");
      var gi = new GenerationInfo();
      gi.MakeEmpty();
      gi.GenerateEnemies = true;

      var game = CreateGame(gi:gi);
      MeleePolicyManager.AllowBulkPolicyFinishHeroTurn = false;

      Assert.True(game.GameManager.CurrentNode.SetTile(game.GameManager.Hero, new System.Drawing.Point(3,3)));

      var empOnes = game.GameManager.CurrentNode.GetEmptyNeighborhoodTiles(game.GameManager.Hero, false);
      Assert.Greater(empOnes.Count, 2);
      var enemies = AllEnemies.Where(i => i.PowerKind == EnemyPowerKind.Champion).ToList();
      var enFirst = enemies[0];
      var enSec = enemies[1];
      var enThird = enemies[2];
      enFirst.Stats.GetStat(EntityStatKind.Health).Value.Nominal *= 10;//make sure wont' die
      float en1Health = enFirst.Stats.Health;
      float en2Health = enSec.Stats.Health;
      float en3Health = enThird.Stats.Health;
      for (int i = 0; i < 3; i++)
      {
        Assert.True(game.GameManager.CurrentNode.SetTile(enemies[i], empOnes[i].point));
      }
      var ab = game.GameManager.Hero.GetPassiveAbility(AbilityKind.BulkAttack);
      MaximizeAbility(ab, game.Hero);

      game.Hero.RecalculateStatFactors(false);
      var sb = game.Hero.GetTotalValue(EntityStatKind.ChanceToBulkAttack);

      for (int i = 0; i < 25; i++)
      {
        //hit only 1st enemy
        game.GameManager.InteractHeroWith(enFirst);
        GotoNextHeroTurn();
      }

      Assert.Greater(en1Health, enFirst.Stats.Health);

      //2nd shall be hit by an ability
      var secHit = en2Health > enSec.Stats.Health;
      var thirdHit = en3Health > enThird.Stats.Health;
      Assert.True(secHit || thirdHit);
      Debug.WriteLine("TestBulkAttackReal end");
    }

    [Test]
    [Repeat(1)]
    public void TestStrikeBack()
    {
      var game = CreateGame();
      game.Hero.d_immortal = true;
      var en = PlainEnemies.First();
      PlaceCloseToHero(en);
      float en1Health = en.Stats.Health;

      var sb1 = game.Hero.GetTotalValue(EntityStatKind.ChanceToStrikeBack);
      Assert.AreEqual(sb1, 0);
      var ab = game.GameManager.Hero.GetPassiveAbility(AbilityKind.StrikeBack);
      
      MaximizeAbility(ab, game.Hero);

      game.Hero.RecalculateStatFactors(false);
      var sb2 = game.Hero.GetTotalValue(EntityStatKind.ChanceToStrikeBack);
      Assert.Greater(sb2, 10);//> 10%
      for (int i = 0; i < 50; i++)
      {
        game.GameManager.ApplyPhysicalAttackPolicy(en, game.Hero, (p) => { }, EntityStatKind.Unset);
        //GotoNextHeroTurn();
      }

      Assert.Greater(en1Health, en.Stats.Health);
    }

    [Test]
    public void TestLootMasteryBarrels()
    {
      TestLootMastery<Barrel>(()=>new Barrel(Container));
    }

    private void TestLootMastery<T>(Container cont) where T : Enemy, ILootSource
    {
    }

    private void TestLootMastery<T>(Func<T> tileFac) where T : Roguelike.Tiles.Interactive.InteractiveTile, ILootSource//, new()
    {
      var game = CreateGame();
      var numEntities = 150;
      var lootSources = game.Level.GetTiles<T>();
      var missing = numEntities - lootSources.Count;
      for (int i = 0; i < missing; i++)
      {
        game.Level.SetTileAtRandomPosition(tileFac());
      }
      lootSources = game.Level.GetTiles<T>();
      Assert.AreEqual(lootSources.Count, numEntities);

      var numOfLootSourcesLootBeforeAbility = GetLootFromSrc(game, lootSources);
      game.Hero.AbilityPoints = 10;
      for (int i = 0; i < 5; i++)
        game.Hero.IncreaseAbility(AbilityKind.LootingMastering);

      var numOfLootSourcesLootAfterAbility = GetLootFromSrc(game, lootSources);

      Assert.Greater(numOfLootSourcesLootAfterAbility.Count, numOfLootSourcesLootBeforeAbility.Count);
    }

    [Test]
    public void TestLootMasteryEnemies()
    {
      TestLootMastery<Enemy>(Container);
    }

    private List<Loot> GetLootFromSrc(Roguelike.RoguelikeGame game, IEnumerable<ILootSource> enemies)
    {
      List<Loot> res = new List<Loot>();
      int numOfLoot = 0;
      foreach (var en in enemies)
      {
        var loot = game.GameManager.LootManager.TryAddForLootSource(en);
        if (loot.Any())
        {
          numOfLoot++;
          foreach (var l in loot)
          {
            game.Level.RemoveLoot(l.point);
            game.Level.SetEmptyTile(l.point);
            res.Add(l);
          }
        }
      }

      return res;
    }

    

    [Test]
    public void TestExplosiveMasteringBurning()
    {
      var game = CreateGame();
      var champion = ChampionEnemies.First();
      PrepareEnemyToBeBeaten(champion);
      var chempBeginHealth = champion.Stats.Health;
      var hero = game.GameManager.Hero;

      var explosiveCocktail = new ProjectileFightItem(FightItemKind.ExplosiveCocktail, hero);
      explosiveCocktail.Count = 10;
      hero.Inventory.Add(explosiveCocktail);
      hero.ActiveFightItem = explosiveCocktail;

      for (int i = 0; i < 10; i++)
      {
        //champion.OnHitBy(explosiveCocktail);
        UseFightItem(hero, champion, hero.ActiveProjectileFightItem);
        if (champion.HasLastingEffect(Roguelike.Effects.EffectType.Firing))
          break;
        GotoNextHeroTurn();
      }

      Assert.True(champion.HasLastingEffect(Roguelike.Effects.EffectType.Firing));
    }

    [TestCase(FightItemKind.ExplosiveCocktail)]
    [TestCase(FightItemKind.Stone)]
    [TestCase(FightItemKind.WeightedNet)]
    [TestCase(FightItemKind.ThrowingKnife)]
    [TestCase(FightItemKind.ThrowingTorch)]
    //[Repeat(5)]
    public void TestBasicFightItem(FightItemKind kind)
    {
      var game = CreateGame(true, 100);

      //take one which is active to make sure will have it's turn
      RevealAllEnemies();
      var enemy = PlainNormalEnemies.First();
      Assert.True(enemy.Revealed && enemy.Alive);
      PrepareEnemyToBeBeaten(enemy);
      

      var enemyBeginHealth = enemy.Stats.Health;
      var hero = game.GameManager.Hero;
      hero.Stats.SetNominal(EntityStatKind.Health, 300);
      hero.AlwaysHit[AttackKind.PhysicalProjectile] = true;//TODO

      var fi = ActivateFightItem(kind, hero, 20);
      
      var damage1 = fi.Damage;
      if(kind != FightItemKind.WeightedNet)
        Assert.Greater(damage1, 0);
      //Assert.AreEqual(enemy.OnHitBy(fi), HitResult.Hit);
      int repeat = 5;
      for (int i = 0; i < repeat; i++)
      {
        Assert.True(UseFightItem(hero, enemy, hero.ActiveProjectileFightItem));
        GotoNextHeroTurn();
      }

      if (kind != FightItemKind.WeightedNet)//TODO test range?
      {
        var enemyAfter1HitHealth = enemy.Stats.Health;
        Assert.Greater(enemyBeginHealth, enemyAfter1HitHealth);
        var firstDamageWithAblity = enemyBeginHealth - enemyAfter1HitHealth;

        IncreaseAbility(hero, fi);

        fi = ActivateFightItem(kind, hero);
        var damage2 = fi.Damage;
        Assert.Greater(damage2, damage1);
        //Assert.Less(damage2/damage1, 1.6f);
        for (int i = 0; i < repeat; i++)
        {
          Assert.True(UseFightItem(hero, enemy, hero.ActiveProjectileFightItem));
          GotoNextHeroTurn();
        }
        var chempAfter2HitHealth = enemy.Stats.Health;
        var secondDamageWithAblity = enemyAfter1HitHealth - chempAfter2HitHealth;
        Assert.Greater(secondDamageWithAblity, firstDamageWithAblity);
      }
      else
      {
        int counter = 0;
        while (true)
        {
          var le = enemy.GetFirstLastingEffect(Roguelike.Effects.EffectType.WebTrap);
          if(le == null)
            break;

          counter++;
          Assert.AreEqual(le.Description, "Web Trap");
          Assert.NotNull(le);
          Assert.AreEqual(enemyBeginHealth, enemy.Stats.Health);
          GotoNextHeroTurn();
        }

        Assert.Greater(counter, 1);
      }
    }

    [TestCase(FightItemKind.ThrowingKnife)]
    public void TestBasicFightItemFactor(FightItemKind kind)
    {
      var game = CreateGame(true, 100);

      //take one which is active to make sure will have it's turn
      var enemy = PlainNormalEnemies.First();
      PrepareEnemyToBeBeaten(enemy);

      var hero = game.GameManager.Hero;

      var fi = ActivateFightItem(kind, hero);

      var damage1 = fi.Damage;

      IncreaseAbility(hero, fi, 1);
              
      var damage2 = fi.Damage;
      Assert.Greater(damage2, damage1);
      Assert.Less(damage2/damage1, 1.6f);
        
    }

    private static void IncreaseAbility(Hero hero, FightItem fi, int? steps = null)//AbilityKind kind)
    {
      var ab = hero.GetActiveAbility(fi.AbilityKind);
      hero.AbilityPoints = steps.HasValue ? steps.Value : ab.MaxLevel;
      hero.Level = 11;
      var max = hero.AbilityPoints;
      for (int i = 0; i < max; i++)
      {
        var inc = hero.IncreaseAbility(fi.AbilityKind);
        Assert.IsTrue(inc);
      }
    }

    [Test]
    public void TestFightSkillsVsSpellDamage()
    {
      ////Dictionary<float, float> vals = new Dictionary<float, float>();
      ////var ab = Hero.GetAbility(AbilityKind.ExplosiveMastering);
      ////for (int i = 0; i < 10; i++)
      ////{
      ////  vals[i] = ab.GetExplDamage(i);
      ////}

      ////base.GotoLastLevel();
      //var game = CreateGame();
      //var champion = ChampionEnemies.First();
      //var chempBeginHealth = champion.Stats.Health;
      //var explosiveCocktail = new ProjectileFightItem(FightItemKind.ExplosiveCocktail, game.GameManager.Hero);
      //champion.OnHitBy(explosiveCocktail);
      //var chempAfter1stHitHealth = champion.Stats.Health;

      //Assert.Greater(chempBeginHealth, chempAfter1stHitHealth);
      //var firstExplCoctailDamage = chempBeginHealth - chempAfter1stHitHealth;

      ////var scroll = new Scroll(SpellKind.FireBall);
      ////HitEnemyWithSpell(scroll, champion);
      ////var chempAfterSpellHitHealth = champion.Stats.Health;
      ////AssertGreater(chempAfter1stHitHealth, chempAfterSpellHitHealth);
      ////var diffSpell = chempAfter1stHitHealth - chempAfterSpellHitHealth;

      //////shall be bigger...
      ////AssertLess(Math.Abs(firstExplCoctailDamage - diffSpell), 0.3f);
      ////AssertGreater(firstExplCoctailDamage * 2, diffSpell);

      ////// but not that big...
      ////AssertGreater(diffSpell * 2, firstExplCoctailDamage);
      ////for (int i = 0; i < 15; i++)//hack 15 ?
      ////  UpdateSpellToNextLevel<FireBallSpell>(scroll);

      ////HitEnemyWithSpell(scroll, champion);

      ////var enHealth3 = champion.Stats.Health;
      ////AssertGreater(chempAfterSpellHitHealth, enHealth3);
      ////var diffSpell1 = chempAfterSpellHitHealth - enHealth3;
      ////AssertGreater(diffSpell1, firstExplCoctailDamage * 10);

      ////var ab = Hero.GetAbility(AbilityKind.ExplosiveMastering);
      ////Hero.Character.AbilityPoints = ab.MaxLevel;
      ////Hero.Character.Level = 11;
      ////for (int i = 0; i < ab.MaxLevel; i++)
      ////{
      ////  var inc = Hero.IncreaseAbility(AbilityKind.ExplosiveMastering);

      ////  Assert.IsTrue(inc);
      ////}

      ////explosiveCocktail = new ExplosiveCocktail();
      //////explosiveCocktail.SetCaster(Hero);
      ////champion.OnHitBy(explosiveCocktail);
      ////var enHealth4 = champion.Stats.Health;
      ////AssertGreater(enHealth3, enHealth4);
      ////var diffExpl2 = enHealth3 - enHealth4;
      ////AssertGreater(diffExpl2, firstExplCoctailDamage * 5);

      ////AssertGreater(diffSpell1, diffExpl2);
      //////AssertGreater( diffSpell1, diffExpl2 * 2);
      ////AssertLess(diffSpell1, diffExpl2 * 3);
    }
        

    [Test]
    public void BasicManaAndHealthTests()
    {
      var game = CreateGame();
      Assert.IsTrue(game.Hero.Abilities.PassiveItems.Any());
      Assert.IsTrue(game.Hero.Abilities.PassiveItems.All(i => i.Level == 0));//shall have all at 0
      TestRestoreFactorChange(false);
    }
        
    private void TestRestoreFactorChange(bool forMana)
    {
      var Hero = game.Hero;
      Hero.AbilityPoints = 5;
      var abVal = 0.0;
      var ab = Hero.GetPassiveAbility(forMana ? AbilityKind.RestoreMana : AbilityKind.RestoreHealth);
      for (int i = 0; i < ab.MaxLevel + 1; i++)
      {
        var done = Hero.IncreaseAbility(forMana ? AbilityKind.RestoreMana : AbilityKind.RestoreHealth);
        ab = Hero.GetPassiveAbility(forMana ? AbilityKind.RestoreMana : AbilityKind.RestoreHealth);
        Assert.AreEqual(ab.PrimaryStat.Unit, EntityStatUnit.Percentage);
        AssertNextValue(i, ab, abVal, null);
        var factor = GetFactor(ab, true);
        Assert.Less(factor, 10);
        abVal = factor;
      }
      if (forMana)
      {
        var en = PlainEnemies.First();
        var mana = Hero.Stats.Mana;
        var fireBallScroll = new Scroll(Roguelike.Spells.SpellKind.FireBall);
        UseScroll(Hero, fireBallScroll);

        var mana1 = Hero.Stats.Mana;
        Assert.Less(mana1, mana);

        if (game.GameManager.Context.TurnOwner == Roguelike.TurnOwner.Hero)//TODO
          game.GameManager.Context.MoveToNextTurnOwner();

        GotoNextHeroTurn();

        var mana2 = Hero.Stats.Mana;
        Assert.Greater(mana2, mana1);
      }
      else
      {
        var en = PlainEnemies.First();
        var health = Hero.Stats.Health;
        while (Hero.OnMeleeHitBy(en) == 0)
          ;
        var health1 = Hero.Stats.Health;
        Assert.Less(health1, health);

        if (game.GameManager.Context.TurnOwner == Roguelike.TurnOwner.Hero)//TODO
          game.GameManager.Context.MoveToNextTurnOwner();

        GotoNextHeroTurn();

        var health2 = Hero.Stats.Health;
        Assert.Greater(health2, health1);
      }
    }

    float GetFactor(PassiveAbility ab, bool primary)
    {
      return ab.GetFactor(primary);
    }

    void AssertNextValue(int i, PassiveAbility ab, double valuePrimary, double? valueAux)
    {
      var factor = GetFactor(ab, true);
      var factorAux = GetFactor(ab, false);
      if (i < ab.MaxLevel)
      {
        Assert.Greater(factor, valuePrimary);
        if (valueAux != null)
          Assert.Greater(factorAux, valueAux.Value);
      }
      else
      {
        Assert.AreEqual(factor, valuePrimary);
        if (valueAux != null)
          Assert.AreEqual(factorAux, valueAux.Value);
      }
    }

    [Test]
    public void TestChanceToRepeatMeleeAttack()
    {
      var game = CreateGame();
      game.Hero.Stats.SetNominal(EntityStatKind.ChanceToMeleeHit, 100);

      var en = PlainEnemies.First();
      en.Stats.SetNominal(EntityStatKind.Health, 300);
      var enHealthBase = en.Stats.Health;
      en.OnMeleeHitBy(game.Hero);
      var healthDiffBase = enHealthBase - en.Stats.Health;
      enHealthBase = en.Stats.Health;
      Assert.Greater(healthDiffBase, 0);
      Assert.AreEqual(game.Hero.Stats.GetCurrentValue(EntityStatKind.ChanceToRepeatMeleeAttack), 0);
      game.Hero.Stats.SetNominal(EntityStatKind.ChanceToRepeatMeleeAttack, 50);
      bool works = false; 
      for (int i = 0; i < 20; i++)
      {
        game.GameManager.InteractHeroWith(en);
        GotoNextHeroTurn();
        var healthDiff = enHealthBase - en.Stats.Health;
        if (healthDiff > healthDiffBase)
        {
          works = true;
          break;
        }

        enHealthBase = en.Stats.Health;
      }
      Assert.True(works);
    }
    
    [Test]
    public void TestScepterMastering()//ChanceToCauseElementalAilment()
    {
      var game = CreateGame();
      float originalStatValue = 0;
      var destExtraStat = SetWeapon(AbilityKind.SceptersMastering, game.Hero, out originalStatValue);
      var weapon = game.Hero.GetActiveWeapon();
      Assert.AreEqual(weapon.SpellSource.Kind, SpellKind.FireBall);

      Assert.Greater(PlainEnemies.Count, 5);
      Assert.AreEqual(game.Hero.Stats.GetCurrentValue(EntityStatKind.ChanceToCauseElementalAilment), 0);
      game.Hero.Stats.SetNominal(EntityStatKind.ChanceToCauseElementalAilment, 100);
      for (int i=0;i<5;i++)
      {
        var en = PlainEnemies[i];
        var spell = weapon.SpellSource.CreateSpell(game.Hero);
        PlaceCloseToHero(en);
        Assert.AreEqual(game.GameManager.SpellManager.ApplyAttackPolicy(game.Hero, en, weapon.SpellSource), ApplyAttackPolicyResult.OK);
        Assert.True(en.HasLastingEffect(Roguelike.Effects.EffectType.Firing));
        GotoNextHeroTurn();
      }
    }

    [Test]
    [Repeat(1)]
    public void TestWandMastering()//ChanceToElementalProjectileBulkAttack
    {
      var gi = new Roguelike.Generators.GenerationInfo();
      gi.MakeEmpty();
      gi.GenerateInterior = false;//no walls inside
      gi.GenerateEnemies = true;
      var game = CreateGame(genNumOfEnemies:2, gi : gi);
      float originalStatValue = 0;
      var destExtraStat = SetWeapon(AbilityKind.WandsMastering, game.Hero, out originalStatValue);
      var weapon = game.Hero.GetActiveWeapon();
      game.GameManager.Hero.d_immortal = true;

      //Assert.Greater(empOnes.Count, 1);
      var enemies = AllEnemies;
      var en1 = enemies[0];
      var en2 = enemies[1];
      EnsureUniqNames(en1, en2);
      PrepareEnemyToBeBeaten(en1);
      PrepareEnemyToBeBeaten(en2);
      Log("TestWandMastering en1:" + en1 + " en2: " + en2);
      float en1Health = en1.Stats.Health;
      float en2Health = en2.Stats.Health;

      var chanceForBulk = game.Hero.Stats.GetCurrentValue(EntityStatKind.ChanceToElementalProjectileBulkAttack);
      Assert.AreEqual(chanceForBulk, 0);

      var ab = game.GameManager.Hero.GetPassiveAbility(AbilityKind.WandsMastering);
      for (int i = 0; i < 10; i++)
        ab.IncreaseLevel(game.Hero);

      game.Hero.RecalculateStatFactors(false);
      chanceForBulk = game.Hero.Stats.GetCurrentValue(EntityStatKind.ChanceToElementalProjectileBulkAttack);
      Assert.Greater(chanceForBulk, 40);

      for (int i = 0; i < 20; i++)
      {
        weapon.SpellSource.Count = 20;
        //hit only 1st enemy
        var spell = weapon.SpellSource.CreateSpell(game.Hero);
        Assert.AreEqual(game.GameManager.SpellManager.ApplyAttackPolicy(game.Hero, en1, weapon.SpellSource), ApplyAttackPolicyResult.OK);
        GotoNextHeroTurn();
        if (en2Health > en2.Stats.Health)
        {
          Log("ChanceToElementalProjectileBulkAttack worked at: " + i);
          break;
        }
      }

      Assert.Greater(en1Health, en1.Stats.Health);

      //2nd shall be hit by an ability - bulk attack (ChanceToElementalProjectileBulkAttack)
      Assert.Greater(en2Health, en2.Stats.Health);

    }

    

    [TestCase(EntityStatKind.ChanceToRepeatElementalProjectileAttack, AbilityKind.StaffsMastering)]
    [TestCase(EntityStatKind.StaffExtraElementalProjectileDamage, AbilityKind.StaffsMastering)]
    [TestCase(EntityStatKind.ScepterExtraElementalProjectileDamage, AbilityKind.SceptersMastering)]
    [TestCase(EntityStatKind.WandExtraElementalProjectileDamage, AbilityKind.WandsMastering)]
    public void TestMagicProjectileMasteringStats(EntityStatKind esk, AbilityKind ak)
    {
      var game = CreateGame();
      float originalStatValue = 0;
      SetWeapon(ak, game.Hero, out originalStatValue);
      var weapon = game.Hero.GetActiveWeapon();

      var en = PlainEnemies.First();
      float enHealthBase = PrepareEnemyToBeBeaten(en);
      en.ImmuneOnEffects = true;

      //hit enemy before enhancning ability
      var damageComparer1 = new OuaDDamageComparer(en, this);
      AttackEnemy(game, weapon.SpellSource, en);
      damageComparer1.RegisterHealth();
      Assert.Greater(damageComparer1.HealthDifference, 0);

      var stat1Value = game.Hero.Stats.GetStat(EntityStatKind.ChanceToRepeatElementalProjectileAttack).Value;
      Assert.AreEqual(stat1Value.Factor, 0);
      Assert.AreEqual(game.Hero.Stats.GetStat(EntityStatKind.StaffExtraElementalProjectileDamage).Value.Factor, 0);
      var ab = game.Hero.GetPassiveAbility(ak);

      //do not call  MaximizeAbility as it's not possible to distinguish what cause greater damage ChanceToRepeat or StaffExtraDamage
      //MaximizeAbility(ab, game.Hero);
      var factor = 50;//%
      game.Hero.Stats.SetFactor(esk, factor);
      Assert.Greater(game.Hero.Stats.GetStat(esk).Value.Factor, 0);
      bool works = false;

      for (int i = 0; i < 10; i++)
      {
        var damageComparer2 = new OuaDDamageComparer(en, this);
        AttackEnemy(game, weapon.SpellSource, en);
        damageComparer2.RegisterHealth();

        if (damageComparer2.HealthDifference > damageComparer1.HealthDifference)
        {
          if (esk == EntityStatKind.StaffExtraElementalProjectileDamage)
          {
            AssertHealthDiffPercentageInRange(damageComparer1, damageComparer2, 110, 210);//+factor%
          }
          works = true;
          break;
        }

      }
      Assert.True(works);
      ////SumPercentageFactorAndValue of EnityStat depends on Factor, so check it
      //var scroll = PrepareScroll(game.Hero, SpellKind.Skeleton);
      //scroll.Count = 10;
      //var gm = game.GameManager;
      //var spellSk = gm.SpellManager.ApplySpell(game.Hero, scroll) as SkeletonSpell;
      //Assert.NotNull(spellSk.Ally);

      //Assert.AreEqual(spellSk.Ally.Stats.GetStat(EntityStatKind.StaffExtraElementalProjectileDamage).Value.Factor, 0);
      //Assert.AreEqual(spellSk.Ally.Stats.GetStat(EntityStatKind.ChanceToRepeatElementalProjectileAttack).Value.Factor, 0);
      //MaximizeAbility(ab, spellSk.Ally);
      //Assert.Greater(spellSk.Ally.Stats.GetStat(EntityStatKind.StaffExtraElementalProjectileDamage).Value.Factor, 0);
      //Assert.Greater(spellSk.Ally.Stats.GetStat(EntityStatKind.ChanceToRepeatElementalProjectileAttack).Value.Factor, 0);

    }

    private Roguelike.Abstract.Spells.ISpell AttackEnemy(Roguelike.RoguelikeGame game, SpellSource ss, Enemy en)
    {
      var spell = ss.CreateSpell(game.Hero);

      Assert.AreEqual(game.GameManager.SpellManager.ApplyAttackPolicy(game.Hero, en, ss), ApplyAttackPolicyResult.OK);
      GotoNextHeroTurn();
      return spell;
    }

    private float PrepareEnemyToBeBeaten(Enemy en)
    {
      PlaceCloseToHero(en);
      en.Stats.SetNominal(EntityStatKind.Health, 300);
      var enHealthBase = en.Stats.Health;
      return enHealthBase;
    }

    [TestCase(Roguelike.Abilities.AbilityKind.AxesMastering)]
    [TestCase(Roguelike.Abilities.AbilityKind.BashingMastering)]
    [TestCase(Roguelike.Abilities.AbilityKind.DaggersMastering)]
    [TestCase(Roguelike.Abilities.AbilityKind.SwordsMastering)]
    [TestCase(Roguelike.Abilities.AbilityKind.StaffsMastering)]
    [TestCase(Roguelike.Abilities.AbilityKind.WandsMastering)]
    [TestCase(Roguelike.Abilities.AbilityKind.SceptersMastering)]
    [TestCase(Roguelike.Abilities.AbilityKind.BowsMastering)]
    [TestCase(Roguelike.Abilities.AbilityKind.CrossBowsMastering)]
    public void BasicWeaponsMasteryTests(Roguelike.Abilities.AbilityKind ab)//test if melee damage is increased
    {
      var game = CreateGame();
      var val = TestWeaponKindMastering(ab);
    }
        
    private float TestWeaponKindMastering(AbilityKind kind)
    {

      var abVal = 0.0f;
      var abValAux = 0.0f;
      var hero = game.Hero;
      hero.AlwaysHit[AttackKind.PhysicalProjectile] = true;//TODO
      hero.UseAttackVariation = false;
      float auxStatValue;
      var destStat = SetWeapon(kind, hero, out auxStatValue);
      var en = PlainEnemies.First();
      en.Stats.SetNominal(EntityStatKind.Health, 100);
      en.AddImmunity(Roguelike.Effects.EffectType.Bleeding);//not to mix test results
      PlaceCloseToHero(en);

      var wpn = hero.GetActiveWeapon();
      wpn.StableDamage = true;
      Assert.Greater(wpn.LevelIndex, 0);

      Func<float> hitEnemy = () =>
      {
        var health = en.Stats.Health;
        if(!wpn.IsBowLike)
          en.OnMeleeHitBy(hero);
        else
          UseFightItem(hero, en, hero.ActiveFightItem as ProjectileFightItem);
        var health1 = en.Stats.Health;
        return health - health1;
      };
            
      if (wpn.IsBowLike)
      {
        ProjectileFightItem pfi = null;
        if (wpn.Kind == Weapon.WeaponKind.Bow)
        {
          pfi = new ProjectileFightItem(FightItemKind.PlainArrow) { Count = 2};
        }
        else if (wpn.Kind == Weapon.WeaponKind.Crossbow)
        {
          pfi = new ProjectileFightItem(FightItemKind.PlainBolt) { Count = 2 };
        }
        pfi.Caller = hero;
        hero.Inventory.Add(pfi);
        hero.ActiveFightItem = pfi;
      }
      var damage = hitEnemy();

      Assert.Greater(damage, 0);

      var heroAttack = hero.GetAttackValue(AttackKind.Unset).CurrentTotal;

      var ab = hero.GetPassiveAbility(kind);
      int range = -1;
      if (kind == AbilityKind.BowsMastering ||
        kind == AbilityKind.CrossBowsMastering)
      {
        range = ab.GetExtraRange();
      }
      for (int i = 0; i < ab.MaxLevel + 1; i++)
      {
        hero.IncreaseAbility(kind);
        ab = hero.GetPassiveAbility(kind);
        Assert.AreNotEqual(ab.PrimaryStat.Kind, EntityStatKind.Unset);
        AssertNextValue(i, ab, abVal, abValAux);

        abVal = GetFactor(ab, true);
        abValAux = GetFactor(ab, false);
        Assert.Less(abVal, 21*5);
        Assert.Less(abValAux, 26 *5);

        abVal = ab.PrimaryStat.Factor;
        //Debug.WriteLine(kind + " Level: " + ab.Level + ", value :" + ab.PrimaryStat.Factor);
      }
      var statValueWithAbility = hero.Stats.GetCurrentValue(destStat);
      Assert.Greater(statValueWithAbility, auxStatValue);

      var heroAttackWithAbility = hero.GetAttackValue(AttackKind.Unset).CurrentTotal;
      Assert.Greater(heroAttackWithAbility, heroAttack);
      GotoNextHeroTurn();
      var damageWithAbility = hitEnemy();

      Assert.Greater(damageWithAbility, damage);

      var abAfter = hero.GetPassiveAbility(kind);
      if (kind == AbilityKind.BowsMastering ||
        kind == AbilityKind.CrossBowsMastering)
      {
        var range1 = abAfter.GetExtraRange();
        Assert.Greater(range1, range);
      }

      return abVal;
    }

    private EntityStatKind SetWeapon(Roguelike.Abilities.AbilityKind kind, Hero Hero, out float originalStatValue)
    {
      var auxStat = EntityStatKind.Unset;
      Weapon wpn = null;
      string wpnName = "";
      switch (kind)
      {
        case Roguelike.Abilities.AbilityKind.AxesMastering:
          wpnName = "sickle";
          auxStat = EntityStatKind.ChanceToCauseTearApart;
          break;
        case Roguelike.Abilities.AbilityKind.BashingMastering:
          wpnName = "hammer";
          auxStat = EntityStatKind.ChanceToCauseStunning;
          break;
        case Roguelike.Abilities.AbilityKind.DaggersMastering:
          wpnName = "war_dagger";
          auxStat = EntityStatKind.ChanceToCauseBleeding;
          break;
        case Roguelike.Abilities.AbilityKind.SwordsMastering:
          wpnName = "rusty_sword";
          auxStat = EntityStatKind.ChanceToMeleeHit;
          break;

        case Roguelike.Abilities.AbilityKind.SceptersMastering:
          wpnName = "scepter";
          auxStat = EntityStatKind.ChanceToCauseElementalAilment;
          break;
        case Roguelike.Abilities.AbilityKind.StaffsMastering:
          wpnName = "staff";
          auxStat = EntityStatKind.ChanceToRepeatElementalProjectileAttack;
          break;
        case Roguelike.Abilities.AbilityKind.WandsMastering:
          wpnName = "wand";
          auxStat = EntityStatKind.ChanceToElementalProjectileBulkAttack;
          break;
        case Roguelike.Abilities.AbilityKind.CrossBowsMastering:
          wpnName = "crossbow";
          auxStat = EntityStatKind.ChanceToCauseBleeding;
          break;
        case Roguelike.Abilities.AbilityKind.BowsMastering:
          wpnName = "bow";
          auxStat = EntityStatKind.ChanceToPhysicalProjectileHit;
          break;
        case Roguelike.Abilities.AbilityKind.ArrowVolley:
          wpnName = "bow";
          break;
        case Roguelike.Abilities.AbilityKind.PerfectHit:
          wpnName = "bow";
          break;
        case Roguelike.Abilities.AbilityKind.PiercingArrow:
          wpnName = "bow";
          break;
        default:
          break;
      }

      originalStatValue = Hero.Stats.GetCurrentValue(auxStat);
      wpn = game.GameManager.LootGenerator.GetLootByAsset(wpnName) as Weapon;
      Assert.NotNull(wpn);
      SetHeroEquipment(wpn, CurrentEquipmentKind.Weapon);
      return auxStat;
    }

    [Test]
    [Repeat(1)]
    public void TestTorchMastering()//ChanceToCauseFiring
    {
      var game = CreateGame(genNumOfEnemies: 100);
      var hero = game.GameManager.Hero;
      hero.d_immortal = true;


      var fi = ActivateFightItem(FightItemKind.ThrowingTorch, hero);
      fi.Count = 100;
      var damage1 = fi.Damage;
      Assert.Greater(damage1, 0);


      var enemy = PlainNormalEnemies.First();
      PrepareEnemyToBeBeaten(enemy);
      int firingCounterBefore = CountLE(hero, enemy);
      Assert.Greater(firingCounterBefore, 0);

      MaximizeAbility(hero.GetActiveAbility(AbilityKind.ThrowingTorch), hero);
      int firingCounterAfter = CountLE(hero, enemy);
      Assert.Greater(firingCounterAfter, firingCounterBefore);
    }

    private int CountLE(Hero hero, Enemy enemy)
    {
      int firingCounterBefore = 0;
      for (int i = 0; i < 20; i++)
      {
        Assert.True(UseFightItem(hero, enemy, hero.ActiveProjectileFightItem));
        if (enemy.HasLastingEffect(Roguelike.Effects.EffectType.Firing))
        {
          firingCounterBefore++;
          enemy.RemoveLastingEffect(enemy.GetFirstLastingEffect(Roguelike.Effects.EffectType.Firing));
        }
        GotoNextHeroTurn();
      }

      return firingCounterBefore;
    }
  }
}
