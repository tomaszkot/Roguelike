using Roguelike.Extensions;

namespace Roguelike.Tiles.Looting
{
  public enum RecipeKind
  {
    Unset, Custom, ThreeGems, OneEq, TransformPotion, TwoEq, TransformGem, Toadstools2Potion, ExplosiveCocktail, Pendant,
    EnchantEquipment, CraftSpecialPotion
  }

  public class Recipe : StackedLoot
  {
    RecipeKind kind;
    public int MinDropDungeonLevel = 0;

    public int MagicDustRequired { get; set; }

    public Recipe() : this(RecipeKind.Unset)
    {
    }

    public override string ToString()
    {
      var res = base.ToString() + " " + Name;
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
      LootKind = LootKind.Recipe;
      Price = 20;
      MagicDustRequired = 1;
      this.Kind = kind;
      Symbol = '&';
      PageIndex = -1;

      PositionInPage = -1;

    }

    void SetPrimaryStatDescription()
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
        case RecipeKind.TransformPotion:
          desc = "Turns potion into other kind of potion";
          break;
        case RecipeKind.TwoEq:
          desc = "Turns two equipments into one of better quality";
          break;
        case RecipeKind.TransformGem:
          desc = "Turns given gem into other kind of gem";
          break;
        case RecipeKind.Toadstools2Potion:
          desc = "Turns toadstools into a potion";
          break;
        case RecipeKind.ExplosiveCocktail:
          desc = "Turns Hooch plus Sulfur into Explosive Cocktail";
          break;
        case RecipeKind.Pendant:
          desc = "Turns a piece of cord into a pendant";
          break;
        case RecipeKind.EnchantEquipment:
          desc = "Enchants equipment with a gem or a hunter's trophy";
          break;

        case RecipeKind.CraftSpecialPotion:
          desc = "Turns potion plus boletus into a special potion";
          break;
        default:
          break;
      }

      PrimaryStatDescription = desc;
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

    public override string GetId()
    {
      return base.GetId() + Kind;
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
        Name = kind + " Recipe";
        DisplayedName = kind.ToDescription() + " Recipe";
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
            tag1 += "three_gems";
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
          case RecipeKind.TransformPotion:
            tag1 += "transform_potion";
            //Name = "";
            break;
          case RecipeKind.TransformGem:
            tag1 += "transform_gem";
            //Name = "";
            break;
          case RecipeKind.Toadstools2Potion:
            tag1 += "toad_potions";
            //Name = "";
            break;
          case RecipeKind.ExplosiveCocktail:
            tag1 += "expl_cocktail";
            //Name = "";
            break;
          case RecipeKind.Pendant:
            tag1 += "pendant";
            break;
          case RecipeKind.EnchantEquipment:
            tag1 += "enchant";
            MagicDustRequired = 0;//drop in inv is free
            break;
          case RecipeKind.CraftSpecialPotion:
            tag1 += "special_potion";
            break;
          default:
            break;
        }

        SetPrimaryStatDescription();

      }
    }
  }
}
