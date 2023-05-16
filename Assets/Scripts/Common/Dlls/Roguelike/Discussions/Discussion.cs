using Newtonsoft.Json;
using Roguelike.Extensions;
using Roguelike.Managers;
using Roguelike.Tiles.LivingEntities;
using SimpleInjector;
using System.Diagnostics;

namespace Roguelike.Discussions
{
  public enum KnownSentenceKind { Unset, WhatsUp, LetsTrade, Bye, SellHound, Back, QuestAccepted, QuestProgress, WorkingOnQuest, AwaitingReward, 
    Cheating, DidIt , RewardDeny, AwaitingRewardAfterRewardDeny, RewardSkipped, AllyAccepted, AllyRejected
  }


  public class Discussion
  {
      
    protected Container container;
    public string EntityName { get; set; }
    DiscussionTopic mainItem;

    public Discussion(Container container)
    {
      this.container = container;
      mainItem = CreateMainItem();
    }

    protected virtual DiscussionTopic CreateMainItem()
    {
      return container.GetInstance<DiscussionTopic>();
    }

    public static void CreateMerchantResponseOptions(DiscussionTopic item, bool allowBuyHound)
    {
      item.AddTopic(KnownSentenceKind.LetsTrade);
      if (allowBuyHound)
        item.AddTopic(KnownSentenceKind.SellHound, " (" + Merchant.HoundPrice + " gold)", "I'm afraid you can not afford it.");
    }

    public void SetMainItem(DiscussionTopic topic) 
    {
      topic.AddTopic(KnownSentenceKind.Bye);
      MainItem = topic;
    }

    public DiscussionTopic MainItem 
    { 
      get => mainItem; 
      set => mainItem = value; 
    }
    
    [JsonIgnore]
    public Container Container { get => container; set => container = value; }

    public string ToXml()
    {
      return "";
    }

    public void FromXml(string xml)
    {
      return;
    }

    public void ToXmlFile()
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

    public static Discussion FromXmlFile(string entityName)
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

    public virtual void EmitCheating(DiscussionTopic item, INPC inpc)
    {
    }

    public virtual bool AcceptQuest(QuestManager qm, string questKind)
    {
      return false;
    }

    public void SetParents()
    {
      SetParentsForTopic(MainItem);
    }

    private void SetParentsForTopic(DiscussionTopic parent)
    {
      foreach (var topic in parent.Topics)
      {
        if (topic.Parent == null)
        {
          topic.Parent = parent;
          if(topic.RightKnownSentenceKind == KnownSentenceKind.Back)
            topic.Parent = parent.Parent;
        }

        SetParentsForTopic(topic);
      }
    }
  }
}
