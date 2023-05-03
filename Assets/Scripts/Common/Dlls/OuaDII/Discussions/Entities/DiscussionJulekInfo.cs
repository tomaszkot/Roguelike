
namespace OuaDII.Discussions
{

  public class DiscussionJulekInfo : DiscussionNpcInfo
  {
    public DiscussionJulekInfo(SimpleInjector.Container container) : base(container, "Julek")
    {
    }

    public override void GetHeroClipTimes(string clipId, out string fileName, out float start, out float end)
    {
      start = 0;
      end = 0;
      fileName = "Hero1";
      if (clipId == "WhatsUp")
      {
        start = 0;
        end = 1;
      }
      else if (clipId == "MeatSource")
      {
        start = 1.35f;
        end = 5.7f;
      }
      else if (clipId == "BeenDealing")
      {
        start = 6.2f;
        end = 9.6f;
      }
      else if (clipId == "AreYouKidding")
      {
        start = 10.3f;
        end = 13.25f;
      }
      else if (clipId == "AllKiddingAside")
      {
        start = 14.2f;
        end = 17f;
      }
    }

    protected override void GetRightLeft(string rightId, out string left, out string right)
    {
      left = "";
      right = "";
      if (rightId == "WhatsUp")
      {
        right = "What's up?";
        var respPart1 = "Trouble. Zyndram and his reinforcements will soon return. As you may well know, man needs three things: food, alcohol and women - in that order. So, as a butcher I'm the frontline in their needs.";
        left = respPart1;
      }
      else if (rightId == "MeatSource")
      {
        right = "Gotcha, but I can see a source of meat source in a nearby farmhouse...";

        left = "Yeah, the thing is it's a boar. I can handle a pig, but not such a wild beast. Maybe you could kill it? I'll give you a club.";
      }
      else if (rightId == "BeenDealing")
      {
        right = "Sure thing, I've dealt with more difficult tasks";

        left = "Awesome";
      }
      else if (rightId == "AreYouKidding")
      {
        right = "Are you kidding? it would tear me apart";

        left = "Understood";
      }
      else if (rightId == "AllKiddingAside")
      {
        right = "All kidding aside. I am against eating meat.";

        left = "You're joking...right?";
      }
    }
  }
}