using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Roguelike.Discussions
{
  public class DiscussionItem
  {
    public DiscussionSentence Right { get; set; } = new DiscussionSentence();
    public DiscussionSentence Left { get; set; } = new DiscussionSentence();
    public KnownSentenceKind KnownSentenceKind { get; set; }
    public List<DiscussionItem> Topics { get; set; } = new List<DiscussionItem>();

    [XmlIgnoreAttribute]
    public DiscussionItem Parent { get => parent; set => parent = value; }
    protected const bool merchantItemsAtAllLevels = false;

    bool allowBuyHound;
    DiscussionItem parent;

    public bool HasBack()
    {
      return Topics.Any(i => i.Right.Body == "Back");
    }

    public override string ToString()
    {
      var res = Right + "->" + Left;
      return res;
    }

    public DiscussionItem() { }

    public DiscussionItem(string right, string left, bool allowBuyHound = false, bool addMerchantItems = merchantItemsAtAllLevels)
    {
      this.allowBuyHound = allowBuyHound;
      Right = new DiscussionSentence(right);
      Left = new DiscussionSentence(left);
      if (addMerchantItems)
        Discussion.CreateMerchantResponseOptions(this, allowBuyHound);
    }

    public DiscussionItem(string right, KnownSentenceKind knownSentenceKind, bool allowBuyHound = false)
      : this(right, knownSentenceKind.ToString(), allowBuyHound, false)
    {
    }

    public void InsertTopic(DiscussionItem subItem, bool atBegining = true)
    {
      subItem.parent = this;
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
      var item = new DiscussionItem(right, left, allowBuyHound, addMerchantItems);
      InsertTopic(item, true);
    }

    public void AddTopic(string right, KnownSentenceKind knownSentenceKind)
    {
      AddTopic(right, knownSentenceKind.ToString(), false, knownSentenceKind);
    }

    public void AddTopic(string right, string left, bool addMerchantItems = merchantItemsAtAllLevels, KnownSentenceKind knownSentenceKind = KnownSentenceKind.Unset)
    {
      var item = new DiscussionItem(right, left, allowBuyHound, addMerchantItems);
      item.KnownSentenceKind = knownSentenceKind;
      InsertTopic(item, false);
    }
  }
}
