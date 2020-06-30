using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike
{
  public class EnemySymbols
  {
    public const char VampireSymbol = 'a';
    public const char BatSymbol = 'b';
    public const char SnakeSymbol = 'c';//'c';
    public const char MerchantBroSymbol = '}';//'c';
    public const char WizardSymbol = 'd';
    public const char CommonEnemySymbol = 'e';// reserved for generic enemy
    public const char FallenOneSymbol = 'f';
    public const char GriffinSymbol = 'g';
    public const char HydraSymbol = 'h';
    public const char HienaSymbol = 'i';
    public const char DragonSymbol = 'j';
    public const char FaunSymbol = 'k';
    public const char GoblinSymbol = 'l';
    public const char MorphSymbol = 'm';
    public const char DaemonSymbol = 'n';
    public const char ScorpionSymbol = 'o';

    public const char DungBeatleSymbol = 'u';
    public const char SpiderSymbol = 'p';
    public const char RatSymbol = 'r';
    public const char SkeletonSymbol = 's';
    public const char TreantSymbol = 't';
    public const char WolfSymbol = 'w';

    public const char OgreSymbol = 'v';
    public const char ManEaterSymbol = 'x';
    public const char FallenOneSymbolPhantom = 'y';
    public const char ZombieSymbol = 'z';

    public const char QuestBoss = 'q';

    public static Dictionary<string, char> EnemiesToSymbols = new Dictionary<string, char>()
    {
      {"rat", RatSymbol},
      {"bat", BatSymbol},
      {"spider", SpiderSymbol},
      {"skeleton", SkeletonSymbol},
      {"snake", SnakeSymbol},
      {"wolf", WolfSymbol},
      {"bear", CommonEnemySymbol},
      {"boar", CommonEnemySymbol},
      {"lynx", CommonEnemySymbol},
      {"worm", CommonEnemySymbol},
      {"wolverine", CommonEnemySymbol},
      
    };

    public static char GetSymbolFromName(string name)
    {
      if(EnemiesToSymbols.ContainsKey(name))
        return EnemiesToSymbols[name];
      return '\0';
    }


  }
}
