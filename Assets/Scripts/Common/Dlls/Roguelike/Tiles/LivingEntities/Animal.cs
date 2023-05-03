using Dungeons.Core;
using Dungeons.Fight;
using Roguelike.Spells;
using Roguelike.TileContainers;
using Roguelike.Tiles.Looting;
using SimpleInjector;

namespace Roguelike.Tiles.LivingEntities
{
  public class Animal : LivingEntity, ILootSource 
  {
    public int RandMoveCoolDown = 1;
    public bool PreventMove { get; set; } = false;
    AnimalKind kind;
    public int LastHitCoolDown { get; set; }

    public Animal(Container cont, AnimalKind kind) : base(cont)
    {
      AnimalKind = kind;
      EntityKind = EntityKind.Animal;

      var he = Stats.GetStat(Attributes.EntityStatKind.Health);
      he.Value.Nominal /= 3;
    }

    public Animal(Container cont) : this(cont, AnimalKind.Unset)
    {
    }

    public AnimalKind AnimalKind 
    {
      get
      {
        return kind;
      }
      set {
        kind = value;
        DisplayedName = kind.ToString();

      }
    }

    public AnimalKind GetKindFromTag1()
    {
      if (tag1 == "rooster")
        return AnimalKind.Rooster;
      if (tag1 == "hen")
        return AnimalKind.Hen;
      if (tag1 == "deer")
        return AnimalKind.Deer;
      if (tag1 == "pig")
        return AnimalKind.Pig;

      return AnimalKind.Unset;
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

    protected override HitResult OnHitBy(ProjectileFightItem pfi)
    {
      var res = base.OnHitBy(pfi);
      if(res == HitResult.Hit)
        OnDamaged();
      return res;
    }

    protected override void OnHitBy(float amount, Spell spell, string damageDesc = null)
    {
      OnDamaged();
      base.OnHitBy(amount, spell, damageDesc);
    }

    public override bool CalcShallMoveFaster(AbstractGameLevel node)
    {
      return LastHitCoolDown > 0;
    }

    public override float OnMeleeHitBy(LivingEntity attacker)
    {
      var res = base.OnMeleeHitBy(attacker);
      OnDamaged();
      return res;
    }

    private void OnDamaged()
    {
      if (AnimalKind == AnimalKind.Hen || AnimalKind == AnimalKind.Rooster)
      {
        if(Alive)
          PlaySound("rooster_scream");
        else
          PlaySound("hen_scream_short");//rooster_death
        LastHitCoolDown = 2;
      }
      else if (AnimalKind == AnimalKind.Pig)
      {
        PlaySound("pig_scream");
        LastHitCoolDown = 2;
      }

      else if (AnimalKind == AnimalKind.Deer)
      {
        PlaySound("deer");
        LastHitCoolDown = 3;
      }
      //else if (tag1 == "deer")
        //this.Speed = 2;
    }

    public override string ToString()
    {
      return base.ToString() + " " + AnimalKind + " "+State;
    }
  }
}
