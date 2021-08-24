using Dungeons.Tiles;
//using Dungeons.Tiles.Abstract;
using Roguelike.Abstract.Projectiles;
using Roguelike.Spells;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.LivingEntities;

namespace Roguelike.Policies
{
  public class ProjectileCastPolicy : Policy
  {
    LivingEntity caster;
    
    public ProjectileCastPolicy()
    {
      this.Kind = PolicyKind.SpellCast;
    }

    public IDestroyable TargetDestroyable { get => Target as IDestroyable; }
    
    public Tile Target { get; set; }
    public IProjectile Projectile { get; set; }
    public LivingEntity Caster { get => caster; set => caster = value; }
    public IProjectilesFactory ProjectilesFactory { get; set; }

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
    }
  }
}
