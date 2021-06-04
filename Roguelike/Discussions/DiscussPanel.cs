using Dungeons.Core;
using Roguelike.Tiles.LivingEntities;
using Roguelike.UI.Models;
using System;
using System.Linq;

namespace Roguelike.Discussions
{
  public class DiscussPanel : Roguelike.Abstract.Discussions.IDiscussPanel
  {
    protected Roguelike.Tiles.LivingEntities.AdvancedLivingEntity ale;
    GenericListModel<DiscussionTopic> boundTopics = new GenericListModel<DiscussionTopic>();

    public GenericListModel<DiscussionTopic> BoundTopics { get => boundTopics; set => boundTopics = value; }

    public event EventHandler<DiscussionTopic> DiscussionOptionChosen;

    public DiscussPanel()
    {
    }

    public virtual bool ChooseDiscussionTopic(DiscussionTopic topic)
    {
      var knownSentenceKind = topic.RightKnownSentenceKind;
      if (knownSentenceKind == KnownSentenceKind.Back)
      {
        BindTopics(topic.Parent, ale);
      }
      else if (knownSentenceKind == KnownSentenceKind.Bye || knownSentenceKind == KnownSentenceKind.LetsTrade ||
        knownSentenceKind == KnownSentenceKind.QuestAccepted)
      {
        Hide();
      }
      else if (knownSentenceKind == KnownSentenceKind.SellHound)
      {
      }
      else
      {
        //var merch = ale as Merchant;
        var itemToBind = topic;

        if (knownSentenceKind == KnownSentenceKind.WorkingOnQuest||
            knownSentenceKind == KnownSentenceKind.AwaitingReward||
            knownSentenceKind == KnownSentenceKind.RewardSkipped||
            knownSentenceKind == KnownSentenceKind.Cheating||
            knownSentenceKind == KnownSentenceKind.AwaitingRewardAfterRewardDeny)
        {
          if (knownSentenceKind == KnownSentenceKind.AwaitingReward||
              knownSentenceKind == KnownSentenceKind.AwaitingRewardAfterRewardDeny ||
              knownSentenceKind == KnownSentenceKind.RewardSkipped)
          {
            RewardHero(ale as INPC, topic);
          }
          else if (knownSentenceKind == KnownSentenceKind.Cheating)
          {
            ale.Discussion.EmitCheating(topic);
            if (ale.RelationToHero.CheatingCounter >= 2)
            {
              Hide();
              //RelationChanged.Raise(this, merch.RelationToHero.Kind);
            }
          }

          itemToBind = itemToBind.Parent.Parent;
        }

        BindTopics(itemToBind, ale);
      }

      DiscussionOptionChosen.Raise(this, topic);
      return true;
    }

    protected virtual void RewardHero(Roguelike.Tiles.LivingEntities.INPC npc, DiscussionTopic topic)
    {

    }

    public void Bind(AdvancedLivingEntity leftEntity, AdvancedLivingEntity rightEntity, GenericListModel<DiscussionTopic> options = null)
    {
      throw new NotImplementedException();
    }

    public virtual GenericListModel<DiscussionTopic> BindTopics(DiscussionTopic parentTopic, Roguelike.Tiles.LivingEntities.AdvancedLivingEntity npc)
    {
      this.ale = npc;
      boundTopics.Clear();
      foreach (var topic in parentTopic.Topics)
      {
        boundTopics.Add(new GenericListItemModel<DiscussionTopic>(topic, topic.Right.Id, topic.Right.Body));
      }

      return boundTopics;
    }

    public GenericListItemModel<DiscussionTopic> GetTopicModel(KnownSentenceKind kind)
    {
      return boundTopics.TypedItems.Where(i => i.Item.RightKnownSentenceKind == kind).SingleOrDefault();
    }

    public DiscussionTopic GetTopic(KnownSentenceKind kind)
    {
      var found = boundTopics.TypedItems.Where(i => i.Item.RightKnownSentenceKind == kind).SingleOrDefault();
      return found != null ? found.Item : null;
    }

    public GenericListItemModel<DiscussionTopic> GetTopicModel(DiscussionTopic topic)
    {
      return boundTopics.TypedItems.Where(i => i.Item == topic).SingleOrDefault();
    }

    public virtual void Hide()
    {

    }
  }
}
