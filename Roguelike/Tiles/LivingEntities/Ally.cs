using Dungeons.Core;
using Roguelike.Abstract.Inventory;
using Roguelike.Abstract.Tiles;
using Roguelike.LootContainers;
using SimpleInjector;
using System.Drawing;

namespace Roguelike.Tiles.LivingEntities
{
  public abstract class Ally : AdvancedLivingEntity, IAlly
  {
    public Ally(Container cont, char symbol = '!') : base(cont, new Point().Invalid(), symbol)
    {
      canAdvanceInExp = true;
      Inventory.InvBasketKind = InvBasketKind.AllyEquipment;
    }

    public bool Active { get; set; }

    public AllyKind Kind
    {
      get
      {
        return AllyKind.Enemy;
      }
      set { }
    }

    public Point Point { get => point; set => point = value; }

    public bool TakeLevelFromCaster { get; protected set; }

    public override bool SetLevel(int level)
    {
      return base.SetLevel(level);
    }

    public static Ally Spawn<T>(Container cont, char symbol, int level) where T : Ally, new()
    {
      var ally = cont.GetInstance<T>();
      ally.InitSpawned(symbol, level);

      return ally;
    }

    public void InitSpawned(char symbol, int level) //where T : Ally, new()
    {
      Symbol = symbol;
      SetLevel(level);
      SetTag();
      Revealed = true;
      Active = true;

      var abp = 5;
      AbilityPoints = abp;
      for (int i = 0; i < abp; i++)
        this.IncreaseAbility(Roguelike.Abilities.PassiveAbilityKind.RestoreHealth);

    }

    public abstract void SetTag();

    public override bool GetGoldWhenSellingTo(IInventoryOwner dest)
    {
      return false;
    }
  }

}
