using OuaDII.Discussions;
using OuaDII.Extensions;
using OuaDII.Generators;
using OuaDII.Quests;
using OuaDII.TileContainers;
using OuaDII.Tiles;
using OuaDII.Tiles.LivingEntities;
using OuaDII.Tiles.Looting;
using OuaDII.Tiles.Looting.Equipment;
using Roguelike.Events;
using Roguelike.Extensions;
using Roguelike.LootContainers;
using Roguelike.Quests;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII.Managers
{
  public class QuestProgressDesc
  {
    public QuestKind QuestKind { get; set; }
  }

  public class QuestManager : Roguelike.Managers.QuestManager
  {
    public static string HornetsHerdApiaryHerd = "HornetsHerdApiary";
    public static string HornetsHerdApiaryMap = "Hidden_Apiary_Enemies";

    public static string PondCreatureHerd = "PondCreatureHerd";
    public static string PondCreatureName = "Pond Creature";
    public static string PondCreatureMap = "Hidden_Pond_Enemies";

    public static Roguelike.Tiles.LootKind LootKindForQuest = Roguelike.Tiles.LootKind.Unset;

    Dictionary<QuestKind, string> questDescriptions = new Dictionary<QuestKind, string>();
    Dictionary<string, QuestKind> questKindFromHerd = new Dictionary<string, QuestKind>() {
      { HornetsHerdApiaryHerd, QuestKind.Hornets },
      { PondCreatureHerd, QuestKind.CreatureInPond }
    };
    GameManager gm;

    public GameManager GameManager { get => gm; set => gm = value; }

    static QuestManager()
    {
    }

    public QuestKind GetQuestKindFromHerd(string herdName)
    {
      return questKindFromHerd.ContainsKey(herdName) ? questKindFromHerd[herdName] : QuestKind.Unset;
    }

    public QuestManager(GameManager gm)
    {
      this.gm = gm;
      questDescriptions[QuestKind.Hornets] = "Defeat hornets attacking Ludoslaw's beehive.";
      questDescriptions[QuestKind.ToadstoolsForWanda] = "Bring {0} red toadstools to Wanda. She will give you a recipe as a reward.";
      questDescriptions[QuestKind.IronOreForSmith] = "Bring {0} pieces of iron ore to smith. He promised to forge a sword as a reward.";
      questDescriptions[QuestKind.SilesiaCoalForSmith] = "Bring {0} pieces of silesia coal to the smith Ziemowit. He promised to forge a sword as a reward.";
      questDescriptions[QuestKind.HourGlassForMiller] = "Deliver a hourglass to the miller Bratomir.";
      questDescriptions[QuestKind.WarewolfHarassingVillage] = "One of villagers claims he saw warewolf taking prowling at night. I promised to take care of it.";
      questDescriptions[QuestKind.CreatureInPond] = "Naslaw says a creature is occupying the  village's pond making fishing not possible. Maybe I can deal with it.";
      questDescriptions[QuestKind.FernForDobromila] = "Bring a magic fern to Dobromila. She will give you a great potion.";
      questDescriptions[QuestKind.StonesMine] = "Go to stones mine, look for missing two paladins.";
      questDescriptions[QuestKind.KillBoar] = "Kill the boar in the Camp's enclosure.";
      questDescriptions[QuestKind.SacksOfBarley] = "Bring {0} sacks of barley to Tonik. You shall find them at Bratomir's mill.";
      //Naslaw
    }
        
    public string GetQuestDesc(QuestKind kind, QuestRequirement qr)
    {
      if (questDescriptions.ContainsKey(kind))
      {
        var str = questDescriptions[kind];
        if (qr != null && qr is LootQuestRequirement qlr)
          str = String.Format(str, qlr.LootQuantity);

        return str;
      }

      return "";
    }

    int CalcDoneExperienceAmount(QuestKind kind)
    {
      float doneExperienceAmount = 500;
      switch (kind)
      {
        case QuestKind.Unset:
          break;
        //case QuestKind.CrazyMiller://TODO these are not really quests
        //  break;
        //case QuestKind.Smiths:
        //  break;
        case QuestKind.StonesMine:
          doneExperienceAmount *= 4;
          break;
        case QuestKind.Malbork:
          break;
        case QuestKind.IronOreForSmith:
          doneExperienceAmount *= 3;
          break;
        case QuestKind.SilesiaCoalForSmith:
          doneExperienceAmount *= 2;
          break;
        case QuestKind.HourGlassForMiller:
          break;
        case QuestKind.WarewolfHarassingVillage:
          break;
        //case QuestKind.GatheringEntry:
        //  doneExperienceAmount *= 10;
        //  break;
        case QuestKind.ToadstoolsForWanda:
          doneExperienceAmount *= 5;
          break;
        case QuestKind.Hornets:
          doneExperienceAmount *= 6;
          break;
        case QuestKind.CreatureInPond:
          doneExperienceAmount *= 5;
          break;
        case QuestKind.FernForDobromila:
          doneExperienceAmount *= 6;
          break;
        case QuestKind.RescueJurantDaughter:
          doneExperienceAmount *= 7;
          break;
        case QuestKind.KillBoar:
          doneExperienceAmount = 100;
          break;

        case QuestKind.SacksOfBarley:
          doneExperienceAmount = 300;
          break;
        default:
          break;
      }

      return (int)doneExperienceAmount;
    }

    public Quest CreateQuest(QuestKind kind)
    {
      var quest = new Quest();
      quest.Tag = GetTagFromKind(kind);
      quest.DoneExperienceAmount = CalcDoneExperienceAmount(kind);
      quest.Status = Roguelike.Quests.QuestStatus.Proposed;
      QuestRequirement qr = null;
      if (kind == QuestKind.IronOreForSmith || kind == QuestKind.SilesiaCoalForSmith || kind == QuestKind.ToadstoolsForWanda ||
          kind == QuestKind.FernForDobromila || kind == QuestKind.SacksOfBarley)
      {
        qr = CreateQuestLootRequirement(kind, quest);
      }
      else if (kind == QuestKind.Hornets || kind == QuestKind.CreatureInPond)
      {
        qr = CreateQuestEnemyRequirement(kind, quest);
        if (kind == QuestKind.Hornets)
        {
          quest.RewardLootKind = LootKindForQuest;
          LootKindForQuest = Roguelike.Tiles.LootKind.Unset;
        }
      }
      
      quest.Description = GetQuestDesc(kind, qr);

      if (quest.GetKind() == QuestKind.StonesMine)
        quest.QuestPrincipalName = DiscussionFactory.NPCJosefName;
      else if (quest.GetKind() == QuestKind.KillBoar)
        quest.QuestPrincipalName = DiscussionFactory.NPCJulekName;
      return quest;
    }

    public static QuestRequirement CreateQuestEnemyRequirement(QuestKind kind, Quest quest)
    {
      var lr = CreateQuestEnemyRequirement(kind);
      quest.QuestRequirement = lr;
      if (kind == QuestKind.Hornets)
      {
        quest.QuestPrincipalName = DiscussionFactory.NPCLudoslawName;
      }

      else if (kind == QuestKind.CreatureInPond)
      {
        quest.QuestPrincipalName = DiscussionFactory.NPCNaslawName;
      }

      return lr;
    }

    public static EnemyQuestRequirement CreateQuestEnemyRequirement(QuestKind kind)
    {
      if (kind == QuestKind.Hornets)
      {
        return new EnemyQuestRequirement("Hornet", EnemyQuestRequirement.QuestEnemyQuantity(kind), QuestManager.HornetsHerdApiaryHerd);
      }
      else if (kind == QuestKind.CreatureInPond)
      {
        return new EnemyQuestRequirement(QuestManager.PondCreatureName, EnemyQuestRequirement.QuestEnemyQuantity(kind), QuestManager.PondCreatureHerd);
      }
      return null;
    }

    public static LootQuestRequirement CreateQuestLootRequirement(QuestKind kind, Quest quest)
    {
      LootQuestRequirement lr = CreateQuestLootRequirement(kind);
      quest.QuestRequirement = lr;
      if(kind == QuestKind.IronOreForSmith || kind == QuestKind.SilesiaCoalForSmith)
        quest.QuestPrincipalName = DiscussionFactory.MerchantZiemowitName;
      else if (kind == QuestKind.ToadstoolsForWanda)
        quest.QuestPrincipalName = DiscussionFactory.MerchantWandaName;
      else if (kind == QuestKind.FernForDobromila)
        quest.QuestPrincipalName = DiscussionFactory.MerchantDobromilaName;

      else if (kind == QuestKind.SacksOfBarley)
        quest.QuestPrincipalName = DiscussionFactory.NPCTonikName;

      return lr;
    }

    public static Quests.QuestKind GetQuestKindFromPitName(string pitName)
    {
      Quests.QuestKind questKind = QuestKind.Unset;
      if (pitName == WorldGenerator.PitRats)
        questKind = Quests.QuestKind.IronOreForSmith;
      if (pitName == WorldGenerator.PitBats)
        questKind = QuestKind.SilesiaCoalForSmith;
      return questKind;
    }

    public static LootQuestRequirement CreateQuestLootRequirement(QuestKind kind)
    {
      var lr = new LootQuestRequirement();

      if (kind == QuestKind.IronOreForSmith || kind == QuestKind.SilesiaCoalForSmith)
      {
        MinedLootKind minedLootKind = GetMinedLootKind(kind);
        lr.LootName = new MinedLoot(minedLootKind).Name;
      }
      else
      {
        if (kind == QuestKind.ToadstoolsForWanda)
          lr.LootName = new Mushroom(MushroomKind.RedToadstool).Name;
        if (kind == QuestKind.FernForDobromila)
          lr.LootName = DiscussionFactory.MagicFern;
      }

      lr.LootQuantity = LootQuestRequirement.QuestLootQuantity(kind);
      return lr;
    }

    public static MinedLootKind GetMinedLootKind(QuestKind kind)
    {
      MinedLootKind minedLootKind = MinedLootKind.IronOre;
      if (kind == QuestKind.SilesiaCoalForSmith)
        minedLootKind = MinedLootKind.SilesiaCoal;
      return minedLootKind;
    }

    public static QuestKind GetQuestKind(string pitName)
    {
      var quests = Enum.GetValues(typeof(QuestKind)).Cast<QuestKind>();
      foreach (var quest in quests)
      {
        if (pitName.Contains(quest.ToString()))
          return quest;
      }

      return QuestKind.Unset;
    }

    public static bool IsPitWithQuest(string pitName)
    {
      var kind = GetQuestKind(pitName);
      return kind != QuestKind.Unset;
    }

    public override bool EnsureQuestAssigned(string questKindStr)
    {
      var questKind = Discussion.QuestKindFromString(questKindStr);
      return EnsureQuestAssigned(questKind);
    }

    public bool EnsureQuestAssigned(QuestKind questKind)
    {
      var quest = GetHeroQuest(questKind);
      if (quest == null)
      {
        quest = CreateQuest(questKind);
        quest.Status = QuestStatus.Accepted;
        gm.Hero.Quests.Add(quest);

        if (questKind == QuestKind.HourGlassForMiller)
        {
          var loot = gm.LootGenerator.GetLootByAsset("hour_glass");
          gm.Hero.Inventory.Add(loot);
        }
        else if (questKind == Quests.QuestKind.KillBoar)
        {
          var club = gm.LootGenerator.GetLootByAsset("club") as Roguelike.Tiles.Looting.Weapon;
          club.MakeEnchantable(1);
          gm.Hero.Inventory.Add(club);
        }
        gm.EnsureAutoSave(AutoSaveContextValues.QuestAssigned);
      }

      return true;
    }

            

    public Quest GetHeroQuest(QuestKind questKind)
    {
      return gm.OuadHero.GetQuest(questKind);
    }

    public static string GetTagFromKind(QuestKind kind)
    {
      return OuaDII.Extensions.QuestExt.Kind2String(kind);
    }

    //Dictionary<MinedLootKind, QuestKind> questKind;

    public void HandleGameAction(GameEvent e)
    {
      var hero = gm.OuadHero;

      if (e is InventoryAction ia)
      {
        if (ia.Inv.InvBasketKind == InvBasketKind.Hero && ia.Kind == InventoryActionKind.ItemAdded)
        {
          var quest = hero.GetQuest(ia.Loot.Name);
          if (quest == null || quest.Status != Roguelike.Quests.QuestStatus.Accepted)
            return;

          HandleQuestStatus(quest);
        }
      }
    }

    public void HandleQuestStatus(QuestKind questKind)
    {
      var quest = GetHeroQuest(questKind);
      if (quest == null)
        return;
      HandleQuestStatus(quest);
    }

    public void HandleQuestStatus(Quest quest)
    {
      var questKind = quest.GetKind();
      if (IsQuestDone(questKind))
      {
        SetQuestAwaitingReward(quest);
      }
    }

    public void SetQuestAwaitingReward(QuestKind kind)
    {
      var quest = GetHeroQuest(kind);
      SetQuestAwaitingReward(quest);
    }

    public void SetQuestAwaitingReward(Quest quest)
    {
      if (quest == null)
        return;
      quest.Status = QuestStatus.AwaitingReward;
      var npc = gm.GetNPC(quest.QuestPrincipalName);
      
      var disc = npc.Discussion.AsOuaDDiscussion();
      disc.UpdateQuestDiscussion(quest.GetKind(), Roguelike.Discussions.KnownSentenceKind.AwaitingReward, null, quest);
      gm.AppendAction(new QuestAction() { QuestID = (int)quest.GetKind(), QuestActionKind = QuestActionKind.AwaitingReward, Info = "Quest done" });
    }

    bool IsQuestDone(QuestKind questKind)
    {
      var quest = GetHeroQuest(questKind);
      if (quest == null)
        return false;

      if (questKind == QuestKind.HourGlassForMiller)
      {
        return quest.Status == QuestStatus.FailedToDo;
      }
      else if (questKind == QuestKind.KillBoar)
      {
        return quest.Status == QuestStatus.AwaitingReward;
      }
      else if (questKind == QuestKind.StonesMine)
      {
        return GameManager.GameState.History.WasEngaged("paladin__name__Roslaw");
      }

      var hero = gm.OuadHero;
      var count = -1;
      var qr = quest.QuestRequirement as QuantityQuestRequirement;
      if (qr == null)
        return false;

      var lqr = qr as LootQuestRequirement;
      if (lqr != null)
      {
        count = hero.Inventory.GetStackedCount(lqr.LootName);
      }
      else
      {
        var eqr = quest.GetQuestRequirement<EnemyQuestRequirement>();
        if (eqr != null)
          count = GameManager.GameState.History.LivingEntity.CountByHerd(eqr.EnemyHerd);
      }
       
      return count >= qr.EntityQuantity;
    }

    public void RewardHero
    (
      Roguelike.Tiles.LivingEntities.INPC npc, 
      QuestKind questKind, 
      Roguelike.Discussions.KnownSentenceKind knownSentenceKind = Roguelike.Discussions.KnownSentenceKind.Unset
    )
    {
      var quest = GetHeroQuest(questKind);
      if (quest == null)
        return;
      bool forceAllDone = false;

      if(!quest.SkipReward)
        quest.SkipReward = knownSentenceKind == Roguelike.Discussions.KnownSentenceKind.RewardSkipped;

      if (IsQuestDone(questKind))
      {
        var rewards = quest.SkipReward ? null : GetRewards(questKind, quest);
        bool allDone = false;
        if (quest.SkipReward)
          allDone = true;
        else
        {
          foreach (var reward in rewards)
          {
            var added = gm.Hero.Inventory.Add(reward);
            if (!added)
            {
              var target = GameManager.CurrentNode.GetClosestEmpty(gm.Hero, true, true);
              GameManager.AddLootToNode(reward, target, false);
              GameManager.SoundManager.PlaySound(reward.DroppedSound);
            }
          }
          if (rewards.Any())
          {
            var desc = gm.Hero.Name+ " received quest reward: "+ rewards.First().Name;
            //if (rewards.Count > 0)
            //  desc += " x"+ rewards.Count;
            GameManager.AppendAction(new GameEvent(desc, ActionLevel.Important));
          }
          allDone = true;
          RemoveQuestLoot(quest);
        }
        if (allDone || forceAllDone)
        {
          quest.Status = QuestStatus.Done;
          GameManager.Hero.IncreaseExp(quest.DoneExperienceAmount);

          var ouadDisc = npc.Discussion.AsOuaDDiscussion();
          ouadDisc.RemoveDoneQuest(questKind);
          GameManager.AppendAction(new QuestAction() { Info = "Quest finished: " + questKind.ToDescription(), Level = ActionLevel.Important, QuestID = (int)questKind,
            QuestActionKind = QuestActionKind.Done
           });
        }
      }
    }

    private List<Loot> GetRewards(QuestKind questKind, Quest quest)
    {
      List<Loot> rewards = new List<Loot>();
      Loot reward = null;
      if (questKind == QuestKind.HourGlassForMiller)
        reward = new SpecialPotion(SpecialPotionKind.Strength, SpecialPotionSize.Small);
      else if (questKind == QuestKind.StonesMine)
      {
        for (int i = 0; i < 3; i++)
        {
          var gem = new Gem(gm.Hero.Level);
          rewards.Add(gem);
        }
      }
      else if (questKind == QuestKind.ToadstoolsForWanda)
        reward = new Recipe(RecipeKind.Toadstools2Potion);
      else if (questKind == QuestKind.FernForDobromila)
      {
        reward = new SpecialPotion(SpecialPotionKind.Strength, SpecialPotionSize.Big);
      }
      else if (questKind == QuestKind.Hornets)
      {
        if (!quest.SkipReward)
        {
          //var quest = gm.OuadHero.GetQuest(QuestKind.Hornets);
          Roguelike.Tiles.Loot loot = quest.RewardLootKind == Roguelike.Tiles.LootKind.Gem ? (Roguelike.Tiles.Loot)new Gem(GemKind.Amber) 
            : (Roguelike.Tiles.Loot)new SpecialPotion(SpecialPotionKind.Strength, SpecialPotionSize.Small);
          if (loot is Gem gem)
            gem.EnchanterSize = EnchanterSize.Medium;
          reward = loot;
        }
      }
      else if (questKind == QuestKind.CreatureInPond)
      {
        if (quest.RewardLootKind == Roguelike.Tiles.LootKind.Gem)//otherwise a hound was given
        {
          var loot = new Gem(GemKind.Diamond);
          loot.EnchanterSize = EnchanterSize.Medium;
          reward = loot;
        }
      }
      else if (questKind == QuestKind.IronOreForSmith ||
               questKind == QuestKind.SilesiaCoalForSmith)
      {
        var mat = EquipmentMaterial.Bronze;
        if (questKind == QuestKind.SilesiaCoalForSmith)
          mat = EquipmentMaterial.Steel;
        else
          mat = EquipmentMaterial.Iron;
        var sword = gm.LootGenerator.GetLootByAsset("viking_sword") as Roguelike.Tiles.Looting.Weapon;
        sword.MakeEnchantable(3);
        sword.SourceQuestKind = questKind.ToDescription();
        sword.SetMaterial(mat);
        reward = sword;

      }
      
      if(reward!=null)
        rewards.Add(reward);
      return rewards;
    }

    private void RemoveQuestLoot(Quest quest)
    {
      var lqr = quest.GetQuestRequirement<LootQuestRequirement>();
      if(lqr !=null)
        gm.Hero.Inventory.Remove(gm.Hero.Inventory.GetStackedLoot(lqr.LootName), new RemoveItemArg() { StackedCount = lqr.LootQuantity });
    }

    private MinedLoot GetMinedLootCount(MinedLootKind kind)
    {
      return gm.Hero.Inventory.GetStacked<MinedLoot>().Where(i => i.Kind == kind).FirstOrDefault();
    }
  }
}
