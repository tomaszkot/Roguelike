using Roguelike.TileContainers;
using Roguelike.Tiles;

namespace Roguelike.Serialization
{
  public interface IPersister
  {
    void SaveHero(Hero hero);
    Hero LoadHero();
    
    void SaveGameState(GameState gameState);
    GameState LoadGameState();

    void SaveLevel(GameLevel level);
    GameLevel LoadLevel(int index);
  }
}