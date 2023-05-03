using OuaDII.Quests;
using System.Collections;
using System.Collections.Generic;

namespace OuaDII.Discussions
{

  public class DiscussionZiemowitInfo : DiscussionNpcInfo
  {
    public DiscussionZiemowitInfo(SimpleInjector.Container container) : base(container, "Ziemowit")
    {
    }

    protected override void GetRightLeft(string rightId, out string left, out string right)
    {
      left = "";
      right = "";
      if (rightId == "CanYouMakeIronSword")
      {
        right = "Could you forge an iron sword for me?";
        left = "Nope, in accordance with the royal edict, we can only sell equipment made from iron to knights.";
      }
      else if (rightId == "ComeOnOneSword")
      {
        right = "Come on, it's just one sword...";
        var respPart2 = "There is a way this can be done.";
        var respPart3 = " If you deliver me " + LootQuestRequirement.QuestLootQuantity(Quests.QuestKind.IronOreForSmith) + " pieces of the iron ore I can devote part of it to making you a weapon.";
        left = respPart2+ respPart3;
      }
      else if (rightId == "WhereFindIronOre")
      {
        right = "Where would I find iron ore?";
        left = "There is a mine nearby. Be aware that  monsters have settled down there, so it won't be easy.";
      }
      else if (rightId == "AllRightDoIt")
      {
        right = "All right, I'll do it";
      }
      else if (rightId == "CouldNotFind_ore")
      {
        right = "I could not find any iron ore (a lie)";

        var p1 = "hmm, your backpack seems to be quite full, let me see... ";
        var p2 = "Yeah, you lied to me! If you cheat me again I won't make business with you anymore. ";
        
        var p3 = "If you wish to be rewarded, the price is now " + (LootQuestRequirement.QuestLootQuantity(Quests.QuestKind.IronOreForSmith) + 
          LootQuestRequirement.QuestCheatingPunishmentLootQuantity(Quests.QuestKind.IronOreForSmith)) + " pieces of iron ore.";
        left = p1 + p2 + p3;
                
      }
    }

    public override void GetHeroClipTimes(string clipId, out string fileName, out float start, out float end)
    {
      start = 0;
      end = 0;
      fileName = "Hero2";
      //
      if (clipId == "CanYouMakeIronSword")
      {
        start = 141.7f;
        end = 144.5f;
      }
      if (clipId == "ComeOnOneSword")
      {
        start = 145;
        end = 147.5f;
      }
      if (clipId == "WhereFindIronOre")
      {
        start = 152;
        end = 154;
      }
      if (clipId == "AllRightDoIt")
      {
        start = 155;
        end = 156;
      }
      if (clipId == "CouldNotFind_ore")
      {
        start = 159;
        end = 161.6f;
      }
    }

  }
}