using Dungeons.Core;
using Roguelike.Extensions;
using Roguelike.Tiles.Looting;
using System;

namespace Roguelike.Tiles.Looting
{
  public enum MushroomKind { Unset, BlueToadstool, RedToadstool, Boletus };

  public class Mushroom : Food
  {
    public MushroomKind mushroomKind;
    public MushroomKind MushroomKind 
    {
      get { return mushroomKind; }
      set 
      {
        if (mushroomKind == value)
          return;
        if (mushroomKind != MushroomKind.Unset)
        {
          throw new Exception("mushroomKind != MushroomKind.Unset, "+ mushroomKind);//chagning not supported due to SetPoisoned method
        }
        mushroomKind = value;
        Name = MushroomKind.ToDescription();
        if (MushroomKind == MushroomKind.BlueToadstool)
        {
          SrcPotion = PotionKind.Mana;
          DestPotion = SpecialPotionKind.Magic;

          StatKind = Attributes.EntityStatKind.Mana;
        }
        else
        {
          SrcPotion = PotionKind.Health;
          DestPotion = SpecialPotionKind.Strength;
          StatKind = Attributes.EntityStatKind.Health;
        }
        NegativeFactor = MushroomKind == MushroomKind.RedToadstool;

        //yes, both are poisonous
        if (MushroomKind == MushroomKind.BlueToadstool || MushroomKind == MushroomKind.RedToadstool)
          SetPoisoned();

        DisplayedName = Name;
        SetPrimaryStatDesc();
        SetDefaultTagFromKind();
      } 
    }
    public PotionKind SrcPotion { get; set; }
    public SpecialPotionKind DestPotion { get; set; }

    public Mushroom() : this(MushroomKind.Unset)
    {
    }

    public Mushroom(MushroomKind kind) : base(FoodKind.Mushroom)
    {
      Symbol = '-';
      MushroomKind = kind;
      Price = 15;
    }

    public override string GetId()
    {
      return base.GetId() + "_" + MushroomKind.ToString();
    }

    protected virtual void SetDefaultTagFromKind()
    {
      if (MushroomKind == MushroomKind.BlueToadstool)
        tag1 = "mash_BlueToadstool1";
      else if (MushroomKind == MushroomKind.RedToadstool)
        tag1 = "mash_RedToadstool1";
      else
        tag1 = "mash_Boletus";
    }

    //public override Loot CreateCrafted(Loot other)
    //{
    //  if (other is Potion && (other as Potion).Kind == PotionKind.Health)
    //  {
    //    return new SpecialPotion(SpecialPotionKind.Strength, SpecialPotionSize.Small);
    //  }
    //  else if (other is Potion && (other as Potion).Kind == PotionKind.Mana)
    //  {
    //    return new SpecialPotion(SpecialPotionKind.Magic, SpecialPotionSize.Small);
    //  }

    //  return null;
    //}

    //public override bool IsCraftableWith(LootBase other)
    //{
    //  if (other is Potion)
    //  {
    //    return true;
    //  }

    //  return false;
    //}
    void SetPrimaryStatDesc()
    {
      //string desc = "Turns " + Extensions.FirstCharToUpper(SrcPotion.ToString()) + " Potion into";
      //if (DestPotion == SpecialPotionKind.Magic)
      //  desc += " a Magic ";
      //else
      //  desc += " a Strength ";

      //desc += "Potion.";
      string desc = PartOfCraftingRecipe;
      desc += GetConsumeDesc(" Consumable");
      PrimaryStatDescription = desc;
    }

    public override string ToString()
    {
      return base.ToString() + " " + MushroomKind;
    }

    //public Loot Loot => this;

    //public EntityStatKind EnhancedStat => EntityStatKind.Health;

    public override string[] GetExtraStatDescription()
    {
      return extraStatDescription;
    }
  }
}
