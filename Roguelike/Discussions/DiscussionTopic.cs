using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Roguelike.Discussions
{
  public class DiscussionTopic
  {
    public DiscussionSentence Right { get; set; } = new DiscussionSentence();
    public DiscussionSentence Left { get; set; } = new DiscussionSentence();
    public KnownSentenceKind KnownSentenceKind { get; set; }
    public List<DiscussionTopic> Topics { get; set; } = new List<DiscussionTopic>();
        
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

    public DiscussionTopic(string right, string left, bool allowBuyHound = false, bool addMerchantItems = merchantItemsAtAllLevels)
    {
      this.allowBuyHound = allowBuyHound;
      Right = new DiscussionSentence(right);
      Left = new DiscussionSentence(left);
      if (addMerchantItems)
        Discussion.CreateMerchantResponseOptions(this, allowBuyHound);
    }

    public DiscussionTopic(string right, KnownSentenceKind knownSentenceKind, bool allowBuyHound = false)
      : this(right, knownSentenceKind.ToString(), allowBuyHound, false)
    {
    }

    public bool ShowableOnDiscussionList
    {
      get
      {
        return KnownSentenceKind != KnownSentenceKind.Back && KnownSentenceKind != KnownSentenceKind.Bye &&
               KnownSentenceKind != KnownSentenceKind.LetsTrade;
      }
    }

    public DiscussionTopic GetTopic(string topic)
    {
      return Topics.FirstOrDefault(i => i.Right.Body == topic);
    }

    public DiscussionTopic GetTopic(KnownSentenceKind kind)
    {
      return Topics.SingleOrDefault(i => i.KnownSentenceKind == kind);
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

    private DiscussionTopic CreateBack(DiscussionTopic parent)
    {
      var back = new DiscussionTopic("Back", KnownSentenceKind.Back.ToString());
      back.KnownSentenceKind = KnownSentenceKind.Back;
      back.Parent = parent;
      return back;
    }

    public void InsertTopic(DiscussionTopic subItem, bool atBegining = true)
    {
      subItem.parent = this;
      if (!subItem.HasBack())
      {
        var back = CreateBack(this);//TODO call subItem.CreateBack
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

    public void AddTopic(string right, KnownSentenceKind knownSentenceKind)
    {
      AddTopic(right, knownSentenceKind.ToString(), false, knownSentenceKind);
    }

    public void AddTopic(string right, string left, bool addMerchantItems = merchantItemsAtAllLevels, KnownSentenceKind knownSentenceKind = KnownSentenceKind.Unset)
    {
      var item = new DiscussionTopic(right, left, allowBuyHound, addMerchantItems);
      item.KnownSentenceKind = knownSentenceKind;
      InsertTopic(item, false);
    }
  }
}
