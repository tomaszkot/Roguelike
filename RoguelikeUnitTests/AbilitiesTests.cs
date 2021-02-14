﻿using Dungeons.Tiles;
using NUnit.Framework;
using Roguelike.Abilities;
using Roguelike.Attributes;
using Roguelike.Policies;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.Looting;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  public class AbilitiesTests : TestBase
  {
    const int MaxAbilityInc = 5;

    [Test]
    public void TestBulkAttack()
    {
      var game = CreateGame();
      var empOnes = game.GameManager.CurrentNode.GetEmptyNeighborhoodTiles(game.GameManager.Hero, false);
      Assert.Greater(empOnes.Count, 1);
      float en1Health = AllEnemies[0].Stats.Health;
      float en2Health = AllEnemies[1].Stats.Health;
      for (int i = 0; i < 2; i++)
      {
        game.GameManager.CurrentNode.SetTile(AllEnemies[i], empOnes[i].Point);
      }
      var ab = game.GameManager.Hero.GetAbility(AbilityKind.BulkAttack);
      ab.PrimaryStat.Value.Factor = 100;
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
      float en1Health = enemies[0].Stats.Health;
      float en2Health = enemies[1].Stats.Health;
      for (int i = 0; i < 2; i++)
      {
        game.GameManager.CurrentNode.SetTile(enemies[i], empOnes[i].Point);
      }
      var ab = game.GameManager.Hero.GetAbility(AbilityKind.BulkAttack);
      for(int i=0;i<5;i++)
        ab.IncreaseLevel(game.Hero);

      game.Hero.RecalculateStatFactors(false);
      var sb = game.Hero.GetTotalValue(EntityStatKind.ChanceToBulkAttack);

      for (int i = 0; i < 20; i++)
      {
        game.GameManager.InteractHeroWith(enemies[0]);
        GotoNextHeroTurn();
      }

      Assert.Greater(en1Health, enemies[0].Stats.Health);
      Assert.Greater(en2Health, enemies[1].Stats.Health);
    }

    [Test]
    public void TestStrikeBack()
    {
      //AbilityKind.StrikeBack
    }

    [Test]
    public void TestLootMastery()
    {
      var game = CreateGame();
      var barrels = game.Level.GetTiles<Barrel>();
      int barrelsCount = barrels.Count;
      Assert.Greater(barrelsCount, 15);

      var numOfBarrelLootBeforeAbility = GetLootFromSrc(game, barrels);

      var enemies = GetPlainEnemies();
      var numOfEnemiesLootBeforeAbility = GetLootFromSrc(game, enemies);

      game.Hero.AbilityPoints = 10;
      for (int i = 0; i < 5; i++)
        game.Hero.IncreaseAbility(AbilityKind.LootingMastering);

      var numOfBarrelLootAfterAbility = GetLootFromSrc(game, barrels);
      var numOfEnemiesLootAfterAbility = GetLootFromSrc(game, enemies);

      Assert.Greater(numOfBarrelLootAfterAbility.Count, numOfBarrelLootBeforeAbility.Count);
      Assert.Greater(numOfEnemiesLootAfterAbility.Count, numOfEnemiesLootBeforeAbility.Count);
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
            game.Level.RemoveLoot(l.Point);
            game.Level.SetEmptyTile(l.Point);
            res.Add(l);
          }
        }
      }

      return res;
    }

    //private int GetLootFromBarrels(List<Barrel> barrels, int barrelsCount)
    //{
    //  int numOfLoot = 0;
    //  for (int i = 0; i < barrelsCount; i++)
    //  {
    //    var loot = game.GameManager.LootManager.TryAddForLootSource(barrels[i]);
    //    if (loot.Any())
    //    {
    //      numOfLoot++;
    //      foreach (var l in loot)
    //      {
    //        game.Level.RemoveLoot(l.Point);
    //        game.Level.SetEmptyTile(l.Point);
    //      }
    //    }
    //  }

    //  return numOfLoot;
    //}

    //[Test]
    //public void TestFightSkills()
    //{
    //  //base.GotoLastLevel();
    //  var en = Level.Enemies.Where(e => e.Kind == Enemy.PowerKind.Champion).First();
    //  var enHealth = en.Stats.Health;
    //  var explosiveCocktail = new ExplosiveCocktail();
    //  var knife = new ThrowingKnife();
    //  var trap = new Trap();

    //  FightItem[] fightSkills = new FightItem[] { explosiveCocktail, knife, trap };
    //  Dictionary<FightItem, List<float>> primaryValues = new Dictionary<FightItem, List<float>>();
    //  Dictionary<FightItem, List<float>> secValues = new Dictionary<FightItem, List<float>>();
    //  gm.Hero.Character.AbilityPoints = 100;
    //  gm.Hero.Character.Level = 30;
    //  for (int abilityLevel = 0; abilityLevel < MaxAbilityInc; abilityLevel++)
    //  {
    //    foreach (var fi in fightSkills)
    //    {
    //      if (!primaryValues.ContainsKey(fi))
    //      {
    //        primaryValues.Add(fi, new List<float>());
    //      }
    //      if (!secValues.ContainsKey(fi))
    //      {
    //        secValues.Add(fi, new List<float>());
    //      }
    //      var ab = fi.GetAbility();

    //      primaryValues[fi].Add(ab.GetFactor(true));
    //      secValues[fi].Add(ab.GetFactor(false));
    //      Assert.IsTrue(ab.Kind != AbilityKind.Unknown);

    //      if (abilityLevel == 0)
    //      {
    //        AssertAreEqual(primaryValues[fi][0], 0);
    //        AssertAreEqual(secValues[fi][0], 0);
    //      }
    //      else
    //      {
    //        AssertGreater(primaryValues[fi][abilityLevel], primaryValues[fi][abilityLevel - 1]);
    //        AssertGreater(secValues[fi][abilityLevel], secValues[fi][abilityLevel - 1]);
    //      }
    //    }
    //    foreach (var fi in fightSkills)
    //    {
    //      var ab = fi.GetAbility();
    //      if (ab.IsPercentage(true))
    //        AssertLess(primaryValues[fi][abilityLevel], 100);
    //      if (ab.IsPercentage(false))
    //        AssertLess(secValues[fi][abilityLevel], 100);
    //      var increased = Hero.IncreaseAbility(ab.Kind);
    //      Assert.IsTrue(increased);

    //    }
    //    //if (abilityLevel > 0)
    //    {
    //      var ab1 = Hero.GetAbility(AbilityKind.ThrowingWeaponsMastering).PrimaryStat.CurrentValue;
    //      var ab2 = Hero.GetAbility(AbilityKind.HuntingMastering).PrimaryStat.CurrentValue;
    //      AssertGreater(ab1, ab2);
    //      //AssertGreater(primaryValues[knife][abilityLevel], primaryValues[trap][abilityLevel]);//trap has 2 turns so knife shall be stronger
    //    }
    //  }
    //}

    //[Test]
    //public void TestFightSkillsVsSpellDamage()
    //{
    //  //Dictionary<float, float> vals = new Dictionary<float, float>();
    //  //var ab = Hero.GetAbility(AbilityKind.ExplosiveMastering);
    //  //for (int i = 0; i < 10; i++)
    //  //{
    //  //  vals[i] = ab.GetExplDamage(i);
    //  //}

    //  base.GotoLastLevel();
    //  var champion = Level.Enemies.Where(e => e.Kind == Enemy.PowerKind.Champion).First();//GetPlainEnemies().First();
    //  var chempBeginHealth = champion.Stats.Health;
    //  var explosiveCocktail = new ExplosiveCocktail();
    //  //explosiveCocktail.SetCaster(Hero);
    //  champion.OnHitBy(explosiveCocktail);
    //  var chempAfter1stHitHealth = champion.Stats.Health;
    //  AssertGreater(chempBeginHealth, chempAfter1stHitHealth);
    //  var firstExplCoctailDamage = chempBeginHealth - chempAfter1stHitHealth;

    //  var scroll = new Scroll(SpellKind.FireBall);
    //  HitEnemyWithSpell(scroll, champion);
    //  var chempAfterSpellHitHealth = champion.Stats.Health;
    //  AssertGreater(chempAfter1stHitHealth, chempAfterSpellHitHealth);
    //  var diffSpell = chempAfter1stHitHealth - chempAfterSpellHitHealth;

    //  //shall be bigger...
    //  AssertLess(Math.Abs(firstExplCoctailDamage - diffSpell), 0.3f);
    //  AssertGreater(firstExplCoctailDamage * 2, diffSpell);

    //  // but not that big...
    //  AssertGreater(diffSpell * 2, firstExplCoctailDamage);
    //  for (int i = 0; i < 15; i++)//hack 15 ?
    //    UpdateSpellToNextLevel<FireBallSpell>(scroll);

    //  HitEnemyWithSpell(scroll, champion);

    //  var enHealth3 = champion.Stats.Health;
    //  AssertGreater(chempAfterSpellHitHealth, enHealth3);
    //  var diffSpell1 = chempAfterSpellHitHealth - enHealth3;
    //  AssertGreater(diffSpell1, firstExplCoctailDamage * 10);

    //  var ab = Hero.GetAbility(AbilityKind.ExplosiveMastering);
    //  Hero.Character.AbilityPoints = ab.MaxLevel;
    //  Hero.Character.Level = 11;
    //  for (int i = 0; i < ab.MaxLevel; i++)
    //  {
    //    var inc = Hero.IncreaseAbility(AbilityKind.ExplosiveMastering);

    //    Assert.IsTrue(inc);
    //  }

    //  explosiveCocktail = new ExplosiveCocktail();
    //  //explosiveCocktail.SetCaster(Hero);
    //  champion.OnHitBy(explosiveCocktail);
    //  var enHealth4 = champion.Stats.Health;
    //  AssertGreater(enHealth3, enHealth4);
    //  var diffExpl2 = enHealth3 - enHealth4;
    //  AssertGreater(diffExpl2, firstExplCoctailDamage * 5);

    //  AssertGreater(diffSpell1, diffExpl2);
    //  //AssertGreater( diffSpell1, diffExpl2 * 2);
    //  AssertLess(diffSpell1, diffExpl2 * 3);
    //}

    //private void HitEnemyWithSpell(Scroll scroll, LivingEntity en)
    //{
    //  Hero.ScrollsPanel.Add(scroll);
    //  Hero.ActiveScroll = scroll;
    //  var spell = Hero.CreateActiveSpell<FireBallSpell>();
    //  en.OnHitBy(spell);
    //}

    [Test]
    public void BasicManaAndHealthTests()
    {
      var game = CreateGame();
      Assert.IsTrue(game.Hero.Abilities.GetItems().Any());//shall have all at 0
      Assert.IsTrue(game.Hero.Abilities.GetItems().All(i => i.Level == 0));
      //TestRestoreFactorChange(true);
      TestRestoreFactorChange(false);
    }

    List<Enemy> GetPlainEnemies()
    {
      return AllEnemies.Where(i => i.PowerKind == EnemyPowerKind.Plain).ToList();
    }

    private void TestRestoreFactorChange(bool forMana)
    {
      var Hero = game.Hero;
      Hero.AbilityPoints = 5;
      var abVal = 0.0;
      for (int i = 0; i < MaxAbilityInc + 1; i++)
      {
        var done = Hero.IncreaseAbility(forMana ? AbilityKind.RestoreMana : AbilityKind.RestoreHealth);
        var ab = Hero.GetAbility(forMana ? AbilityKind.RestoreMana : AbilityKind.RestoreHealth);
        AssertNextValue(i, ab, abVal, null);
        var factor = GetFactor(ab, true);
        Assert.Less(factor, 10);
        abVal = factor;
      }
      if (forMana)
      {
        var en = GetPlainEnemies().First();
        var mana = Hero.Stats.Mana;
        var fireBallScroll = new Scroll(Roguelike.Spells.SpellKind.FireBall);
        Hero.Inventory.Add(fireBallScroll);

        game.GameManager.Context.ApplySpellAttackPolicy(Hero, en, fireBallScroll, (Policy pb) =>{ }, (Policy pa) => { });

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
        var en = GetPlainEnemies().First();
        var health = Hero.Stats.Health;
        while (Hero.OnPhysicalHit(en) == 0)
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

    float GetFactor(Ability ab, bool primary)
    {
      return ab.GetFactor(primary);
    }

    void AssertNextValue(int i, Ability ab, double valuePrimary, double? valueAux)
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

    //[Test]
    //public void BasicDefenceTests()
    //{
    //  Dictionary<AbilityKind, float> abs = new Dictionary<AbilityKind, float>()
    //  {
    //    {AbilityKind.MeleeDefender, 0 },
    //    {AbilityKind.MagicDefender, 0 }
    //  };
    //  foreach (var abKV in abs)
    //  {
    //    var val = TestDefence(abKV.Key);
    //  }
    //  Debug.WriteLine("end");
    //}

    //private float TestDefence(AbilityKind kind)
    //{
    //  var abVal = 0.0f;
    //  var abValAux = 0.0f;
    //  var en = GetPlainEnemies().First();
    //  float health = Hero.Character.Health;
    //  float mana = Hero.Character.Mana;
    //  float health1 = 0;
    //  float healthDiff = 0;

    //  gm.HeroTurn = false;
    //  if (kind == AbilityKind.MeleeDefender)
    //  {
    //    while (Hero.OnPhysicalHit(en) == 0)
    //      ;
    //  }
    //  else
    //  {
    //    en.ActiveScroll = new Scroll(SpellKind.FireBall);
    //    en.DamageApplier.ApplySpellDamage(en, Hero, en.ActiveScroll.CreateSpell(en) as AttackingSpell);
    //  }
    //  health1 = Hero.Character.Health;
    //  healthDiff = health - health1;
    //  AssertGreater(healthDiff, 0);
    //  for (int i = 0; i < MaxAbilityInc + 1; i++)
    //  {
    //    Hero.IncreaseAbility(kind);
    //    var ab = Hero.GetAbility(kind);
    //    AssertNextValue(i, ab, abVal, abValAux);

    //    abVal = GetFactor(ab, true);
    //    abValAux = GetFactor(ab, false);
    //    AssertLess(abVal, 15);
    //    AssertLess(abValAux, 26);

    //    Debug.WriteLine(kind + " Level: " + ab.Level + ", value :" + ab.PrimaryStat.Factor);
    //  }
    //  gm.HeroTurn = false;
    //  if (kind == AbilityKind.MeleeDefender)
    //  {
    //    while (Hero.OnPhysicalHit(en) == 0)
    //      ;

    //  }
    //  else
    //  {
    //    en.DamageApplier.ApplySpellDamage(en, Hero, en.ActiveScroll.CreateSpell(en) as AttackingSpell);
    //  }
    //  var health2 = Hero.Character.Health;
    //  var healthDiff1 = health1 - health2;
    //  AssertGreater(healthDiff, healthDiff1);
    //  return abVal;
    //}

    [Test]
    public void BasicWeaponsMasteryTests()
    {
      var game = CreateGame();
      Dictionary<AbilityKind, float> abs = new Dictionary<AbilityKind, float>()
      {
        {AbilityKind.AxesMastering, 0 },
        {AbilityKind.BashingMastering, 0 },
        {AbilityKind.DaggersMastering, 0 },
        {AbilityKind.SwordsMastering, 0 }
      };
      Dictionary<AbilityKind, float> absR = new Dictionary<AbilityKind, float>()
      {
        {AbilityKind.AxesMastering, 0 },
        {AbilityKind.BashingMastering, 0 },
        {AbilityKind.DaggersMastering, 0 },
        {AbilityKind.SwordsMastering, 0 }
      };
      foreach (var abKV in abs)
      {
        var val = TestWeaponKindMastering(abKV.Key);
        absR[abKV.Key] = val;
      }
      //Debug.WriteLine("end");
    }

    private float TestWeaponKindMastering(AbilityKind kind)
    {
      var abVal = 0.0f;
      var abValAux = 0.0f;
      Weapon wpn = null;
      string wpnName = "";
      var destStat = EntityStatKind.Unset;

      switch (kind)
      {
        case AbilityKind.AxesMastering:
          wpnName = "axe";
          destStat = EntityStatKind.ChanceToCauseTearApart;
          break;
        case AbilityKind.BashingMastering:
          wpnName = "hammer";
          destStat = EntityStatKind.ChanceToCauseStunning;
          break;
        case AbilityKind.DaggersMastering:
          wpnName = "war_dagger";
          destStat = EntityStatKind.ChanceToCauseBleeding;
          break;
        case AbilityKind.SwordsMastering:
          wpnName = "rusty_sword";
          destStat = EntityStatKind.ChanceToHit;
          break;

        default:
          break;
      }
      var Hero = game.Hero;
      var statValue = Hero.Stats.GetCurrentValue(destStat);
      //wpn = game.LootDescriptionManager.GetEquipment(LootKind.Weapon, wpnName) as Weapon;
      wpn = game.GameManager.LootGenerator.GetLootByAsset(wpnName) as Weapon;
      Assert.NotNull(wpn);
      Hero.SetEquipment(CurrentEquipmentKind.Weapon, wpn);
      var en = GetPlainEnemies().First();
      en.Stats.SetNominal(EntityStatKind.Health, 100);
      var health = en.Stats.Health;
      while (en.OnPhysicalHit(Hero) == 0)
        ;
      var health1 = en.Stats.Health;
      var damage = health - health1;
      Assert.Greater(damage, 0);
      var heroAttack = Hero.GetHitAttackValue(false);
      var hav0 = Hero.GetHitAttackValue(false);
      for (int i = 0; i < MaxAbilityInc + 1; i++)
      {
        Hero.IncreaseAbility(kind);
        var ab = Hero.GetAbility(kind);
        Assert.AreNotEqual(ab.PrimaryStat.Kind, EntityStatKind.Unset);
        AssertNextValue(i, ab, abVal, abValAux);

        abVal = GetFactor(ab, true);
        abValAux = GetFactor(ab, false);
        Assert.Less(abVal, 9);
        Assert.Less(abValAux, 26);

        abVal = ab.PrimaryStat.Factor;
        //Debug.WriteLine(kind + " Level: " + ab.Level + ", value :" + ab.PrimaryStat.Factor);
      }
      var statValue1 = Hero.Stats.GetCurrentValue(destStat);
      Assert.Greater(statValue1, statValue);

      var heroAttack1 = Hero.GetHitAttackValue(false);
      var hav = Hero.GetHitAttackValue(false);
      Assert.Greater(heroAttack1, heroAttack);
      while (en.OnPhysicalHit(Hero) == 0)
        ;
      var health2 = en.Stats.Health;
      var damage1 = health1 - health2;
      Assert.Greater(damage1, damage);
      return abVal;
    }
  }
}
