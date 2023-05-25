using Dungeons.Tiles;
using Godot;
using System;
using System.Runtime.CompilerServices;

public static class WorldTileMap
{
  public const int TileSize = 128;
  public static void SetTileCell(this TileMap tilemap, Dungeons.Tiles.Tile tile, int layer, int tileId)
  {
	tilemap.SetCell(layer, new Vector2I(tile.point.X, tile.point.Y), tileId, new Vector2I(0, 0));
  }
}
