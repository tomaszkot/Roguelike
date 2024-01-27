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

    

    void SetPrimaryStatDesc()
    {
      string desc = PartOfCraftingRecipe;
      desc += GetConsumeDesc(" Consumable");
      PrimaryStatDescription = desc;
    }

    public override string ToString()
    {
      return base.ToString() + " " + MushroomKind;
    }

    public override string[] GetExtraStatDescription()
    {
      return extraStatDescription;
    }

    public override bool IsMatchingRecipe(RecipeKind kind)
    {
      if (base.IsMatchingRecipe(kind))
        return true;
      if((kind == RecipeKind.Toadstools2Potion || kind == RecipeKind.TransformPotion) && 
        (MushroomKind == MushroomKind.BlueToadstool || MushroomKind == MushroomKind.RedToadstool))
        return true;
      if (kind == RecipeKind.CraftSpecialPotion && MushroomKind == MushroomKind.Boletus)
        return true;
      return false;
    }
  }
}
