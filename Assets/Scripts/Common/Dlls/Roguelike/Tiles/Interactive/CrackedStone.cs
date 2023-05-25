using Dungeons.Core.Policy;
using Dungeons.Fight;
using Dungeons.Tiles.Abstract;
using Newtonsoft.Json;
using Roguelike.Calculated;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System.Drawing;

namespace Roguelike.Tiles.Interactive
{
  public class CrackedStone : InteractiveTile, IDestroyable
  {
    private float durability;
    [JsonIgnore]
    public bool RewardGenerated { get; set; }
    public float Durability
    {
      get => durability;
      set 
      {
        durability = value;
        if (value > MaxDurability)
          MaxDurability = value;
      }
    }
    public float MaxDurability { get; set; }
    public bool Destroyed
    {
      get
      {
        return Durability <= 0;
      }
      set {
        Durability = 0;
      }
    }

    public bool Damaged
    {
      get
      {
        return Durability < MaxDurability;
      }
    }

    
    public string OriginMap { get; set; }
    public bool LevelSet { get; set; }
    public Loot ForcedReward { get; set; }

    public CrackedStone(Container cont) : this(cont, 5)
    {
    }

    public CrackedStone(Container cont, int durability) : base(cont, '%')
    {
      Kind = InteractiveTileKind.CrackedStone;
      Name = Kind.ToString();
      tag1 = "cracked_stone";
      InteractSound = "punch";
      DestroySound = "bones_fall_golem";
      Durability = durability;
    }

    internal CrackedStone Clone()
    {
      return MemberwiseClone() as CrackedStone;
    }

    void CallEmitInteraction()
    {
      EmitInteraction();
    }

    public override HitResult OnHitBy(IDamagingSpell damager, IPolicy policy)
    {
      var res = base.OnHitBy(damager, policy);
      return HandleHit(damager, res);
    }

    const float ProjectileMult = 3;

    public HitResult OnHitBy(ProjectileFightItem pfi)
    {
      PlayHitSound(pfi);
      return OnHitBy(pfi.Damage * ProjectileMult); 
    }

    public override HitResult OnHitBy(IProjectile md, IPolicy policy)
    {
      if (md is IDamagingSpell ds)
        return OnHitBy(ds, policy);
      else if (md is ProjectileFightItem pfi)
        return OnHitBy(pfi);


      return HitResult.Unset;
    }

    private HitResult HandleHit(IDamagingSpell md, HitResult res)
    {
      if (res == HitResult.Hit)
      {
        if (md != null)
        {
          var dmg = md.Damage * ProjectileMult;
          PlayHitSound(md);
          return OnHitBy(dmg);
        }
      }
      return res;
    }

    public override HitResult OnHitBy(ILivingEntity le)
    {
      var ad = new AttackDescription(le as LivingEntity, true, Attributes.AttackKind.Melee);
      return OnHitBy(ad.CurrentPhysicalVariated);
    }

    HitResult OnHitBy(float damage)
    {
      Durability -= damage;
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
