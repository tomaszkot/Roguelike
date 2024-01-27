using Dungeons.Core;
using Roguelike.Quests;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.LivingEntities;
using Roguelike.UI.Models;
using System;
using System.Linq;

namespace Roguelike.Discussions
{
  public class DiscussPanel : Roguelike.Abstract.Discussions.IDiscussPanel
  {
    protected INPC npc;
    GenericListModel<DiscussionTopic> boundTopics = new GenericListModel<DiscussionTopic>();
    
    public INPC NPC => npc;

    public GenericListModel<DiscussionTopic> BoundTopics 
    {
      get => boundTopics; 
      set => boundTopics = value; 
    }

    public event EventHandler<DiscussionTopic> DiscussionOptionChosen;
    public event EventHandler Hidden;

    //protected INPC AdvancedLivingEntity
    //{
    //  get{ return npc;  } 
    //}

    public DiscussPanel()
    {
    }

    public virtual void Reset()
    {
      npc.Discussion.Reset();
      BindTopics(npc.Discussion.MainItem, npc);
    }

    protected virtual bool PreventPanelRebind(Roguelike.Discussions.DiscussionTopic topic)
    {
      return false;
    }

    protected virtual bool ShallHideOn(DiscussionTopic topic, KnownSentenceKind knownSentenceKind)
    {
      if (topic.ClosesPanel)
        return true;

      if (knownSentenceKind == KnownSentenceKind.AllyAccepted && topic.Right.Id == "NoWorry")
        return false;

      return (knownSentenceKind == KnownSentenceKind.Bye || knownSentenceKind == KnownSentenceKind.LetsTrade ||
        knownSentenceKind == KnownSentenceKind.QuestAccepted || knownSentenceKind == KnownSentenceKind.AllyAccepted) ;
    }

    public virtual bool ChooseDiscussionTopic(DiscussionTopic topic)
    {
      var knownSentenceKind = topic.RightKnownSentenceKind;
      if (knownSentenceKind == KnownSentenceKind.Back)
      {
        BindTopics(topic.Parent, npc);
      }
      else if (topic.ClosesPanel || knownSentenceKind == KnownSentenceKind.Bye || knownSentenceKind == KnownSentenceKind.LetsTrade ||
        knownSentenceKind == KnownSentenceKind.QuestAccepted || knownSentenceKind == KnownSentenceKind.AllyAccepted)
      {
        if(ShallHideOn(topic, knownSentenceKind))
          Hide();
        else if(topic.Right.Id == "NoWorry")//TODO!
          BindTopics(topic, npc);

        if (PreventPanelRebind(topic))
          BindTopics(topic, npc);
      }

      else
      {
        var itemToBind = topic;
        //var qs = GetQuestStatus(topic);
                
        if (knownSentenceKind == KnownSentenceKind.QuestAccepted||
            knownSentenceKind == KnownSentenceKind.AwaitingReward ||
            knownSentenceKind == KnownSentenceKind.RewardSkipped||
            knownSentenceKind == KnownSentenceKind.Cheating||
            knownSentenceKind == KnownSentenceKind.AwaitingRewardAfterRewardDeny)
        {
          if (knownSentenceKind == KnownSentenceKind.AwaitingReward ||
              knownSentenceKind == KnownSentenceKind.AwaitingRewardAfterRewardDeny ||
              knownSentenceKind == KnownSentenceKind.RewardSkipped)
          {
            RewardHero(npc as INPC, topic);
          }
          else if (knownSentenceKind == KnownSentenceKind.Cheating)
          {
            npc.Discussion.EmitCheating(topic, npc);
            if (npc.RelationToHero.CheatingCounter >= 2)
            {
              Hide();
              //RelationChanged.Raise(this, merch.RelationToHero.Kind);
            }
          }

          //itemToBind = itemToBind.Parent.Parent;
          if (ShallBindMainTopic(itemToBind))
            itemToBind = npc.Discussion.MainItem;
        }

        BindTopics(itemToBind, npc);
      }

      DiscussionOptionChosen.Raise(this, topic);
      return true;
    }

    protected virtual bool ShallBindMainTopic(DiscussionTopic topic)
    {
      return !(npc.Name == "Zyndram" && topic.RightKnownSentenceKind == KnownSentenceKind.Cheating);
    }

    protected virtual QuestStatus GetQuestStatus(DiscussionTopic topic)
    {
      return QuestStatus.Unset;
    }

    protected virtual void RewardHero(Roguelike.Tiles.LivingEntities.INPC npc, DiscussionTopic topic)
    {

    }

    public void Bind(INPC leftEntity, AdvancedLivingEntity rightEntity, GenericListModel<DiscussionTopic> options = null)
    {
      throw new NotImplementedException();
    }

    public virtual GenericListModel<DiscussionTopic> BindTopics(DiscussionTopic parentTopic,
      INPC npc)
    {
      this.npc = npc;
      boundTopics.Clear();

      parentTopic.EnsureBack();
      foreach (var topic in parentTopic.Topics)
      {
        boundTopics.Add(new GenericListItemModel<DiscussionTopic>(topic, topic.Right.Id, topic.Right.Body+ topic.RightSuffix));
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
      if(npc != null)
        npc.SetHasUrgentTopic(false);
      if(Hidden!=null)
        Hidden(this, EventArgs.Empty);
    }
  }
}
