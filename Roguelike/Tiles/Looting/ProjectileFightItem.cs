using Dungeons.Tiles;
using Dungeons.Tiles.Abstract;
using Newtonsoft.Json;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles.Looting
{
  public class ProjectileFightItem : FightItem, Roguelike.Abstract.Projectiles.IProjectile
  {
    public const int DefaultMaxDistance = 6;

    public ProjectileFightItem() : this(FightItemKind.Unset, null)
    {
    }

    public ProjectileFightItem(FightItemKind kind, LivingEntity caller = null) : base(kind)
    {
      Caller = caller;
      DiesOnHit = false;
    }

    [JsonIgnore]
    public Tile Target { get; set; }

    public override bool IsCollectable
    {
      get
      {
        if (this.FightItemKind != FightItemKind.HunterTrap)
          return true;

        return FightItemState == FightItemState.Deactivated ||
               FightItemState == FightItemState.Unset;
      }
    }

    public bool DiesOnHit 
    {
      get; 
      set; 
    }
  }
}
