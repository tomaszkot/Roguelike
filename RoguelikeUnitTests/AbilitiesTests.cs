using Dungeons.Tiles;
using NUnit.Framework;
using Roguelike.Abilities;
using Roguelike.Attributes;
using Roguelike.Policies;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  public class AbilitiesTests : TestBase
  {
    const int MaxAbilityInc = 5;

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
      var ab = game.GameManager.Hero.GetAbility(PassiveAbilityKind.BulkAttack);
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
      enemies[0].Stats.GetStat(EntityStatKind.Health).Value.Nominal *= 10;//make sure wont' die
      float en1Health = enemies[0].Stats.Health;
      float en2Health = enemies[1].Stats.Health;
      for (int i = 0; i < 2; i++)
      {
        game.GameManager.CurrentNode.SetTile(enemies[i], empOnes[i].point);
      }
      var ab = game.GameManager.Hero.GetAbility(PassiveAbilityKind.BulkAttack);
      for(int i=0;i<5;i++)
        ab.IncreaseLevel(game.Hero);

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
    public void TestStrikeBack()
    {
      var game = CreateGame();
      var empOnes = game.GameManager.CurrentNode.GetEmptyNeighborhoodTiles(game.GameManager.Hero, false);
      Assert.Greater(empOnes.Count, 1);
      var enemies = PlainEnemies;
      float en1Health = enemies[0].Stats.Health;
      game.GameManager.CurrentNode.SetTile(enemies[0], empOnes[0].point);

      var ab = game.GameManager.Hero.GetAbility(PassiveAbilityKind.StrikeBack);
      for (int i = 0; i < 5; i++)
        ab.IncreaseLevel(game.Hero);

      game.Hero.RecalculateStatFactors(false);
      var sb = game.Hero.GetTotalValue(EntityStatKind.ChanceToStrikeBack);

      for (int i = 0; i < 20; i++)
      {
        game.GameManager.Context.ApplyPhysicalAttackPolicy(enemies[0], game.Hero, (p) => { });
        //GotoNextHeroTurn();
      }

      Assert.Greater(en1Health, enemies[0].Stats.Health);
    }

    [Test]
    public void TestLootMasteryBarrels()
    {
      TestLootMastery<Barrel>();
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
        game.Hero.IncreaseAbility(PassiveAbilityKind.LootingMastering);

      var numOfLootSourcesLootAfterAbility = GetLootFromSrc(game, lootSources);

      Assert.Greater(numOfLootSourcesLootAfterAbility.Count, numOfLootSourcesLootBeforeAbility.Count);
    }

    [Test]
    public void TestLootMasteryEnemies()
    {
      TestLootMastery<Enemy>();
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
        var done = Hero.IncreaseAbility(forMana ? PassiveAbilityKind.RestoreMana : PassiveAbilityKind.RestoreHealth);
        var ab = Hero.GetAbility(forMana ? PassiveAbilityKind.RestoreMana : PassiveAbilityKind.RestoreHealth);
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
        var en = GetPlainEnemies().First();
        var health = Hero.Stats.Health;
        while (Hero.OnPhysicalHitBy(en) == 0)
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
      Dictionary<PassiveAbilityKind, float> abs = new Dictionary<PassiveAbilityKind, float>()
      {
        {PassiveAbilityKind.AxesMastering, 0 },
        {PassiveAbilityKind.BashingMastering, 0 },
        {PassiveAbilityKind.DaggersMastering, 0 },
        {PassiveAbilityKind.SwordsMastering, 0 }
      };
      Dictionary<PassiveAbilityKind, float> absR = new Dictionary<PassiveAbilityKind, float>()
      {
        {PassiveAbilityKind.AxesMastering, 0 },
        {PassiveAbilityKind.BashingMastering, 0 },
        {PassiveAbilityKind.DaggersMastering, 0 },
        {PassiveAbilityKind.SwordsMastering, 0 }
      };
      foreach (var abKV in abs)
      {
        var val = TestWeaponKindMastering(abKV.Key);
        absR[abKV.Key] = val;
      }
      //Debug.WriteLine("end");
    }

    private float TestWeaponKindMastering(PassiveAbilityKind kind)
    {
      var abVal = 0.0f;
      var abValAux = 0.0f;
      var Hero = game.Hero;

      float statValue;
      var destStat = SetWeapon(kind, Hero, out statValue);
      var en = GetPlainEnemies().First();
      en.Stats.SetNominal(EntityStatKind.Health, 100);

      Func<float> hitEnemy = () =>
      {
        var health = en.Stats.Health;
        en.OnPhysicalHitBy(Hero);
        var health1 = en.Stats.Health;
        return health - health1;
      };
      var damage = hitEnemy();

      Assert.Greater(damage, 0);
      var heroAttack = Hero.GetHitAttackValue(false);
      
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
      var statValueWithAbility = Hero.Stats.GetCurrentValue(destStat);
      Assert.Greater(statValueWithAbility, statValue);

      var heroAttackWithAbility = Hero.GetHitAttackValue(false);
      Assert.Greater(heroAttackWithAbility, heroAttack);
      var damageWithAbility = hitEnemy();
      
      Assert.Greater(damageWithAbility, damage);
      return abVal;
    }

    private EntityStatKind SetWeapon(PassiveAbilityKind kind, Hero Hero, out float statValue)
    {
      var destStat = EntityStatKind.Unset;
      Weapon wpn = null;
      string wpnName = "";
      switch (kind)
      {
        case PassiveAbilityKind.AxesMastering:
          wpnName = "axe";
          destStat = EntityStatKind.ChanceToCauseTearApart;
          break;
        case PassiveAbilityKind.BashingMastering:
          wpnName = "hammer";
          destStat = EntityStatKind.ChanceToCauseStunning;
          break;
        case PassiveAbilityKind.DaggersMastering:
          wpnName = "war_dagger";
          destStat = EntityStatKind.ChanceToCauseBleeding;
          break;
        case PassiveAbilityKind.SwordsMastering:
          wpnName = "rusty_sword";
          destStat = EntityStatKind.ChanceToHit;
          break;

        default:
          break;
      }

      statValue = Hero.Stats.GetCurrentValue(destStat);
      //wpn = game.LootDescriptionManager.GetEquipment(LootKind.Weapon, wpnName) as Weapon;
      wpn = game.GameManager.LootGenerator.GetLootByAsset(wpnName) as Weapon;
      Assert.NotNull(wpn);
      Hero.SetEquipment(wpn, CurrentEquipmentKind.Weapon);
      return destStat;
    }
  }
}
