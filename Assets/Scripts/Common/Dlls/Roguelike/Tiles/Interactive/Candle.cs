using Roguelike.Tiles.Interactive;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles.Interactive
{
  public class Candle : InteractiveTile
  {
    public Candle(Container cont) : base(cont, '~')
    {
      tag1 = "candle";
      Symbol = '~';
      Name = "candle";

      Kind = InteractiveTileKind.Candle;
    }
  }
}
