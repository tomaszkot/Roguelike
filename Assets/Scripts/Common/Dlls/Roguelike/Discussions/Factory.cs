using Roguelike.Core.Discussions.Entities;
using Roguelike.Discussions;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Text;

namespace Roguelike.Core.Discussions
{
  public class Factory
  {
    public static Discussion Create(Container container, string entityName)
    {
      var dis = new Discussion(container);
      dis.EntityName = entityName;
      var mainItem = new DiscussionTopic(container, "", "", false, false);

      var discussionNpcInfo = new Lech(container, dis.EntityName);
      var topic = discussionNpcInfo.GetTopicByLeftRight("CanYouMakeIronSword", "NopeEdict");

      var subTopic = discussionNpcInfo.GetTopicByLeftRight("ComeOnOneSword", "ThereIsAWay");
      topic.InsertTopic(subTopic);

      var subTopic1 = discussionNpcInfo.GetTopicByLeftRight("WhereFindIronOre", "MineNearBy");
      subTopic.InsertTopic(subTopic1);

      mainItem.InsertTopic(topic);
      dis.MainItem = mainItem;
      //var ok = discussionNpcInfo.GetTopic("AllRightDoIt", KnownSentenceKind.QuestAccepted, "QuestKind_IronOreForSmith");
      //subTopic1.InsertTopic(ok);

      return dis;
    }

  }
}
