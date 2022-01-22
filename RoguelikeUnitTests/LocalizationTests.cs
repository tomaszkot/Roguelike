using NUnit.Framework;
using Roguelike.Extensions;
using Roguelike.History.Hints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class LocalizationTests : TestBase
  {
    [Test]
    public void TestHints()
    {
      var hintHistory = new HintHistory();

      var hint = hintHistory.Get(Roguelike.Help.HintKind.LootCollectShortcut);
      Assert.AreEqual(hint.Info, "Press 'G' to collect a single loot.");
            
      hintHistory.SetKeyCode(Roguelike.Help.HintKind.LootHightlightShortcut, (int)KeyCode.LeftControl, (Roguelike.Help.HintKind hk) => { return KeyCode.LeftControl.ToDescription(); });
      hint = hintHistory.Get(Roguelike.Help.HintKind.LootHightlightShortcut);
      Assert.AreEqual(hint.Info, "Press 'Left Control' to see collectable/interactive items.");

      //hintHistory.SetKeyCode(Roguelike.Help.HintKind.LootCollectShortcut, (int)KeyCode.A, (Roguelike.Help.HintKind hk) => { return KeyCode.A.ToDescription(); });
      //Assert.AreEqual(hint.Info, "Press 'A' to collect a single loot.");

      //hintHistory.SetKeyCode(Roguelike.Help.HintKind.LootCollectShortcut, (int)KeyCode.Alpha1, (Roguelike.Help.HintKind hk) => { return KeyCode.Alpha1.ToDescription(); });
      //Assert.AreEqual(hint.Info, "Press 'Alpha1' to collect a single loot.");
    }
  }
}
