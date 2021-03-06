using Roguelike.Tiles;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Roguelike.Discussions
{
  public enum KnownSentenceKind { Unset, LetsTrade, Bye, SellHound, Back, QuestAccepted }
   
  
  public class Discussion
  {
    public string EntityName { get; set; }
    DiscussionItem mainItem = new DiscussionItem();
        
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
