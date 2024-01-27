using Dungeons.Core;
using Dungeons.Fight;
using Dungeons.Tiles.Abstract;
using Roguelike.Spells;
using Roguelike.TileContainers;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Tiles.LivingEntities
{
  public class Animal : LivingEntity, ILootSource
  {
    static readonly Dictionary<string, AnimalKind> tag2Kind;
    //= new Dictionary<string, AnimalKind>
    //{
    //  { "rooster", AnimalKind.Rooster},
    //  { "hen", AnimalKind.Hen},
    //  { "deer", AnimalKind.Deer},
    //  { "pig", AnimalKind.Pig},
    //  { "horse", AnimalKind.Horse},
    //  { "hound", AnimalKind.Hound},


    //};

    static readonly Dictionary<AnimalKind, string> kind2Tag;

    static Animal()
    {
      tag2Kind = EnumHelper.GetEnumValues<AnimalKind>(true).ToDictionary(kv => kv.ToString().ToLower(), kv => kv);
      kind2Tag = tag2Kind.ToDictionary(kv => kv.Value, kv => kv.Key);
    }

    public int RandMoveCoolDown = 1;
    public bool PreventMove { get; set; } = false;
    AnimalKind kind;
    public int LastHitCoolDown { get; set; }

    public Animal(Container cont, AnimalKind kind) : base(cont)
    {
      AnimalKind = kind;
      EntityKind = EntityKind.Animal;
      if(kind!= AnimalKind.Unset)
        tag1 = kind2Tag[kind];  
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
        if (kind == AnimalKind.Horse)
          Stats.GetStat(Attributes.EntityStatKind.Health).Value.Nominal *= 4;
        else if (kind == AnimalKind.Hen || kind == AnimalKind.Rooster)
        {
          Stats.GetStat(Attributes.EntityStatKind.Health).Value.Nominal /= 2;
          Stats.GetStat(Attributes.EntityStatKind.Defense).Value.Nominal /= 2;
        }
      }
    }

   

    public AnimalKind GetKindFromTag1()
    {
      if (tag1 == "rooster")
        return AnimalKind.Rooster;
      else if (tag1 == "hen")
        return AnimalKind.Hen;
      else if (tag1 == "deer")
        return AnimalKind.Deer;
      else if (tag1 == "pig")
        return AnimalKind.Pig;
      else if (tag1.Contains("horse"))//can be zyndram_horse
        return AnimalKind.Horse;
      else if (tag1.Contains("hound"))
        return AnimalKind.Hound;

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
    //public  HitResult OnMeleeHitBy(ILivingEntity attacker)
    //{
    //  OnMeleeHitBy(attacker as LivingEntity);
    //  return HitResult.Hit;
    //}


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
      else if (AnimalKind == AnimalKind.Horse)
      {
        PlaySound("horse_hit");
        LastHitCoolDown = 2;
      }
      //
      //else if (tag1 == "deer")
      //this.Speed = 2;
    }

    public override string ToString()
    {
      return base.ToString() + " " + AnimalKind + " "+State;
    }
  }
}
