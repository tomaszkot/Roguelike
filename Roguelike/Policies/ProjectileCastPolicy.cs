using Dungeons.Tiles;
//using Dungeons.Tiles.Abstract;
using Roguelike.Abstract.Projectiles;
using Roguelike.Spells;
using Roguelike.Tiles.LivingEntities;

namespace Roguelike.Policies
{
  public class ProjectileCastPolicy : Policy
  {
    LivingEntity caster;
    Tile target;

    public ProjectileCastPolicy()
    {
      this.Kind = PolicyKind.SpellCast;
    }

    public IObstacle TargetObstacle { get => target as IObstacle;}
    public Tile Target { get; set; }
    public IProjectile Projectile { get; set; }
    public LivingEntity Caster { get => caster; set => caster = value; }
    public IProjectilesFactory ProjectilesFactory { get; set; }

    public void Apply(LivingEntity caster)
    {
      Apply(this.Projectile, caster, this.TargetObstacle, this.ProjectilesFactory);
    }

    public void Apply(IProjectile projectile, LivingEntity caster, IObstacle target, IProjectilesFactory projectilesFactory)
    {
      this.Projectile = projectile;
      this.caster = caster;
      this.target = target as Tile;
      this.ProjectilesFactory = projectilesFactory;

      caster.State = EntityState.CastingProjectile;
      DoApply(caster);
      ReportApplied(caster);
    }

    protected virtual void DoApply(LivingEntity caster)
    {
      TargetObstacle.OnHitBy(Projectile);
    }
  }
}
