using Roguelike.Discussions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Roguelike.Core.Discussions.Entities
{
  public class Lech : DiscussionNpcInfo
  {
    public Lech(SimpleInjector.Container container, string npc) : base(container, npc)
    {
    }

    public override void GetHeroClipTimes(string clipId, out string fileName, out float start, out float end)
    {
      fileName = "";
      start = 0;
      end = 0;
    }

    protected override void GetRightLeft(string rightId, out string left, out string right)
    {
      left = ""; 
      right = "";
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
        var respPart3 = " If you deliver me " + 5 + " pieces of the iron ore I can devote part of it to making you a weapon.";
        left = respPart2 + respPart3;
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

        var p3 = "If you wish to be rewarded, the price is now " + 5 +
          5 + " pieces of iron ore.";
        left = p1 + p2 + p3;

      }
    }
  }
}
