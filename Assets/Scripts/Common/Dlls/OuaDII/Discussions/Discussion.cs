using Newtonsoft.Json;
using OuaDII.Extensions;
using OuaDII.Managers;
using OuaDII.Quests;
using Roguelike.Discussions;
using Roguelike.Extensions;
using Roguelike.Quests;
using Roguelike.Tiles.LivingEntities;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII.Discussions
{
  public class Discussion : Roguelike.Discussions.Discussion
  {
    DiscussionFactory discussionFactory;

    public Discussion(Container container) : base(container)
    {
      this.container = container;
      discussionFactory = container.GetInstance<DiscussionFactory>();
    }

    protected override Roguelike.Discussions.DiscussionTopic CreateMainItem()
    {
      return this.container.GetInstance<Roguelike.Discussions.DiscussionTopic>();
    }

    public DiscussionTopic GetTopicByQuest(QuestKind quest)
    {
      if (MainItem.AsOuaDItem().QuestKind == quest)
        return MainItem.AsOuaDItem();

      foreach (var childItem in MainItem.Topics)
      {
        var item = childItem.AsOuaDItem().GetItemByQuest(quest);
        if (item != null)
          return item;
      }
      return null;
    }

    public static QuestKind QuestKindFromString(string questKind) 
    {
      var types = Enum.GetValues(typeof(QuestKind)).Cast<QuestKind>().ToList();
      return types.FirstOrDefault(i=> i.ToString() == questKind);
    }

    public bool AcceptQuest(string questKindStr)
    {
      var questKind = QuestKindFromString(questKindStr);
      return AcceptQuest( questKind);
    }

    public override bool AcceptQuest(Roguelike.Managers.QuestManager mgr, string questKindStr)
    {
      var questKind = QuestKindFromString(questKindStr);
      var acc = AcceptQuest(questKind);
      if (acc)
      {
        return mgr.EnsureQuestAssigned(questKindStr);
      }

      return false;
    }

    string GetConcerningQuestName(QuestKind questKind)
    {
      if (questKind == QuestKind.IronOreForSmith)
        return "Iron Ore";
      if (questKind == QuestKind.HourGlassForMiller)
        return "Hourglass";
      else if (questKind == QuestKind.SilesiaCoalForSmith)
        return "Silesia Coal";
      else if (questKind == QuestKind.ToadstoolsForWanda)
        return "Toadstools";
      else if (questKind == QuestKind.FernForDobromila)
        return "Magic Fern";

      return questKind.ToDescription();
    }

    public bool AcceptQuest(QuestKind questKind)
    {
      var item = GetTopicByQuest(questKind);
      int counter = 0;
      DiscussionTopic parent = item.Parent.AsOuaDItem();
      while (parent != null && parent.ParentForQuest != questKind && counter < 20)
      {
        parent = parent.Parent.AsOuaDItem();
        counter++;
      }

      if (parent != null)
      {
        parent.Parent.Topics.Remove(parent);
      }
      else
        Debug.WriteLine("ParentForQuest not removed for: "+ questKind);
        

      var conc = "Concerning "+GetConcerningQuestName(questKind);
      //if (questKind == QuestKind.ToadstoolsForWanda)
      //  conc += "Toadstools";
      //else
      //  conc += questKind.ToDescription();

      conc += "...";
      var taskProgress = new DiscussionTopic(container, conc, KnownSentenceKind.QuestProgress, questKind);
      this.MainItem.InsertTopic(taskProgress, false);
      taskProgress.Left.Body = "Yes?";
      UpdateQuestDiscussion(questKind, KnownSentenceKind.WorkingOnQuest, taskProgress);
      
      return true;
    }

    public void RemoveDoneQuest(QuestKind questKind)
    {
      var taskProgress = GetTopicByQuest(questKind);
      this.MainItem.Topics.Remove(taskProgress);
    }

    public void SetQuestProgressTopics(QuestKind questKind, KnownSentenceKind knownSentenceKind, Quest quest = null)
    {
      UpdateQuestDiscussion(questKind, knownSentenceKind, null, quest);
    }

    public void UpdateQuestDiscussion(QuestKind questKind, KnownSentenceKind knownSentenceKind, DiscussionTopic task = null, Quest quest = null)
    {
      var taskProgress = task == null ? GetTopicByQuest(questKind) : task;
      if (taskProgress == null)
      {
        return;//ups
      }
      taskProgress.Topics.Clear();

      DiscussionTopic subTopic = null;
      string left = "I'm glad to hear it!";
      if (knownSentenceKind == KnownSentenceKind.WorkingOnQuest || knownSentenceKind == KnownSentenceKind.Cheating)
      {
        subTopic = discussionFactory.create("I'm working on it", KnownSentenceKind.WorkingOnQuest, questKind);
      }
      else if (knownSentenceKind == KnownSentenceKind.AwaitingReward)
      {
        if (questKind != QuestKind.HourGlassForMiller)
        {
          subTopic = discussionFactory.create("I did it!", KnownSentenceKind.AwaitingReward, questKind);
          if (questKind != QuestKind.Hornets)
          {
            if (
              (questKind == QuestKind.CreatureInPond && quest.RewardLootKind == Roguelike.Tiles.LootKind.Unset)
              || quest.SkipReward
              )
            {
              left += " Thank you!";
            }
            else if (questKind == QuestKind.KillBoar)
            {
              left += " Thank you! please keep the weapon.";
            }
            else
              left += " Here is your reward...";
          }
          else
          {
            subTopic.Left.Id = KnownSentenceKind.RewardDeny.ToString();
            subTopic.RightKnownSentenceKind = KnownSentenceKind.RewardDeny;

            var subTopic0 = discussionFactory.create("Where is my reward?", "yyyy, reward? I do not remember we agreed on any...");//I'm quite old

            var subTopic1 = discussionFactory.create("Give me it, or I will refresh your memory for a good...", "ok, ok, here it is, forgive me I'm quite old, my memory is making tricks on me");//
            subTopic1.Left.Id = KnownSentenceKind.AwaitingRewardAfterRewardDeny.ToString();
            subTopic1.QuestKind = questKind;

            subTopic1.RightKnownSentenceKind = KnownSentenceKind.AwaitingRewardAfterRewardDeny;
            subTopic0.InsertTopic(subTopic1);
            var subTopic2 = discussionFactory.create("no problem old man, enjoy your apiary", "Thank you for your help");
            subTopic2.RightKnownSentenceKind = KnownSentenceKind.RewardSkipped;
            subTopic2.QuestKind = QuestKind.Hornets;
            subTopic0.InsertTopic(subTopic2);

            subTopic.InsertTopic(subTopic0);
          }
        }
        else
        {
          if (questKind == QuestKind.HourGlassForMiller)
          {
            subTopic = discussionFactory.create("He is not interested in measuring time anymore. He got possesed by evil foces. I had to kill him.", KnownSentenceKind.AwaitingReward, questKind);
            left = "Sorry to hear it.";
            left += " Anyway, here is your reward...";
          }
        }
      }

      subTopic.Left.Body = left;

      if (subTopic != null)
        taskProgress.InsertTopic(subTopic);

      if (knownSentenceKind == KnownSentenceKind.AwaitingReward &&
        (questKind == QuestKind.SilesiaCoalForSmith || questKind == QuestKind.IronOreForSmith)
        )
      {
        var matter = "ore";
        if (questKind == QuestKind.SilesiaCoalForSmith)
          matter = "coal";

        var discussionNpcInfo = new DiscussionZiemowitInfo(container);
        subTopic = discussionNpcInfo.GetTopic("CouldNotFind_" + matter, "BackpackFull");
        subTopic.RightKnownSentenceKind = KnownSentenceKind.Cheating;
        subTopic.QuestKind = questKind;


        taskProgress.InsertTopic(subTopic, false);
      }

      taskProgress.EnsureBack();
    }

    public void EmitCheating(QuestKind questKind, INPC inpc)
    {
      var quant = 0;
      if (questKind == QuestKind.IronOreForSmith)
        quant = LootQuestRequirement.QuestLootQuantity(questKind) + LootQuestRequirement.QuestCheatingPunishmentLootQuantity(questKind);

      var gm = container.GetInstance<Roguelike.Managers.GameManager>() as GameManager;
      var quest = gm.OuadHero.GetQuest(questKind);
      
      var npc = inpc as AdvancedLivingEntity;
      npc.RelationToHero.CheatingCounter++;
      if (npc.RelationToHero.CheatingCounter == 2)
      {
        npc.RelationToHero.Kind = RelationToHeroKind.Antagonistic;
        npc.Discussion.MainItem.Topics.Clear();
        npc.Discussion.MainItem.EnsureBack();
        quest.Status = QuestStatus.FailedToDo;
        return;
      }
      quest.SetLootQuestRequirement<LootQuestRequirement>(gm.QuestManager, quant);

      quest.Status = QuestStatus.Accepted;

      SetQuestProgressTopics(questKind, KnownSentenceKind.WorkingOnQuest);
    }

    public override void EmitCheating(Roguelike.Discussions.DiscussionTopic item, INPC inpc)
    {
      EmitCheating(item.AsOuaDItem().QuestKind, inpc);
    }
  }
}
