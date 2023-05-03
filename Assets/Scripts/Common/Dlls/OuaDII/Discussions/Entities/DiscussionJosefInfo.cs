namespace OuaDII.Discussions
{


  public class DiscussionJosefInfo : DiscussionNpcInfo
  {

    public DiscussionJosefInfo(SimpleInjector.Container container) : base(container, "Josef")
    {
    }

    protected override void GetRightLeft(string rightId, out string left, out string right)
    {
      left = "";
      right = "";
      if (rightId == "WhatsUp")
      {
        right = "What's up?";
        left = "I eagerly await the return of Zyndram's squad. They are looking for the secret pagan encampment. We must convert them.";
      }
      else if (rightId == "WhatIfTheyRefuse")//, "WeAreConvincing")
      {
        right = "What if they refuse to convert?";
        left = "Believe me, we are very convincing...";
      }
      else if (rightId == "SuposseSoWiseMan")
      {
        right = "I suppose so. You are a wise man.  Please tell me, do you have any idea why it is so dark during the middle of the day?";
        left = "I suppose no mortal knows for sure, but it started once the evil minions  took possession of these pagan statues.I suspect that this is the cause of the darkening of the sun.";

      }
      else if (rightId == "InterestingTheoryTasksForMe")
      {
        right = "An interesting theory. Do you have any tasks for me?";
        var stones = "We are trying to build a cathedral here but we currently lack stones. There is a mine nearby, but they stopped delivering stones a few days ago. We sent two mercenaries there but they did not return. I'm afraid there may Be a serious problem there. Could you check  it?";
        left = stones;
      }
      else if (rightId == "PreferSomethingLife")
      {
        right = "Geez, I'd prefer something during my life here on earth.";
        left = "Oh ye of little faith. Ok then, I'll give you three precious gems.";
      }
      else if (rightId == "AllRightDoItSoon")
      {
        right = "All right, I'll do it soon.";
      }
      else if (rightId == "ShallDoThisForthwith")
      {
        right = "All right, I shall do this forthwith";
      }

      else if (rightId == "PraiseTheLord")
      {
        right = "Praise the Lord, I'll do it.";
      }
      else if (rightId == "DangerTask")
      {
        right = "Well sound like a dangerous task, what would be a reward?";
        left = "You will be rewarded by the Lord upon your death.";
      }
      //Oh ye of little faith. Ok then, I'll give you three precious gems.
    }
    public override void GetHeroClipTimes(string clipId, out string fileName, out float start, out float end)
    {
      start = 0;
      end = 0;
      fileName = "Hero1";
      //
      if (clipId == "WhatsUp")
      {
        start = 0;
        end = 1;
      }
      else if (clipId == "WhatIfTheyRefuse")//, "WeAreConvincing")
      {
        start = 125;
        end = 127.5f;
      }
      else if (clipId == "SuposseSoWiseMan")
      {
        fileName = "During";
        start = 0;
        end = 8f;
      }
      else if (clipId == "InterestingTheoryTasksForMe")
      {
        start = 129;
        end = 133f;
      }
      else if (clipId == "DangerTask")
      {
        start = 134;
        end = 138;
      }
      else if (clipId == "PraiseTheLord")
      {
        start = 139.5f;
        end = 141.3f;
      }
      else if (clipId == "PreferSomethingLife")
      {
        start = 142;
        end = 145;
      }
      else if (clipId == "ShallDoThisForthwith")
      {
        start = 152;
        end = 155;
      }
    }
  }
}