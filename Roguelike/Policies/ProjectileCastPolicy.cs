using Dungeons.Tiles;
//using Dungeons.Tiles.Abstract;
using Roguelike.Abstract.Projectiles;
using Roguelike.Managers;
using Roguelike.Spells;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.LivingEntities;
using System.Linq;

namespace Roguelike.Policies
{
  public class ProjectileCastPolicy : Policy
  {
    LivingEntity caster;
    public GameManager GameManager { get; set; }


    public ProjectileCastPolicy()
    {
      this.Kind = PolicyKind.SpellCast;
    }

    public IDestroyable TargetDestroyable { get => Target as IDestroyable; }
    
    public Tile Target { get; set; }
    public IProjectile Projectile { get; set; }
    public LivingEntity Caster { get => caster; set => caster = value; }
    public IProjectilesFactory ProjectilesFactory { get; set; }
    public int ProjectilesCount { get; internal set; } = 1;
    public bool ContinueAfterHit { get; set; } = false;
    public int MaxVictimsCount = 1;

    public void Apply(LivingEntity caster)
    {
      Apply(this.Projectile, caster, this.Target, this.ProjectilesFactory);
    }

    public void Apply(IProjectile projectile, LivingEntity caster, Tile target, IProjectilesFactory projectilesFactory)
    {
      this.Projectile = projectile;
      this.caster = caster;
      this.Target = target as Tile;
      this.ProjectilesFactory = projectilesFactory;

      caster.State = EntityState.CastingProjectile;
      DoApply(caster);
      ReportApplied(caster);
    }

    protected virtual void DoApply(LivingEntity caster)
    {
      TargetDestroyable.OnHitBy(Projectile);

      if (ProjectilesCount > 1)
      {
        var neibs = GameManager.CurrentNode.GetNeighborhoodTiles<Enemy>(caster, 5);
        var en = TargetDestroyable as Enemy;
        if (en != null)
          neibs.Remove(en);
        for (int i = 1; i < ProjectilesCount; i++)
        {
          en = neibs.First();
          if (en != null)
          {
            en.OnHitBy(Projectile);
            neibs.Remove(en);
          }
          else
            break;
        }
      }
      else if (ContinueAfterHit)
      {
        //TODO
        var en = TargetDestroyable as Enemy;
        var neibs = GameManager.CurrentNode.GetNeighborhoodTiles<Enemy>(caster, 1);
        if (neibs.Any())
        { 
        }

      }
    }
  }
}
