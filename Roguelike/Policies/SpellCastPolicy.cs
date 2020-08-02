using Roguelike.Abstract;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;

namespace Roguelike.Policies
{
  public class SpellCastPolicy : Policy
  {
    LivingEntity caster;
    LivingEntity target;
    Scroll scroll;

    public SpellCastPolicy()
    {
      this.Kind = PolicyKind.SpellCast;
    }

    public LivingEntity Target { get => target; set => target = value; }
    public Scroll Scroll { get => scroll; set => scroll = value; }
    public LivingEntity Caster { get => caster; set => caster = value; }
    public IProjectilesFactory ProjectilesFactory { get ; set; }

    public void Apply(LivingEntity caster)
    {
      Apply(this.Scroll, caster, this.Target, this.ProjectilesFactory);
    }

    public void Apply(Scroll scroll, LivingEntity caster, LivingEntity target, IProjectilesFactory projectilesFactory)
    {
      this.scroll = scroll;
      this.caster = caster;
      this.target = target;
      this.ProjectilesFactory = projectilesFactory;

      caster.State = EntityState.CastingSpell;
      DoApply(caster);

      ReportApplied(caster);
    }

    protected virtual void DoApply(LivingEntity caster)
    {
      var spell = Scroll.CreateSpell(Caster);
      Target.OnHitBy(spell);
    }
  }
}
