using Roguelike.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Roguelike.Discussions
{
  public class DiscussionTopic
  {
    public bool SkipReward { get; set; }
    public DiscussionSentence Right { get; set; } = new DiscussionSentence();
    public DiscussionSentence Left { get; set; } = new DiscussionSentence();
    public KnownSentenceKind RightKnownSentenceKind 
    {
      get { return Right.KnownSentenceKind; }
      set { Right.KnownSentenceKind = value; } 
    }
    public List<DiscussionTopic> Topics { get; set; } = new List<DiscussionTopic>();
    public bool NPCJoinsAsAlly { get; internal set; }
    public bool HoundJoinsAsAlly { get;  set; }

    [XmlIgnoreAttribute]
    public DiscussionTopic Parent { get => parent; set => parent = value; }
    protected const bool merchantItemsAtAllLevels = false;

    bool allowBuyHound;
    DiscussionTopic parent;

    public bool HasBack()
    {
      return Topics.Any(i => i.Right.Body == "Back");
    }

    public override string ToString()
    {
      var res = Right + "->" + Left;
      return res;
    }

    public DiscussionTopic() { }

    public DiscussionTopic(KnownSentenceKind right, string left, bool allowBuyHound = false, bool addMerchantItems = merchantItemsAtAllLevels)
    {
      //RightKnownSentenceKind = right;
      Right = new DiscussionSentence(right);
      if(right == KnownSentenceKind.Bye && left == "")
        Left = new DiscussionSentence(KnownSentenceKind.Bye);
      else
        Left = new DiscussionSentence(left);

      Init(allowBuyHound, addMerchantItems);
    }

    private void Init(bool allowBuyHound, bool addMerchantItems)
    {
      this.allowBuyHound = allowBuyHound;
      if (addMerchantItems)
        Discussion.CreateMerchantResponseOptions(this, allowBuyHound);
    }

    public DiscussionTopic(string right, string left, bool allowBuyHound = false, bool addMerchantItems = merchantItemsAtAllLevels)
    {
      Right = new DiscussionSentence(right);
      Left = new DiscussionSentence(left);

      Init(allowBuyHound, addMerchantItems);
    }

    public DiscussionTopic(string right, KnownSentenceKind leftKnownSentenceKind, bool allowBuyHound = false)
    {
      Right = new DiscussionSentence(right);
      Left = new DiscussionSentence(leftKnownSentenceKind);

      Init(allowBuyHound, false);
    }

    public string RightSuffix { get; set; } = ""; 

    public void AddTopic(KnownSentenceKind rightKnownSentenceKind, string rightSuffix = "", string left ="")
    {
      var item = new DiscussionTopic(rightKnownSentenceKind, "", false, false);
      item.RightSuffix = rightSuffix;

      item.Left.Body = left;
      InsertTopic(item, false);
    }

    public void AddTopic(string right, string left, bool addMerchantItems = merchantItemsAtAllLevels)
    {
      var item = new DiscussionTopic(right, left, allowBuyHound, addMerchantItems);
      InsertTopic(item, false);
    }

    public bool ShowableOnDiscussionList
    {
      get
      {
        return RightKnownSentenceKind != KnownSentenceKind.Back && RightKnownSentenceKind != KnownSentenceKind.Bye &&
               RightKnownSentenceKind != KnownSentenceKind.LetsTrade;
      }
    }

    public DiscussionTopic GetTopic(string topic)
    {
      return Topics.FirstOrDefault(i => i.Right.Body == topic);
    }

    public DiscussionTopic GetTopic(KnownSentenceKind kind)
    {
      return Topics.SingleOrDefault(i => i.RightKnownSentenceKind == kind);
    }

    public bool HasTopics(string topic)
    {
      return GetTopic(topic) != null;
    }

    public bool HasTopics(KnownSentenceKind kind)
    {
      return GetTopic(kind) != null;
    }

    public void EnsureBack()
    {
      if (!HasBack())
      {
        var back = CreateBack(this.parent);
        Topics.Add(back);
      }
    }

    public DiscussionTopic CreateBack(DiscussionTopic parent)
    {
      var back = new DiscussionTopic(KnownSentenceKind.Back, KnownSentenceKind.Back.ToString());
      back.Parent = parent;
      return back;
    }

    public void InsertTopic(DiscussionTopic subItem, bool atBegining = true)
    {
      if (subItem == this)
        throw new Exception("subItem == this!"+this);
      subItem.parent = this;
      if (!subItem.HasBack())
      {
        var back = CreateBack(this);
        subItem.Topics.Add(back);
      }
      if (atBegining)
        Topics.Insert(0, subItem);
      else
      {
        if (HasBack())
        {
          Topics.Insert(Topics.Count - 1, subItem);
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
      var item = new DiscussionTopic(right, left, allowBuyHound, addMerchantItems);
      InsertTopic(item, true);
    }

    
  }
}
