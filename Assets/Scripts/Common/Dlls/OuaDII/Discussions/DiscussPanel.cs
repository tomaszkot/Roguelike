using OuaDII.Extensions;
using OuaDII.Managers;
using OuaDII.Quests;
using OuaDII.Tiles.LivingEntities;
using Roguelike.Discussions;
using Roguelike.Events;
using Roguelike.UI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII.Discussions
{
  public class DiscussPanel : Roguelike.Discussions.DiscussPanel
  {
    QuestManager qm;
    Hero hero;

    DiscussPanel() { }

    public DiscussPanel(QuestManager qm, Hero hero)
    {
      this.qm = qm;
      this.hero = hero;
    }

    public OuaDII.Discussions.DiscussionTopic GetTopic(string text)
    {
      var res = OuadTopics()
        .Where(i => i.Right.Body == text).SingleOrDefault();

      return res;
    }

    public OuaDII.Discussions.DiscussionTopic GetTopic(Quests.QuestKind kind)
    {
      var res = GetTopics(kind).SingleOrDefault();

      return res;
    }

    public IEnumerable<OuaDII.Discussions.DiscussionTopic> GetTopics(Quests.QuestKind kind)
    {
      var res = OuadTopics()
        .Where(i => i.QuestKind == kind);

      return res;
    }

    public OuaDII.Discussions.DiscussionTopic GetParentTopic(Quests.QuestKind kind)
    {
      var res = OuadTopics()
        .Where(i => i.ParentForQuest == kind)
        .SingleOrDefault();

      return res;
    }

    private IEnumerable<DiscussionTopic> OuadTopics()
    {
      return BoundTopics.TypedItems.Where(i => i.Item is OuaDII.Discussions.DiscussionTopic)
              .Select(i => i.Item)
              .Cast<OuaDII.Discussions.DiscussionTopic>();
    }

    public bool ChooseDiscussionTopic(KnownSentenceKind kind)
    {
      var topic = GetTopic(kind);
      if (topic == null)
        return false;

      return ChooseDiscussionTopic(topic);
    }

    public override bool ChooseDiscussionTopic(Roguelike.Discussions.DiscussionTopic topic)
    {
      if (topic == null)
        return false;
      var res = base.ChooseDiscussionTopic(topic);
      var id = topic.RightKnownSentenceKind;
      var ouadItem = topic.AsOuaDItem();
      if (res && id == Roguelike.Discussions.KnownSentenceKind.QuestAccepted)
      {

        //var npc = ale as Roguelike.Tiles.LivingEntities.NPC;
        if (npc != null)
        {
          //update Discussion and Quests
          QuestManager.LootKindForQuest = ouadItem.LootKind;
          res = npc.Discussion.AcceptQuest(qm, ouadItem.QuestKind.ToString());
          QuestManager.LootKindForQuest = Roguelike.Tiles.LootKind.Unset;
          if (res)
          {
            var quest = hero.GetQuest(ouadItem.QuestKind);
            quest.SkipReward = topic.SkipReward;
            if (topic.NPCJoinsAsAlly)
            {
              //qm.GameManager.AddAlly(ale);//TODO
            }

            if (topic.HoundJoinsAsAlly)
            {
              qm.GameManager.TryAddAlly<Roguelike.Tiles.LivingEntities.TrainedHound>();
              quest.RewardLootKind = Roguelike.Tiles.LootKind.Unset;
            }
            else
            {
              if (quest.GetKind() == Quests.QuestKind.CreatureInPond)
                quest.RewardLootKind = Roguelike.Tiles.LootKind.Gem;

            }

            if (ouadItem.UnhidingMapName.Any())
            {
              qm.GameManager.AppendHiddenTiles(ouadItem.UnhidingMapName);
            }
          }
          BindTopics(npc.Discussion.MainItem, npc);
        }
      }
      else if (res &&
        (id == Roguelike.Discussions.KnownSentenceKind.AllyAccepted ||
        id == Roguelike.Discussions.KnownSentenceKind.AllyRejected)
        )
      {
        if (id == Roguelike.Discussions.KnownSentenceKind.AllyAccepted)
          qm.GameManager.AddAlly(this.npc as Roguelike.Abstract.Tiles.IAlly);
        var le = npc as Roguelike.Tiles.LivingEntities.LivingEntity;
        if (le.tag1.Contains("Roslaw"))
        {
          if (qm.GetHeroQuest(Quests.QuestKind.StonesMine) != null)
            qm.SetQuestAwaitingReward(Quests.QuestKind.StonesMine);
          qm.GameManager.GameState.History.SetEngaged(le.tag1);
        }
      }

      return res;

    }

    protected override void RewardHero(Roguelike.Tiles.LivingEntities.INPC merch, Roguelike.Discussions.DiscussionTopic topic)
    {
      Debug.Assert(topic.AsOuaDItem().QuestKind != Quests.QuestKind.Unset);
      qm.RewardHero(merch, topic.AsOuaDItem().QuestKind, topic.RightKnownSentenceKind);
      if (topic.AsOuaDItem().QuestKind == Quests.QuestKind.IronOreForSmith)
      {
        merch.Discussion.MainItem.InsertTopic(merch.Discussion.Container.GetInstance<DiscussionFactory>().CreateSilesiaCoalQuest());
      }
    }

    public override GenericListModel<Roguelike.Discussions.DiscussionTopic> BindTopics(Roguelike.Discussions.DiscussionTopic parentTopic, Roguelike.Tiles.LivingEntities.INPC npc)
    {
      
      //foreach (var topic in parentTopic.Topics)
      //{
      //  var top = topic.AsOuaDItem();
      //  if (top.QuestKind != Quests.QuestKind.Unset && top.QuestKind == QuestKind.KillBoar)
      //  {
      //    var quest = qm.GetHeroQuest(top.QuestKind);
      //    if (quest != null && quest.Status == Roguelike.Quests.QuestStatus.AwaitingReward)
      //    {
      //      top.RightKnownSentenceKind = KnownSentenceKind.AwaitingReward;
      //      top.Right.Body = "I did it!";
      //      top.Left.Body = "Thank you! please keep the weapon";
      //      //subTopic = discussionFactory.create("I did it!", KnownSentenceKind.AwaitingReward, questKind);
      //    }
      //  }
      //}
      return base.BindTopics(parentTopic, npc);
    }
  }
}
