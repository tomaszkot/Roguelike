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
    IObstacle target;

    public ProjectileCastPolicy()
    {
      this.Kind = PolicyKind.SpellCast;
    }

    public IObstacle Target { get => target; set => target = value; }
    public IProjectile Projectile { get; set; }
    public LivingEntity Caster { get => caster; set => caster = value; }
    public IProjectilesFactory ProjectilesFactory { get; set; }

    public void Apply(LivingEntity caster)
    {
      Apply(this.Projectile, caster, this.Target, this.ProjectilesFactory);
    }

    public void Apply(IProjectile projectile, LivingEntity caster, IObstacle target, IProjectilesFactory projectilesFactory)
    {
      this.Projectile = projectile;
      this.caster = caster;
      this.target = target;
      this.ProjectilesFactory = projectilesFactory;

      caster.State = EntityState.CastingProjectile;
      DoApply(caster);
      ReportApplied(caster);
    }

    protected virtual void DoApply(LivingEntity caster)
    {
      //var spell = Scroll.CreateSpell(Caster);
      Target.OnHitBy(Projectile);
    }
  }
}
