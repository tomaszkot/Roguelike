using Roguelike.Settings;
using Roguelike.State;
using Roguelike.TileContainers;
using Roguelike.Tiles.LivingEntities;

namespace Roguelike.Serialization
{
  public interface IPersister
  {
    void SaveHero(Hero hero);
    Hero LoadHero(string heroName);
    void DeleteGame(string heroName);

    AlliesStore LoadAllies(string heroName);
    void SaveAllies(string heroName, AlliesStore allies);

    void SaveGameState(string heroName, GameState gameState);
    GameState LoadGameState(string heroName);

    void SaveLevel(string heroName, GameLevel level);
    GameLevel LoadLevel(string heroName, int index);

    void SaveOptions(Options opt);

    Options LoadOptions();
  }
}