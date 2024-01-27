using Dungeons.Tiles.Abstract;
using Newtonsoft.Json;
using Roguelike.Abstract.Spells;
using Roguelike.Events;
using Roguelike.Managers;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.LivingEntities;
using SimpleInjector;
using System;
using System.Linq;

namespace Roguelike.Tiles.Interactive
{
  public enum InteractiveTileKind
  {
    Unset, Stairs, Doors, Barrel, TreasureChest,
    Trap, Lever, DeadBody, TorchSlot, Candle, CrackedStone, Bonfire, Privy, Ladder, Other
  }

  public interface IApproachableByHero
  {
    bool ApproachedByHero { get; set; }
    double DistanceFrom(Dungeons.Tiles.Tile other);
    bool Activate();
    string GetPlaceName();
    string ActivationSound { get; set; }

    event EventHandler Activated;
  }

  public class InteractiveTile : Dungeons.Tiles.InteractiveTile
    {
    private InteractiveTileKind _kind = InteractiveTileKind.Unset;
    public bool OutOfOrder { get; set; }
    public int Level
    {
      get;
      set;
    } = -1;//should match level of dungeon or a level of world part
    [JsonIgnore]
    public virtual string InteractSound { get; set; }
    public virtual string AppendedSound { get; set; }

    private bool isLooted;

    public event EventHandler<bool> Looted;
    Container container;

    public InteractiveTile(Container cont, char symbol) : base(symbol)
    {
      this.container = cont;
    }

    LivingEntity busyBy;
    [JsonIgnore]
    public bool Busy { get { return busyBy != null; } }

    [JsonIgnore]
    public bool IsNpcDestMoveTarget { get { return DestPointActivityKind != DestPointActivityKind.Unset; } }
    public DestPointActivityKind DestPointActivityKind { get; set; }

    public void SetBusy(LivingEntity le)
    {
      if (le != null)
        this.busyBy = le;
      else
        this.busyBy = null;
    }

    public override void PlayHitSound(IProjectile proj)
    {
      PlaySound(proj.HitSound);
    }
    public override void PlayHitSound(Dungeons.Tiles.Abstract.IDamagingSpell spell)
    {
      PlaySound(spell.HitSound);
    }

    protected void PlaySound(string sound)
    {
      if (sound != null && sound.Any())
        AppendAction(new SoundRequestAction() { SoundName = sound });
    }

    protected void AppendAction(GameEvent ac)
    {
      if (EventsManager != null)
        EventsManager.AppendAction(ac);
    }

    [JsonIgnore]
    public EventsManager EventsManager
    {
      get { return container.GetInstance<EventsManager>(); }
    }

    public virtual void ResetToDefaults()
    {
    }
     
    public bool IsLooted
    {
      get => isLooted;
      set
      {
        isLooted = value;
      }
    }

    public virtual bool SetLooted(bool looted)
    {
      IsLooted = looted;
      Looted?.Invoke(this, looted);
      return true;
    }

    public InteractiveTileKind Kind
    {
      get => _kind;
      set
      {
        _kind = value;
      }
    }
    public bool CanBeHitBySpell()
    {
      return true;
    }

    public override string ToString()
    {
      var res = " L:" + Level + " " + base.ToString();
      res += " " + Kind;
      return res;
    }

    internal virtual bool InteractWith(LivingEntity npc)
    {
      return false;  
    }

    /// <summary>
    /// Was hero ever close to the tile?
    /// </summary>
    public bool ApproachedByHero { get; set; }
    public bool HidesInteractedEntity { get; set; }
    [JsonIgnore]
    public LivingEntity BusyBy { get => busyBy; set => busyBy = value; }
  }
}
