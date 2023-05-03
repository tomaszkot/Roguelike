using Dungeons.Tiles;
using NUnit.Framework;
using OuaDII.Discussions;
using OuaDII.Extensions;
using OuaDII.Managers;
using OuaDII.Quests;
using OuaDII.TileContainers;
using OuaDII.Tiles.LivingEntities;
using OuaDII.Tiles.Looting;
using Roguelike.Discussions;
using Roguelike.Extensions;
using Roguelike.Quests;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.Looting;
using System.Linq;

namespace OuaDIIUnitTests
{
  [TestFixture]
  class QuestTests : TestBase
  {
    [Test]
    public void TestGeneral()
    {
      CreateWorld();

      var hero = GameManager.Hero as Hero;
      Assert.AreEqual(hero.Quests.Count, 0);
      GameManager.QuestManager.EnsureQuestAssigned(QuestKind.IronOreForSmith);
      Assert.AreEqual(hero.Quests.Count, 1);
      GameManager.QuestManager.EnsureQuestAssigned(QuestKind.IronOreForSmith);
      Assert.AreEqual(hero.Quests.Count, 1);
      Assert.AreEqual(hero.Quests[0].Tag, QuestKind.IronOreForSmith.ToDescription());
      Assert.AreEqual(hero.Quests[0].Status, QuestStatus.Accepted);

      GameManager.Save();
      hero.Quests.Clear();
      GameManager.Load(hero.Name);
      hero = GameManager.Hero as Hero;
      Assert.AreEqual(hero.Quests.Count, 1);
    }

    //[Test]
    //public void TestSerialization()
    //{
    //  var disc = Discussion.CreateForLionel(true);
    //  disc.ToXml();
    //  var loaded = Discussion.FromXml(disc.EntityName);
    //  Assert.AreEqual(loaded.MainItem.Topics.Count, disc.MainItem.Topics.Count);
    //}

    [Test]
    public void WarewolfHarassingVillageTest()
    {
      var questKind = QuestKind.WarewolfHarassingVillage;

      CreateWorld();
      var hero = GameManager.Hero as Hero;
      var npc = new NPC(Container);
      Assert.NotNull(npc.Discussion);
      npc.Discussion.MainItem.InsertTopic(this.Container.GetInstance<DiscussionFactory>().CreateWarewolfHarassingVillage());
      Assert.AreEqual(hero.Quests.Count, 0);
      npc.AcceptQuest(GameManager.QuestManager, hero, questKind);

    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void PaladinRoslawTest(bool assignQuest)
    {
      CreateWorld();
      var hero = GameManager.Hero as Hero;
      var npc = new Paladin(GameManager.Container);// _Container);
      npc.Name += " Roslaw";
      npc.tag1 = "paladin__name__Roslaw";

      npc.Discussion = this.Container.GetInstance<DiscussionFactory>().Create(npc);
      var discussPanel = CreatePanel(hero, npc);
      Assert.AreEqual(hero.Quests.Count, 0);

      if (assignQuest)
        GameManager.QuestManager.EnsureQuestAssigned(QuestKind.StonesMine);

      var startTopic = discussPanel.GetTopic("Why are you hiding here?");//Why are you hiding here?
      Assert.NotNull(startTopic);
      discussPanel.ChooseDiscussionTopic(startTopic);

      Assert.False(GameManager.GameState.History.WasEngaged("paladin__name__Roslaw"));
      Assert.AreEqual(GameManager.AlliesManager.AllAllies.Count(), 0);
      var okTopic = discussPanel.GetTopic(KnownSentenceKind.AllyAccepted);
      discussPanel.ChooseDiscussionTopic(KnownSentenceKind.AllyAccepted);
      Assert.AreEqual(GameManager.AlliesManager.AllAllies.Count(), 1);
      Assert.True(GameManager.GameState.History.WasEngaged("paladin__name__Roslaw"));

      InteractHeroWithPit(QuestKind.CrazyMiller);
      Assert.AreEqual(GameManager.AlliesManager.AllAllies.Count(), 1);
      var stairsPitUp = GameManager.CurrentNode.GetStairs(StairsKind.PitUp);
      GameManager.InteractHeroWith(stairsPitUp);
      Assert.AreEqual(GameManager.AlliesManager.AllAllies.Count(), 0);

      GameManager.QuestManager.RewardHero(GameManager.GetNPC(DiscussionFactory.NPCJosefName), QuestKind.StonesMine);
      if (assignQuest)
      {
        Assert.Greater(hero.Inventory.GetItems<Gem>().Count(), 0);
      }
      else
        Assert.AreEqual(hero.Inventory.GetItems<Gem>().Count(), 0);
    }

    [Test]
    public void SmithTestSilesiaCoalQuestAfterIronOre()
    {
      CreateWorld();
      var hero = GameManager.Hero as Hero;
      var discussPanel = CreatePanel(hero, "Ziemowit");

      var ironOreForSmithTopic = discussPanel.GetParentTopic(QuestKind.IronOreForSmith);
      Assert.NotNull(ironOreForSmithTopic);

      var silesiaCoalForSmith = discussPanel.GetParentTopic(QuestKind.SilesiaCoalForSmith);
      Assert.Null(silesiaCoalForSmith);

      //do one quest
      TestSmithQuest(QuestKind.IronOreForSmith, MinedLootKind.IronOre, ref discussPanel);

      //do other
      var eq = TestSmithQuest(QuestKind.SilesiaCoalForSmith, MinedLootKind.SilesiaCoal, ref discussPanel);
      Assert.AreEqual(eq.Material, EquipmentMaterial.Steel);
    }


    [Test]
    public void SmithTestIronOreForSmith()
    {
      var questKind = QuestKind.IronOreForSmith;
      CreateWorld();
      var hero = GameManager.Hero as Hero;
      var discussPanel = CreatePanel(hero, "Ziemowit");
      var eq = TestSmithQuest(questKind, MinedLootKind.IronOre, ref discussPanel);
      Assert.AreEqual(eq.Material, EquipmentMaterial.Iron);
    }

    private Equipment TestSmithQuest(QuestKind questKind, MinedLootKind lootKind, ref OuaDII.Discussions.DiscussPanel discussPanel)
    {
      var hero = GameManager.Hero as Hero;
      Assert.IsEmpty(hero.Inventory.GetItems().Where(i => i.SourceQuestKind == questKind.ToDescription()));

      var topic = discussPanel.GetParentTopic(questKind);
      Assert.NotNull(topic);
      discussPanel.ChooseDiscussionTopic(topic);

      //Where would I find...
      topic = discussPanel.BoundTopics.TypedItems[0].Item.AsOuaDItem();
      discussPanel.ChooseDiscussionTopic(topic);

      //do quest
      return DoSmithQuest(questKind, lootKind, ref discussPanel);
    }

    private OuaDII.Discussions.DiscussPanel CreatePanel(Hero hero, string merchantName)
    {
      var merchant = GetNPC(merchantName);
      Assert.NotNull(merchant.Discussion);
      Assert.AreEqual(hero.Quests.Count, 0);

      var discussPanel = new OuaDII.Discussions.DiscussPanel(GameManager.QuestManager, hero);
      discussPanel.BindTopics(merchant.Discussion.MainItem, merchant);
      return discussPanel;
    }

    private OuaDII.Discussions.DiscussPanel CreatePanel(Hero hero, Roguelike.Tiles.LivingEntities.INPC npc)
    {
      Assert.NotNull(npc.Discussion);
      
      var discussPanel = new OuaDII.Discussions.DiscussPanel(GameManager.QuestManager, hero);
      discussPanel.BindTopics(npc.Discussion.MainItem, npc);
      return discussPanel;
    }

    private Equipment DoSmithQuest
    (
      QuestKind questKind, MinedLootKind lootKind, ref OuaDII.Discussions.DiscussPanel discussPanel
    )
    {
      var merchant = GetNPC(DiscussionFactory.MerchantZiemowitName) as Merchant;
      var hero = GameManager.Hero as Hero;
      Assert.AreEqual(hero.Quests.Count, questKind == QuestKind.IronOreForSmith ? 0 : 1);
      var topic = discussPanel.GetTopic(questKind);
      Assert.NotNull(topic);
      discussPanel.ChooseDiscussionTopic(topic);
      Assert.AreEqual(topic.RightKnownSentenceKind, KnownSentenceKind.QuestAccepted); 

      Assert.AreEqual(hero.Quests.Count, questKind == QuestKind.IronOreForSmith ? 1 : 2);
      Assert.AreEqual(hero.Quests.Last().GetKind(), questKind);
      Assert.AreEqual(hero.Quests.Last().Status, QuestStatus.Accepted);

      Reload(DiscussionFactory.MerchantZiemowitName);
      merchant = GetNPC(DiscussionFactory.MerchantZiemowitName) as Merchant;
      hero = GameManager.Hero as Hero;
      discussPanel = CreatePanel(hero, merchant);
      var topicAfter = discussPanel.GetTopic(questKind);
      Assert.AreEqual(topicAfter.RightKnownSentenceKind, KnownSentenceKind.QuestProgress);
            
      Assert.AreNotEqual(topicAfter, topic);
      Assert.GreaterOrEqual(topicAfter.Topics.Count, 2);//bye, on progress

      bool fakeLoot = false;
      if (fakeLoot)
      {
        MinedLoot minedLoot = new MinedLoot(lootKind);
        minedLoot.Count = 10;
        hero.Inventory.Add(minedLoot);
      }
      else
      {
        var pitName = questKind == QuestKind.IronOreForSmith ? OuaDII.Generators.WorldGenerator.PitRats : OuaDII.Generators.WorldGenerator.PitBats;
        var pit = GotoNonQuestPit(GameManager.World, pitName);
        var stairsDown = GameManager.CurrentNode.GetStairs(StairsKind.LevelDown);
        GameManager.InteractHeroWith(stairsDown);
        var loot = GameManager.CurrentNode.GetTiles<MinedLoot>().Cast<MinedLoot>().Where(i => i.Kind == lootKind).ToList();
        var count = loot.Count();
        Assert.AreEqual(count, LootQuestRequirement.QuestLootQuantity(questKind) + LootQuestRequirement.QuestCheatingPunishmentLootQuantity(questKind));
        foreach (var lootItem in loot)
          hero.Inventory.Add(lootItem);
      }

      Assert.AreEqual(hero.Quests.Last().Status, QuestStatus.AwaitingReward);

      var exp = hero.Experience;
      var doneItem = topicAfter.GetTopicInChildren(questKind);
      Assert.AreEqual(doneItem.RightKnownSentenceKind, KnownSentenceKind.AwaitingReward);
      discussPanel.ChooseDiscussionTopic(doneItem);
      Assert.AreEqual(hero.Quests.Last().Status, QuestStatus.Done);
      Assert.Greater(hero.Experience, exp);

      var reward = hero.Inventory.GetItems().Where(i => i.SourceQuestKind == questKind.ToDescription()).SingleOrDefault();
      Assert.IsNotNull(reward);
      var sword = reward as Roguelike.Tiles.Weapon;
      Assert.IsNotNull(sword);

      topicAfter = merchant.OuaDDiscussion.GetTopicByQuest(questKind);
      Assert.Null(topicAfter);

      return sword;
    }

    [TestCase(true)]
    [TestCase(false)]
    public void SmithCheatTest(bool saveLoad)
    {
      CreateWorld();
      var hero = GameManager.Hero as Hero;
      Assert.True(GameManager.CurrentNode is World);
      InteractHeroWithPit(QuestKind.Smiths);
      Assert.False(GameManager.CurrentNode is World);
      var merchant = GetNPC("Ziemowit");
      var discussion = merchant.Discussion.AsOuaDDiscussion();

      //Accept Quest
      Assert.AreEqual(merchant.RelationToHero.Kind, Roguelike.Tiles.LivingEntities.RelationToHeroKind.Neutral);
      discussion.AcceptQuest(GameManager.QuestManager, QuestKind.IronOreForSmith.ToString());
      var ironOreForSmithQuest = hero.Quests[0];
      Assert.AreEqual(ironOreForSmithQuest.GetKind(), QuestKind.IronOreForSmith);

      //check description
      {
        var q1 = LootQuestRequirement.QuestLootQuantity(QuestKind.IronOreForSmith);
        Assert.AreEqual(hero.GetLootQuestRequirement(QuestKind.IronOreForSmith).LootQuantity, q1);
        Assert.True(ironOreForSmithQuest.Description.Contains(" " + q1 + " "));
      }
      //go up
      var stairsPitUp = GameManager.CurrentNode.GetStairs(StairsKind.PitUp);
      GameManager.InteractHeroWith(stairsPitUp);
      Assert.True(GameManager.CurrentNode is World);

      if (saveLoad)
      {
        GameManager.Save();
        GameManager.Load(hero.Name);
        hero = GameManager.Hero as Hero;
        merchant = null;
        discussion = null;
        ironOreForSmithQuest = hero.Quests[0];
      }

      //TODO, do quest
      GameManager.QuestManager.SetQuestAwaitingReward(ironOreForSmithQuest);

      InteractHeroWithPit(QuestKind.Smiths);
      merchant = GetNPC("Ziemowit");
      discussion = merchant.Discussion.AsOuaDDiscussion();

      //cheat
      discussion.EmitCheating(QuestKind.IronOreForSmith, merchant);

      //check again description (shall be 10->15)
      var cheatingQuantity = LootQuestRequirement.QuestLootQuantity(QuestKind.IronOreForSmith) + LootQuestRequirement.QuestCheatingPunishmentLootQuantity(QuestKind.IronOreForSmith);
      var required = hero.GetLootQuestRequirement(QuestKind.IronOreForSmith);
      Assert.AreEqual(required.LootQuantity, cheatingQuantity);
      Assert.True(ironOreForSmithQuest.Description.Contains(" " + cheatingQuantity + " "));

      Assert.AreEqual(ironOreForSmithQuest.Status, QuestStatus.Accepted);
      var item = discussion.GetTopicByQuest(QuestKind.IronOreForSmith);
      Assert.AreEqual(item.RightKnownSentenceKind, KnownSentenceKind.QuestProgress);
      Assert.AreEqual(item.Topics.Count, 2);//1) working on it, 2) bye
      
      Assert.AreEqual(merchant.RelationToHero.Kind, Roguelike.Tiles.LivingEntities.RelationToHeroKind.Neutral);
      discussion.EmitCheating(QuestKind.IronOreForSmith, merchant);
      Assert.AreEqual(merchant.RelationToHero.Kind, Roguelike.Tiles.LivingEntities.RelationToHeroKind.Antagonistic);
    }

    [Test]
    public void HourglassForMiller()
    {
      var questKind = QuestKind.HourGlassForMiller;

      CreateWorld();
      var hero = GameManager.Hero as Hero;
      Assert.AreEqual(hero.Inventory.ItemsCount, 0);
      var panel = CreatePanel(hero, "Lionel");
      Assert.IsEmpty(hero.Inventory.GetItems().Where(i => i.name == "Hourglass"));

      Assert.AreEqual(hero.Quests.Count, 0);
      var t1 = panel.GetTopic(KnownSentenceKind.WhatsUp);
      panel.ChooseDiscussionTopic(t1);
      var t2 = panel.BoundTopics.TypedItems[0].Item;
      panel.ChooseDiscussionTopic(t2);

      var t3 = panel.GetParentTopic(questKind);
      panel.ChooseDiscussionTopic(t3);
      var quest = panel.GetTopic(questKind);
      Assert.NotNull(quest);
      panel.ChooseDiscussionTopic(quest);

      Assert.AreEqual(hero.Quests.Count, 1);
      Assert.AreEqual(hero.Quests[0].Status, QuestStatus.Accepted);
      Assert.IsNotEmpty(hero.Inventory.GetItems().Where(i => i.name == "Hourglass"));

      KillMiller();
      Assert.AreEqual(hero.Quests[0].Status, QuestStatus.FailedToDo);
      quest = panel.GetTopic(questKind);
      quest.Right.Body.Contains("He is not interested");
      var questAward = quest.GetTopic(KnownSentenceKind.AwaitingReward);
      panel.ChooseDiscussionTopic(questAward);
      Assert.AreEqual(hero.Inventory.ItemsCount, 2);
      Assert.IsNotEmpty(hero.Inventory.GetItems().Where(i => i.name == "Hourglass"));

      var potion = hero.Inventory.GetItems().Where(i => i is Potion).Cast<Potion>().Single();
      Assert.NotNull(potion);
      Assert.AreEqual(potion.Kind, PotionKind.Special);

      Assert.AreEqual(hero.Quests[0].Status, QuestStatus.Done);
      
      //var merchant = GameManager.GetMerchant("Lionel");
      //Assert.NotNull(merchant.Discussion);

    }

    void KillMiller()
    {
      OuaDII.TileContainers.DungeonPit pit = InteractHeroWithPit(QuestKind.CrazyMiller);

      Assert.AreEqual(pit.Levels.Count, 1);
      var level = pit.Levels[0];

      var rooms = level.GeneratorNodes;
      Assert.AreEqual(rooms.Count, 2);
      var walls = rooms[0].GetTiles<Wall>();
      Assert.Greater(walls.Count, 0);//walls are gen by UI

      var inter = rooms[0].GetTiles<Roguelike.Tiles.Interactive.InteractiveTile>();
      Assert.Greater(inter.Count, 1);//stairs + smth else

      var stairsOnLevel = level.GetTiles<Stairs>();
      Assert.AreEqual(stairsOnLevel.Count, 2);
      Assert.NotNull(stairsOnLevel.Where(i => i.StairsKind == StairsKind.PitUp).SingleOrDefault());

      var stairsLeveDown = stairsOnLevel.Where(i => i.StairsKind == StairsKind.LevelDown).SingleOrDefault();
      Assert.NotNull(stairsLeveDown);
      GameManager.InteractHeroWith(stairsLeveDown);


      var enemies = GameManager.CurrentNode.GetTiles<Enemy>().ToList();
      var boss = enemies.Where(i => i.PowerKind == Roguelike.Tiles.LivingEntities.EnemyPowerKind.Boss).SingleOrDefault();
      Assert.NotNull(boss);
      Assert.AreEqual(boss.Name, "Miller Bratomir");
      KillEnemy(boss);
      //var gi = new Dungeons.GenerationInfo();
      ////  if (gi.ForcedNumberOfEnemiesInRoom >= 3)
      ////    Assert.Greater(enemies.Count, 3);
      //Assert.Less(enemies.Count, 18);

      //  var grouping = enemies.GroupBy(i => i.tag1).ToList();

      //  //if (first)
      //  //{
      //  //  //Assert.AreEqual(grouping.Count, 1);
      //  //  grouping.Any(i => i.Key == "bat" || i.Key == "rat");
      //  //}
      //  //else
      //  {
      //    Assert.Greater(grouping.Count, 1);
      //    grouping.Any(i => i.Key == "bat");
      //    grouping.Any(i => i.Key == "rat");
      //  }

      //  var plains = enemies.Where(i => i.PowerKind == Roguelike.Tiles.LivingEntities.EnemyPowerKind.Plain).ToList();
      //  Assert.Greater(plains.Count, 2);

      //  var champs = enemies.Where(i => i.PowerKind == Roguelike.Tiles.LivingEntities.EnemyPowerKind.Champion).ToList();
      //  Assert.Greater(champs.Count, 0);

      //  Assert.AreEqual(champs.Count, first ? 2 : 2);

      //  var boss = enemies.Where(i => i.PowerKind == Roguelike.Tiles.LivingEntities.EnemyPowerKind.Boss).ToList();
      //  Assert.Less(boss.Count, 2);
      //  if (first)
      //    Assert.Null(boss.FirstOrDefault());
      //  else
      //    Assert.NotNull(boss.FirstOrDefault());
      //};

      //verify(pit.Levels[0], true);

      //GameManager.InteractHeroWith(stairsLeveDown);
      //Assert.AreEqual(pit.Levels.Count, 2);
      //verify(pit.Levels[1], false);
    }


  }
}
