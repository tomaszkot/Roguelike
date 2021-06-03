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
      var handled = false;
      var leftId = topic.Left.Id;
      if (leftId == KnownSentenceKind.Back.ToString())
      {
        BindTopics(topic.Parent, ale);
        handled = true;
      }
      else if (leftId == KnownSentenceKind.Bye.ToString())
      {
        Hide();
        handled = true;
      }
      else if (leftId == KnownSentenceKind.LetsTrade.ToString())
      {
        Hide();
        handled = true;
      }
      else if (leftId == KnownSentenceKind.QuestAccepted.ToString())
      {
        Hide();
        handled = true;
      }
      else if (leftId == KnownSentenceKind.SellHound.ToString())
      {
        handled = true;
      }
      else
      {
        var merch = ale as Merchant;
        var itemToBind = topic;

        if (leftId == KnownSentenceKind.WorkingOnQuest.ToString() ||
            leftId == KnownSentenceKind.AwaitingReward.ToString() ||
            leftId == KnownSentenceKind.RewardSkipped.ToString() ||
            leftId == KnownSentenceKind.Cheating.ToString() ||
            leftId == KnownSentenceKind.AwaitingRewardAfterRewardDeny.ToString())
        {
          if (leftId == KnownSentenceKind.AwaitingReward.ToString() ||
              leftId == KnownSentenceKind.AwaitingRewardAfterRewardDeny.ToString())
          {
            RewardHero(merch, topic);
          }
          else if (leftId == KnownSentenceKind.Cheating.ToString())
          {
            ale.Discussion.EmitCheating(topic);
            if (merch.RelationToHero.CheatingCounter >= 2)
            {
              Hide();
              //RelationChanged.Raise(this, merch.RelationToHero.Kind);
            }
          }

          handled = true;
          itemToBind = itemToBind.Parent.Parent;
        }

        BindTopics(itemToBind, ale);
      }

      if (handled)
        DiscussionOptionChosen.Raise(this, topic);
      return handled;
    }

    protected virtual void RewardHero(Merchant merch, DiscussionTopic topic)
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
      return boundTopics.TypedItems.Where(i => i.Item.KnownSentenceKind == kind).SingleOrDefault();
    }

    public DiscussionTopic GetTopic(KnownSentenceKind kind)
    {
      var found = boundTopics.TypedItems.Where(i => i.Item.KnownSentenceKind == kind).SingleOrDefault();
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
