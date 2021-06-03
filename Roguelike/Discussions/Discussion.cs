using Roguelike.Extensions;
using Roguelike.Managers;
using Roguelike.Tiles.LivingEntities;
using System.Diagnostics;

namespace Roguelike.Discussions
{
  public enum KnownSentenceKind { Unset, WhatsUp, LetsTrade, Bye, SellHound, Back, QuestAccepted, QuestProgress, WorkingOnQuest, AwaitingReward, Cheating }


  public class Discussion
  {
    public string EntityName { get; set; }
    DiscussionTopic mainItem;

    public Discussion()
    {
      mainItem = CreateMainItem();
    }

    protected virtual DiscussionTopic CreateMainItem()
    {
      return new DiscussionTopic();
    }

    public static void CreateMerchantResponseOptions(DiscussionTopic item, bool allowBuyHound)
    {
      item.AddTopic("Let's Trade", KnownSentenceKind.LetsTrade);
      if (allowBuyHound)
        item.AddTopic(KnownSentenceKind.SellHound.ToDescription() + " (" + Merchant.HoundPrice + " gold)", KnownSentenceKind.SellHound);
      //item.AddTopic("Sell me a hound (" + Merchant.HoundPrice + " gold)", KnownSentenceKind.SellHound);
    }

    public void SetMainItem(DiscussionTopic topic) 
    {
      topic.AddTopic("Bye", KnownSentenceKind.Bye);
      MainItem = topic;
    }

    public DiscussionTopic MainItem 
    { 
      get => mainItem; 
      set => mainItem = value; 
    }

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

    public virtual void EmitCheating(DiscussionTopic item)
    {
    }

    public virtual bool AcceptQuest(QuestManager qm, string questKind)
    {
      return false;
    }
  }
}
