using Dungeons.Tiles;

namespace Roguelike.Tiles
{
  public enum SurfaceKind { Unset, Empty, ShallowWater, DeepWater, Lava/*, SwampShallowWater, SwampDeepWater*/ }

  public class Surface : Tile
  {
    SurfaceKind kind;
    public string OriginMap { get; set; }

    public Surface() : base(Constants.SymbolBackground)
    {
#if ASCII_BUILD
      color = ConsoleColor.Green;
#endif
    }

    public SurfaceKind Kind { get => kind; set => kind = value; }
  }
}
