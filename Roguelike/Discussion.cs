using Roguelike.Tiles;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Roguelike.Discussions
{
  public enum KnownSentenceKind { Unset, LetsTrade, Bye, SellHound, Back, QuestAccepted }

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

    public override string ToString()
    {
      return Body;
    }
    //public DiscussionSentence Next { get;set;}
  }

  public class DiscussionItem
  {
    public DiscussionSentence Right { get; set; } = new DiscussionSentence();
    public DiscussionSentence Left { get; set; } = new DiscussionSentence();
    public List<DiscussionItem> Topics { get; set; } = new List<DiscussionItem>();

    [XmlIgnoreAttribute]
    public DiscussionItem Parent { get => parent; set => parent = value; }
    const bool merchantItemsAtAllLevels = false;

    bool allowBuyHound;
    DiscussionItem parent;

    public bool HasBack()
    {
      return Topics.Any(i => i.Right.Body == "Back");
    }

    public override string ToString()
    {
      return Right + "->"+Left;
    }

    public DiscussionItem() { }

    public DiscussionItem(string right, string left, bool allowBuyHound = false, bool addMerchantItems = merchantItemsAtAllLevels)
    {
      this.allowBuyHound = allowBuyHound;
      Right = new DiscussionSentence(right);
      Left = new DiscussionSentence(left);
      if(addMerchantItems)
        Discussion.CreateMerchantResponseOptions(this, allowBuyHound);
    }

    public DiscussionItem(string right, KnownSentenceKind knownSentenceKind, bool allowBuyHound = false) 
      : this(right, knownSentenceKind.ToString(), allowBuyHound, false)
    {
    }

    public void InsertTopic(DiscussionItem subItem, bool atBegining = true)
    {
      if (!subItem.HasBack())
      {
        var back = new DiscussionItem("Back", KnownSentenceKind.Back.ToString());
        back.Parent = this;
        subItem.Topics.Add(back);
      }
      if (atBegining)
        Topics.Insert(0, subItem);
      else
      {
        if (HasBack())
        {
          Topics.Insert(Topics.Count-1, subItem);
        }
        else
          Topics.Add(subItem);
      }
    }

    public void InsertTopic(string right, KnownSentenceKind knownSentenceKind)
    {
      InsertTopic(right, knownSentenceKind.ToString(), false);
    }

    public void InsertTopic(string right, string left, bool addMerchantItems = merchantItemsAtAllLevels)
    {
      var item = new DiscussionItem(right, left, allowBuyHound, addMerchantItems);
      InsertTopic(item, true);
    }

    public void AddTopic(string right, KnownSentenceKind knownSentenceKind)
    {
      AddTopic(right, knownSentenceKind.ToString(), false);
    }

    public void AddTopic(string right, string left, bool addMerchantItems = merchantItemsAtAllLevels)
    {
      var item = new DiscussionItem(right, left, allowBuyHound, addMerchantItems);
      InsertTopic(item, false);
    }
  }

  public class Discussion
  {
    public string EntityName { get; set; }
    DiscussionItem mainItem = new DiscussionItem();

    public static Discussion CreateForMerchant(string merchantName, bool allowBuyHound)
    {
      var dis = new Discussion();
      dis.EntityName = merchantName;
      var mainItem = new DiscussionItem("", "What can I do for you?", allowBuyHound, true);
            
      if (merchantName.Contains("Ziemowit"))//TODO
      {
        var topic = new DiscussionItem("Could you make an iron sword for me ?", 
          "Nope, due to the king's edict we are allowed to sell an iron equipment only to knights. There is a way to do it though. If you deliver me 10 pieces of the iron ore I can devote part of it for making you a weapon."
          );

        var subTopic = new DiscussionItem("Where would I find iron ore?", "There is a mine west of here. Be aware monters have nested there, so it won't be easy.");

        var ok = new DiscussionItem("OK, I'll do it", KnownSentenceKind.QuestAccepted);
        subTopic.InsertTopic(ok);

        topic.InsertTopic(subTopic);
        mainItem.InsertTopic(topic);
      }

      dis.mainItem = mainItem;
      return dis;
    }

    public static Discussion CreateForLionel(bool allowBuyHound)
    {
      var dis = CreateForMerchant("Lionel", allowBuyHound);
      var item1 = new DiscussionItem("What's up?", "Dark times have arrived...", allowBuyHound);
      dis.MainItem.InsertTopic(item1, true);
      return dis;
    }

    public static void CreateMerchantResponseOptions(DiscussionItem item, bool allowBuyHound)
    {
      item.AddTopic("Let's Trade", KnownSentenceKind.LetsTrade);
      if(allowBuyHound)
        item.AddTopic("Sell me a hound ("+Merchant.HoundPrice+" gold)", KnownSentenceKind.SellHound);

      item.AddTopic("Bye", KnownSentenceKind.Bye);
    }

    public DiscussionItem MainItem { get => mainItem; set => mainItem = value; }

    public void ToXml()
    {
      var writer = new System.Xml.Serialization.XmlSerializer(typeof(Discussion));
      var path = EntityName+".xml";
      var file = System.IO.File.Create(path);
      writer.Serialize(file, this);
      file.Close();
    }

    public static Discussion FromXml(string entityName)
    {
      Discussion disc = null;
      var reader = new System.Xml.Serialization.XmlSerializer(typeof(Discussion));
      var file = new System.IO.StreamReader(entityName+".xml");
      disc = (Discussion)reader.Deserialize(file);
      file.Close();
      return disc;
    }
  }
}
