using Dungeons.Core;
using Roguelike.Abstract.Inventory;
using Roguelike.Abstract.Tiles;
using Roguelike.Attributes;
using Roguelike.LootContainers;
using SimpleInjector;
using System.Drawing;

namespace Roguelike.Tiles.LivingEntities
{
  public abstract class Ally : AdvancedLivingEntity, IAlly
  {
    public Ally(char symbol = '!') : base(new Point().Invalid(), symbol)
    {
      Stats.SetNominal(EntityStatKind.Health, 15);
      var str = 15;
      Stats.SetNominal(EntityStatKind.Strength, str);//15
      Stats.SetNominal(EntityStatKind.Attack, str);
      Stats.SetNominal(EntityStatKind.Magic, 10);
      Stats.SetNominal(EntityStatKind.Mana, 40);
      Stats.SetNominal(EntityStatKind.Defense, 10);
      Stats.SetNominal(EntityStatKind.Dexterity, 10);

      CreateInventory(null);

      Dirty = true;//TODO
    }

    protected override void CreateInventory(Container container)
    {
      base.CreateInventory(container);
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

    public override void SetLevel(int level)
    {
      base.SetLevel(level);
    }

    public static Ally Spawn<T>(char symbol, int level) where T : Ally, new()
    {
      var ally = new T();
      ally.Symbol = symbol;
      ally.SetLevel(level);
      ally.SetTag();
      ally.Revealed = true;
      ally.Active = true;

      return ally;
    }

    public abstract void SetTag();

    public override bool GetGoldWhenSellingTo(IInventoryOwner dest)
    {
      return false;
    }
  }
    
}
