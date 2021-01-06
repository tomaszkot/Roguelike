using NUnit.Framework;
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
    public void TestLionel()
    {
      var disc = Discussion.CreateForLionel(true);
      disc.ToXml();
      var loaded = Discussion.FromXml(disc.EntityName);
      Assert.AreEqual(loaded.MainItem.Topics.Count, disc.MainItem.Topics.Count);
    }
  }
}
