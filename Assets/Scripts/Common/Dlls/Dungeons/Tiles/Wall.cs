using Dungeons.Core;
using Dungeons.Fight;
using Dungeons.Tiles.Abstract;
using System;
using System.Drawing;

namespace Dungeons.Tiles
{
  public class Wall : Tile, IObstacle
  {
    public static bool Use25DImages = true;
    EntranceSide entranceSide;
    public Tile Child { get; set; }

    public bool shadowed;
    public bool Shadowed 
    {
      get { return shadowed; }
      set {
        shadowed = value;
        if(shadowed)
          Color = ConsoleColor.DarkGray;
      } 
    }

    public bool DynamicallyGenerated { get; set; }

    public bool GenerateUpperChild { get; set; }
    public EntranceSide EntranceSide
    {
      get { return entranceSide; }
      set 
      {
        entranceSide = value;
      }
    }

    public bool IsSide { get { return entranceSide != EntranceSide.Unset; } }

    public Point Position => point;

    public Wall(Point point) : base(point, Constants.SymbolWall)
    {
    }

    public Wall() : this(new Point().Invalid()) { }

    public HitResult OnHitBy(IProjectile md)
    {
      return HitResult.Hit;
    }

    public HitResult OnHitBy(IDamagingSpell md)
    {
      return HitResult.Hit;
    }

    public virtual void PlayHitSound(IProjectile proj) { }
    public virtual void PlayHitSound(IDamagingSpell spell) { }
  }
}
