using NUnit.Framework;
using Roguelike;
using Roguelike.Attributes;
using Roguelike.Effects;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class LastingEffectTests : TestBase
  {
    void CheckDesc(EffectType et, EntityStatKind esk, char sign)
    {
      Assert.AreNotEqual(et, EffectType.Unset);
      var statValue = game.Hero.Stats[esk].TotalValue;
      var le1 = game.Hero.AddLastingEffect(et, 3, esk, 10);
      Assert.AreEqual(le1.Type, et);

      var desc = le1.GetDescription(game.Hero);
      var expectedDesc = le1.Type.ToDescription() + ", " + sign + statValue / 10 + " to " + le1.StatKind.ToDescription();
      Assert.AreEqual(desc, expectedDesc);
    }

    [Test]
    public void TestLastingEffectDescription()
    {
      var game = CreateGame();
      var calcEffectValue = new LastingEffectCalcInfo(EffectType.Bleeding, 3, new LastingEffectFactor(10));
      var le = game.Hero.AddLastingEffect(calcEffectValue);
      Assert.NotNull(le);

      var expectedDesc = le.Type.ToDescription() + ", -" + le.EffectAbsoluteValue.Factor + " Health (per turn)";///game.Hero.Stats.Defense
      Assert.AreEqual(le.GetDescription(game.Hero), expectedDesc);

      CheckDesc(EffectType.IronSkin, EntityStatKind.Defense, '+');
      CheckDesc(EffectType.Rage, EntityStatKind.Attack, '+');
      CheckDesc(EffectType.Inaccuracy, EntityStatKind.ChanceToHit, '-');
      CheckDesc(EffectType.Weaken, EntityStatKind.Defense, '-');

      le = game.Hero.AddLastingEffect(EffectType.ResistAll, 3, EntityStatKind.Unset, 10);
      Assert.NotNull(le);
      expectedDesc = le.Type.ToDescription() + " +" + le.EffectAbsoluteValue.Factor;
      Assert.AreEqual(le.GetDescription(game.Hero), expectedDesc);
    }
  }
}
