using NUnit.Framework;
using Roguelike.Abilities;
using Roguelike.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoguelikeUnitTests
{
  class AbilitiesPropsTests : TestBase
  {
    const int MaxAbilityInc = 5;

    [Test]
    public void SkeletonMasteringPropsTest()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var ab = game.GameManager.Hero.GetPassiveAbility(AbilityKind.SkeletonMastering);
      Assert.AreEqual(ab.PrimaryStat.Kind, EntityStatKind.PrimaryAttributes);
      Assert.AreEqual(ab.PrimaryStat.Unit, EntityStatUnit.Percentage);

      Assert.AreEqual(ab.AuxStat.Kind, EntityStatKind.MaxSkeletonsCount);
      Assert.AreEqual(ab.AuxStat.Unit, EntityStatUnit.Absolute);

      Assert.AreEqual(ab.PrimaryStat.Factor, 0);
      Assert.AreEqual(ab.AuxStat.Factor, 0);

      ab.IncreaseLevel(game.Hero);
      Assert.Greater(ab.PrimaryStat.Factor, 0);
      Assert.AreEqual(ab.AuxStat.Factor, 2);//2 skeletons
    }

    [Test]
    public void OpenWoundPropsTest()
    {
      var game = CreateGame();
      var hero = game.GameManager.Hero;
      var ab = game.GameManager.Hero.GetActiveAbility(AbilityKind.OpenWound);

      Assert.AreEqual(ab.PrimaryStat.Kind, EntityStatKind.BleedingDuration);
      Assert.AreEqual(ab.PrimaryStat.Unit, EntityStatUnit.Absolute);
      Assert.AreEqual(ab.AuxStat.Kind, EntityStatKind.BleedingExtraDamage);
      Assert.AreEqual(ab.AuxStat.Unit, EntityStatUnit.Percentage);
      Assert.AreEqual(ab.PrimaryStat.Factor, 0);

      var nl = ab.GetEntityStats(false);
      Assert.AreEqual(nl[0].Kind, EntityStatKind.BleedingDuration);
      Assert.AreEqual(nl[0].Unit, EntityStatUnit.Absolute);

      ab.IncreaseLevel(game.Hero);
      Assert.AreEqual(ab.PrimaryStat.Factor, 3);
      Assert.AreEqual(ab.AuxStat.Factor, 5);

      ab.IncreaseLevel(game.Hero);
      Assert.AreEqual(ab.PrimaryStat.Factor, 3);
      Assert.AreEqual(ab.AuxStat.Factor, 10);

      ab.IncreaseLevel(game.Hero);
      Assert.AreEqual(ab.PrimaryStat.Factor, 4);
      Assert.AreEqual(ab.AuxStat.Factor, 15);
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

      Assert.AreEqual(ab.PrimaryStat.Unit, EntityStatUnit.Percentage);
      Assert.AreEqual(ab.AuxStat.Unit, EntityStatUnit.Absolute);

      Assert.AreEqual(ab.PrimaryStat.Factor, 0);

      ab.IncreaseLevel(game.Hero);
      Assert.AreEqual(ab.PrimaryStat.Factor, 80);
      Assert.AreEqual(ab.AuxStat.Factor, 1);


      ab.IncreaseLevel(game.Hero);
      Assert.AreEqual(ab.PrimaryStat.Factor, 85);

      Assert.AreEqual(ab.AuxStat.Factor, 2);

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
        Assert.AreEqual(ab.PrimaryStat.Kind, EntityStatKind.FireAttack);
      else if (ak == AbilityKind.PoisonBallMastering)
        Assert.AreEqual(ab.PrimaryStat.Kind, EntityStatKind.PoisonAttack);
      else if (ak == AbilityKind.IceBallMastering)
        Assert.AreEqual(ab.PrimaryStat.Kind, EntityStatKind.ColdAttack);
      else if (ak == AbilityKind.SkeletonMastering)
        Assert.AreEqual(ab.PrimaryStat.Kind, EntityStatKind.MeleeAttack);

      Assert.AreEqual(ab.PrimaryStat.Unit, EntityStatUnit.Percentage);
      Assert.AreEqual(ab.PrimaryStat.Factor, 0);

      for (int i = 0; i < MaxAbilityInc; i++)
        ab.IncreaseLevel(game.Hero);

      Assert.Greater(ab.PrimaryStat.Factor, 10);
      Assert.Less(ab.PrimaryStat.Factor, 50);
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
    public void WeightedNetPropsTest()
    {
      var game = CreateGame();
      var hero = game.GameManager.Hero;
      var ab = game.GameManager.Hero.GetActiveAbility(AbilityKind.WeightedNet);

      Assert.AreEqual(ab.PrimaryStat.Kind, EntityStatKind.WeightedNetDuration);
      Assert.AreEqual(ab.PrimaryStat.Unit, EntityStatUnit.Absolute);

      Assert.AreEqual(ab.AuxStat.Kind, EntityStatKind.WeightedNetRange);
      Assert.AreEqual(ab.AuxStat.Unit, EntityStatUnit.Absolute);

      Assert.AreEqual(ab.PrimaryStat.Factor, 0);
      Assert.AreEqual(ab.AuxStat.Factor, 0);

      ab.IncreaseLevel(game.Hero);

      Assert.AreEqual(ab.PrimaryStat.Factor, 1);
      Assert.AreEqual(ab.AuxStat.Factor, 1);
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
      Assert.AreEqual(ab.PrimaryStat.Factor, 25);
    }
  }
}
