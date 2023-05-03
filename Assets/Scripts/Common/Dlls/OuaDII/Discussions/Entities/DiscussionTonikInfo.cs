namespace OuaDII.Discussions
{

  public class DiscussionTonikInfo : DiscussionNpcInfo
  {
    public DiscussionTonikInfo(SimpleInjector.Container container) : base(container, "Tonik")
    {
    }

    protected override void GetRightLeft(string rightId, out string left, out string right)
    {
      left = "";
      right = "";
      if (rightId == "WhatsUp")
      {
        right = "What's up?";
        left = "Trouble. Zyndram and his reinforcements will soon return. As you may well know, a man needs three things: alcohol, women and food - in that order. So as a moonshiner I'm the frontline in their needs.";
      }
      else if (rightId == "BeAssistance")// need barley
      {
        right = "Okay, got it. How can I be of assistance?";
        left = "I desparately need barley. Maybe you could pop over to Bratomir's mill? He owes me a few sacks.As a reward I will furnish you with a few explosive cocktails made from my hooch. Believe me, It is worth the effort.";
      }
      else if (rightId == "EagerToTest")
      {
        right = "For sure, I'm eager to try out those cocktails";
        left = "";
      }
    }

    public override void GetHeroClipTimes(string clipId, out string fileName, out float start, out float end)
    {
      start = 0;
      end = 0;
      fileName = "Hero1";
      //1m 06.49s
      if (clipId == "WhatsUp")
      {
        start = 66.49f;
        end = 67.4f;
      }
      else if (clipId == "BeAssistance")
      {
        fileName = "BeAssistance";
        start = 0f;
        end = 2.81f;
      }
      else if (clipId == "EagerToTest")
      {
        start = 75;
        end = 78;
      }

    }
  }
}