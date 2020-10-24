using NUnit.Framework;
using Roguelike;
using Roguelike.Attributes;
using Roguelike.Effects;
using Roguelike.Events;
using Roguelike.Spells;
using Roguelike.Tiles;
using System;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class LastingEffectTests : TestBase
  {
    void CheckDesc(EffectType et, EntityStatKind esk, char sign)
    {
      float statValueBefore;
      LastingEffect le1 = CreateEffect(et, esk, out statValueBefore);

      var desc = le1.Description;

      var eff = Math.Round(le1.PercentageFactor.Value * statValueBefore / 100, 2);
      var sep = ", ";
      if (et == EffectType.ResistAll)
      {
        eff = le1.EffectiveFactor.Value;
        sep = " ";
      }
      var expectedDesc = le1.Type.ToDescription() + sep + sign + eff;
      if (et != EffectType.ResistAll)
        expectedDesc += " to " + le1.StatKind.ToDescription();
      Assert.AreEqual(desc, expectedDesc);
    }

    private LastingEffect CreateEffect(EffectType et, EntityStatKind esk, out float statTotalValueBefore)
    {
      LastingEffect le1 = null;
      Assert.AreNotEqual(et, EffectType.Unset);

      statTotalValueBefore = game.Hero.Stats.GetStat(esk).Value.TotalValue;
      le1 = null;
      var spellKind = SpellConverter.SpellKindFromEffectType(et);
      if (spellKind != SpellKind.Unset)
        le1 = game.Hero.LastingEffectsSet.AddLastingEffectFromSpell(spellKind, et);
      else
      {
        //var statValue = game.Hero.Stats[esk].TotalValue;
        //le1 = game.Hero.LastingEffectsSet.AddPercentageLastingEffect(et, 3, esk, 10);
        //Assert.AreEqual(le1.Type, et);
      }
      Assert.NotNull(le1);
      Assert.Greater(le1.PendingTurns, 0);

      return le1;
    }

    void CheckAction(EffectType et, EntityStatKind esk, char sign, LivingEntity target)
    {
      float statValueBefore;
      LastingEffect le1 = CreateEffect(et, esk, out statValueBefore);

      var expected = "";
      var ownerName = (le1.Owner as LivingEntity).Name;

      var origin = le1.Origin;

      if (origin == EffectOrigin.SelfCasted)
      {
        expected = ownerName;
        expected += " casted: ";
      }
      else if (origin == EffectOrigin.External)
      {
        expected = ownerName;
        expected += " experienced: ";
      }
      else if (origin == EffectOrigin.OtherCasted)
      {
        expected += "Spell was casted on " + ownerName + " ";
      }

      expected += le1.Description;
            

      var ac = le1.CreateAction(le1);
      //Assert.True(ac.Info.StartsWith(expected));
      Assert.AreEqual(ac.Info, expected);
    }

    [Test]
    public void TestLastingEffectAction()
    {
      var game = CreateGame();

      game.Hero.Name = "Edd";
      var en = ActiveEnemies.First();
      var le = game.Hero.LastingEffectsSet.EnsureEffect( EffectType.Bleeding, 10, en);
      Assert.NotNull(le);

      var expectedDesc = le.Type.ToDescription() + ", -10 Health (per turn)";
      var desc = le.Description;
      Assert.AreEqual(desc, expectedDesc);

      CheckAction(EffectType.IronSkin, EntityStatKind.Defense, '+', game.Hero);
      CheckAction(EffectType.Rage, EntityStatKind.Attack, '+', game.Hero);
      CheckAction(EffectType.Inaccuracy, EntityStatKind.ChanceToHit, '-', game.Hero);
      CheckAction(EffectType.Weaken, EntityStatKind.Defense, '-', game.Hero);
      CheckAction(EffectType.ResistAll, EntityStatKind.Unset, '+', game.Hero);
    }

    [Test]
    public void TestLastingEffectDescription()
    {
      var game = CreateGame();

      var en = ActiveEnemies.First();
      var le = game.Hero.LastingEffectsSet.EnsureEffect(EffectType.Bleeding, 10, en);
      Assert.NotNull(le);

      var expectedDesc = le.Type.ToDescription() + ", -10 Health (per turn)";///game.Hero.Stats.Defense
      var desc = le.Description;
      Assert.AreEqual(desc, expectedDesc);

      CheckDesc(EffectType.IronSkin, EntityStatKind.Defense, '+');
      CheckDesc(EffectType.Rage, EntityStatKind.Attack, '+');
      CheckDesc(EffectType.Inaccuracy, EntityStatKind.ChanceToHit, '-');
      CheckDesc(EffectType.Weaken, EntityStatKind.Defense, '-');
      CheckDesc(EffectType.ResistAll, EntityStatKind.Unset, '+');

      //le = game.Hero.LastingEffectsSet.AddLastingEffectFromSpell(EffectType.ResistAll, 3, EntityStatKind.Unset, 10);
      //Assert.NotNull(le);
      //expectedDesc = le.Type.ToDescription() + " +" + le.EffectiveFactor.EffectiveFactor;
      //Assert.AreEqual(le.Description, expectedDesc);

      //var calculated = new LastingEffectCalcInfo(EffectType.Bleeding, 3, new LastingEffectFactor(10));
      //le = game.Hero.AddLastingEffect(calculated);
      //Assert.NotNull(le);
      //expectedDesc = le.Type.ToDescription() + " -" + le.EffectAbsoluteValue.Factor;
      //Assert.AreEqual(le.GetDescription(game.Hero), expectedDesc);
    }

    [Test]
    public void TestIronSkinProlong()
    {
      var game = CreateGame();
      float statTotalValueBefore;

      int actionCounter = 0;

      game.GameManager.EventsManager.ActionAppended += (object sender, Roguelike.Events.GameAction e) =>
      {
        if (e is LivingEntityAction)
        {
          var lea = e as LivingEntityAction;
          if (lea.Kind == LivingEntityActionKind.ExperiencedEffect)
          {
            if (lea.Info.Contains(EffectType.IronSkin.ToDescription()))
              actionCounter++;
          }
        }
      };

      var le1 = CreateEffect(EffectType.IronSkin, EntityStatKind.Defense, out statTotalValueBefore);
      LastingEffect castedAgain = null;

      var value = game.Hero.Stats.GetStat(EntityStatKind.Defense).Value;
      Assert.Greater(value.CurrentValue, statTotalValueBefore);
      var turns = le1.PendingTurns;
      int appliedCounter = 0;
      for (int i = 0; i < turns*2; i++)//there will be prolong
      {
        game.GameManager.SkipHeroTurn();
        GotoNextHeroTurn();
        if (i == 2)
        {
          castedAgain = CreateEffect(EffectType.IronSkin, EntityStatKind.Defense, out statTotalValueBefore);
          Assert.AreEqual(castedAgain, le1);
        }
        if (value.CurrentValue > statTotalValueBefore)
          appliedCounter++;
      }
      Assert.AreEqual(value.CurrentValue, statTotalValueBefore);
      Assert.AreEqual(actionCounter, 1);
      Assert.AreEqual(appliedCounter, 7);
    }
  

    [Test]
    public void TestIronSkin()
    {
      var game = CreateGame();
      float statTotalValueBefore;
      
      int actionCounter = 0;

      game.GameManager.EventsManager.ActionAppended += (object sender, Roguelike.Events.GameAction e)=>
      {
        if (e is LivingEntityAction)
        {
          var lea = e as LivingEntityAction;
          if (lea.Kind == LivingEntityActionKind.ExperiencedEffect)
          {
            if(lea.Info.Contains(EffectType.IronSkin.ToDescription()))
              actionCounter++;
          }
        }
      };

      var le1 = CreateEffect(EffectType.IronSkin, EntityStatKind.Defense, out statTotalValueBefore);

      var value = game.Hero.Stats.GetStat(EntityStatKind.Defense).Value;
      Assert.Greater(value.CurrentValue, statTotalValueBefore);
      var turns = le1.PendingTurns;
      for (int i = 0; i < turns; i++)
      {
        game.GameManager.SkipHeroTurn();
        GotoNextHeroTurn();        
      }
      Assert.AreEqual(le1.PendingTurns, 0);
      Assert.AreEqual(value.CurrentValue, statTotalValueBefore);
      Assert.AreEqual(actionCounter, 1); 
    }

    [Test]
    public void TestBleeding()
    {
      var game = CreateGame();
      int actionCounter = 0;

      game.GameManager.EventsManager.ActionAppended += (object sender, Roguelike.Events.GameAction e) =>
      {
        if (e is LivingEntityAction)
        {
          var lea = e as LivingEntityAction;
          if (lea.Kind == LivingEntityActionKind.ExperiencedEffect)
          {
            if (lea.Info.Contains(EffectType.Bleeding.ToDescription()))
              actionCounter++;
          }
        }
      };

      var enemy = ActiveEnemies.First();
      var healthStat = enemy.Stats.GetStat(EntityStatKind.Health);
      healthStat.Value.Nominal = 150;
      enemy.SetIsWounded();//make sure will bleed

      enemy.OnPhysicalHit(game.Hero);
      var le1 = enemy.LastingEffects.Where(i => i.Type == EffectType.Bleeding).FirstOrDefault();
      Assert.NotNull(le1);
            
      var value = game.Hero.Stats.GetStat(EntityStatKind.Defense).Value;
      var turns = le1.PendingTurns;
      for (int i = 0; i < turns * 2; i++)//there will be prolong
      {
        game.GameManager.SkipHeroTurn();
        GotoNextHeroTurn();
      }
      Assert.AreEqual(actionCounter, 3);
    }

    [Test]
    public void TestBleedingProlong()
    {
      var game = CreateGame();
      int actionCounter = 0;

      game.GameManager.EventsManager.ActionAppended += (object sender, Roguelike.Events.GameAction e) =>
      {
        if (e is LivingEntityAction)
        {
          var lea = e as LivingEntityAction;
          if (lea.Kind == LivingEntityActionKind.ExperiencedEffect)
          {
            if (lea.Info.Contains(EffectType.Bleeding.ToDescription()))
              actionCounter++;
          }
        }
      };

      var enemy = ActiveEnemies.First();
      var healthStat = enemy.Stats.GetStat(EntityStatKind.Health);
      healthStat.Value.Nominal = 150;
      enemy.SetIsWounded();//make sure will bleed

      enemy.OnPhysicalHit(game.Hero);
      var le1 = enemy.LastingEffects.Where(i => i.Type == EffectType.Bleeding).SingleOrDefault();
      Assert.NotNull(le1);

      LastingEffect castedAgain = null;

      var value = game.Hero.Stats.GetStat(EntityStatKind.Defense).Value;
      var turns = le1.PendingTurns;
      for (int i = 0; i < turns * 2; i++)//there will be prolong
      {
        castedAgain = enemy.LastingEffects.Where(le => le.Type == EffectType.Bleeding).SingleOrDefault();
        Assert.AreEqual(castedAgain, le1);

        game.GameManager.SkipHeroTurn();
        GotoNextHeroTurn();
        if (i == 0)
        {
          enemy.OnPhysicalHit(game.Hero);
        }
        
      }
      Assert.AreEqual(actionCounter, 5);
    }


  }
}
