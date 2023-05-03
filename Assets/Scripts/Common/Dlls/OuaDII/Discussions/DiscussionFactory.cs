using OuaDII.Managers;
using OuaDII.Quests;
using Roguelike.Discussions;
using SimpleInjector;

namespace OuaDII.Discussions
{
      
  public class DiscussionFactory
  {
    Container container;
    public const string MerchantZiemowitName = "Ziemowit";
    public const string MerchantLionelName = "Lionel";
    public const string MerchantWandaName = "Wanda";
    public const string NPCLudoslawName = "Ludoslaw";
    public const string NPCNaslawName = "Naslaw";
    public const string NPCJosefName = "Josef";
    public const string MerchantDobromilaName = "Dobromila";
    public const string NPCJulekName = "Julek";
    public const string NPCTonikName = "Tonik";

    public const string MagicFern = "Magic Fern";

    public DiscussionFactory(Container container)
    {
      this.container = container;
    }

    private DiscussionTopic CreateForLionel()
    {
      var discussionNpcInfo = new DiscussionLionelInfo(container);

      var item1 = discussionNpcInfo.GetTopic("WhatsUp", "Intro");

      var topic1 = discussionNpcInfo.GetTopic("LightInTunnel", "");
      var topic11 = discussionNpcInfo.GetTopic("HadDoubts", "");
      item1.InsertTopic(topic1);
      item1.InsertTopic(topic11);

      var topic1_1 = discussionNpcInfo.GetTopic("CanHandleDungeons", "");
      topic1.InsertTopic(topic1_1);

      topic11.InsertTopic(topic1_1);

      var topic1_12 = discussionNpcInfo.GetTopic("ChallengingTask", "");
      topic1_1.InsertTopic(topic1_12);
      topic1_12.ParentForQuest = QuestKind.HourGlassForMiller;

      var topic1_13 = discussionNpcInfo.GetTopic("AlrightHourglass", KnownSentenceKind.QuestAccepted, Quests.QuestKind.HourGlassForMiller);
      topic1_12.InsertTopic(topic1_13);

      return item1;
    }

    private DiscussionTopic CreateIronOreQuest()
    {
      var discussionNpcInfo = new DiscussionZiemowitInfo(container);

      var topic = discussionNpcInfo.GetTopic("CanYouMakeIronSword", "NopeEdict");
      topic.ParentForQuest = QuestKind.IronOreForSmith;

      var subTopic = discussionNpcInfo.GetTopic("ComeOnOneSword", "ThereIsAWay");
      topic.InsertTopic(subTopic);

      var subTopic1 = discussionNpcInfo.GetTopic("WhereFindIronOre", "MineNearBy");
      subTopic.InsertTopic(subTopic1);

      var ok = discussionNpcInfo.GetTopic("AllRightDoIt", KnownSentenceKind.QuestAccepted, Quests.QuestKind.IronOreForSmith);
      subTopic1.InsertTopic(ok);

      //var respPart1 = "Nope, in accordance with the royal edict, we can only sell equipment made from iron to knights.";

      //var respPart2 = " There is a way to do it though.";
      //var respPart3 = " If you deliver me " + LootQuestRequirement.QuestLootQuantity(Quests.QuestKind.IronOreForSmith) + " pieces of the iron ore I can devote part of it for making you a weapon.";
      //DiscussionTopic topic = create("Could you make an iron sword for me ?", respPart1 + respPart2 + respPart3);

      //var subTopic1 = create("Where would I find iron ore?", "There is a mine near by. Be aware monters have nested there, so it won't be easy.");
      //var ok = create("All right, I'll do it", KnownSentenceKind.QuestAccepted, Quests.QuestKind.IronOreForSmith);
      //subTopic.InsertTopic(ok);
      //topic.InsertTopic(subTopic);

      return topic;
    }


    /////////////////////////////////////////
    private DiscussionTopic CreateBarleySackQuest()
    {
      var discussionNpcInfo = new DiscussionTonikInfo(container);

      var topic = discussionNpcInfo.GetTopic("WhatsUp", "Intro");
      var subTopic = discussionNpcInfo.GetTopic("BeAssistance", "NeedBarley");
      
      subTopic.ParentForQuest = QuestKind.SacksOfBarley;

      var subTopicOK = discussionNpcInfo.GetTopic("EagerToTest", KnownSentenceKind.QuestAccepted, QuestKind.SacksOfBarley);
      subTopic.InsertTopic(subTopicOK);

      topic.InsertTopic(subTopic);

      return topic;
    }


    private DiscussionTopic CreateBoarQuest()
    {
      var discussionNpcInfo = new DiscussionJulekInfo(container);

      var topic = discussionNpcInfo.GetTopic("WhatsUp", "Intro");

      var subTopic = discussionNpcInfo.GetTopic("MeatSource", "KillBoar");
      
      subTopic.ParentForQuest = QuestKind.KillBoar;
            
      var subTopicOK = discussionNpcInfo.GetTopic("BeenDealing", KnownSentenceKind.QuestAccepted, QuestKind.KillBoar);//Awesome!
      
      var subTopicNo1 = discussionNpcInfo.GetTopic("AreYouKidding", "Understood");// Are you kidding? it would tear me apart
      
      var subTopicNo2 = discussionNpcInfo.GetTopic("AllKiddingAside", "YouAreJocking");

      subTopic.InsertTopic(subTopicNo1);
      subTopic.InsertTopic(subTopicNo2);
      subTopic.InsertTopic(subTopicOK);

      topic.InsertTopic(subTopic);

      return topic;
    }

    public DiscussionTopic create(KnownSentenceKind right, string left, bool allowBuyHound = false)
    {
      return new DiscussionTopic(container, right, left, allowBuyHound);
    }


    public DiscussionTopic createFromId(string rightId, string leftId, bool allowBuyHound = false, bool addMerchantItems = false)
    {
      string right = "";
      string left = "";
      return create(right, left, allowBuyHound = false, addMerchantItems);
    }

    public DiscussionTopic create(string right, string left, bool allowBuyHound = false, bool addMerchantItems = false)
    {
      return new DiscussionTopic(container, right, left, allowBuyHound, addMerchantItems);
    }

    public DiscussionTopic create(string right,
      KnownSentenceKind rightKnown,
      QuestKind questKind = QuestKind.Unset,
      bool allowBuyHound = false,
      bool skipReward = false
      )
    {
      return new DiscussionTopic(container, right, rightKnown, questKind, allowBuyHound, skipReward);
    }

    public DiscussionTopic CreateStonesMine()
    {
      var discussionNpcInfo = new DiscussionJosefInfo(container);

      var topic = discussionNpcInfo.GetTopic("WhatsUp", "Intro");
      var subTopic = discussionNpcInfo.GetTopic("WhatIfTheyRefuse", "WeAreConvincing");
      topic.InsertTopic(subTopic);

      var subTopicWiseMan = discussionNpcInfo.GetTopic("SuposseSoWiseMan", "Darkening");//
           

      subTopic.InsertTopic(subTopicWiseMan);
      var subTopicStones = discussionNpcInfo.GetTopic("InterestingTheoryTasksForMe", "Catedral");
      subTopicStones.ParentForQuest = QuestKind.StonesMine;
      subTopicWiseMan.InsertTopic(subTopicStones);

      var subTopicMaybe1 = discussionNpcInfo.GetTopic("DangerTask", "YouWillBeRewarded");
      var okNoReward = discussionNpcInfo.GetTopic("ShallDoThisForthwith", KnownSentenceKind.QuestAccepted, QuestKind.StonesMine, skipReward: true);
      subTopicStones.InsertTopic(okNoReward);
      subTopicStones.InsertTopic(subTopicMaybe1);

      {
        var subTopicMaybe1Child1 = discussionNpcInfo.GetTopic("PreferSomethingLife", "LittleFaith");
        var okReward = discussionNpcInfo.GetTopic("ShallDoThisForthwith", KnownSentenceKind.QuestAccepted, QuestKind.StonesMine, skipReward: false);
        subTopicMaybe1Child1.InsertTopic(okReward);

        var subTopicMaybe1Child2 = discussionNpcInfo.GetTopic("PraiseTheLord", KnownSentenceKind.QuestAccepted, Quests.QuestKind.StonesMine, skipReward: true);

        subTopicMaybe1.InsertTopic(subTopicMaybe1Child1);
        subTopicMaybe1.InsertTopic(subTopicMaybe1Child2);
      }

      return topic;
    }

    public void CreateForLionel(DiscussionTopic mainItem)
    {
      var item1 = CreateForLionel();
      mainItem.InsertTopic(item1, true);
    }


    

    public Discussion Create(Roguelike.Tiles.LivingEntities.INPC npc)
    {
      string merchantName = npc.LivingEntity.Name;
      bool allowBuyHound = npc.TrainedHound != null;
      
      var dis = new Discussion(this.container);
      dis.EntityName = merchantName;
      var mainItem = create("", "", allowBuyHound, npc is Tiles.LivingEntities.Merchant);//What can I do for you?

      if (merchantName.Contains(MerchantZiemowitName))
      {
        CreateForZiemowit(mainItem);
      }
      else if (merchantName.Contains(MerchantLionelName))
      {
        CreateForLionel(mainItem);
      }
      else if (merchantName.Contains(MerchantWandaName))
      {
        CreateForWanda(mainItem);
      }
      else if (merchantName.Contains(MerchantDobromilaName))
      {
        CreateForDobromila(mainItem);
      }
      else if (merchantName.Contains(NPCLudoslawName))
      {
        CreateForLudoslaw(mainItem);
      }
      else if (merchantName.Contains(NPCJulekName))
      {
        CreateForJulek(mainItem);
      }
      else if (merchantName.Contains(NPCTonikName))
      {
        CreateForTonik(mainItem);
      }
      else if (merchantName.Contains(NPCNaslawName))
      {
        CreateForNaslaw(mainItem);
      }
      else if (merchantName.Contains(NPCJosefName))
      {
        CreateForJosef(mainItem);
      }
      else if (merchantName.Contains("Roslaw"))
      {
        CreateForRoslaw(mainItem);
      }
      else if (merchantName.Contains("Zyndram"))
      {
        CreateForZyndram(mainItem);
      }
      else if (merchantName.Contains("Jurand"))
      {
        var topic = CreateRescueJurantDaughter();
        mainItem.InsertTopic(topic);
      }

      dis.SetMainItem(mainItem);
      return dis;
    }

    private void CreateForJosef(DiscussionTopic mainItem)
    {
      var topic = CreateStonesMine();
      mainItem.InsertTopic(topic);
    }

    private void CreateForZiemowit(DiscussionTopic mainItem)
    {
      var topic = CreateIronOreQuest();
      mainItem.InsertTopic(topic);
    }

    
    private void CreateForDobromila(DiscussionTopic mainItem)
    {
      var topic = CreateFernQuest();
      mainItem.InsertTopic(topic);
    }

    private void CreateForWanda(DiscussionTopic mainItem)
    {
      var topic = CreateToadstoolQuest();
      mainItem.InsertTopic(topic);
    }
        
    private void CreateForNaslaw(DiscussionTopic mainItem)
    {
      var topic = CreatePondMonsterQuest();
      mainItem.InsertTopic(topic);
    }


    private void CreateForLudoslaw(DiscussionTopic mainItem)
    {
      var topic = CreateHornetsQuest();
      mainItem.InsertTopic(topic);
    }

    private void CreateForJulek(DiscussionTopic mainItem)
    {
      var topic = CreateBoarQuest();
      mainItem.InsertTopic(topic);
    }

    private void CreateForTonik(DiscussionTopic mainItem)
    {
      var topic = CreateBarleySackQuest();
      mainItem.InsertTopic(topic);
    }

    public void CreateForRoslaw(DiscussionTopic mainItem)
    {
      mainItem.InsertTopic(CreateRoslaw());
    }

    public void CreateForZyndram(DiscussionTopic mainItem)
    {
      mainItem.InsertTopic(CreateZyndram());
    }

    private DiscussionTopic CreateFernQuest()
    {
      var respPart1 = "I could prepare you a great potion.";
      var respPart2 = " Bring me a magic fern I'll make it.";
      var topic = create(KnownSentenceKind.WhatsUp, respPart1 + respPart2);

      topic.ParentForQuest = QuestKind.FernForDobromila;
      var subTopic = create("What is the catch?", "There is none. Be cafeful though, many have tried, none returned.");
      var ok = create("All right, looks like a task for me", KnownSentenceKind.QuestAccepted, Quests.QuestKind.FernForDobromila);
      subTopic.InsertTopic(ok);
      topic.InsertTopic(subTopic);

      return topic;
    }

    private DiscussionTopic CreateToadstoolQuest()
    {
      var respPart1 = "I'm shot in ingradients for potions.";
      var respPart2 = " If you deliver me " + LootQuestRequirement.QuestLootQuantity(Quests.QuestKind.ToadstoolsForWanda) + " red toadstools I'll give you a recipe how to use them.";
      var topic = create(KnownSentenceKind.WhatsUp, respPart1 + respPart2);
      
      topic.ParentForQuest = QuestKind.ToadstoolsForWanda;
      var subTopic = create("That's quite a lot of loot, I could monetize them.", "Well, the recipe is very useful, but do as you wish.");
      var ok = create("All right, looks like a good deal", KnownSentenceKind.QuestAccepted, Quests.QuestKind.ToadstoolsForWanda);
      //var maybe = create("Let me think it over", KnownSentenceKind.Back);
      subTopic.InsertTopic(ok);
      //subTopic.InsertTopic(maybe);
      topic.InsertTopic(subTopic);

      return topic;
    }

    private DiscussionTopic CreateHornetsQuest()
    {
      var respPart1 = "yyyy, I have some trouble but I'm quite old and memory is not serving me as it used to be.... Oh I just reminded!" +
        " My beehives were attacked by hornets. Could you defeat them ?";
      var topic = create(KnownSentenceKind.WhatsUp, respPart1);
      topic.ParentForQuest = QuestKind.Hornets;
      var subTopicMaybe1 = create("I fear biting insects, I'd rather avoid them.", "I'll reward you well. It gonna be.... let me remind myself... a gem!, " +
        "I have a precious gem for you.");
      var subTopicOk1 = create("OK I'll kill them", KnownSentenceKind.QuestAccepted, Quests.QuestKind.Hornets);
      subTopicMaybe1.InsertTopic(subTopicOk1);
      subTopicOk1.UnhidingMapName = QuestManager.HornetsHerdApiaryMap;
      subTopicOk1.LootKind = Roguelike.Tiles.LootKind.Gem;

      var subTopicMaybe2 = create("I hate biting insects, I'll kill them. What would be a reward?", "It gonna be.... let me remind myself... a potion!, I have a precious " +
        "potion for you that would permamently increase your strength.");
      var subTopicOk2 = create("OK I'll kill them", KnownSentenceKind.QuestAccepted, Quests.QuestKind.Hornets);
      subTopicMaybe2.InsertTopic(subTopicOk2);
      subTopicOk2.UnhidingMapName = QuestManager.HornetsHerdApiaryMap;
      subTopicOk2.LootKind = Roguelike.Tiles.LootKind.Potion;

      topic.InsertTopic(subTopicMaybe1);
      
      topic.InsertTopic(subTopicMaybe2);
      

      return topic;
    }

   


    

    private DiscussionTopic CreatePondMonsterQuest()
    {
      var respPart1 = @"I can not do fishing at village's pond. Some terrible creature is emerging from the water. 
I'm afraid my fishman's knife is not enough to defeat it. Could you help me?";
      var topic = create(KnownSentenceKind.WhatsUp, respPart1);
      topic.ParentForQuest = QuestKind.CreatureInPond;
      //var subTopicHmm = create("Hmm, doesnt sound like a big deal. I'll have a look", "Do not underestimate it, maybe we shall do this together?");
      //var subTopicOk1 = create("Thanks, but I'll do it myself.", KnownSentenceKind.QuestAccepted, Quests.QuestKind.CreatureInPond);
      //var subTopicOk2 = create("Sure, together we can certainly do it.", KnownSentenceKind.QuestAccepted, Quests.QuestKind.CreatureInPond);

      var subTopicHmm = create("Hmm, doesnt sound like a big deal. I'll have a look", "Do not underestimate it. " +
      "I can reward you after the quest giving you a nice gem or give you my hound upfront. What do you choose?");

      var subTopicOk1 = create("I'll pick the reward later", KnownSentenceKind.QuestAccepted, Quests.QuestKind.CreatureInPond);
      var subTopicOk2 = create("I take the hound now", KnownSentenceKind.QuestAccepted, Quests.QuestKind.CreatureInPond);
      subTopicOk2.HoundJoinsAsAlly = true;

      subTopicHmm.InsertTopic(subTopicOk1);
      subTopicHmm.InsertTopic(subTopicOk2);
      topic.InsertTopic(subTopicHmm);


      return topic;
    }

    
    
    public DiscussionTopic CreateRoslaw()
    {
      var topic = create("Why are you hiding here?",
          "There were too many of these bastards, they killed my fiend Cieszygor... Alone I could not face them, but I'm not a coward, if you wish I can join you and we can defeat them together.");
      var reject = create("Thanks but I think I can handle them.", KnownSentenceKind.AllyRejected);//"All right, wish you luck then.", );// "Then let's make them bite the dust.");
      var ok = create("Why not, any help is welcomed.", KnownSentenceKind.AllyAccepted);
      
      topic.InsertTopic(reject);
      topic.InsertTopic(ok);

      return topic;
    }

    public DiscussionTopic CreateZyndram()
    {
      var topic = create("What's up?",
          "Hmm");
      
      return topic;
    }

    public DiscussionTopic CreateWarewolfHarassingVillage()
    {
      var topic = create("What's up",
        "Bad times, our animals are disppearing at night. People say it's wolf taking them, but I could swear I saw it walking like a human, It must be a warewolf.");
      topic.ParentForQuest = QuestKind.WarewolfHarassingVillage;
      var reject = create("Sorry, but  I'm scared of such creatures, you will have to find other mecenary.", "I'm sad to hear it.");
      var ok = create("All right, I'll try do kill it", KnownSentenceKind.QuestAccepted, Quests.QuestKind.WarewolfHarassingVillage);
      topic.InsertTopic(ok);
      topic.InsertTopic(reject);

      return topic;
    }

    public DiscussionTopic CreateRescueJurantDaughter()
    {
      var topic = create("What's up?",
        //"Spedzam moje dni w smutku. Okaleczyli mnie, zabrali mi wszystko..."
        "I spend my days in sorrow. They maimed me, get everything from me..."
        );
      topic.ParentForQuest = QuestKind.RescueJurantDaughter;

      var subTopic1 = create("Who did it?", "Teutonic order");
      //var subTopic11 = create("Hmm, that's suprising. Myslałem że są pokornymi wyznawami nowej wiary", "Pozornie, naprawdę są wilkami w oczych skórach.");
      var subTopic11 = create("Hmm, that's suprising. I thought they are humble followers of the new faith.", "On the surface, in reality they are wolfs in Sheep's skin");
      subTopic1.InsertTopic(subTopic11);

      //var sentence = "Porwali mi córkę, przybyłem do nich by ja wykupić. Ale powiedzieli że to jakaś pomyłka, że oni jej nie porwali.";//Pokazali mi jakąś niedojdę, twierdząc że to ona.
      var sentence = "They kidnapped my daughter, I arrived to their castle to pay ransom. But they said it's a misuderstanding, that they did not kidnapped her.";
      //sentence += "Wpadłem w szał, wywiązała sie walka. Zabiłem kilku psubratów, ale było ich zbyt wielu. Pojmali mnie i okaleczyli";
      sentence += "I got mad, faight occured. I killed a few basterds, but there were too many of them. They caught me and maimed me.";
      var subTopic111 = create("Tell me more of your story", sentence);
      subTopic11.InsertTopic(subTopic111);

      //var subTopic1111 = create("A skąd pewność że ona tam była?", "Słyszałem jej spiew dochodzący z jednej z wież. Wierz mi, była tam.");
      var subTopic1111 = create("How can you be sure she was there?", "I heard her singing in one of the towers. Believe me, she was there.");
      subTopic111.InsertTopic(subTopic1111);

      //var subTopic11111 = create("Przykro mi to słyszec, mogę spróbować ja odzyskać", KnownSentenceKind.QuestAccepted, Quests.QuestKind.RescueJurantDaughter);
      var subTopic11111 = create("Sorry to hear it, I can try rescue her", KnownSentenceKind.QuestAccepted, Quests.QuestKind.RescueJurantDaughter);
      //var subTopic111111 = create("Przykro mi to słyszec, ale nie mogę nic w tej sprawie zrobić", "Szkoda, bywaj zatem");
      var subTopic111111 = create("Sorry to hear it, but I can not do anything about it.", "It's a pity");

      subTopic1111.InsertTopic(subTopic11111);
      subTopic1111.InsertTopic(subTopic111111);

      topic.InsertTopic(subTopic1);

      return topic;
    }

    

    public DiscussionTopic CreateSilesiaCoalQuest()
    {
      DiscussionTopic topic = create("Could you make a steel equipment for me ?",
                "Hmm, if you deliver me " + LootQuestRequirement.QuestLootQuantity(Quests.QuestKind.SilesiaCoalForSmith) + " pieces of the silesia coal I should be able to provide one."
                );
      topic.ParentForQuest = QuestKind.SilesiaCoalForSmith;
      var subTopic = create("Where would I find silesia coal ?", "There is a mine north of here. Be aware strong monters have nested there.");
      var ok = create("All right, I'll give it a try", KnownSentenceKind.QuestAccepted, Quests.QuestKind.SilesiaCoalForSmith);
      subTopic.InsertTopic(ok);
      topic.InsertTopic(subTopic);
      return topic;
    }
  }
}
