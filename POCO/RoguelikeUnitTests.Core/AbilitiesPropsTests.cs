using NUnit.Framework;
using Roguelike.Abilities;
using Roguelike.Attributes;
using System.Linq;

namespace RoguelikeUnitTests
{
  class AbilitiesPropsTests : TestBase
  {
    const int MaxAbilityInc = 5;

    //[Test]
    //public void SkeletonMasteringPropsTest()
    //{
    //  var game = CreateGame();
    //  var hero = game.Hero;

    //  var ab = game.GameManager.Hero.GetPassiveAbility(AbilityKind.SkeletonMastering);
    //  Assert.AreEqual(ab.PrimaryStat.Kind, EntityStatKind.PrimaryAttributes);
    //  Assert.AreEqual(ab.PrimaryStat.Unit, EntityStatUnit.Percentage);

    //  Assert.AreEqual(ab.AuxStat.Kind, EntityStatKind.MaxSkeletonsCount);
    //  Assert.AreEqual(ab.AuxStat.Unit, EntityStatUnit.Absolute);

    //  Assert.AreEqual(ab.PrimaryStat.Factor, 0);
    //  Assert.AreEqual(ab.AuxStat.Factor, 0);

    //  ab.IncreaseLevel(game.Hero);
    //  Assert.Greater(ab.PrimaryStat.Factor, 0);
    //  Assert.AreEqual(ab.AuxStat.Factor, 1);//2 skeletons
    //}

    [Test]
    public void OpenWoundPropsTest()
    {
      var game = CreateGame();
      var hero = game.GameManager.Hero;
      var ab = game.GameManager.Hero.GetActiveAbility(AbilityKind.OpenWound);

      Assert.AreEqual(ab.AuxStat.Kind, EntityStatKind.BleedingDuration);
      Assert.AreEqual(ab.AuxStat.Unit, EntityStatUnit.Absolute);
      Assert.AreEqual(ab.PrimaryStat.Kind, EntityStatKind.BleedingExtraDamage);
      Assert.AreEqual(ab.PrimaryStat.Unit, EntityStatUnit.Percentage);
      Assert.AreEqual(ab.PrimaryStat.Factor, 0);

      //var stats = ab.GetEntityStats(false);
      //Assert.AreEqual(stats[0].Kind, EntityStatKind.BleedingDuration);
      //Assert.AreEqual(stats[0].Unit, EntityStatUnit.Absolute);

      ab.IncreaseLevel(game.Hero);
      Assert.AreEqual(ab.AuxStat.Factor, 3);
      Assert.AreEqual(ab.PrimaryStat.Factor, 5);

      ab.IncreaseLevel(game.Hero);
      Assert.AreEqual(ab.AuxStat.Factor, 3);
      Assert.AreEqual(ab.PrimaryStat.Factor, 10);

      ab.IncreaseLevel(game.Hero);
      Assert.AreEqual(ab.AuxStat.Factor, 4);
      Assert.AreEqual(ab.PrimaryStat.Factor, 15);
    }

    [Test]
    public void ArrowVolleyPropsTest()
    {
      var game = CreateGame();
      var hero = game.GameManager.Hero;
      var ab = game.GameManager.Hero.GetActiveAbility(AbilityKind.ArrowVolley);

      Assert.AreEqual(ab.PrimaryStat.Unit, EntityStatUnit.Absolute);
      Assert.AreEqual(ab.PrimaryStat.Factor, 0);

      ab.IncreaseLevel(game.Hero);
      Assert.AreEqual(ab.PrimaryStat.Factor, 2);

      ab.IncreaseLevel(game.Hero);
      Assert.AreEqual(ab.PrimaryStat.Factor, 3);
    }

    [TestCase(AbilityKind.PiercingArrow)]
    public void PiercingArrowPropsTest(AbilityKind kind)
    {
      var game = CreateGame();
      var hero = game.GameManager.Hero;
      var ab = game.GameManager.Hero.GetActiveAbility(kind);

      //FindBigFacs(game, true);
      //FindBigFacs(game, false);

      Assert.AreEqual(ab.PrimaryStat.Unit, EntityStatUnit.Percentage);
      Assert.AreEqual(ab.PrimaryStat.UseSign, true);

      Assert.AreEqual(ab.AuxStat.Unit, EntityStatUnit.Absolute);
      Assert.AreEqual(ab.AuxStat.UseSign, true);

      Assert.AreEqual(ab.PrimaryStat.Factor, 0);//EntityStatKind.ChanceForPiercing
      //Assert.AreEqual(game.GameManager.Hero.Stats[EntityStatKind.ChanceForPiercing].CurrentValue, 75);
      Assert.AreEqual(game.GameManager.Hero.GetEffectChance(EntityStatKind.ChanceToPhysicalProjectileHit, null), 75);
      game.GameManager.Hero.SelectedActiveAbility = ab;

      var nextLevelStats = ab.GetEntityStats(currentLevel: false);
      Assert.AreEqual(nextLevelStats[0].Factor, 5);//EntityStatKind.ChanceForPiercing
      Assert.AreEqual(nextLevelStats[1].Factor, 2);//EntityStatKind.NumberOfPiercedVictims

      var fv = EntityStat.GetFormattedValue(nextLevelStats[0], true);
      Assert.AreEqual(fv, "+5%");
      ab.IncreaseLevel(game.Hero);

      Assert.AreEqual(ab.PrimaryStat.Kind, EntityStatKind.ChanceForPiercing);
      Assert.AreEqual(ab.PrimaryStat.Factor, 5);
      Assert.AreEqual(ab.AuxStat.Kind, EntityStatKind.NumberOfPiercedVictims);
      var numberOfPiercedVictims = 2;
      Assert.AreEqual(ab.AuxStat.Factor, numberOfPiercedVictims);
      numberOfPiercedVictims++;

      ab.IncreaseLevel(game.Hero);
      Assert.AreEqual(ab.PrimaryStat.Factor, 10);

      Assert.AreEqual(ab.AuxStat.Factor, numberOfPiercedVictims);
      Assert.AreEqual(game.GameManager.Hero.GetEffectChance(EntityStatKind.ChanceToPhysicalProjectileHit, null), 85);
    }

    private static void FindBigFacs(Roguelike.RoguelikeGame game, bool act)
    {
      var abs = act ? game.GameManager.Hero.Abilities.ActiveItems.Cast<Ability>() : game.GameManager.Hero.Abilities.PassiveItems.Cast<Ability>();
      foreach (var ab1 in abs)
      {
        var nextLevelStats1 = ab1.GetEntityStats(currentLevel: false);
        foreach (var es in nextLevelStats1)
        {
          if (es.Factor > 50)
          {
            int k = 0;
            k++;
          }
        }
      }
    }

    [TestCase(AbilityKind.FireBallMastering)]
    [TestCase(AbilityKind.IceBallMastering)]
    [TestCase(AbilityKind.PoisonBallMastering)]
    //[TestCase(AbilityKind.SkeletonMastering)]
    public void MagicProjectilePropTest(AbilityKind ak)
    {
      var game = CreateGame();
      var ab = game.GameManager.Hero.GetPassiveAbility(ak);
      if (ak == AbilityKind.FireBallMastering)
        Assert.AreEqual(ab.PrimaryStat.Kind, EntityStatKind.FireBallExtraDamage);
      else if (ak == AbilityKind.PoisonBallMastering)
        Assert.AreEqual(ab.PrimaryStat.Kind, EntityStatKind.PoisonBallExtraDamage);
      else if (ak == AbilityKind.IceBallMastering)
        Assert.AreEqual(ab.PrimaryStat.Kind, EntityStatKind.IceBallExtraDamage);
      //else if (ak == AbilityKind.SkeletonMastering)
      //  Assert.AreEqual(ab.PrimaryStat.Kind, EntityStatKind.MeleeAttack);

      Assert.AreEqual(ab.PrimaryStat.Unit, EntityStatUnit.Percentage);
      Assert.AreEqual(ab.PrimaryStat.Factor, 0);

      for (int i = 0; i < MaxAbilityInc; i++)
        ab.IncreaseLevel(game.Hero);

      Assert.Greater(ab.PrimaryStat.Factor, 10);
      Assert.Less(ab.PrimaryStat.Factor, 60);
    }


    [Test]
    public void PerfectHitPropsTest()
    {
      var game = CreateGame();
      var hero = game.GameManager.Hero;
      var ab = game.GameManager.Hero.GetActiveAbility(AbilityKind.PerfectHit);

      Assert.AreEqual(ab.PrimaryStat.Kind, EntityStatKind.PerfectHitDamage);
      Assert.AreEqual(ab.PrimaryStat.Unit, EntityStatUnit.Percentage);

      Assert.AreEqual(ab.AuxStat.Kind, EntityStatKind.PerfectHitChanceToHit);
      Assert.AreEqual(ab.AuxStat.Unit, EntityStatUnit.Percentage);

      Assert.AreEqual(ab.PrimaryStat.Factor, 0);
      Assert.AreEqual(ab.AuxStat.Factor, 0);

      ab.IncreaseLevel(game.Hero);
      Assert.AreEqual(ab.PrimaryStat.Factor, 10);
      Assert.AreEqual(ab.AuxStat.Factor, 5);
    }

    
    [Test]
    public void ThrowingTorchChanceToCauseFiringPropsTest()
    {
      var game = CreateGame();
      var hero = game.GameManager.Hero;
      var ab = hero.GetActiveAbility(AbilityKind.ThrowingTorch);
      Assert.AreEqual(ab.PrimaryStat.Kind, EntityStatKind.ThrowingTorchChanceToCauseFiring);
      Assert.AreEqual(ab.PrimaryStat.Unit, EntityStatUnit.Percentage);
      Assert.AreEqual(ab.PrimaryStat.Factor, 0);//extra chance is 0
      var stat = hero.Stats.GetStat(EntityStatKind.ThrowingTorchChanceToCauseFiring);
      Assert.Greater(stat.Value.CurrentValue, 0);
    }

      [Test]
    public void WeightedNetPropsTest()
    {
      var game = CreateGame();
      var hero = game.GameManager.Hero;
      var ab = game.GameManager.Hero.GetActiveAbility(AbilityKind.WeightedNet);

      Assert.AreEqual(ab.PrimaryStat.Kind, EntityStatKind.WeightedNetDuration);
      Assert.AreEqual(ab.PrimaryStat.Unit, EntityStatUnit.Absolute);

      Assert.AreEqual(ab.AuxStat.Kind, EntityStatKind.WeightedNetExtraRange);
      Assert.AreEqual(ab.AuxStat.Unit, EntityStatUnit.Absolute);

      Assert.AreEqual(ab.PrimaryStat.Factor, 0);
      Assert.AreEqual(ab.AuxStat.Factor, 0);

      ab.IncreaseLevel(game.Hero);

      Assert.AreEqual(ab.PrimaryStat.Factor, 1);
      Assert.AreEqual(ab.AuxStat.Factor, 1);
    }

    [Test]
    public void SmokePropsTest()
    {
      var game = CreateGame();
      var hero = game.GameManager.Hero;
      var ab = game.GameManager.Hero.GetActiveAbility(AbilityKind.Smoke);

      Assert.AreEqual(ab.PrimaryStat.Kind, EntityStatKind.SmokeScope);
      Assert.AreEqual(ab.PrimaryStat.Unit, EntityStatUnit.Absolute);
      Assert.AreEqual(ab.PrimaryStat.UseSign, true);

      Assert.AreEqual(ab.AuxStat.Kind, EntityStatKind.SmokeDuration);
      Assert.AreEqual(ab.PrimaryStat.Unit, EntityStatUnit.Absolute);
      Assert.AreEqual(ab.PrimaryStat.UseSign, true);
            
      Assert.AreEqual(ab.PrimaryStat.Factor, 0);
      Assert.AreEqual(ab.AuxStat.Factor, 0);

      ab.IncreaseLevel(game.Hero);
           
      Assert.AreEqual(ab.PrimaryStat.Factor, 1);
      Assert.AreEqual(ab.AuxStat.Factor, 2);

      var pv = EntityStat.GetFormattedValue(ab.PrimaryStat, true);
      Assert.AreEqual(pv, "+1");

      var av = EntityStat.GetFormattedValue(ab.AuxStat, true);
      Assert.AreEqual(av, "+2");

    }

    [Test]
    public void RagePropsTest()
    {
      var game = CreateGame();
      var hero = game.GameManager.Hero;
      var ab = game.GameManager.Hero.GetActiveAbility(AbilityKind.Rage);

      Assert.AreEqual(ab.PrimaryStat.Kind, EntityStatKind.MeleeAttack);
      Assert.AreEqual(ab.PrimaryStat.Unit, EntityStatUnit.Percentage);

      Assert.AreEqual(ab.PrimaryStat.Factor, 0);

      ab.IncreaseLevel(game.Hero);
      Assert.AreEqual(ab.PrimaryStat.Factor, 15);
    }

    [TestCase(AbilityKind.StaffsMastering, EntityStatKind.StaffExtraRange)]
    [TestCase(AbilityKind.SceptersMastering, EntityStatKind.ScepterExtraRange)]
    [TestCase(AbilityKind.WandsMastering, EntityStatKind.WandExtraRange)]
    public void ElementalProjectilePropTest(AbilityKind ak, EntityStatKind esk)
    {
      var game = CreateGame();
      var ab = game.GameManager.Hero.GetPassiveAbility(ak);
      var range = ab.GetExtraRangeStat();
      Assert.AreEqual(range.Kind, esk);
      Assert.AreEqual(range.Unit, EntityStatUnit.Absolute);
      Assert.AreEqual(range.Value.TotalValue, 0);

      var prevVal = range.Value.TotalValue;
      for (int ind = 0; ind < 10; ind++)
      {
        ab.IncreaseLevel(game.Hero);
        if (ind % 2 == 0)
          Assert.Greater(range.Value.TotalValue, prevVal);
        else
          Assert.AreEqual(range.Value.TotalValue, prevVal);

        prevVal = range.Value.TotalValue;
      }
      
    }

    [Test]
    public void BowPropTest()
    {
      var game = CreateGame();
      var ab = game.GameManager.Hero.GetPassiveAbility(AbilityKind.BowsMastering);
      var range = ab.GetExtraRangeStat();
      Assert.AreEqual(range.Kind, EntityStatKind.BowsExtraRange);
      Assert.AreEqual(range.Unit, EntityStatUnit.Absolute);
      Assert.AreEqual(range.Value.TotalValue, 0);
      Assert.AreEqual(range.UseSign, true);
      var fv = EntityStat.GetFormattedValue(range, true);

      var nextLvl = ab.GetEntityStats(false);
      var nextRange = nextLvl.Where(i => i.IsExtraRange).Single();
      Assert.AreEqual(nextRange.Value.TotalValue, 1);
      //sert.AreEqual(nextRange.Value.TotalValue, 1);

      var prevVal = range.Value.TotalValue;
      for (int ind = 0; ind < 10; ind++)
      {
        ab.IncreaseLevel(game.Hero);
        if (ind % 2 == 0)
          Assert.Greater(range.Value.TotalValue, prevVal);
        else
          Assert.AreEqual(range.Value.TotalValue, prevVal);

        fv = EntityStat.GetFormattedValue(range, true);
        prevVal = range.Value.TotalValue;
      }

    }
  }
}
