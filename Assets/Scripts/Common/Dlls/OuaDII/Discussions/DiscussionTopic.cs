using Newtonsoft.Json;
using OuaDII.Quests;
using Roguelike.Discussions;
using Roguelike.Tiles;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII.Discussions
{
  public class DiscussionTopic : Roguelike.Discussions.DiscussionTopic
  {
    public OuaDII.Quests.QuestKind QuestKind { get; set; }
    public OuaDII.Quests.QuestKind ParentForQuest { get; set; }
    public string UnhidingMapName { get; set; } = "";
    public LootKind LootKind { get; internal set; }

    public DiscussionTopic(Container container) : base(container)
    {
      
    }

    public DiscussionTopic(Container container, KnownSentenceKind right, string left, bool allowBuyHound = false, bool addMerchantItems = merchantItemsAtAllLevels)
      : base(container, right, left, allowBuyHound, addMerchantItems)
    {
    }

    public DiscussionTopic(Container container, string right, string left, bool allowBuyHound = false, bool addMerchantItems = merchantItemsAtAllLevels)
      : base(container, right, left, allowBuyHound, addMerchantItems)
    {
    }

    public DiscussionTopic
    (
      Container container, 
      string right, 
      KnownSentenceKind rightKnown, 
      QuestKind questKind = QuestKind.Unset, 
      bool allowBuyHound = false, 
      bool skipReward = false
    )
    : base(container, rightKnown, "", allowBuyHound, false)
    {
      if (rightKnown == KnownSentenceKind.QuestAccepted)
      {
        Left.Body = "Good luck!";//I wish you well
        if (questKind == QuestKind.Unset)
          throw new Exception(right+" questKind == QuestKind.Unset");
      }

      Right.Body = right;
      QuestKind = questKind;
      SkipReward = skipReward;
    }

    public override string ToString()
    {
      return base.ToString() + " " + QuestKind;
    }

    public DiscussionTopic GetItemByQuest(QuestKind quest)
    {
      if (QuestKind == quest)
        return this;

      return GetTopicInChildren(quest);
    }

    public DiscussionTopic GetTopicInChildren(QuestKind quest)
    {
      foreach (var childItem in Topics.Where(i => i is DiscussionTopic).Cast<DiscussionTopic>())
      {
        var itemInChild = childItem.GetItemByQuest(quest);
        if (itemInChild != null)
          return itemInChild;
      }
      return null;
    }

  }
}
