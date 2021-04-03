using Roguelike.Abstract;
using Roguelike.Abstract.Projectiles;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;

namespace Roguelike.Policies
{
  public class SpellCastPolicy : Policy
  {
    LivingEntity caster;
    Roguelike.Tiles.Abstract.IDestroyable target;
    Spell spell;

    public SpellCastPolicy()
    {
      this.Kind = PolicyKind.SpellCast;
    }

    public Roguelike.Tiles.Abstract.IDestroyable Target { get => target; set => target = value; }
    public Spell Spell { get => spell; set => spell = value; }
    public LivingEntity Caster { get => caster; set => caster = value; }
    public IProjectilesFactory ProjectilesFactory { get ; set; }

    public void Apply(LivingEntity caster)
    {
      Apply(this.Spell, caster, this.Target, this.ProjectilesFactory);
    }

    public void Apply(Spell spell, LivingEntity caster, Roguelike.Tiles.Abstract.IDestroyable target, IProjectilesFactory projectilesFactory)
    {
      this.spell = spell;
      this.caster = caster;
      this.target = target;
      this.ProjectilesFactory = projectilesFactory;

      caster.State = EntityState.CastingSpell;
      DoApply(caster);
      ReportApplied(caster);
    }

    protected virtual void DoApply(LivingEntity caster)
    {
      //var spell = Scroll.CreateSpell(Caster);
      Target.OnHitBy(spell);
    }
  }
}
