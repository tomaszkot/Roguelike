using Roguelike.State;
using Roguelike.TileContainers;
using Roguelike.Tiles;

namespace Roguelike.Serialization
{
  public interface IPersister
  {
    void SaveHero(Hero hero);
    Hero LoadHero(string heroName);
    
    void SaveGameState(string heroName, GameState gameState);
    GameState LoadGameState(string heroName);

    void SaveLevel(string heroName, GameLevel level);
    GameLevel LoadLevel(string heroName, int index);
  }
}