using Dungeons.Core;
using Newtonsoft.Json;
using Roguelike.Managers;
using Roguelike.Serialization;
using Roguelike.Tiles.LivingEntities;
using SimpleInjector;
using System.Diagnostics;

namespace Roguelike.Discussions
{
  public enum KnownSentenceKind { Unset, WhatsUp, LetsTrade, Bye, SellHound, Back, QuestAccepted, QuestRejected, QuestProgress, 
    WorkingOnQuest, AwaitingReward,  
    Cheating, DidIt , RewardDeny, AwaitingRewardAfterRewardDeny, 
    RewardSkipped, AllyAccepted, AllyRejected, AskedToAddAlcohol, AgreeToAddAlcohol, RefusedToAddAlcohol
  }


  public class Discussion : IPersistable
  {
      
    protected Container container;
    public string EntityName { get; set; }
    protected DiscussionTopic mainItem;

    //[JsonConstructor]
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

    public void Reset()
    {
      SetMainItem(CreateMainItem());
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

    public virtual void EmitCheating(DiscussionTopic item, INPC inpc)
    {
    }

    public virtual bool AcceptQuest(QuestManager qm, string questKind, string questPrincipalName)
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

    public bool RemoveTopic(KnownSentenceKind ksk)
    {
      var topic = MainItem.GetTopic(ksk);
      if (topic != null)
        return  RemoveTopic(topic);

      return false;
    }

    public bool RemoveTopic(DiscussionTopic topic)
    {
      if (topic == null)
      {
        container.GetInstance<ILogger>().LogError("RemoveTopic null!"+ topic);
        return false; 
      }
      return topic.Parent.Topics.Remove(topic);
    }

    
  }
}
