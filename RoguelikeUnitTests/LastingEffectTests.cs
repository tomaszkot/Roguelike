using NUnit.Framework;
using Roguelike;
using Roguelike.Attributes;
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
      var statValue = game.Hero.Stats[esk].TotalValue;
      var le1 = game.Hero.AddLastingEffect(et, 3, esk, 10);
      var desc = le1.GetDescription(game.Hero);
      var expectedDesc = le1.Type.ToDescription() + ", " + sign + statValue / 10 + " to " + le1.StatKind.ToDescription();
      Assert.AreEqual(desc, expectedDesc);
    }

    [Test]
    public void TestLastingEffectDescription()
    {
      var game = CreateGame();
      var le = game.Hero.AddLastingEffect(EffectType.Bleeding, 3, 10);
      Assert.NotNull(le);

      var expectedDesc = le.Type.ToDescription() + ", -" + le.DamageAmount/game.Hero.Stats.Defense + " Health (per turn)";
      Assert.AreEqual(le.GetDescription(game.Hero), expectedDesc);

      CheckDesc(EffectType.IronSkin, EntityStatKind.Defense, '+');
      CheckDesc(EffectType.Rage, EntityStatKind.Attack, '+');
      CheckDesc(EffectType.Inaccuracy, EntityStatKind.ChanceToHit, '-');
      CheckDesc(EffectType.Weaken, EntityStatKind.Defense, '-');

      le = game.Hero.AddLastingEffect(EffectType.ResistAll, 3, EntityStatKind.Unset, 10);
      Assert.NotNull(le);
      expectedDesc = le.Type.ToDescription() + " +" + le.Subtraction;
      Assert.AreEqual(le.GetDescription(game.Hero), expectedDesc);
    }
  }
}
