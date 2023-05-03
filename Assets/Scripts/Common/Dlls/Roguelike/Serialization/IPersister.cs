using Roguelike.Settings;
using Roguelike.State;
using Roguelike.TileContainers;
using Roguelike.Tiles.LivingEntities;

namespace Roguelike.Serialization
{
  public interface IPersister
  {
    void SaveHero(Hero hero, bool quick);

    void SaveHero(string json, string heroName, bool quick);

    string MakeHero(Hero hero);//, bool quick);

    Hero LoadHero(string heroName, bool quick);
    void DeleteGame(string heroName, bool quick);

    AlliesStore LoadAllies(string heroName, bool quick);
    void SaveAllies(string heroName, AlliesStore allies, bool quick);

    void SaveGameState(string heroName, GameState gameState, bool quick);
    GameState LoadGameState(string heroName, bool quick);

    void SaveLevel(string heroName, GameLevel level, bool quick);
    GameLevel LoadLevel(string heroName, int index, bool quick);

    void SaveOptions(Options opt);

    Options LoadOptions();
  }
}