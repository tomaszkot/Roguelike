using Dungeons.Core;
using Roguelike.Tiles.LivingEntities;
using Roguelike.UI.Models;
using System;
using System.Linq;

namespace Roguelike.Discussions
{
  public class DiscussPanel : Roguelike.Abstract.Discussions.IDiscussPanel
  {
    protected Roguelike.Tiles.LivingEntities.AdvancedLivingEntity npc;
    GenericListModel<DiscussionTopic> boundTopics = new GenericListModel<DiscussionTopic>();

    public GenericListModel<DiscussionTopic> BoundTopics { get => boundTopics; set => boundTopics = value; }

    public event EventHandler<DiscussionTopic> DiscussionOptionChosen;
    //public event EventHandler<Roguelike.Tiles.LivingEntities.RelationToHeroKind> RelationChanged;
    //Container container;

    public DiscussPanel()
    {
      //this.container = container;
    }

    public virtual bool ChooseDiscussionTopic(DiscussionTopic itemModel)
    {
      var handled = false;
      var id = itemModel.Left.Id;
      if (id == KnownSentenceKind.Back.ToString())
      {
        BindTopics(itemModel.Parent, npc);
        handled = true;
      }
      else if (id == KnownSentenceKind.Bye.ToString())
      {
        Hide();
        handled = true;
      }
      else if (id == KnownSentenceKind.LetsTrade.ToString())
      {
        Hide();
        handled = true;
      }
      else if (id == KnownSentenceKind.QuestAccepted.ToString())
      {
        Hide();
        handled = true;
      }
      else
      {
        var merch = npc as Merchant;
        var itemToBind = itemModel;

        if (id == KnownSentenceKind.WorkingOnQuest.ToString() ||
            id == KnownSentenceKind.AwaitingReward.ToString() ||
            id == KnownSentenceKind.Cheating.ToString())
        {
          if (id == KnownSentenceKind.AwaitingReward.ToString())
          {
            RewardHero(merch, itemModel);
          }
          else if (id == KnownSentenceKind.Cheating.ToString())
          {
            npc.Discussion.EmitCheating(itemModel);
            if (merch.RelationToHero.CheatingCounter >= 2)
            {
              Hide();
              //RelationChanged.Raise(this, merch.RelationToHero.Kind);
            }
          }

          handled = true;
          itemToBind = itemToBind.Parent.Parent;
        }

        BindTopics(itemToBind, npc);
      }

      if (handled)
        DiscussionOptionChosen.Raise(this, itemModel);
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
      this.npc = npc;
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

    public GenericListItemModel<DiscussionTopic>  GetTopicModel(DiscussionTopic topic)
    {
      return boundTopics.TypedItems.Where(i => i.Item == topic).SingleOrDefault();
    }

    public virtual void Hide()
    {
      
    }
  }
}
