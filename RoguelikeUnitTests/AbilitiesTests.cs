using Dungeons.Fight;
using Dungeons.Tiles;
using NUnit.Framework;
using Roguelike.Abilities;
using Roguelike.Attributes;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using RoguelikeUnitTests.Helpers;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  public class AbilitiesTests : TestBase
  {
    const int MaxAbilityInc = 5;

    public void MaximizeAbility(Ability ab, AdvancedLivingEntity le)
    {
      while (ab.Level < ab.MaxLevel)
      {
        Assert.True(le.IncreaseAbility(ab.Kind));
      }
    }

    [Test]
    public void SkeletonMasteringTest()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var scroll = PrepareScroll(hero, SpellKind.Skeleton);
      scroll.Count = 10;
      var gm = game.GameManager;
      Assert.AreEqual(gm.AlliesManager.AllAllies.Count(), 0);
      var spell = gm.SpellManager.ApplySpell(hero, scroll) as SkeletonSpell;
      Assert.NotNull(spell.Ally);
      Assert.Greater(spell.Ally.Stats.Strength, 0);
      var ally1 = spell.Ally;
      Assert.AreEqual(gm.AlliesManager.AllAllies.Count(), 1);

      var spell1 = gm.SpellManager.ApplySpell(hero, scroll) as SkeletonSpell;
      Assert.AreEqual(gm.AlliesManager.AllAllies.Count(), 1);//shall fail as so far 1 skeleton allowed

      var ab = game.GameManager.Hero.GetPassiveAbility(AbilityKind.SkeletonMastering);
      ab.IncreaseLevel(game.Hero);
      //game.GameManager.AlliesManager.AllEntities.Clear();

      GotoNextHeroTurn();
      spell1 = gm.SpellManager.ApplySpell(hero, scroll) as SkeletonSpell;
      Assert.AreEqual(gm.AlliesManager.AllAllies.Count(), 2);
      Assert.NotNull(spell1.Ally);
      Assert.AreNotEqual(spell.Ally, spell1.Ally);
      Assert.Greater(spell1.Ally.Stats.Strength, ally1.Stats.Strength);
      var ratio = spell1.Ally.Stats.Strength / ally1.Stats.Strength;
      Assert.That(ratio, Is.EqualTo(1).Within(.15));
    }

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
      var enemyBeginHealth = enemy.Stats.Health;
      UseSpellSource(game.Hero, enemy, true, AbilityKind2SpellKind[ak]);
      Assert.Less(enemy.Stats.Health, enemyBeginHealth);
      var diff1 = enemyBeginHealth - enemy.Stats.Health;
      enemyBeginHealth = enemy.Stats.Health;
      
      for(int i=0;i< MaxAbilityInc;i++)
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
      var empOnes = game.GameManager.CurrentNode.GetEmptyNeighborhoodTiles(game.GameManager.Hero, false);
      Assert.Greater(empOnes.Count, 1);
      float en1Health = AllEnemies[0].Stats.Health;
      float en2Health = AllEnemies[1].Stats.Health;
      for (int i = 0; i < 2; i++)
      {
        game.GameManager.CurrentNode.SetTile(AllEnemies[i], empOnes[i].point);
      }
      var ab = game.GameManager.Hero.GetPassiveAbility(Roguelike.Abilities.AbilityKind.BulkAttack);
      ab.PrimaryStat.Value.Factor = 100;
      game.Hero.AlwaysHit[AttackKind.Melee] = true;
      game.Hero.RecalculateStatFactors(false);
      game.GameManager.InteractHeroWith(AllEnemies[0]);

      Assert.Greater(en1Health, AllEnemies[0].Stats.Health);
      Assert.Greater(en2Health, AllEnemies[1].Stats.Health);
    }

    [Test]
    public void TestBulkAttackReal()
    {
      var game = CreateGame();
      var empOnes = game.GameManager.CurrentNode.GetEmptyNeighborhoodTiles(game.GameManager.Hero, false);
      Assert.Greater(empOnes.Count, 1);
      var enemies = AllEnemies.Where(i => i.PowerKind == EnemyPowerKind.Champion).ToList();
      enemies[0].Stats.GetStat(EntityStatKind.Health).Value.Nominal *= 10;//make sure wont' die
      float en1Health = enemies[0].Stats.Health;
      float en2Health = enemies[1].Stats.Health;
      for (int i = 0; i < 2; i++)
      {
        game.GameManager.CurrentNode.SetTile(enemies[i], empOnes[i].point);
      }
      var ab = game.GameManager.Hero.GetPassiveAbility(Roguelike.Abilities.AbilityKind.BulkAttack);
      MaximizeAbility(ab, game.Hero);

      game.Hero.RecalculateStatFactors(false);
      var sb = game.Hero.GetTotalValue(EntityStatKind.ChanceToBulkAttack);

      for (int i = 0; i < 20; i++)
      {
        //hit only 1st enemy
        game.GameManager.InteractHeroWith(enemies[0]);
        GotoNextHeroTurn();
      }

      Assert.Greater(en1Health, enemies[0].Stats.Health);

      //2nd shall be hit by an ability
      Assert.Greater(en2Health, enemies[1].Stats.Health);
    }

    [Test]
    [Repeat(5)]
    public void TestStrikeBack()
    {
      var game = CreateGame();
      var en = PlainEnemies.First();
      PlaceCloseToHero(en);
      float en1Health = en.Stats.Health;

      var ab = game.GameManager.Hero.GetPassiveAbility(AbilityKind.StrikeBack);
      MaximizeAbility(ab, game.Hero);

      game.Hero.RecalculateStatFactors(false);
      var sb = game.Hero.GetTotalValue(EntityStatKind.ChanceToStrikeBack);

      for (int i = 0; i < 50; i++)
      {
        game.GameManager.ApplyPhysicalAttackPolicy(en, game.Hero, (p) => { });
        //GotoNextHeroTurn();
      }

      Assert.Greater(en1Health, en.Stats.Health);
    }

    [Test]
    public void TestLootMasteryBarrels()
    {
      TestLootMastery<Barrel>();
    }

    private void TestLootMastery<T>(Container cont) where T : Enemy, ILootSource
    {
    }

    private void TestLootMastery<T>() where T : Tile, ILootSource, new()
    {
      var game = CreateGame();
      var numEntities = 150;
      var lootSources = game.Level.GetTiles<T>();
      var missing = numEntities - lootSources.Count;
      for (int i = 0; i < missing; i++)
      {
        game.Level.SetTileAtRandomPosition(new T());
      }
      lootSources = game.Level.GetTiles<T>();
      Assert.AreEqual(lootSources.Count, numEntities);

      var numOfLootSourcesLootBeforeAbility = GetLootFromSrc(game, lootSources);
      game.Hero.AbilityPoints = 10;
      for (int i = 0; i < 5; i++)
        game.Hero.IncreaseAbility(Roguelike.Abilities.AbilityKind.LootingMastering);

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
      var chempBeginHealth = champion.Stats.Health;
      var hero = game.GameManager.Hero;

      var explosiveCocktail = new ProjectileFightItem(FightItemKind.ExplosiveCocktail, hero);
      explosiveCocktail.Count = 10;
      hero.Inventory.Add(explosiveCocktail);
      hero.ActiveFightItem = explosiveCocktail;

      for (int i = 0; i < 10; i++)
      {
        champion.OnHitBy(explosiveCocktail);
        if (champion.HasLastingEffect(Roguelike.Effects.EffectType.Firing))
          break;
      }

      Assert.True(champion.HasLastingEffect(Roguelike.Effects.EffectType.Firing));
    }

    [TestCase(FightItemKind.ExplosiveCocktail)]
    [TestCase(FightItemKind.Stone)]
    [TestCase(FightItemKind.ThrowingKnife)]
    [TestCase(FightItemKind.WeightedNet)]
    public void TestBasicFightItem(FightItemKind kind)
    {
      var game = CreateGame();

      //take one which is active to make sure will have it's turn
      RevealAllEnemies(game);
      var enemy = ChampionEnemies.First();
      Assert.True(enemy.Revealed && enemy.Alive);
      enemy.Stats.SetNominal(EntityStatKind.Health, 100);
      var enemyBeginHealth = enemy.Stats.Health;
      var hero = game.GameManager.Hero;

      var fi = ActivateFightItem(kind, hero);
      
      var damage1 = fi.Damage;
      if(kind != FightItemKind.WeightedNet)
        Assert.Greater(damage1, 0);
      Assert.AreEqual(enemy.OnHitBy(fi), HitResult.Hit);

      if (kind != FightItemKind.WeightedNet)//TODO test range?
      {
        var chempAfter1HitHealth = enemy.Stats.Health;
        Assert.Greater(enemyBeginHealth, chempAfter1HitHealth);
        var firstExplCoctailDamage = enemyBeginHealth - chempAfter1HitHealth;

        IncreaseAbility(hero, fi);

        fi = ActivateFightItem(kind, hero);
        var damage2 = fi.Damage;
        Assert.Greater(damage2, damage1);
        enemy.OnHitBy(fi);
        GotoNextHeroTurn();//effect prolonged - make turn to see effect
        var chempAfter2HitHealth = enemy.Stats.Health;
        var secExplCoctailDamage = chempAfter1HitHealth - chempAfter2HitHealth;
        Assert.Greater(secExplCoctailDamage, firstExplCoctailDamage);
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

    private void RevealAllEnemies(Roguelike.RoguelikeGame game)
    {
      AllEnemies.ForEach(i => i.Revealed = true);
    }

    private static void IncreaseAbility(Hero hero, FightItem fi)//AbilityKind kind)
    {
      var ab = hero.GetActiveAbility(fi.AbilityKind);
      hero.AbilityPoints = ab.MaxLevel;
      hero.Level = 11;
      for (int i = 0; i < ab.MaxLevel; i++)
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
      //TestRestoreFactorChange(true);
      TestRestoreFactorChange(false);
    }
        
    private void TestRestoreFactorChange(bool forMana)
    {
      var Hero = game.Hero;
      Hero.AbilityPoints = 5;
      var abVal = 0.0;
      for (int i = 0; i < MaxAbilityInc + 1; i++)
      {
        var done = Hero.IncreaseAbility(forMana ? AbilityKind.RestoreMana : AbilityKind.RestoreHealth);
        var ab = Hero.GetPassiveAbility(forMana ? AbilityKind.RestoreMana : AbilityKind.RestoreHealth);
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
      if (i < MaxAbilityInc)
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
        Assert.AreEqual(game.GameManager.SpellManager.ApplyAttackPolicy(game.Hero, en, weapon.SpellSource), Roguelike.Managers.ApplyAttackPolicyResult.OK);
        Assert.True(en.HasLastingEffect(Roguelike.Effects.EffectType.Firing));
        GotoNextHeroTurn();
      }
    }

    [Test]
    public void TestWandMastering()//ChanceToElementalBulkAttack()
    {
      var game = CreateGame(numEnemies:100);
      float originalStatValue = 0;
      var destExtraStat = SetWeapon(AbilityKind.WandsMastering, game.Hero, out originalStatValue);
      var weapon = game.Hero.GetActiveWeapon();
            
      //Assert.Greater(empOnes.Count, 1);
      var enemies = AllEnemies.Where(i => i.PowerKind == EnemyPowerKind.Champion).ToList();
      enemies[0].Stats.GetStat(EntityStatKind.Health).Value.Nominal *= 10;//make sure wont' die
      float en1Health = enemies[0].Stats.Health;
      float en2Health = enemies[1].Stats.Health;
      for (int i = 0; i < 2; i++)
      {
        var empOne = game.GameManager.CurrentNode.GetClosestEmpty(game.GameManager.Hero, true);
        game.GameManager.CurrentNode.SetTile(enemies[i], empOne.point);
      }
      var ab = game.GameManager.Hero.GetPassiveAbility(Roguelike.Abilities.AbilityKind.WandsMastering);
      for (int i = 0; i < 5; i++)
        ab.IncreaseLevel(game.Hero);

      game.Hero.RecalculateStatFactors(false);
      //var sb = game.Hero.GetTotalValue(EntityStatKind.ChanceToBulkAttack);

      for (int i = 0; i < 20; i++)
      {
        weapon.SpellSource.Count = 20;
        //hit only 1st enemy
        var spell = weapon.SpellSource.CreateSpell(game.Hero);
        Assert.AreEqual(game.GameManager.SpellManager.ApplyAttackPolicy(game.Hero, enemies[0], weapon.SpellSource), Roguelike.Managers.ApplyAttackPolicyResult.OK);
        GotoNextHeroTurn();
      }

      Assert.Greater(en1Health, enemies[0].Stats.Health);

      //2nd shall be hit by an ability
      Assert.Greater(en2Health, enemies[1].Stats.Health);

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
      var damageComparer1 = new OuaDDamageComparer(en, this);

      var spell = weapon.SpellSource.CreateSpell(game.Hero);
      
      Assert.AreEqual(game.GameManager.SpellManager.ApplyAttackPolicy(game.Hero, en, weapon.SpellSource), Roguelike.Managers.ApplyAttackPolicyResult.OK);
      GotoNextHeroTurn();

      damageComparer1.RegisterHealth(en);
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

      for (int i = 0; i < 20; i++)
      {
        var damageComparer2 = new OuaDDamageComparer(en, this);
        spell = weapon.SpellSource.CreateSpell(game.Hero);
        Assert.AreEqual(game.GameManager.SpellManager.ApplyAttackPolicy(game.Hero, en, weapon.SpellSource), Roguelike.Managers.ApplyAttackPolicyResult.OK);
        GotoNextHeroTurn();
        damageComparer2.RegisterHealth(en);
        if (damageComparer2.HealthDifference > damageComparer1.HealthDifference)
        {
          if (esk == EntityStatKind.StaffExtraElementalProjectileDamage)
          {
            AssertHealthDiffPercentageInRange(damageComparer1, damageComparer2, 140, 175);//+factor%
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
      hero.UseAttackVariation = false;
      float auxStatValue;
      var destStat = SetWeapon(kind, hero, out auxStatValue);
      var en = PlainEnemies.First();
      en.Stats.SetNominal(EntityStatKind.Health, 100);
      en.AddImmunity(Roguelike.Effects.EffectType.Bleeding);//not to mix test results
      var wpn = hero.GetActiveWeapon();
      wpn.StableDamage = true;
      Assert.Greater(wpn.LevelIndex, 0);

      Func<float> hitEnemy = () =>
      {
        var health = en.Stats.Health;
        if(!wpn.IsBowLike)
          en.OnMeleeHitBy(hero);
        else
          en.OnHitBy(hero.ActiveFightItem as ProjectileFightItem);
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

      for (int i = 0; i < MaxAbilityInc + 1; i++)
      {
        hero.IncreaseAbility(kind);
        var ab = hero.GetPassiveAbility(kind);
        Assert.AreNotEqual(ab.PrimaryStat.Kind, EntityStatKind.Unset);
        AssertNextValue(i, ab, abVal, abValAux);

        abVal = GetFactor(ab, true);
        abValAux = GetFactor(ab, false);
        Assert.Less(abVal, 21);
        Assert.Less(abValAux, 26);

        abVal = ab.PrimaryStat.Factor;
        //Debug.WriteLine(kind + " Level: " + ab.Level + ", value :" + ab.PrimaryStat.Factor);
      }
      var statValueWithAbility = hero.Stats.GetCurrentValue(destStat);
      Assert.Greater(statValueWithAbility, auxStatValue);

      var heroAttackWithAbility = hero.GetAttackValue(AttackKind.Unset).CurrentTotal;
      Assert.Greater(heroAttackWithAbility, heroAttack);
      var damageWithAbility = hitEnemy();

      if (damageWithAbility < damage)
      {
        int k = 0;
        k++;
      }
      Assert.Greater(damageWithAbility, damage);
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
  }
}
