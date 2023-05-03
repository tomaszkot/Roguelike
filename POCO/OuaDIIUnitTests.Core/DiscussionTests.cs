using NUnit.Framework;
using OuaDII.Discussions;
using OuaDII.Extensions;
using OuaDII.Generators;
using OuaDII.Managers;
using OuaDII.Quests;
using OuaDII.Tiles.Looting;
using Roguelike.Discussions;
using Roguelike.Quests;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using System;
using System.Linq;

namespace OuaDIIUnitTests
{
  class BaseDiscussionTests : TestBase
  {
    protected Roguelike.Tiles.LivingEntities.NPC npc;
    protected OuaDII.Tiles.LivingEntities.Hero hero;
    protected OuaDII.Discussions.DiscussPanel discussPanel;
    protected Roguelike.Discussions.DiscussionTopic modelWhatUp;

    protected override void Reload(string merchantName)
    {
      var npcBefore = GetNPC(merchantName);
      base.Reload(merchantName);

      hero = GameManager.OuadHero;
      npc = GetNPC(merchantName);
      Assert.AreEqual(npcBefore.Discussion.MainItem.Topics.Count, npc.Discussion.MainItem.Topics.Count);
      for (int i = 0; i < npcBefore.Discussion.MainItem.Topics.Count; i++)
      {
        var npcBeforeTopic = npcBefore.Discussion.MainItem.Topics[i].ToString();
        var npcTopic = npc.Discussion.MainItem.Topics[i].ToString();
        Assert.AreEqual(npcBeforeTopic, npcTopic);
      }
    }
  }

  [TestFixture]
  class DiscussionTests : BaseDiscussionTests
  {
    [Test]
    public void TestBasic()
    {
      CreateWorld();
      hero = GameManager.OuadHero;
      discussPanel = new OuaDII.Discussions.DiscussPanel(GameManager.QuestManager, hero);
      npc = GameManager.GetNPC("Lionel");
      
      Assert.NotNull(npc.Discussion);
      Assert.Greater(npc.Discussion.MainItem.Topics.Count, 0);
      Assert.True(npc.Discussion.MainItem.HasTopics(KnownSentenceKind.Bye));

      discussPanel.BindTopics(npc.Discussion.MainItem, npc);
      var byeTopic = npc.Discussion.MainItem.GetTopic(KnownSentenceKind.Bye);
      Assert.NotNull(byeTopic);
      Assert.AreEqual(byeTopic.Right.Body, "Bye");
      //Assert.True(byeTopic.Left.Body.StartsWith("Bye"));

      Assert.True(discussPanel.ChooseDiscussionTopic(byeTopic));
      
      {
        var trade = npc.Discussion.MainItem.GetTopic(KnownSentenceKind.LetsTrade);
        Assert.NotNull(trade);
        Assert.True(discussPanel.ChooseDiscussionTopic(trade));
      }
    }

    [Test]
    public void TestBack()
    {
      var modelWhatUp = ChooseWhatsUp(DiscussionFactory.MerchantLionelName);
      modelWhatUp = discussPanel.GetTopic(KnownSentenceKind.WhatsUp);
      Assert.Null(modelWhatUp);//submenu shall not have it
      Assert.Greater(discussPanel.BoundTopics.Count, 0);
      //Assert.AreEqual(modelWhatUpChildCount, discussPanel.BoundTopics.Count);
      var back = discussPanel.GetTopicModel(KnownSentenceKind.Back);
      Assert.NotNull(back);

      //choose back topic
      discussPanel.ChooseDiscussionTopic(back.Item);
      modelWhatUp = discussPanel.GetTopic(KnownSentenceKind.WhatsUp);
      Assert.NotNull(modelWhatUp);//main menu shall have it

    }

    [Test]
    public void TestWanda()
    {
      var modelWhatUp = ChooseWhatsUp(DiscussionFactory.MerchantWandaName);
            
      Assert.AreEqual(discussPanel.BoundTopics.Count, 2);

      //continue
      var questIntro = discussPanel.BoundTopics.TypedItems[0].Item.AsOuaDItem();
      Assert.AreEqual(questIntro.QuestKind, QuestKind.Unset);
      discussPanel.ChooseDiscussionTopic(questIntro);

      Assert.Greater(discussPanel.BoundTopics.Count, 1);
      var questItem = discussPanel.GetTopic(QuestKind.ToadstoolsForWanda);
      Assert.NotNull(questItem);
      Assert.True(discussPanel.ChooseDiscussionTopic(questItem));

      var quest = GameManager.OuadHero.GetQuest(QuestKind.ToadstoolsForWanda);
      Assert.AreEqual(quest.Status, QuestStatus.Accepted);

      Assert.AreEqual(discussPanel.BoundTopics.Count, 3);
      var prog = npc.Discussion.MainItem.GetTopic(KnownSentenceKind.QuestProgress);
      Assert.NotNull(prog);
      modelWhatUp = discussPanel.GetTopic(KnownSentenceKind.WhatsUp);
      Assert.Null(modelWhatUp);
      
      //add mashes
      var quantity = quest.GetQuestRequirement<LootQuestRequirement>().EntityQuantity;
      for (int i = 0; i < quantity; i++)
      {
        hero.Inventory.Add(new Mushroom(MushroomKind.RedToadstool));
      }
      Assert.AreEqual(quest.Status, QuestStatus.AwaitingReward);

      Assert.True(discussPanel.ChooseDiscussionTopic(KnownSentenceKind.QuestProgress));
      Assert.True(discussPanel.ChooseDiscussionTopic(KnownSentenceKind.AwaitingReward));
      Assert.AreEqual(quest.Status, QuestStatus.Done);

      Assert.Null(discussPanel.GetTopic(KnownSentenceKind.QuestProgress));
    }

    [Test]
    public void TestSendHound()
    {
      CreateWorld();
      hero = GameManager.OuadHero;
      npc = GameManager.GetNPC(DiscussionFactory.MerchantLionelName);

      discussPanel = new OuaDII.Discussions.DiscussPanel(GameManager.QuestManager, hero);
      discussPanel.BindTopics(npc.Discussion.MainItem, npc);

      var sell = discussPanel.GetTopic(KnownSentenceKind.SellHound);
      Assert.NotNull(sell);
      var oldTopicsCount = discussPanel.BoundTopics.Count;
      discussPanel.ChooseDiscussionTopic(sell);
      Assert.AreEqual(oldTopicsCount, discussPanel.BoundTopics.Count);

      sell = discussPanel.GetTopic(KnownSentenceKind.SellHound);
      Assert.NotNull(sell);
    }


    protected override void GotoPit(string pitName)
    {
      base.GotoPit(pitName);
      
    }

    private OuaDII.Discussions.DiscussPanel CreateZiemowitDiscussPanel(string npcName, bool createWorld = true)
    {
      if (createWorld)
        CreateWorld();
      hero = GameManager.OuadHero;
      npc = GetNPC(DiscussionFactory.MerchantZiemowitName, false);
      //GotoPit("pit_down_Smiths");

      Assert.NotNull(npc);
      return CreateDiscussPanel();
    }

    private OuaDII.Discussions.DiscussPanel CreateDiscussPanel()
    {
      discussPanel = new OuaDII.Discussions.DiscussPanel(GameManager.QuestManager, hero);
      discussPanel.BindTopics(npc.Discussion.MainItem, npc);
      return discussPanel;
    }

    [Test]
    [Repeat(1)]
    public void TestZiemowit()
    {
      var discussPanel = CreateZiemowitDiscussPanel(DiscussionFactory.MerchantZiemowitName);

      var quest = GameManager.OuadHero.GetQuest(QuestKind.IronOreForSmith);
      Assert.Null(quest);
            
      Func<OuaDII.Discussions.DiscussionTopic> getQuestParentTopic = () =>
      {
        var npc = GetNPC(DiscussionFactory.MerchantZiemowitName);
        var items = npc.Discussion.MainItem.AsOuaDItem().Topics;
        var par = items.Where(i => i.AsOuaDItem().ParentForQuest == QuestKind.IronOreForSmith).FirstOrDefault();
        return par.AsOuaDItem();
      };

      var questTopic = getQuestParentTopic();
      Assert.NotNull(questTopic);

      //accept it 
      discussPanel.ChooseDiscussionTopic(questTopic);
      Assert.Greater(discussPanel.BoundTopics.Count, 1);
      var items = discussPanel.BoundTopics.TypedItems;
      discussPanel.ChooseDiscussionTopic(items[0].Item);
      Assert.Greater(discussPanel.BoundTopics.Count, 1);

      var whereToFindIt = items[0].Item;
      discussPanel.ChooseDiscussionTopic(whereToFindIt);

      items = discussPanel.BoundTopics.TypedItems;
      var questAccItem = items[0].Item;
      Assert.AreEqual(questAccItem.RightKnownSentenceKind, KnownSentenceKind.QuestAccepted);
      discussPanel.ChooseDiscussionTopic(questAccItem);

      quest = GameManager.OuadHero.GetQuest(QuestKind.IronOreForSmith);
      Assert.NotNull(quest);

      questTopic = getQuestParentTopic();
      Assert.Null(questTopic);

      CheckZiemowitParent(discussPanel, npc);

      //save and load
      Reload(DiscussionFactory.MerchantZiemowitName);
      discussPanel = CreateDiscussPanel();
      CheckZiemowitParent(discussPanel, npc);

      questTopic = getQuestParentTopic();
      Assert.Null(questTopic);

      //do it
      var quantity = quest.GetQuestRequirement<LootQuestRequirement>().EntityQuantity;
      for (int i = 0; i < quantity; i++)
      {
        hero.Inventory.Add(new MinedLoot(MinedLootKind.IronOre));
      }
      quest = GameManager.OuadHero.GetQuest(QuestKind.IronOreForSmith);
      Assert.AreEqual(quest.Status, QuestStatus.AwaitingReward);

      discussPanel = CreateDiscussPanel(DiscussionFactory.MerchantZiemowitName, false);
      Assert.NotNull(discussPanel.GetTopic(KnownSentenceKind.QuestProgress));
      Assert.True(discussPanel.ChooseDiscussionTopic(KnownSentenceKind.QuestProgress));
      Assert.True(discussPanel.ChooseDiscussionTopic(KnownSentenceKind.AwaitingReward));
      Assert.AreEqual(quest.Status, QuestStatus.Done);

      Assert.AreEqual(discussPanel.BoundTopics.TypedItems[0].Item.Parent, npc.Discussion.MainItem);

      Assert.Null(discussPanel.GetTopic(KnownSentenceKind.QuestProgress));
      questTopic = getQuestParentTopic();
      Assert.Null(questTopic);

      Reload(DiscussionFactory.MerchantZiemowitName);
      questTopic = getQuestParentTopic();
      Assert.Null(questTopic);

      //TODO check after reward panel scrolled to main menu
    }

    private static void CheckZiemowitParent(OuaDII.Discussions.DiscussPanel discussPanel, NPC npc)
    {
      var to = discussPanel.GetTopic(KnownSentenceKind.QuestProgress);
      Assert.NotNull(to);
      Assert.AreEqual(to.Topics.Count, 2);
      Assert.NotNull(to.Topics[0].Parent);
      Assert.AreEqual(to.Topics[1].Parent, npc.Discussion.MainItem);
    }

    [Test]
    public void TestLionel()
    {
      var modelWhatUp = ChooseWhatsUp(DiscussionFactory.MerchantLionelName);
      //modelWhatUp.To
      var quest = GameManager.OuadHero.GetQuest(QuestKind.HourGlassForMiller);
      Assert.Null(quest);

      //continue
      discussPanel.ChooseDiscussionTopic(discussPanel.BoundTopics.TypedItems[0].Item);
      Assert.Greater(discussPanel.BoundTopics.Count, 1);

      Assert.True(discussPanel.ChooseDiscussionTopic(discussPanel.BoundTopics.TypedItems[0].Item));//Where would I find the main dungeon

      var questParentItem = discussPanel.GetParentTopic(QuestKind.HourGlassForMiller);
      Assert.NotNull(questParentItem);

      discussPanel.ChooseDiscussionTopic(questParentItem);
      var questItem = discussPanel.GetTopic(QuestKind.HourGlassForMiller);
      Assert.NotNull(questItem);

      Assert.True(discussPanel.ChooseDiscussionTopic(questItem));
      quest = GameManager.OuadHero.GetQuest(QuestKind.HourGlassForMiller);
      Assert.AreEqual(quest.Status, QuestStatus.Accepted);

      CheckQuestStatus(QuestKind.HourGlassForMiller, KnownSentenceKind.WorkingOnQuest);

      var bye = discussPanel.GetTopic(KnownSentenceKind.Bye);
      Assert.NotNull(bye);
      Assert.AreEqual(bye.Parent, npc.Discussion.MainItem);//main menu

      var whatUp = discussPanel.BoundTopics.TypedItems[0].Item;
      Assert.AreEqual(whatUp.RightKnownSentenceKind, KnownSentenceKind.WhatsUp);
      discussPanel.ChooseDiscussionTopic(whatUp);
      questParentItem = discussPanel.GetParentTopic(QuestKind.HourGlassForMiller);
      Assert.Null(questParentItem);//in progress

      CheckQuestStatus(QuestKind.HourGlassForMiller, KnownSentenceKind.WorkingOnQuest);

      GameManager.Save(false);
      GameManager.Load(GameManager.OuadHero.Name, false);
      hero = GameManager.OuadHero;
      npc = GameManager.GetNPC(DiscussionFactory.MerchantLionelName);
      CheckQuestStatus(QuestKind.HourGlassForMiller, KnownSentenceKind.WorkingOnQuest);
    }

    private OuaDII.Discussions.DiscussionTopic CheckQuestStatus(QuestKind kind, KnownSentenceKind status)
    {
      var topic = npc.Discussion.MainItem.GetTopic(KnownSentenceKind.QuestProgress);
      Assert.NotNull(topic);
      var ouadTopic = topic.AsOuaDItem();
      Assert.AreEqual(ouadTopic.QuestKind, kind);
      Assert.AreEqual(ouadTopic.Topics[0].RightKnownSentenceKind, status);
      return ouadTopic;
    }

    private static void ChooseWhatsUpTopic(OuaDII.Discussions.DiscussPanel discussPanel, Roguelike.Discussions.DiscussionTopic modelWhatUp)
    {
      discussPanel.ChooseDiscussionTopic(modelWhatUp);
      Assert.Greater(discussPanel.BoundTopics.Count, 1);
      var back = discussPanel.GetTopic(KnownSentenceKind.Back);
      Assert.NotNull(back);
      Assert.AreNotEqual(discussPanel.BoundTopics.TypedItems[0].Item.RightKnownSentenceKind, KnownSentenceKind.Back);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void TestLudoslaw(bool getReward)
    {
      var modelWhatUp = ChooseWhatsUp(DiscussionFactory.NPCLudoslawName);
      Assert.AreEqual(modelWhatUp.Topics.Count, 3);

      for (int i = 0; i < 2; i++)
      {
        //TODO move to gen
        var en = new Enemy(GameManager.Container);
        en.Name = "hornet";
        en.tag1 = "hornet";
        en.Herd = QuestManager.HornetsHerdApiaryHerd;
        GameManager.CurrentNode.SetTile(en, GameManager.CurrentNode.GetRandomEmptyTile(Dungeons.TileContainers.DungeonNode.EmptyCheckContext.DropLoot).point);
        GameManager.EnemiesManager.AllEntities.Add(en);
        GameManager.CurrentNode.HiddenTiles.Ensure(QuestManager.HornetsHerdApiaryMap).Add(en);
      }

      var progress = ChooseQuest(QuestKind.Hornets);

      Assert.AreEqual(discussPanel.BoundTopics.Count, 2);

      var hornets = GameManager.CurrentNode.GetTiles<Enemy>().Where(i => i.Herd.StartsWith("Hornet")).ToList();
      var herdOnes = hornets.Where(i => i.Herd == QuestManager.HornetsHerdApiaryHerd).ToList();
      var quest = GameManager.OuadHero.GetQuest(QuestKind.Hornets);
      Assert.NotNull(quest);
      var quantity = quest.GetQuestRequirement<EnemyQuestRequirement>().EnemyQuantity;
      Assert.GreaterOrEqual(herdOnes.Count, quantity);

      Assert.AreEqual(quest.Status, QuestStatus.Accepted);
      Assert.AreEqual(quest.RewardLootKind, LootKind.Potion);
      for (int i = 0; i < quantity; i++)
        KillEnemy(herdOnes.Where(e => e.Alive).First());

      Assert.AreEqual(quest.Status, QuestStatus.AwaitingReward);
      discussPanel.BindTopics(npc.Discussion.MainItem, npc);
      progress = npc.Discussion.MainItem.GetTopic(KnownSentenceKind.QuestProgress);
      Assert.NotNull(progress);
      discussPanel.ChooseDiscussionTopic(progress);
      Assert.AreEqual(discussPanel.BoundTopics.Count, 2);
      discussPanel.ChooseDiscussionTopic(KnownSentenceKind.RewardDeny);
      Assert.AreEqual(discussPanel.BoundTopics.Count, 2);

      //
      var item = discussPanel.BoundTopics.TypedItems[0].Item;
      Assert.True(item.Right.Body.Contains("Where is my reward?"));
      discussPanel.ChooseDiscussionTopic(item);
      Assert.AreEqual(quest.Status, QuestStatus.AwaitingReward);
      Assert.AreEqual(discussPanel.BoundTopics.Count, 3);

      int invCount = GameManager.Hero.Inventory.ItemsCount;
      Assert.AreEqual(invCount, 0);

      if(getReward)
        Assert.True(discussPanel.ChooseDiscussionTopic(KnownSentenceKind.AwaitingRewardAfterRewardDeny));
      else
        Assert.True(discussPanel.ChooseDiscussionTopic(KnownSentenceKind.RewardSkipped));

      Assert.AreEqual(quest.Status, QuestStatus.Done);
      if (getReward)
        Assert.AreEqual(GameManager.Hero.Inventory.ItemsCount, 1);
      else
        Assert.AreEqual(GameManager.Hero.Inventory.ItemsCount, 0);

      Assert.Null(discussPanel.GetTopic(KnownSentenceKind.QuestProgress));
      //GameManager.QuestManager.RewardHero(merchant, QuestKind.Hornets);
    }

    private Roguelike.Discussions.DiscussionTopic ChooseWhatsUp(string npcName)
    {
      CreateDiscussPanel(npcName);

      var modelWhatUp = discussPanel.GetTopic(KnownSentenceKind.WhatsUp);
      Assert.NotNull(modelWhatUp);
      //choose it
      ChooseWhatsUpTopic(discussPanel, modelWhatUp);

      return modelWhatUp;
    }

    private OuaDII.Discussions.DiscussPanel CreateDiscussPanel(string npcName, bool createWorld = true)
    {
      if(createWorld)
        CreateWorld();
      hero = GameManager.OuadHero;
      npc = GameManager.GetNPC(npcName);

      discussPanel = new OuaDII.Discussions.DiscussPanel(GameManager.QuestManager, hero);
      discussPanel.BindTopics(npc.Discussion.MainItem, npc);
      return discussPanel;
    }

    [Test]
    public void TestNaslaw()
    {
      var modelWhatUp = ChooseWhatsUp(DiscussionFactory.NPCNaslawName);

      Assert.AreEqual(GameManager.AlliesManager.AllAllies.Count(), 0);

      discussPanel.ChooseDiscussionTopic(discussPanel.BoundTopics.TypedItems[0].Item);

      var questItems = discussPanel.GetTopics(QuestKind.CreatureInPond).ToList();
      Assert.True(questItems.Any());
      var topic = questItems.Where(i => i.Right.Body == "I take the hound").SingleOrDefault();
      var progress = ChooseQuest(QuestKind.CreatureInPond, ()=> {
        return discussPanel.GetTopic("I take the hound");
      });
            
      Assert.AreEqual(discussPanel.BoundTopics.Count, 2);
      Assert.AreEqual(GameManager.AlliesManager.AllAllies.Count(), 1);

      //TODO move to gen
      var en = new Enemy(GameManager.Container);
      en.Name = QuestManager.PondCreatureName;
      en.tag1 = "pond_creature_ch";
      en.Herd = QuestManager.PondCreatureHerd;
      GameManager.CurrentNode.SetTile(en, GameManager.CurrentNode.GetRandomEmptyTile( Dungeons.TileContainers.DungeonNode.EmptyCheckContext.DropLoot).point);
      GameManager.EnemiesManager.AllEntities.Add(en);

      KillEnemy(GameManager.CurrentNode.GetTiles<Enemy>().Where(i => i.Name == QuestManager.PondCreatureName).Single());
      var quest = GameManager.OuadHero.GetQuest(QuestKind.CreatureInPond);
      Assert.AreEqual(quest.Status, QuestStatus.AwaitingReward);
      int invCount = GameManager.Hero.Inventory.ItemsCount;
      Assert.AreEqual(invCount, 0);
      GameManager.QuestManager.RewardHero(npc, QuestKind.CreatureInPond);
      Assert.AreEqual(GameManager.Hero.Inventory.ItemsCount, 0);//hound chosen

    }

    private Roguelike.Discussions.DiscussionTopic ChooseQuest(QuestKind questKind, Func<Roguelike.Discussions.DiscussionTopic> questFunc = null)
    {
      discussPanel.ChooseDiscussionTopic(discussPanel.BoundTopics.TypedItems[0].Item);
      Roguelike.Discussions.DiscussionTopic topic;
      if (questFunc != null)
      {
        topic = questFunc();
      }
      else
        topic = discussPanel.GetTopic(questKind);
      discussPanel.ChooseDiscussionTopic(topic);

      //WhatsUp shall be gone
      modelWhatUp = discussPanel.GetTopic(KnownSentenceKind.WhatsUp);
      Assert.Null(modelWhatUp);
      var progress = npc.Discussion.MainItem.GetTopic(KnownSentenceKind.QuestProgress);
      Assert.NotNull(progress);
      return progress;
    }
  }
}
