using Dungeons.Core;
using Dungeons.Core.Policy;
using Dungeons.Tiles.Abstract;
using Dungeons.Fight;
using Dungeons.Tiles;
using Roguelike.Spells;
using Roguelike.Tiles.Looting;
using System;
using System.Diagnostics;
using System.Drawing;

namespace Roguelike.Tiles
{
  public enum SurfaceKind { Unset, ShallowWater, DeepWater, Lava/*, SwampShallowWater, SwampDeepWater*/, Oil }

  public enum SurfacePlacementSide { Unset, Left, Top, Right, Bottom};

  public class Surface : Tile, ISurface
  {
    SurfaceKind kind;
    public string OriginMap { get; set; }
    public int Durability { get; set; } = -1;//-1 infinite
    public SurfacePlacementSide PlacementSide { get; set; }

    public Surface() : base(Constants.SymbolBackground)
    {
#if ASCII_BUILD
      color = ConsoleColor.Green;
#endif
    }

    public Surface(string tag) : base(Constants.SymbolBackground)
    {
      if (tag == "water_shallow")
      {
        Kind = SurfaceKind.ShallowWater;
      }
      else if (tag == "water_deep")
      {
        Kind = SurfaceKind.DeepWater;
      }
      else if (tag == "pure_lava" ||
               tag == "pure_lava_round")
      {
        Kind = SurfaceKind.Lava;
        
      }
      else if (tag == "oil" || tag == "pure_oil")
      {
        Kind = SurfaceKind.Oil;
      }
      else
        Debug.Write("unhandled tag: "+ tag);

      tag1 = tag;
    }

    public bool IsBurning { get; set; }
    public event EventHandler Destroyed;
    public bool IsWater { get { return Kind == SurfaceKind.ShallowWater || Kind == SurfaceKind.DeepWater; } }

    public bool IsWaterLikeSound { get { return IsWater || Kind == SurfaceKind.Oil; } }
    public SurfaceKind Kind 
    { 
      get => kind;
      set
      {
        kind = value;
        if (SurfaceKind.Lava == kind)
          IsBurning = true;
        else if (SurfaceKind.Oil == kind)
        {
          SupportsDurability = true;
          Durability = (int)RandHelper.GetRandomFloatInRange(4, 6);
        }
      }
    }
    public bool SupportsDurability { get; internal set; }

    public override string ToString()
    {
      return base.ToString() + "[" + Kind + "]";
    }

    public void ReportDestroyed()
    {
      if (Destroyed != null)
        Destroyed(this, EventArgs.Empty);
    }
  }


  public class HitableSurface : Surface, Dungeons.Tiles.Abstract.IHitable
  {
    public Point Position => point;
    public event EventHandler StartedBurning;
    

    public HitableSurface() : base()
    {
      
    }

    public HitableSurface(string tag) : base(tag)
    {
    }

    

    public HitResult OnHitBy(IProjectile md, IPolicy policy)
    {
      if (md is FireBallSpell ||
        (md is ProjectileFightItem pfi && pfi.CausesFire))
      {
        StartBurning();
      }
      return HitResult.Hit;
    }

    public void StartBurning()
    {
      if (!IsBurning)
      {
        IsBurning = true;
        if (StartedBurning != null)
          StartedBurning(this, EventArgs.Empty);
      }
    }

    public HitResult OnHitBy(IDamagingSpell ds, IPolicy policy)
    {
      return HitResult.Hit;
    }

    public void PlayHitSound(IProjectile proj)
    {
      
    }

    public void PlayHitSound(IDamagingSpell spell)
    {
      
    }

    public HitResult OnHitBy(ILivingEntity livingEntity)
    {
      return HitResult.Hit;
    }
  }
}
