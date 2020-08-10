using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike.Tiles.Looting
{
  public enum RecipeKind { Unset, Custom, ThreeGems, OneEq, Potion, TwoEq, Gem, Toadstool2Potions, ExplosiveCocktail, Pendant }

  public class Recipe : Loot
  {
    RecipeKind kind;
    public int MinDropDungeonLevel = 0;

    public int MagicDustRequired { get; set; }

    public Recipe() : this(RecipeKind.Unset)
    {
    }

    public override string ToString()
    {
      var res = Name;
      //if (GenerationTestFilter.MockingOn)
      //  res += ",PinCS=" + PlacedInCraftSlot + ", Ind ="+PositionInPage+ ", Page="+PageIndex; 
      return res;
    }

    public override bool Positionable
    {
      get { return true; }
    }

    public Recipe(RecipeKind kind)
    {
      Price = 20;
      MagicDustRequired = 1;
      this.Kind = kind;
      Symbol = '&';
      PageIndex = -1;

      PositionInPage = -1;
      
    }

    public override string PrimaryStatDescription
    {
      get
      {
        var desc = "";
        switch (Kind)
        {
          case RecipeKind.Custom:
            desc = "Can craft anything, provided ingredients are valid";
            break;
          case RecipeKind.ThreeGems:
            desc = "Turns three gems into a better one";
            break;
          case RecipeKind.OneEq:
            desc = "Turns given equipment into other kind of equipment";
            break;
          case RecipeKind.Potion:
            desc = "Turns potion into other kind of potion";
            break;
          case RecipeKind.TwoEq:
            desc = "Turns two equipments into one of better quality";
            break;
          case RecipeKind.Gem:
            desc = "Turns given gem into other kind of gem";
            break;
          case RecipeKind.Toadstool2Potions:
            desc = "Turns given toadstool into potions";
            break;
          case RecipeKind.ExplosiveCocktail:
            desc = "Turns Hooch plus Sulfur into Explosive Cocktail";
            break;
          default:
            break;
        }
        return desc;
      }
    }

    public override string[] GetExtraStatDescription()
    {
      if (extraStatDescription == null)
      {
        extraStatDescription = new string[1];
       
        extraStatDescription[0] = "Required Magic Dust: " + MagicDustRequired;

      }
      return extraStatDescription;
    }

    public Recipe Clone()
    {
      return MemberwiseClone() as Recipe;
    }

    public RecipeKind Kind
    {
      get
      {
        return kind;
      }

      set
      {
        kind = value;
        Name = kind+ " Recipe";
        DisplayedName = Name;
        tag1 = "craft_";
        switch (kind)
        {
          case RecipeKind.Custom:
            tag1 += "unknown";
            Price = 40;
            MagicDustRequired = 2;
            MinDropDungeonLevel = 5;
            break;
          case RecipeKind.ThreeGems:
            tag1 += "gems";
            Name = "Three Gems";
            break;
          case RecipeKind.OneEq:
            tag1 += "one_eq";
            Name = "One Equipment";
            break;
          case RecipeKind.TwoEq:
            tag1 += "two_eq";
            MagicDustRequired = 2;
            Name = "Two Equipments";
            Price = 30;
            MinDropDungeonLevel = 2;
            break;
          case RecipeKind.Potion:
            tag1 += "potions";
            //Name = "";
            break;
          case RecipeKind.Gem:
            tag1 += "gem";
            //Name = "";
            break;
          case RecipeKind.Toadstool2Potions:
            tag1 += "toad_potions";
            //Name = "";
            break;
          case RecipeKind.ExplosiveCocktail:
            tag1 += "expl_cocktail";
            //Name = "";
            break;
          default:
            break;
        }

        

      }
    }
  }
}
