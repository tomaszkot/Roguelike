using System.Collections.Generic;

namespace Roguelike.Discussions
{
  public enum KnownSentenceKind { Unset, LetsTrade, Bye, SellHound }

  public class DiscussionSentence
  {
    public string Id { get; set; } = "";
    public string Body { get; set; } = ""; 

    public DiscussionSentence() { }
    public DiscussionSentence(string body) 
    {
      Body = body;
      Id = Body;
    }

    public DiscussionSentence(string body, string id)
    {
      Body = body;
      Id = id;
    }
    //public DiscussionSentence Next { get;set;}
  }

  public class DiscussionItem
  {
    public DiscussionSentence Right { get; set; } = new DiscussionSentence();
    public DiscussionSentence Left { get; set; } = new DiscussionSentence();
    public List<DiscussionItem> Responses { get; set; } = new List<DiscussionItem>();

    public DiscussionItem() { }

    public DiscussionItem(string right, string left)
    {
      Right = new DiscussionSentence(right);
      Left = new DiscussionSentence(left);
    }

    public DiscussionItem(string right, KnownSentenceKind knownSentenceKind)
    {
      Right = new DiscussionSentence(right, knownSentenceKind.ToString());
    }
  }

  public class Discussion
  {
    public string EntityName { get; set; }
    List<DiscussionItem> Items = new List<DiscussionItem>();

    public static Discussion CreateForMerchant(string merchantName, bool allowBuyHound)
    {
      var dis = new Discussion();
      dis.EntityName = merchantName;
      var mainItem = new DiscussionItem("", "What can I do for you?");

      CreateMerchantItems(mainItem, allowBuyHound);

      dis.Items.Add(mainItem);
      return dis;
    }

    public static Discussion CreateForLionel()
    {
      var dis = CreateForMerchant("Lionel", true);
      var item1 = new DiscussionItem("What's up?", "Dark times have arrived...");
      dis.Items[0].Responses.Insert(0, item1);
      return dis;
    }

    private static void CreateMerchantItems(DiscussionItem mainItem, bool allowBuyHound)
    {
      mainItem.Responses.Add(new DiscussionItem("Let's Trade", KnownSentenceKind.LetsTrade));
      if(allowBuyHound)
        mainItem.Responses.Add(new DiscussionItem("Sell me a hound", KnownSentenceKind.SellHound));

      mainItem.Responses.Add(new DiscussionItem("Bye", KnownSentenceKind.Bye));
    }

    public DiscussionItem DiscussionItem { get { return Items[0]; } }
  }
}
