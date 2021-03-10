using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;

namespace Roguelike.Discussions
{
  public enum KnownSentenceKind { Unset, LetsTrade, Bye, SellHound, Back, QuestAccepted, QuestProgress, WorkingOnQuest, AwaitingReward, Cheating }
   
  
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

    //TODO use json
    public void ToXml()
    {
      try
      {
        var writer = new System.Xml.Serialization.XmlSerializer(typeof(Discussion));
        var path = EntityName + ".xml";
        var file = System.IO.File.Create(path);
        writer.Serialize(file, this);
        file.Close();
      }
      catch (System.Exception ex)
      {
#if DEBUG
        Debug.WriteLine(ex);
#endif
        throw;
      }
    }

    public static Discussion FromXml(string entityName)
    {
      try
      {
        Discussion disc = null;
        var reader = new System.Xml.Serialization.XmlSerializer(typeof(Discussion));
        var file = new System.IO.StreamReader(entityName + ".xml");
        disc = (Discussion)reader.Deserialize(file);
        file.Close();
        return disc;
      }
      catch (System.Exception ex)
      {
#if DEBUG
        Debug.WriteLine(ex);
#endif
        throw;
      }
    }

    
  }
}
