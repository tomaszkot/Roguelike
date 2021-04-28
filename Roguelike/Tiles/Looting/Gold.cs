using Dungeons.Core;

namespace Roguelike.Tiles.Looting
{
  public class Gold : StackedLoot
  {
    public Gold(int amount)
    {
      Symbol = GoldSymbol;
#if ASCII_BUILD
      color = GoldColor;
#endif
      tag1 = "coin";
      Count = amount;
      LootKind = LootKind.Gold;
      collectedSound = "coin_collected";
      PrimaryStatDescription = Count + " gold coins";
    }

    public Gold() : this(RandHelper.Random.Next(4, 8))
    {

    }
  }
}
