using NUnit.Framework;
using Roguelike.Abilities;
using Roguelike.Attributes;
using Roguelike.Calculated;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using RoguelikeUnitTests.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OuaDIIUnitTests
{
  class OuadIIDamageComparer : DamageComparer
  {
    TestBase tb;
    public OuadIIDamageComparer(LivingEntity le, TestBase tb) : base(le)
    {
      this.tb = tb;
    }
        
    public override void GotoNextHeroTurn()
    {
      tb.GotoNextHeroTurn();
    }
  }

  [TestFixture]
  class AbilitiesTests : TestBase
  {
    protected override void OnInit()
    {
      base.OnInit();
    }

    //[TestCase(AbilityKind.ArrowVolley)]
    [TestCase(AbilityKind.PerfectHit)]
    public void TestAttackDescForAbility(AbilityKind ak)
    {
      var game = CreateWorld();
      var hero = GameManager.OuadHero;

      AddBowToHero(hero);

      var ab = ActivateAblityInHotBar(hero, ak);
      Assert.NotNull(ab);
      Assert.NotNull(hero.GetFightItemFromActiveProjectileAbility());
      Assert.NotNull(hero.SelectedActiveAbility);

      var ad0 = new AttackDescription(hero, false, AttackKind.PhysicalProjectile);
      Assert.Greater(ad0.CurrentTotal, 0);
            
      MaximizeAbility(ab, hero);

      var ad1 = new AttackDescription(hero, false, AttackKind.PhysicalProjectile);
      Assert.Greater(ad1.CurrentTotal, 0);
      Assert.Greater(ad1.CurrentTotal, ad0.CurrentTotal);
    }

    private void AddBowToHero(OuaDII.Tiles.LivingEntities.Hero hero)
    {
      var wpn = GetTestBow();
      Assert.True(SetHeroEquipment(wpn));

      var fi = new ProjectileFightItem(FightItemKind.PlainArrow, hero);
      fi.Count = 10;
      hero.Inventory.Add(fi);
                  
      Assert.NotNull(hero.ActiveFightItem);
    }

    [Test]
    public void TestLootMastery()
    {
      CreateWorld();
      const int BarrelsCount = 150;

      for (int i = 0; i < BarrelsCount; i++)
      {
        var emp = GameManager.CurrentNode.GetRandomEmptyTile();
        GameManager.CurrentNode.SetTile(new Barrel(), emp.point);
      }
      var barrels = GameManager.CurrentNode.GetTiles<Barrel>();
      int barrelsCount = barrels.Count;
      Assert.GreaterOrEqual(barrelsCount, BarrelsCount);

      var numOfBarrelLootBeforeAbility = GetLootFromSrc(barrels);

      var enemies = Enemies.Cast<Enemy>().Where(e => e.PowerKind == EnemyPowerKind.Plain).ToList();
      var numOfEnemiesLootBeforeAbility = GetLootFromSrc(enemies);

      GameManager.Hero.AbilityPoints = 10;
      for (int i = 0; i < 5; i++)
        GameManager.Hero.IncreaseAbility(AbilityKind.LootingMastering);

      var numOfBarrelLootAfterAbility = GetLootFromSrc(barrels);
      var numOfEnemiesLootAfterAbility = GetLootFromSrc(enemies);

      Assert.Greater(numOfBarrelLootAfterAbility.Count, numOfBarrelLootBeforeAbility.Count);
      Assert.Greater(numOfEnemiesLootAfterAbility.Count, numOfEnemiesLootBeforeAbility.Count);
    }

    [Test]
    //[Repeat(3)]
    public void TestThrowWeb()
    {
      var game = CreateWorld();
      var hero = PrepareHero(AbilityKind.WeightedNet, true);
      var fi = hero.ActiveProjectileFightItem;
      Assert.NotNull(fi);

      var en = PrepareEnemy();
      var initEnemyHealth = en.Stats.Health;
      var initHeroHealth = hero.Stats.Health;

      var ab = PrepareAbility(hero, AbilityKind.WeightedNet, 5);
      Assert.NotNull(ab);
      ab = ActivateAblityInHotBar(hero, AbilityKind.WeightedNet);
      Assert.NotNull(ab);

      //GameManager.ApplyHeroPhysicalAttackPolicy(en, true);
      Assert.True(UseFightItem(hero, en, fi));
      string reason = "";
      Assert.True(en.IsMoveBlockedDueToLastingEffect(out reason));
      Assert.AreEqual(en.Stats.Health, initEnemyHealth);
      GotoNextHeroTurn();
      Assert.AreEqual(hero.Stats.Health, initHeroHealth);//eneamy is webbed
      while(en.LastingEffects.Any())
        GotoNextHeroTurn();

      GotoNextHeroTurn();
      Assert.Greater(initHeroHealth, hero.Stats.Health);//eneamy is NOT webbed
    }

    [Test]
    //[Repeat(3)]
    public void TestRage()
    {
      var game = CreateWorld();
      var hero = GameManager.OuadHero;
      var wpn = GameManager.LootGenerator.GetLootByTileName<Weapon>("rusty_sword");
      SetHeroEquipment(wpn);

      hero.AlwaysHit[AttackKind.Melee] = true;
      var en = PrepareEnemy();

      var enHealth = en.Stats.Health;
      GameManager.ApplyHeroPhysicalAttackPolicy(en, true);
      Assert.Less(en.Stats.Health, enHealth);
      var hDiff1 = enHealth - en.Stats.Health;
      enHealth = en.Stats.Health;

      GotoNextHeroTurn();

      var ab = hero.Abilities.ActiveItems.Where(i => i.Kind == AbilityKind.Rage).SingleOrDefault();
      PrepareAbility(hero, AbilityKind.Rage, 5);

      var adBefore = new AttackDescription(hero, false, AttackKind.Melee);
      ActivateAblityInHotBar(hero, AbilityKind.Rage);
      var adAfter = new AttackDescription(hero, false, AttackKind.Melee);
      Assert.Greater(adAfter.CurrentTotal, adBefore.CurrentTotal);

      GameManager.ApplyHeroPhysicalAttackPolicy(en, true);
      Assert.Less(en.Stats.Health, enHealth);
      var hDiff2 = enHealth - en.Stats.Health;
      Assert.Greater(hDiff2, hDiff1);
      var rate = hDiff2 / hDiff1;
      Assert.Greater(rate, 1.3f);

      adAfter = new AttackDescription(hero, false, AttackKind.Melee);
      Assert.AreEqual(adAfter.CurrentTotal, adBefore.CurrentTotal);//cooldown
    }

    public static void AssertHealthDiffPercentageInRange(DamageComparer dc1, DamageComparer dc2, int percMin, int percMax)
    {
      RoguelikeUnitTests.TestBase.AssertHealthDiffPercentageInRange(dc1, dc2, percMin, percMax);
    }

    public static void AssertDurationDiffInRange(DamageComparer dc1, DamageComparer dc2, int min, int max)
    {
      RoguelikeUnitTests.TestBase.AssertDurationDiffInRange(dc1, dc2, min, max);
    }

    [Test]
    //[Repeat(3)]
    public void OpenWoundTest()
    {
      var game = CreateWorld();
      var hero = GameManager.OuadHero;

      var ab = PrepareAbility(hero, AbilityKind.OpenWound);

      hero.AlwaysHit[AttackKind.Melee] = true;
      var en = PlainEnemies.First();
      MakeEntityLongLiving(en);
      var damageComparer1 = new OuadIIDamageComparer(en, this);
      
      PlaceCloseToHero(hero, en);
      en.SetIsWounded(false);

      var wpn = GameManager.LootGenerator.GetLootByTileName<Weapon>("rusty_sword");
      SetHeroEquipment(wpn);
      ActivateAblityInHotBar(hero, AbilityKind.OpenWound);

      GameManager.ApplyHeroPhysicalAttackPolicy(en, true);
      Assert.NotNull(en.LastingEffectsSet.GetByType(Roguelike.Effects.EffectType.Bleeding));
      damageComparer1.RegisterHealth(en);
      Assert.Greater(damageComparer1.HealthDifference, 0);
      var damageComparer2 = new OuadIIDamageComparer(en, this);

      damageComparer1.RegisterDuration(en, Roguelike.Effects.EffectType.Bleeding);

      MaximizeAbility(ab, hero);

      WaitForAbilityCooldown(ab);

      GameManager.ApplyHeroPhysicalAttackPolicy(en, true);

      damageComparer2.RegisterHealth(en);

      AssertHealthDiffPercentageInRange(damageComparer1, damageComparer2, 200, 300);

      damageComparer2.RegisterDuration(en, Roguelike.Effects.EffectType.Bleeding);

      AssertDurationDiffInRange(damageComparer1, damageComparer2, 150, 300);
    }

    [TestCase(AbilityKind.ArrowVolley, 3)]
    //[TestCase(AbilityKind.PiercingArrow, 2)]
    [Repeat(1)]
    public void AdvArrowTest(AbilityKind kind, int enemiesCount)
    {
      var game = CreateWorld();
      var hero = PrepareHero(kind, true);
      var wpn = GetTestBow();
      SetHeroEquipment(wpn);
      var fi = hero.ActiveProjectileFightItem;
      var initFiCount = fi.Count;
      Assert.NotNull(fi);

      var ab = PrepareAbility(hero, kind, 5);

      var enemies = PlainEnemies.Take(enemiesCount).ToList();
      var enemiesHealth = new Dictionary<LivingEntity, float>();
      foreach (var en in enemies)
      {
        InitEnemy(hero, en);
        enemiesHealth[en] = en.Stats.Health;
      }
            
      if (kind == AbilityKind.PiercingArrow)
        AlignOneAfterAnotherNextToHero(enemies[0], enemies[1], hero);

      Assert.AreEqual(fi.ActiveAbilitySrc, AbilityKind.Unset);
      ActivateAblityInHotBar(hero, kind);
      Assert.AreEqual(fi.ActiveAbilitySrc, AbilityKind.Unset);
      //foreach (var en in enemies)
      //{
        Assert.True(UseFightItem(hero, enemies[0], fi));//inside AbilityKind was not Unset !
      //}
      Assert.AreEqual(fi.ActiveAbilitySrc, AbilityKind.Unset);
      
      if (kind == AbilityKind.PiercingArrow)
      {
        Assert.Less(enemies[0].Stats.Health, enemiesHealth[enemies[0]]);
        //TODO 2nd?
      }

      if (kind == AbilityKind.ArrowVolley)//TODO
      {
        int lessHealthCounter = 0;
        foreach (var en in enemies)
        {
          if (enemiesHealth[en] > en.Stats.Health)
            lessHealthCounter++;
        }
        Assert.Greater(lessHealthCounter, 1);
        Assert.AreEqual(fi.Count, initFiCount - lessHealthCounter);
      }
    }

    private void AlignOneAfterAnotherNextToHero(Enemy en1, Enemy en2, OuaDII.Tiles.LivingEntities.Hero hero)
    {
      var pt = en1.point;
      if (en1.point.X == hero.point.X)
      {
        pt.Y = en1.point.Y+1;
        if (pt == hero.point)
          pt.Y = en1.point.Y-1;
      }
      else
      {
        pt.X = en1.point.X + 1;
        if (pt == hero.point)
          pt.X = en1.point.X - 1;
      }
      var tileAt = this.GameManager.CurrentNode.GetTile(pt);
      if (!tileAt.IsEmpty)
      {
        Assert.True(false);
      }
      Assert.True(tileAt.IsEmpty);
      this.GameManager.CurrentNode.SetTile(en2, pt);
    }

    private void InitEnemy(OuaDII.Tiles.LivingEntities.Hero hero, Enemy en)
    {
      en.Stats.SetNominal(EntityStatKind.Health, 300);
      var en1HealthBase = en.Stats.Health;
      PlaceCloseToHero(hero, en);
    }

    [Test]
    [Repeat(1)]
    public void TestPerfectHitWorks()
    {
      var game = CreateWorld();
      var hero = PrepareHero(AbilityKind.PerfectHit, true);
      Enemy enemy = PrepareEnemy();

      var damageComparer1 = new OuadIIDamageComparer(enemy, this);
      
      var bow = GenerateEquipment<Weapon>("bow");
      Assert.True(SetHeroEquipment(bow));
      var fi = hero.ActiveProjectileFightItem;
      Assert.NotNull(fi);
      
      Assert.True(UseFightItem(hero, enemy, fi));
      damageComparer1.RegisterHealth(enemy);
      var damageComparer2 = new OuadIIDamageComparer(enemy, this);
      var ab = ActivateAblityInHotBar(hero, AbilityKind.PerfectHit);
      MaximizeAbility(ab, hero);
      GotoNextHeroTurn();
      Assert.True(UseFightItem(hero, enemy, fi));
      damageComparer2.RegisterHealth(enemy);

      AssertHealthDiffPercentageInRange(damageComparer1, damageComparer2, 145, 210);
    }

    private Enemy PrepareEnemy(bool addImmunities = true)
    {
      var hero = GameManager.OuadHero;
      var enemy = PlainEnemies.First();
      enemy.Stats.Stats[EntityStatKind.Health].Value.Nominal = 300;
      PlaceCloseToHero(hero, enemy);
      if(addImmunities)
        enemy.AddImmunity(Roguelike.Effects.EffectType.Bleeding);

      if(!enemy.Name.Any())
        enemy.Name = "enemy";
      return enemy;
    }

    [Test]
    public void TestStrideWorks()
    {
      var game = CreateWorld();
      var hero = GameManager.OuadHero;
      for (int i = 0; i < 100; i++)
        Assert.False(hero.CanUseAbility(AbilityKind.Stride));

      var melee = hero.GetAttackValue(AttackKind.Melee);
      PrepareAbility(hero, AbilityKind.Stride);
      var meleeAfterInc = hero.GetAttackValue(AttackKind.Melee);
      Assert.AreEqual(meleeAfterInc.CurrentPhysical, melee.CurrentPhysical);
      hero.AlwaysHit[AttackKind.Melee] = true;

      Assert.AreEqual(hero.ShortcutsBar.ActiveItemDigit, -1);
      var en = PlainEnemies.First();
      PlaceCloseToHero(hero, en);
      var enHealth = en.Stats.Health;
      GameManager.ApplyHeroPhysicalAttackPolicy(en, true);
      var healthDiff = enHealth - en.Stats.Health;
      Assert.Greater(healthDiff, 0);
      enHealth = en.Stats.Health;

      ActivateAblityInHotBar(hero, AbilityKind.Stride);

      GotoNextHeroTurn();
      GameManager.ApplyHeroPhysicalAttackPolicy(en, true);
      var healthDiff1 = enHealth - en.Stats.Health;
      Assert.Greater(healthDiff1, healthDiff);
    }
        

    [Test]
    [Repeat(5)]
    public void TestStrideIncrease()
    {
      var game = CreateWorld();
      var hero = PrepareHero(AbilityKind.Stride,  true);
      var stride = PrepareAbility(hero, AbilityKind.Stride);
      var en = PrepareEnemy();

      //hit enemy without stride
      Assert.AreEqual(hero.ShortcutsBar.ActiveItemDigit, -1);
                  
      var enHealth = en.Stats.Health;
      GameManager.ApplyHeroPhysicalAttackPolicy(en, true);
      var healthDiff = enHealth - en.Stats.Health;
      Assert.Greater(healthDiff, 0);
      enHealth = en.Stats.Health;

      Assert.NotNull(ActivateAblityInHotBar(hero, AbilityKind.Stride));

      //hit enemy with stride
      for (int level = 0; level < stride.MaxLevel - 1; level++)
      {
        GotoNextHeroTurn();
        Assert.AreEqual(en.LastingEffects.Count, 0);
        WriteLine("ApplyHeroPhysicalAttackPolicy ... level: " + level);
        GameManager.ApplyHeroPhysicalAttackPolicy(en, true);
        var healthDiff1 = enHealth - en.Stats.Health;
        
        Assert.Greater(healthDiff1, healthDiff, "level = " + level);
        enHealth = en.Stats.Health;
        healthDiff = healthDiff1;
        Assert.AreEqual(en.LastingEffects.Count, 0);
        //inc ab
        Assert.True(stride.IncreaseLevel(hero));

        while (stride.CoolDownCounter > 0)
          GotoNextHeroTurn();
      }
    }

    private OuaDII.Tiles.LivingEntities.Hero PrepareHero(AbilityKind ak, bool activateProjectileFightItem)
    {
      var hero = GameManager.OuadHero;
      hero.Stats.Stats[EntityStatKind.Health].Value.Nominal = 300;
      hero.AlwaysHit[AttackKind.Melee] = true;
      hero.UseAttackVariation = false;
      hero.AlwaysHit[AttackKind.PhysicalProjectile] = true;
      ProjectileFightItem fi = null;
      if (ak == AbilityKind.PerfectHit ||
         ak == AbilityKind.ArrowVolley ||
         ak == AbilityKind.PiercingArrow)
      {
        fi = new ProjectileFightItem(FightItemKind.PlainArrow, hero);
      }
      else if (ak == AbilityKind.WeightedNet)
      {
        fi = new ProjectileFightItem(FightItemKind.WeightedNet, hero);
      }
      if (fi != null)
      {
        fi.Count = 10;
        hero.Inventory.Add(fi);
        hero.ActiveFightItem = fi;
      }

      return hero;
    }

    //private static void InitAbility(OuaDII.Tiles.LivingEntities.Hero hero, ActiveAbility ability)
    //{
    //  var melee = hero.GetAttackValue(AttackKind.Melee);

    //  InitAbility(hero, AbilityKind.Stride);
    //  var meleeAfterInc = hero.GetAttackValue(AttackKind.Melee);
    //  Assert.AreEqual(meleeAfterInc.CurrentPhysical, melee.CurrentPhysical);
    //}

    private Ability PrepareAbility(OuaDII.Tiles.LivingEntities.Hero hero, AbilityKind abilityKind, int increaseCount = 1)
    {
      var ability = hero.Abilities.ActiveItems.Where(i => i.Kind == abilityKind).SingleOrDefault();
      if(ability == null)
        hero.Abilities.PassiveItems.Where(i => i.Kind == abilityKind).SingleOrDefault();

      Assert.NotNull(ability);
      Assert.AreEqual(ability.Level, 0);
      Assert.AreEqual(ability.PrimaryStat.Factor, 0);
      Assert.AreEqual(ability.AuxStat.Factor, 0);

      //MaximizeAbility(ability, hero);
      for(int i=0;i< increaseCount;i++)
        Assert.True(ability.IncreaseLevel(hero));
      
      if(abilityKind != AbilityKind.ArrowVolley && abilityKind != AbilityKind.PiercingArrow)
        Assert.Greater(ability.PrimaryStat.Factor, 0);

      return ability;
    }

    private List<Loot> GetLootFromSrc(IEnumerable<ILootSource> lootSource)
    {
      var res = new List<Loot>();
      int numOfLoot = 0;
      foreach (var en in lootSource)
      {
        var loot = GameManager.LootManager.TryAddForLootSource(en);
        if (loot.Any())
        {
          numOfLoot++;
          foreach (var l in loot)
          {
            GameManager.CurrentNode.RemoveLoot(l.point);
            GameManager.CurrentNode.SetEmptyTile(l.point);
            res.Add(l);
          }
        }
      }

      return res;
    }
  }
}
