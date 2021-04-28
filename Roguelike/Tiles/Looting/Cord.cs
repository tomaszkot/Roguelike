namespace Roguelike.Tiles.Looting
{
  public class Cord : StackedLoot
  {
    public Cord()
    {
      Price = 5;
      Symbol = '&';
      LootKind = LootKind.Other;
      tag1 = "cord";
      PrimaryStatDescription = Strings.PartOfCraftingRecipe;
    }
  }
}
