using NUnit.Framework;
using Roguelike.Core.Discussions;
using Roguelike.Core.Discussions.Entities;
using Roguelike.Discussions;
using Roguelike.Quests;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoguelikeUnitTests.Core
{
  [TestFixture]
  public class DiscussionTests : TestBase
  {
    [Test]
    public void TestDiscussWithLech()
    {
      var game = CreateGame();

      Action<Discussion> assertDisc = (Discussion discussion) =>{
        Assert.NotNull(discussion.MainItem);
        Assert.AreEqual(discussion.MainItem.Topics.Count, 1);
        Assert.AreEqual(discussion.MainItem.Topics[0].Topics.Count, 2);
        Assert.AreEqual(discussion.MainItem.Topics[0].Topics[0].Left.ToString(),
          "There is a way this can be done. If you deliver me 5 pieces of the iron ore I can devote part of it to making you a weapon.");
      };

      var  discussion = Factory.Create(Container, "Lech");
      assertDisc(discussion);
      var xml = discussion.ToXml();
      discussion.MainItem = null;
      discussion.FromXml(xml);
      assertDisc(discussion);
    }
  }
}
