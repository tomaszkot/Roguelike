using Dungeons.Core;
using Roguelike.Spells;
using SimpleInjector;

namespace Roguelike.Tiles.LivingEntities
{
  public class Animal : LivingEntity
  {
    public int RandMoveCoolDown = 1;
    public bool PreventMove { get; set; } = false;

    public Animal(Container cont) : base(cont)
    {
    }

    internal override bool CanMakeRandomMove()
    {
      if (PreventMove)
        return false;
      if(RandMoveCoolDown > 0)
        RandMoveCoolDown--;
      var res = RandMoveCoolDown == 0;
      if (res)
        RandMoveCoolDown = RandHelper.GetRandomInt(4);
      return res;
    }

    protected override void OnHitBy(float amount, Spell spell, string damageDesc = null)
    {
      base.OnHitBy(amount, spell, damageDesc);
    }

    public override float OnMeleeHitBy(LivingEntity attacker)
    {
      if(tag1 == "rooster" || tag1 == "hen")
        PlaySound("rooster_scream");
      else if (tag1 == "pig")
        PlaySound("pig_scream");
      return base.OnMeleeHitBy(attacker);
    }
  }
}
