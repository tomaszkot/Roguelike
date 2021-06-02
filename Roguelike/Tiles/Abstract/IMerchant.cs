namespace Roguelike.Tiles.Abstract
{
  public interface IMerchant
  {
    int GetPrice(Loot loot);
    bool AllowBuyHound { get; set; }
  }
}
