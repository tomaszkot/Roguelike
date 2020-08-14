using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles.Looting
{
  public class Cord : StackedLoot
  {
    public string primaryStatDescription = Strings.PartOfCraftingRecipe;

    public override string PrimaryStatDescription => primaryStatDescription;

    public Cord()
    {
      Price = 5;
      Symbol = '&';
      LootKind = LootKind.Other;
    }
  }
}
