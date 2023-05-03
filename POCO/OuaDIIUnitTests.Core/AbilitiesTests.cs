using Dungeons.Core;
using Dungeons.Tiles;
using NUnit.Framework;
using Roguelike.Abilities;
using Roguelike.Attributes;
using Roguelike.Calculated;
using Roguelike.Events;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using RoguelikeUnitTests.Core.Utils;
using RoguelikeUnitTests.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using static Dungeons.TileContainers.DungeonNode;

namespace OuaDIIUnitTests
{
  public static class PointExt
  {

    public static Point MoveBy(this Point src, int x, int y)
    {
      return new Point(src.X + (int)x, src.Y + (int)y);
    }

    public static Vector2D ToVector2D(this Point src)
    {
      return new Vector2D() { X = src.X, Y = src.Y };
    }

  }

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

      var ab = ActivateActiveAblityInHotBar(hero, ak);
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

    //TODO
    //[Test]
    //[Repeat(1)]
    //public void TestLootMastery()
    //{
    //  CreateWorld();
    //  const int BarrelsCount = 150;

    //  for (int i = 0; i < BarrelsCount; i++)
    //  {
    //    var emp = GameManager.CurrentNode.GetRandomEmptyTile(EmptyCheckContext.DropLoot);
    //    GameManager.CurrentNode.SetTile(new Barrel(), emp.point);
    //  }
    //  var barrels = GameManager.CurrentNode.GetTiles<Barrel>();
    //  int barrelsCount = barrels.Count;
    //  Assert.GreaterOrEqual(barrelsCount, BarrelsCount);

    //  var numOfBarrelLootBeforeAbility = GetLootFromSrc(barrels);

    //  var enemies = Enemies.Cast<Enemy>().Where(e => e.PowerKind == EnemyPowerKind.Plain).ToList();
    //  var numOfEnemiesLootBeforeAbility = GetLootFromSrc(enemies);

    //  GameManager.Hero.AbilityPoints = 10;
    //  for (int i = 0; i < 5; i++)
    //    GameManager.Hero.IncreaseAbility(AbilityKind.LootingMastering);

    //  var numOfBarrelLootAfterAbility = GetLootFromSrc(barrels);
    //  var numOfEnemiesLootAfterAbility = GetLootFromSrc(enemies);

    //  Assert.Greater(numOfBarrelLootAfterAbility.Count, numOfBarrelLootBeforeAbility.Count);
    //  Assert.Greater(numOfEnemiesLootAfterAbility.Count, numOfEnemiesLootBeforeAbility.Count);
    //}

    [Test]
    //[Repeat(3)]
    public void TestThrowWeb()
    {
      var game = CreateWorld();
      var hero = PrepareHero(AbilityKind.WeightedNet);
      var fi = hero.ActiveProjectileFightItem;
      Assert.NotNull(fi);

      var en = PrepareEnemy();
      en.AlwaysHit[AttackKind.Melee] = true;
      var initEnemyHealth = en.Stats.Health;
      var initHeroHealth = hero.Stats.Health;

      var ab = PrepareAbility(hero, AbilityKind.WeightedNet, 5);
      Assert.NotNull(ab);
      ab = ActivateActiveAblityInHotBar(hero, AbilityKind.WeightedNet);
      Assert.NotNull(ab);

      Assert.True(UseFightItem(hero, en, fi));
      string reason = "";
      Assert.True(en.IsMoveBlockedDueToLastingEffect(out reason));
      Assert.AreEqual(en.Stats.Health, initEnemyHealth);
      GotoNextHeroTurn();
      Assert.AreEqual(hero.Stats.Health, initHeroHealth);//eneamy is webbed
      WaitForEntityHasNoLE(en);

      GotoNextHeroTurn();
      Assert.Greater(initHeroHealth, hero.Stats.Health);
    }

    private void WaitForHeroHasNoLE()
    {
      WaitForEntityHasNoLE(GameManager.Hero);
    }

    private void WaitForEntityHasNoLE(LivingEntity le)
    {
      while (le.LastingEffects.Any())
        GotoNextHeroTurn();
    }

    [Test]
    [Repeat(1)]
    public void TestRage()
    {
      var game = CreateWorld();
      var hero = PrepareHero("rusty_sword");
      var en = PrepareEnemy();

      GameManager.EventsManager.EventAppended += (object sender, Roguelike.Events.GameEvent e) =>
      {
        if (e is LivingEntityAction lea && lea.Kind == LivingEntityActionKind.GainedDamage)
        {
          //Console.WriteLine(lea.InvolvedEntity+ " GainedDamage " + );
          Debug.WriteLine(lea.Info);
        }
      };

      var damageComparer1 = new OuadIIDamageComparer(en, this);
      GameManager.ApplyHeroPhysicalAttackPolicy(en, true);
      damageComparer1.RegisterHealth();

      GotoNextHeroTurn();

      var ab = PrepareActiveAbility(hero, AbilityKind.Rage, 5);

      var adBefore = new AttackDescription(hero, false, AttackKind.Melee);

      ActivateActiveAblityInHotBar(hero, AbilityKind.Rage);
      if (!ab.AutoApply)
      {
        GameManager.UseActiveAbility(ab, hero, true);
      }
      if (ab.TurnsIntoLastingEffect)
        Assert.NotNull(hero.GetFirstLastingEffect(Roguelike.Effects.EffectType.Rage));
      var adAfter = new AttackDescription(hero, false, AttackKind.Melee);
      Assert.Greater(adAfter.CurrentTotal, adBefore.CurrentTotal);

      var damageComparer2 = new OuadIIDamageComparer(en, this);
      GameManager.ApplyHeroPhysicalAttackPolicy(en, true);
      damageComparer2.RegisterHealth();
      var diffPerc = AssertHealthDiffPercentageInRange(damageComparer1, damageComparer2, 150, 250);

      
      for(int i=0;i<3;i++)
        GotoNextHeroTurn();
      Assert.Null(hero.GetFirstLastingEffect(Roguelike.Effects.EffectType.Rage));
      //while (hero.GetFirstLastingEffect(Roguelike.Effects.EffectType.Rage) != null)
      //GotoNextHeroTurn();
      var adAfter1 = new AttackDescription(hero, false, AttackKind.Melee);
      Assert.AreEqual(adAfter1.CurrentTotal, adBefore.CurrentTotal);//cooldown
    }


    public static float AssertHealthDiffPercentageInRange(DamageComparer dc1, DamageComparer dc2, int percMin, int percMax)
    {
      return RoguelikeUnitTests.TestBase.AssertHealthDiffPercentageInRange(dc1, dc2, percMin, percMax);
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
      ActivateActiveAblityInHotBar(hero, AbilityKind.OpenWound);
      //Assert.NotNull(hero.LastingEffectsSet.GetByType(Roguelike.Effects.EffectType.OpenWound));//shall be shown above hero as active ability

      //1st attack with ability
      GameManager.ApplyHeroPhysicalAttackPolicy(en, true);
      Assert.NotNull(en.LastingEffectsSet.GetByType(Roguelike.Effects.EffectType.Bleeding));
      damageComparer1.RegisterHealth();
      Assert.Greater(damageComparer1.HealthDifference, 0);

      //wait for effect to end
      var waitedTurns1 = damageComparer1.WaitForEffectEnd(en, Roguelike.Effects.EffectType.Bleeding);

      MaximizeAbility(ab, hero);
      WaitForAbilityCoolDown(ab);

      var damageComparer2 = new OuadIIDamageComparer(en, this);
      //2nd attack with ability
      GameManager.ApplyHeroPhysicalAttackPolicy(en, true);
      Assert.NotNull(en.LastingEffectsSet.GetByType(Roguelike.Effects.EffectType.Bleeding));
      //wait for effect to end
      var waitedTurns2 = damageComparer2.WaitForEffectEnd(en, Roguelike.Effects.EffectType.Bleeding);
      Assert.Greater(waitedTurns2, waitedTurns1);
      Assert.Less(waitedTurns2, waitedTurns1 * 3);
      damageComparer2.RegisterHealth();

      AssertHealthDiffPercentageInRange(damageComparer1, damageComparer2, 200, 450);

      AssertDurationDiffInRange(damageComparer1, damageComparer2, 150, 300);
    }

    [TestCase(AbilityKind.ArrowVolley, 3)]
    [TestCase(AbilityKind.PiercingArrow, 2)]
    [Repeat(1)]
    public void AdvArrowTest(AbilityKind kind, int enemiesCount)
    {
      var game = CreateWorld();
      var hero = PrepareHero(kind);
      hero.AlwaysHit[AttackKind.PhysicalProjectile] = true;//TODO
      var wpn = GetTestBow();
      SetHeroEquipment(wpn);
      var fi = hero.ActiveProjectileFightItem;
      var initFiCount = fi.Count;
      Assert.NotNull(fi);

      var ab = PrepareAbility(hero, kind, 5);

      //var close = AllEnemies.Where(i => i.DistanceFrom(hero) < 10).ToList();
      var enemies = AllEnemies.Take(enemiesCount).ToList();//*2 cause not sure how many would be hit
      
      var enemiesHealth = new Dictionary<LivingEntity, float>();
      foreach (var en in enemies)
      {
        InitEnemy(hero, en);
        //PlaceCloseToHero(hero, )
        enemiesHealth[en] = en.Stats.Health;
      }

      if (kind == AbilityKind.PiercingArrow)
        BaseHelper.AlignOneAfterAnotherNextToHero(GameManager, enemies[0], enemies[1], hero);

      Assert.AreEqual(fi.ActiveAbilitySrc, AbilityKind.Unset);
      ActivateActiveAblityInHotBar(hero, kind);
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
          if (en.GetFightItemKindHitCounter(fi.FightItemKind) > 0)
            lessHealthCounter++;
        }
        Assert.Greater(lessHealthCounter, 1);
        var expectedAmmoCount = initFiCount - lessHealthCounter;
        Assert.AreEqual(fi.Count, expectedAmmoCount);
      }

      GotoNextHeroTurn();
      initFiCount = fi.Count;
      Assert.True(UseFightItem(hero, enemies[0], fi));

      if (kind == AbilityKind.ArrowVolley)//TODO
      {
        
        int lessHealthCounter = 0;
        foreach (var en in enemies)
        {
          if (en.GetFightItemKindHitCounter(fi.FightItemKind) == 2)
            lessHealthCounter++;
        }
        Assert.AreEqual(lessHealthCounter, 1);//Cooldown makes it switched to normal attack
        var expectedAmmoCount = initFiCount - lessHealthCounter;
        Assert.AreEqual(fi.Count, expectedAmmoCount);
      }
    }

    //[TestCase(AbilityKind.ArrowVolley, 3)]
    //[TestCase(AbilityKind.PiercingArrow, 2)]
    //[Repeat(1)]
    //public void ArrowCoolDownTest(AbilityKind kind, int enemiesCount)
    //{
    //  var game = CreateWorld();
    //  var hero = PrepareHero(kind);
    //  var wpn = GetTestBow();
    //  SetHeroEquipment(wpn);
    //  var fi = hero.ActiveProjectileFightItem;
    //  var initFiCount = fi.Count;
    //  Assert.NotNull(fi);

    //  var ab = PrepareAbility(hero, kind, 5);

    //  var enemies = AllEnemies.Take(enemiesCount * 2).ToList();//*2 cause not sure how many would be hit
    //  var enemiesHealth = new Dictionary<LivingEntity, float>();
    //  foreach (var en in enemies)
    //  {
    //    InitEnemy(hero, en);
    //    enemiesHealth[en] = en.Stats.Health;
    //  }

    //  if (kind == AbilityKind.PiercingArrow)
    //    AlignOneAfterAnotherNextToHero(enemies[0], enemies[1], hero);

    //  Assert.AreEqual(fi.ActiveAbilitySrc, AbilityKind.Unset);
    //  ActivateActiveAblityInHotBar(hero, kind);
    //  Assert.AreEqual(fi.ActiveAbilitySrc, AbilityKind.Unset);
    //  //foreach (var en in enemies)
    //  //{
    //  Assert.True(UseFightItem(hero, enemies[0], fi));//inside AbilityKind was not Unset !
    //  //}
    //  Assert.AreEqual(fi.ActiveAbilitySrc, AbilityKind.Unset);

    //  if (kind == AbilityKind.PiercingArrow)
    //  {
    //    Assert.Less(enemies[0].Stats.Health, enemiesHealth[enemies[0]]);
    //    //TODO 2nd?
    //  }

    //  if (kind == AbilityKind.ArrowVolley)//TODO
    //  {
    //    int lessHealthCounter = 0;
    //    foreach (var en in enemies)
    //    {
    //      if (en.GetFightItemKindHitCounter(fi.FightItemKind) > 0)
    //        lessHealthCounter++;
    //    }
    //    Assert.Greater(lessHealthCounter, 1);
    //    var expectedAmmoCount = initFiCount - lessHealthCounter;
    //    Assert.AreEqual(fi.Count, expectedAmmoCount);
    //  }
    //}

    
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
      var hero = PrepareHero(AbilityKind.PerfectHit);
      Enemy enemy = PrepareEnemy();

      var damageComparer1 = new OuadIIDamageComparer(enemy, this);

      var bow = GenerateEquipment<Weapon>("bow");
      Assert.True(SetHeroEquipment(bow));
      var fi = hero.ActiveProjectileFightItem;
      Assert.NotNull(fi);

      Assert.True(UseFightItem(hero, enemy, fi));
      damageComparer1.RegisterHealth();
      var damageComparer2 = new OuadIIDamageComparer(enemy, this);
      var ab = ActivateActiveAblityInHotBar(hero, AbilityKind.PerfectHit);
      MaximizeAbility(ab, hero);
      GotoNextHeroTurn();
      Assert.True(UseFightItem(hero, enemy, fi));
      damageComparer2.RegisterHealth();

      AssertHealthDiffPercentageInRange(damageComparer1, damageComparer2, 145, 210);
    }

    private Enemy PrepareEnemy(bool addImmunities = true)
    {
      var enemy = PlainEnemies.Where(i => i.Level <= 3).First();
      return PrepareEnemy(addImmunities, enemy);
    }

    public Enemy PrepareEnemy(bool addImmunities, Enemy enemy)
    {
      var hero = GameManager.OuadHero;
      enemy.Stats.Stats[EntityStatKind.Health].Value.Nominal = 300;
      PlaceCloseToHero(hero, enemy);
      if (addImmunities)
        enemy.AddImmunity(Roguelike.Effects.EffectType.Bleeding);

      if (!enemy.Name.Any())
        enemy.Name = "enemy";
      return enemy;
    }

    [Test]
    public void TestStrideWorks()
    {
      var game = CreateWorld();
      var hero = GameManager.OuadHero;
      string reason;
      for (int i = 0; i < 100; i++)
        Assert.False(hero.CanUseAbility(AbilityKind.Stride, GameManager.CurrentNode, out reason));

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

      ActivateActiveAblityInHotBar(hero, AbilityKind.Stride);

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
      var hero = PrepareHero(AbilityKind.Stride);
      var stride = PrepareAbility(hero, AbilityKind.Stride);
      Assert.AreEqual(stride.GetEntityStats(true).Count, 1);
      var en = PrepareEnemy();

      //hit enemy without stride
      Assert.AreEqual(hero.ShortcutsBar.ActiveItemDigit, -1);

      var enHealth = en.Stats.Health;
      GameManager.ApplyHeroPhysicalAttackPolicy(en, true);
      var healthDiff = enHealth - en.Stats.Health;
      Assert.Greater(healthDiff, 0);
      enHealth = en.Stats.Health;

      Assert.NotNull(ActivateActiveAblityInHotBar(hero, AbilityKind.Stride));

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

        WaitForAbilityCoolDown(stride);
      }
    }

    private void WaitForAbilityCoolDown(Ability ab, bool exactlyZero = true)
    {
      if (exactlyZero)
      {
        int i = 0;
        while (ab.CoolDownCounter > 0)
        {
          Assert.True(GameManager.Hero.Alive);
          GotoNextHeroTurn();
          i++;
          if (i > 30)
          {
            Assert.True(false);
          }
        }
        return;
      }
      var ct = ab.CoolDownCounter;
      for (int i = 0; i <= ct; i++)
        GotoNextHeroTurn();
    }

    private OuaDII.Tiles.LivingEntities.Hero PrepareHero(string weapon)
    {
      var hero = PrepareHero(AbilityKind.Unset);
      var wpn = GameManager.LootGenerator.GetLootByTileName<Weapon>("rusty_sword");
      SetHeroEquipment(wpn);
      return hero;
    }

    private OuaDII.Tiles.LivingEntities.Hero PrepareHero(AbilityKind ak)
    {
      var hero = GameManager.OuadHero;
      hero.Stats.Stats[EntityStatKind.Health].Value.Nominal = 1300;
      hero.AlwaysHit[AttackKind.Melee] = true;
      hero.UseAttackVariation = false;
      hero.AlwaysHit[AttackKind.PhysicalProjectile] = true;

      ProjectileFightItem fi = null;
      if (ak == AbilityKind.PerfectHit ||
         ak == AbilityKind.ArrowVolley ||
         ak == AbilityKind.PiercingArrow 
         )
      {
        fi = new ProjectileFightItem(FightItemKind.PlainArrow, hero);
      }

      if (ak == AbilityKind.ThrowingStone
         )
      {
        fi = new ProjectileFightItem(FightItemKind.Stone, hero);
      }
      else if (ak == AbilityKind.WeightedNet)
      {
        fi = new ProjectileFightItem(FightItemKind.WeightedNet, hero);
      }
      else if (ak == AbilityKind.Cannon)
      {
        fi = new ProjectileFightItem(FightItemKind.CannonBall, hero);
      }
      else if (ak == AbilityKind.Smoke)
      {
        fi = new ProjectileFightItem(FightItemKind.Smoke, hero);
      }
      else if (ak == AbilityKind.IronSkin)
      {
      }
      else if (ak == AbilityKind.ElementalVengeance)
      {
        var wpn = GameManager.LootGenerator.GetLootByTileName<Weapon>("rusty_sword");
        SetHeroEquipment(wpn);
      }
      else if (ak == AbilityKind.ZealAttack)
      {
      }
      else if (ak == AbilityKind.Stride)
      {
      }

      if (fi != null)
      {
        fi.Count = 10;
        hero.Inventory.Add(fi);
        hero.ActiveFightItem = fi;
        if (ak == AbilityKind.Cannon)
        {
          hero.Inventory.Add(new Cannon());
        }
      }

      return hero;
    }

    [Test]
    [Repeat(1)]
    public void TestSmoke()
    {
      var game = CreateWorld();
      var hero = PrepareHero(AbilityKind.Smoke);
      var fi = hero.ActiveProjectileFightItem;
      Assert.NotNull(fi);
      Assert.AreEqual(fi.FightItemKind, FightItemKind.Smoke);

      var en = PrepareEnemy();

      var ab = ActivateActiveAblityInHotBar(hero, AbilityKind.Smoke) as ActiveAbility;
      Assert.NotNull(ab);
      Assert.True(GameManager.EnemiesManager.ShallChaseTarget(en, hero));

      GameManager.UseActiveAbility(ab, hero, true);
      //GameManager.CurrentNode.SetTile(fi, en.point);
      Assert.False(GameManager.EnemiesManager.ShallChaseTarget(en, hero));
      var smokes = GameManager.CurrentNode.Layers.GetTypedLayerTiles<ProjectileFightItem>(KnownLayer.Smoke).ToList();
      Assert.Greater(smokes.Count, 0);
      //var emp = GameManager.CurrentNode.GetEmptyNeighborhoodPoint(en);

      WaitForAbilityCoolDown(ab);
      //GameManager.CurrentNode.SetTile(en, emp.Item1);
      Assert.True(GameManager.EnemiesManager.ShallChaseTarget(en, hero));
      smokes = GameManager.CurrentNode.Layers.GetTypedLayerTiles<ProjectileFightItem>(KnownLayer.Smoke).ToList();
      Assert.AreEqual(smokes.Count, 0);
    }

    [Test]
    public void TestSmokeScope()
    {
      var game = CreateWorld();
      var hero = PrepareHero(AbilityKind.Smoke);
      var fi = hero.ActiveProjectileFightItem;
      Assert.AreEqual(fi.FightItemKind, FightItemKind.Smoke);
     
      var ab = ActivateActiveAblityInHotBar(hero, AbilityKind.Smoke) as ActiveAbility;
      Assert.NotNull(ab);
      Assert.AreEqual(ab.Level, 1);
      Assert.AreEqual(ab.PrimaryStat.Factor, 1);//scope
      var dur = ab.AuxStat.Factor;

      GameManager.UseActiveAbility(ab, hero, true);
      var smokes = GameManager.CurrentNode.Layers.GetTypedLayerTiles<ProjectileFightItem>(KnownLayer.Smoke).ToList();
      Assert.Greater(smokes.Count, 0);
      Assert.Less(smokes.Count, 11);
      
      WaitForAbilityCoolDown(ab);
      hero.IncreaseAbility(AbilityKind.Smoke);
      //GotoNextHeroTurn();
      GameManager.UseActiveAbility(ab, hero, true);

      Assert.AreEqual(ab.PrimaryStat.Factor, 2);//scope
      Assert.Greater(ab.AuxStat.Factor, dur);
      smokes = GameManager.CurrentNode.Layers.GetTypedLayerTiles<ProjectileFightItem>(KnownLayer.Smoke).ToList();
      Assert.Greater(smokes.Count, 11);
      Assert.Less(smokes.Count, 31);
    }

    [Test]
    public void TestIronSkin()
    {
      var game = CreateWorld();
      var hero = PrepareHero(AbilityKind.IronSkin);
      PrepareAbility(hero, AbilityKind.IronSkin, 5);
     
      //bool usedAb = false;
      //GameManager.EventsManager.ActionAppended += (object sender, Roguelike.Events.GameEvent e) =>
      //{
      //  if (e is LivingEntityAction lea && lea.Kind == LivingEntityActionKind.UsedAbility)
      //  {
      //    Debug.WriteLine(lea.Info);
      //    usedAb = true;
      //  }
      //};

      var defenseBefore = hero.Stats.GetCurrentValue(EntityStatKind.Defense);
      var ab = ActivateActiveAblityInHotBar(hero, AbilityKind.IronSkin);
      Assert.AreEqual(ab.PrimaryStat.Kind, EntityStatKind.Defense);
      Assert.AreEqual(ab.PrimaryStat.Unit, EntityStatUnit.Percentage);

      Assert.AreEqual(ab.AuxStat.Kind, EntityStatKind.IronSkinDuration);
      Assert.AreEqual(ab.AuxStat.Unit, EntityStatUnit.Absolute);

      Assert.Greater(ab.MaxCollDownCounter, ab.AuxStat.Factor);

      if (!ab.AutoApply)
        GameManager.UseActiveAbility(ab, hero, true);

      var defenseAfter = hero.Stats.GetCurrentValue(EntityStatKind.Defense);
      Assert.Greater(defenseAfter, defenseBefore);
      Assert.Less(defenseAfter, defenseBefore * 2.1);
      var effect = hero.GetFirstLastingEffect(Roguelike.Effects.EffectType.IronSkin);

      Assert.Greater(hero.Stats.GetCurrentValue(EntityStatKind.Defense), defenseBefore);
      hero.ActiveShortcutsBarItemDigit = -1;

      WaitForAbilityCoolDown(ab, false);

      Assert.Null(hero.GetFirstLastingEffect(Roguelike.Effects.EffectType.IronSkin));
      Assert.AreEqual(defenseBefore, hero.Stats.GetCurrentValue(EntityStatKind.Defense));
    }

    [Test]
    [Repeat(1)]
    public void TestElementalVengeanceIncrease()
    {
      var game = CreateWorld();
      var hero = PrepareHero(AbilityKind.ElementalVengeance);
      hero.ImmuneOnEffects = true;

      var ab = PrepareActiveAbility(hero, AbilityKind.ElementalVengeance);
      var en = PlainEnemies.First();
      PrepareEnemy(true, en);
      var en2 = PlainEnemies.ElementAt(1);
      PrepareEnemy(true, en2);

      GameManager.EventsManager.EventAppended += (object sender, Roguelike.Events.GameEvent e) =>
      {
        if (e is LivingEntityAction lea)
        {
          if (lea.Kind == LivingEntityActionKind.GainedDamage)
          {
            //Console.WriteLine(lea.InvolvedEntity+ " GainedDamage " + );
            //Debug.WriteLine(lea.Info);
          }
          else if (lea.Kind == LivingEntityActionKind.UsedAbility)
          {
            //Debug.WriteLine(lea.Info);
          }
        }
      };

      var adBefore = new AttackDescription(hero, hero.UseAttackVariation, AttackKind.Melee);
      Assert.AreEqual(hero.ShortcutsBar.ActiveItemDigit, -1);
      var damageComparer1 = new OuaDDamageComparer(en, this);
      //hit enemy without ab
      GameManager.ApplyHeroPhysicalAttackPolicy(en, true);
      damageComparer1.RegisterHealth();
      GotoNextHeroTurn();

      Assert.Null(hero.GetFirstLastingEffect(Roguelike.Effects.EffectType.FireAttack));
      Assert.NotNull(ActivateActiveAblityInHotBar(hero, AbilityKind.ElementalVengeance));
      if (!ab.AutoApply)
        Assert.True(GameManager.UseActiveAbility(ab, hero, true));
      if (ab.TurnsIntoLastingEffect)
        Assert.NotNull(hero.GetFirstLastingEffect(Roguelike.Effects.EffectType.FireAttack));

      Assert.Greater(ab.CoolDownCounter, 0);
      Assert.GreaterOrEqual(hero.LastingEffects.Count, 3);//fire_attack, poison_attack...
      Assert.Greater(hero.Stats.GetCurrentValue(EntityStatKind.FireAttack), 0);
      Assert.Greater(hero.Stats.GetCurrentValue(EntityStatKind.PoisonAttack), 0);
      Assert.Greater(hero.Stats.GetCurrentValue(EntityStatKind.ColdAttack), 0);

      var turns = 2;
      Assert.AreEqual(hero.LastingEffects[0].PendingTurns, turns);
      Assert.AreEqual(hero.LastingEffects[1].PendingTurns, turns);
      Assert.AreEqual(hero.LastingEffects[2].PendingTurns, turns);

      var adAfter = new AttackDescription(hero, hero.UseAttackVariation, AttackKind.Melee);
      Assert.Greater(adAfter.CurrentTotal, adBefore.CurrentTotal);
      //hit enemy with ab
      int okCount = 0;
      float dam = hero.Stats.GetCurrentValue(EntityStatKind.PoisonAttack);
      for (int level = 0; level < ab.MaxLevel - 1; level++)
      {
        //GotoNextHeroTurn();
        WriteLine("ApplyHeroPhysicalAttackPolicy ... level: " + level);
        var damageComparer2 = new OuaDDamageComparer(en, this);

        GameManager.ApplyHeroPhysicalAttackPolicy(en, true);

        damageComparer2.RegisterHealth();
        if (damageComparer2.HealthDifference > damageComparer1.HealthDifference)
          okCount++;
        //inc ab
        Assert.True(ab.IncreaseLevel(hero));

        WaitForAbilityCoolDown(ab, false);

        GameManager.UseActiveAbility(ab, hero, true);
        //var ab = ActivateActiveAblityInHotBar(hero, AbilityKind.PerfectHit);
        //GameManager.UseA
        //usedAb = false;
        //int counter = 0;
        //while (!usedAb)
        //{
        //  counter++;
        //  GotoNextHeroTurn();
        //  if (!usedAb)
        //  {
        //    var fa = hero.Stats.GetCurrentValue(EntityStatKind.FireAttack);
        //    Assert.AreEqual(hero.Stats.GetCurrentValue(EntityStatKind.FireAttack), 0);
        //    Assert.AreEqual(hero.Stats.GetCurrentValue(EntityStatKind.PoisonAttack), 0);
        //    Assert.AreEqual(hero.Stats.GetCurrentValue(EntityStatKind.ColdAttack), 0);
        //  }
        //  if (counter > 50)
        //    Assert.False(true);
        //}

        damageComparer1 = damageComparer2;
        var dam1 = hero.Stats.GetCurrentValue(EntityStatKind.PoisonAttack);
        Assert.Greater(dam1, dam);
        dam = dam1;
        Assert.True(hero.HasLastingEffect(Roguelike.Effects.EffectType.FireAttack));
      }

      Assert.Greater(okCount, ab.MaxLevel / 2);
    }

    [Test]
    [Repeat(1)]
    public void TestZealAttack()
    {
      var game = CreateWorld();
      var abKind = AbilityKind.ZealAttack;
      var hero = PrepareHero(abKind);
      var ab = PrepareAbility(hero, abKind);
      Assert.AreEqual(ab.PrimaryStat.Kind, EntityStatKind.ZealAttackVictimsCount);
      Assert.AreEqual(ab.PrimaryStat.Unit, EntityStatUnit.Absolute);
      var enFirst = PrepareEnemy();

      //change name of easy debug
      var enSec = PlainEnemies.ElementAt(1);
      EnsureUniqNames(enFirst, enSec);
      PrepareEnemy(true, enSec);

      Assert.NotNull(ActivateActiveAblityInHotBar(hero, abKind));
      float en1Health = enFirst.Stats.Health;
      float en2Health = enSec.Stats.Health;

      for (int i = 0; i < 25; i++)
      {
        //hit only 1st enemy
        GameManager.InteractHeroWith(enFirst);
        GotoNextHeroTurn();
        if (en2Health > enSec.Stats.Health)
          break;
      }

      Assert.Greater(en1Health, enFirst.Stats.Health);

      //2nd shall be hit by an ability
      Assert.Greater(en2Health, enSec.Stats.Health);
      //Debug.WriteLine("TestBulkAttackReal end");
    }

   

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [Repeat(1)]
    public void TestCannon(int ballsCount)
    {
      var info = new OuaDII.Generators.GenerationInfo();
      info.MakeEmpty();
      info.GenerateEnemies = true;
      var game = CreateWorld(info);
      var walls = GameManager.CurrentNode.GetTiles<Wall>();
      var inters = GameManager.CurrentNode.GetTiles<Roguelike.Tiles.Interactive.InteractiveTile>();

      var abKind = AbilityKind.Cannon;
      var hero = PrepareHero(abKind);
      var ab = PrepareAbility(hero, abKind, 4);

      var originalBallsCount = ballsCount;
      if (ballsCount > 2)
        ballsCount = 2;//HACK

      //enemies same Y
      var constraints = new GenerationConstraints(GameManager.Hero.point.MoveBy(-5, -1), GameManager.Hero.point.MoveBy(5, 1));
      var tiles = GetEmptiesForCannonTargets(constraints);
      var ens = AppendCannonEnemies(tiles, "stone_golem", false, ballsCount);

      if (originalBallsCount > 2)
      {
        //enemies same X
        if (originalBallsCount == 3)
          ballsCount = 1;
        constraints = new GenerationConstraints(GameManager.Hero.point.MoveBy(-1, -5), GameManager.Hero.point.MoveBy(1, 5));
        tiles = GetEmptiesForCannonTargets(constraints);
        ens.AddRange(AppendCannonEnemies(tiles, "stone_golem", false, ballsCount));
      }
      Assert.AreEqual(ens.Count, originalBallsCount);
      var neibs = GameManager.EnemiesManager.GetInRange(GameManager.Hero, 8, null);
      foreach (var en in ens)
        Assert.True(neibs.Contains(en));

      var activeAbility = ActivateActiveAblityInHotBar(hero, abKind);
      Assert.AreEqual(hero.SelectedActiveAbility, activeAbility);
      var fi = hero.ActiveProjectileFightItem;

      var enHealths = new List<float>();
      foreach (var en in ens)
      {
        enHealths.Add(en.Stats.Health);
      }
      Assert.True(UseFightItem(hero, ens[0], fi));//inside AbilityKind was not Unset !
      for (int i = 0; i < ens.Count; i++)
      {
        Assert.Less(ens[i].Stats.Health, enHealths[i]);
      }

      //var cannonPH = tiles.OrderBy(i => i.DistanceFrom(hero)).First();
    }

    private List<Tile> GetEmptiesForCannonTargets(GenerationConstraints constraints)
    {
      return GameManager.CurrentNode.GetEmptyTiles(constraints).Where(i => i.DistanceFrom(GameManager.Hero) > 1).ToList();
    }

    private List<Enemy> AppendCannonEnemies(List<Dungeons.Tiles.Tile> points, string tag1, bool strong, int count)
    {
      var ens = new List<Enemy>();
      for (int i = 0; i < count; i++)
      {
        var empt = i == 0 ? points.First() : points.Last();
        points.Remove(empt);
        var en = GameManager.CurrentNode.SpawnEnemy(1);
        var tag = tag1;
        if (tag1 == "bandit")
        {
          tag += i == 0 ? "1" : "3";
        }
        en.tag1 = tag;
        en.SetNameFromTag1();
        en.DisplayedName = en.Name;
        GameManager.AppendEnemy(en, empt.point, strong ? 5 : 1);
        ens.Add(en);

      }

      return ens;
    }

    [Test]
    [Repeat(1)]
    public void TestStone()
    {
      var game = CreateWorld();
      var hero = PrepareHero(AbilityKind.ThrowingStone);
      Enemy enemy = PrepareEnemy();

      var damageComparer1 = new OuadIIDamageComparer(enemy, this);
      var fi = new ProjectileFightItem(FightItemKind.Stone, hero) { Count = 10 };
      hero.Inventory.Add(fi);
      Assert.True(UseFightItem(hero, enemy, fi));
      damageComparer1.RegisterHealth();
      var damageComparer2 = new OuadIIDamageComparer(enemy, this);
      var ab = ActivateActiveAblityInHotBar(hero, AbilityKind.ThrowingStone);
      MaximizeAbility(ab, hero);
      GotoNextHeroTurn();
      Assert.True(UseFightItem(hero, enemy, fi));
      damageComparer2.RegisterHealth();

      AssertHealthDiffPercentageInRange(damageComparer1, damageComparer2, 130, 210);
    }
  }
}
