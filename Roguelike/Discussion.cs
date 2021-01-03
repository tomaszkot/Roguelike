﻿using Roguelike.Tiles;
using System.Collections.Generic;

namespace Roguelike.Discussions
{
  public enum KnownSentenceKind { Unset, LetsTrade, Bye, SellHound, Back }

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
    public List<DiscussionItem> DiscussionSubItems { get; set; } = new List<DiscussionItem>();
    public DiscussionItem Parent { get => parent; set => parent = value; }
    const bool merchantItemsAtAllLevels = false;

    bool allowBuyHound;
    DiscussionItem parent;

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

    public void InsertSubItem(DiscussionItem subItem, bool atBegining)
    {
      var back = new DiscussionItem("Back", KnownSentenceKind.Back.ToString(), false, false);
      back.parent = this;
      subItem.DiscussionSubItems.Add(back);

      if(atBegining)
        DiscussionSubItems.Insert(0, subItem);
      else
        DiscussionSubItems.Add(subItem);
    }

    public void InsertSubItem(string right, KnownSentenceKind knownSentenceKind)
    {
      InsertSubItem(right, knownSentenceKind.ToString(), false);
    }

    public void InsertSubItem(string right, string left, bool addMerchantItems = merchantItemsAtAllLevels)
    {
      var item = new DiscussionItem(right, left, allowBuyHound, addMerchantItems);
      InsertSubItem(item, true);
    }

    public void AddSubItem(string right, KnownSentenceKind knownSentenceKind)
    {
      //var item = new DiscussionItem(right, knownSentenceKind.ToString(), allowBuyHound, false);
      AddSubItem(right, knownSentenceKind.ToString(), false);
    }

    public void AddSubItem(string right, string left, bool addMerchantItems = merchantItemsAtAllLevels)
    {
      var item = new DiscussionItem(right, left, allowBuyHound, addMerchantItems);
      InsertSubItem(item, false);
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
        var item = new DiscussionItem("Could you make an iron sword for me ?", 
          "Nope, due to the king's edict we are allowed to sell an iron equipment only to knights. There is a way to do it though. If you deliver me 10 pieces of the iron ore I can devote part of it for making you a weapon."
          );

        var subItem = new DiscussionItem("Where would I find iron ore?", "There is a mine west of here. Be aware monters have nested there, so it won't be easy.");
        
        
        item.InsertSubItem(subItem, true);
        mainItem.InsertSubItem(item, true);
      }

      dis.mainItem = mainItem;
      return dis;
    }

    public static Discussion CreateForLionel(bool allowBuyHound)
    {
      var dis = CreateForMerchant("Lionel", allowBuyHound);
      var item1 = new DiscussionItem("What's up?", "Dark times have arrived...", allowBuyHound);
      dis.MainItem.InsertSubItem(item1, true);
      return dis;
    }

    public static void CreateMerchantResponseOptions(DiscussionItem item, bool allowBuyHound)
    {
      item.AddSubItem("Let's Trade", KnownSentenceKind.LetsTrade);
      if(allowBuyHound)
        item.AddSubItem("Sell me a hound ("+Merchant.HoundPrice+" gold)", KnownSentenceKind.SellHound);

      item.AddSubItem("Bye", KnownSentenceKind.Bye);
    }

    public DiscussionItem MainItem { get => mainItem; set => mainItem = value; }
  }
}
