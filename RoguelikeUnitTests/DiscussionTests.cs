﻿using NUnit.Framework;
using Roguelike.Discussions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class DiscussionTests : TestBase
  {
    [Test]
    public void TestCustomInteriorGen()
    {
      var game = CreateGame();
      //Assert.AreEqual(game.Level, null);
    }
  }
}
