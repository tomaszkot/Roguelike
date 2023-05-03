using Dungeons.Fight;
using Roguelike.Calculated;
using Roguelike.Spells;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System.Drawing;

namespace Roguelike.Tiles.Interactive
{
  public class CrackedStone : InteractiveTile, IDestroyable
  {
    public const float StartHealthBase = 40;
    public float Health { get; set; }
    public float StartHealth { get; set; } = StartHealthBase;
    public bool Destroyed
    {
      get
      {
        return Health <= 0;
      }
    }

    public bool Damaged
    {
      get
      {
        return Health < StartHealth;
      }
    }

    bool IDestroyable.Destroyed { get ; set ; }
    public string OriginMap { get; set ; }
    public bool LevelSet { get; set ; }
    public Loot ForcedReward { get; set; }

    public CrackedStone(Container cont) : base(cont, '%')
    {
      Kind = InteractiveTileKind.CrackedStone;
      Name = Kind.ToString();
      tag1 = "cracked_stone";
      InteractSound = "punch";
      Health = StartHealth;
      DestroySound = "bones_fall_golem";
    }

    internal CrackedStone Clone()
    {
      return MemberwiseClone() as CrackedStone;
    }

    void CallEmitInteraction()
    {
      //Destroyed = true;
      EmitInteraction();
    }

    public override HitResult OnHitBy(Dungeons.Tiles.Abstract.IDamagingSpell damager)
    {
      var res = base.OnHitBy(damager);
      return HandleHit(damager, res);
    }

    public HitResult OnHitBy(ProjectileFightItem pfi)
    {
      Health -= pfi.Damage * 4;
      PlayHitSound(pfi);
      return HitResult.Hit;
    }

    public override HitResult OnHitBy(Dungeons.Tiles.Abstract.IProjectile md)
    {
      //var res = base.OnHitBy(md);
      //if (res == HitResult.Hit)
      {
        if (md is Dungeons.Tiles.Abstract.IDamagingSpell ds)
          return OnHitBy(ds);
        //else if (md is Dungeons.Tiles.Abstract.IProjectile proj)
        //  return HandleHit(proj, res);
        else if (md is ProjectileFightItem pfi)
          return OnHitBy(pfi);
      }

      return HitResult.Unset;
    }

    private HitResult HandleHit(Dungeons.Tiles.Abstract.IDamagingSpell md, HitResult res)
    {
      if (res == HitResult.Hit)
      {
        if (md != null)
        {
          Health -= md.Damage * 4;
          PlayHitSound(md);
        }
        CallEmitInteraction();
      }
      return res;
    }

    public HitResult OnHitBy(LivingEntity le)
    {
      var ad = new AttackDescription(le);
      Health -= ad.CurrentPhysicalVariated;
      CallEmitInteraction();
      return HitResult.Hit;
    }

    public bool SetLevel(int level, Difficulty? diff = null)
    {
      return true;//TODO ?
    }

    public Point GetPoint()
    {
      return point;
    }
  }
}
