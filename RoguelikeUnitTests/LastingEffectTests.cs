using NUnit.Framework;
using Roguelike;
using Roguelike.Attributes;
using Roguelike.Effects;
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
      LastingEffect le1;
      CreateEffect(et, esk, out statValueBefore, out le1);

      var desc = le1.Description;

      var eff = Math.Round(le1.CalcInfo.PercentageFactor.Value * statValueBefore / 100, 2);
      var sep = ", ";
      if (et == EffectType.ResistAll)
      {
        eff = le1.CalcInfo.EffectiveFactor.Value;
        sep = " ";
      }
      var expectedDesc = le1.Type.ToDescription() + sep + sign + eff;
      if (et != EffectType.ResistAll)
        expectedDesc += " to " + le1.StatKind.ToDescription();
      Assert.AreEqual(desc, expectedDesc);
    }

    private void CreateEffect(EffectType et, EntityStatKind esk, out float statValueBefore, out LastingEffect le1)
    {
      Assert.AreNotEqual(et, EffectType.Unset);

      statValueBefore = game.Hero.Stats.GetStat(esk).Value.TotalValue;
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
    }

    void CheckAction(EffectType et, EntityStatKind esk, char sign, LivingEntity target)
    {
      float statValueBefore;
      LastingEffect le1;
      CreateEffect(et, esk, out statValueBefore, out le1);

      var expected = "";
      var ownerName = (le1.Owner as LivingEntity).Name;

      var origin = le1.CalcInfo.Origin;

      if (origin == EffectOrigin.SelfCasted)
      {
        expected = ownerName;
        expected += " casted: ";
      }
      else if (origin == EffectOrigin.Experieced)
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
      var en = game.GameManager.EnemiesManager.Enemies.First();
      var le = game.Hero.LastingEffectsSet.AddBleeding(10, en);
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

      var en = game.GameManager.EnemiesManager.Enemies.First();
      var le = game.Hero.LastingEffectsSet.AddBleeding(10, en);
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
  }
}
