using Dungeons.Core;
using Roguelike.Attributes;
using Roguelike.Tiles.Looting;

namespace Roguelike.Tiles.Looting
{
  public enum PlantKind
  {
    Unset,
    Thistle,//oset
    Sorrel  //szczaw
  }

  public class Plant : Consumable
  {
    PlantKind kind;
    public PlantKind Kind 
    {
      get => kind;
      set {
        kind = value;
        SetKindMembers(kind);
      }
    }

    public Plant() : this(PlantKind.Unset)
    {
    }

    public Plant(PlantKind kind)
    {
      Symbol = '-';
      LootKind = LootKind.Plant;
      SetKind(kind);
      Price = 15;
    }

    public override bool IsConsumable()
    {
      return Duration > 0;
    }

    public void SetKind(PlantKind kind)
    {
      Kind = kind;
      SetKindMembers(kind);
    }

    private void SetKindMembers(PlantKind kind)
    {
      if (Kind == PlantKind.Sorrel)
        Duration = 5;
      else
        Duration = 0;
      Name = kind.ToString();


      DisplayedName = Name;
      SetPrimaryStatDesc();
    }

    void SetPrimaryStatDesc()
    {
      string desc = "";
      if (Kind == PlantKind.Thistle)
      {
        desc = "Unpleasant to touch, " + Strings.PartOfCraftingRecipe;
        tag1 = "Thistle1";
        StatKind = EntityStatKind.Unset;
      }
      else if (Kind == PlantKind.Sorrel)
      {
        desc = GetConsumeDesc("Eatable, sour plant");
        tag1 = "Sorrel";
      }
      PrimaryStatDescription = desc;
    }

    public override string GetId()
    {
      return base.GetId() + "_" + Kind;
    }

    public override bool IsMatchingRecipe(RecipeKind kind)
    {
      if (kind == RecipeKind.AntidotePotion && this.Kind == PlantKind.Thistle)
        return true;
      if (kind == RecipeKind.NiesiolowskiSoup && this.Kind == PlantKind.Sorrel)
        return true;
      return false;
    }
  }
}
