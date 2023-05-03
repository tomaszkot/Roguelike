
namespace OuaDII.Discussions
{


  public class DiscussionLionelInfo : DiscussionNpcInfo
  {

    public DiscussionLionelInfo(SimpleInjector.Container container) : base(container, "Josef")
    {
    }

    protected override void GetRightLeft(string rightId, out string left, out string right)
    {
      left = "";
      right = "";

      var situ = "Things have deteriorated rapidly in recent weeks. Bandits are prowling the roads, Unholy forces control the dungeons. The king's forces cannot busy themselves with this as we have an ongoing war on our eastern borders. All supplies are going to the military.  It's been hard to scrounge up food and equipment at the moment.";
      if (rightId == "WhatsUp")
      {
        right = "What's up?";
        left = "Hello my friend, I'm glad to see you have recovered. Your wounds after the last escapade did not look so good, I was afraid you wouldn't make it..";
      }
      else if (rightId == "LightInTunnel")
      {
        right = "Well, I did see the light at the end of the tunnel but I turned back. What's the situation?";
        left = situ;
      }
      else if (rightId == "HadDoubts")
      {
        right = "Yes, I also had my doubts. What's the situation?";
        left = situ;
      }
      else if (rightId == "CanHandleDungeons")
      {
        right = "As you are well aware, I can handle the dungeons. Where might I find the main dungeon?";
        left = "Well I did hear from one of the vagabonds, that there is a place somewhere at a hideout known as The Gathering. The bosses of the unholy minions hold their meetings there from time to time. It's very hard to gain access to it as you have to have all six of the statues of the slavic gods  - together they Unlock the entrance. Thus, you first have to clear the dungeons - the statues can be found there.";
      }
      else if (rightId == "ChallengingTask")
      {
        right = "That sounds like a pretty challenging task, any other things that can help me power up?";
        left = "If I were you I would start by visiting a couple of nearby places:Ziemowit's Blacksmith's workshop and Bratomir's Mill. It so happens that I promised Bratomir I would Deliver an hourglass to him. Would you do this for me?";
      }
      else if (rightId == "AlrightHourglass")
      {
        right = "Alright, I'll deliver the hourglass";
      }
    }

    public override void GetNpcClipTimes(string heroClipId, out string fileName, out float start, out float end)
    {
      fileName = "Friendly_Merchant_Lionel";
      start = 0;
      end = 0;
      if (heroClipId == "WhatsUp")
      {
        start = 0;
        end = 8;
      }
      else if (heroClipId == "LightInTunnel" || heroClipId == "HadDoubts")
      {
        start = 11;
        end = 26;
      }
      else if (heroClipId == "CanHandleDungeons")
      {
        start = 29;
        end = 48;
      }
      else if (heroClipId == "ChallengingTask")
      {
        start = 51.5f;
        end = 64;
      }
      //
      //
    }

    public override void GetHeroClipTimes(string clipId, out string fileName, out float start, out float end)
    {
      start = 0;
      end = 0;
      fileName = "Hero2";
      //
      if (clipId == "WhatsUp")
      {
        fileName = "Hero1";
        start = 0;
        end = 1;
      }
      else if (clipId == "LightInTunnel")
      {
        start = 46;
        end = 51.8f;
      }
      else if (clipId == "HadDoubts")
      {
        start = 53;
        end = 56;
      }
      else if (clipId == "CanHandleDungeons")
      {
        start = 58;
        end = 63;
      }
      else if (clipId == "ChallengingTask")
      {
        start = 64;
        end = 69;
      }
      else if (clipId == "AlrightHourglass")
      {
        start = 71;
        end = 74;
      }
    }
  }
}