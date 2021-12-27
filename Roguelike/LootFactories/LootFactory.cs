using Dungeons.Core;
using Roguelike.Managers;
using Roguelike.Tiles;
using SimpleInjector;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.LootFactories
{
  public class LootFactory : AbstractLootFactory
  {
    public EquipmentFactory EquipmentFactory { get; set; }
    public ScrollsFactory ScrollsFactory { get; set; }
    public BooksFactory BooksFactory { get; set; }
    
    public MiscLootFactory MiscLootFactory { get; set; }
    List<AbstractLootFactory> factories = new List<AbstractLootFactory>();

    public LootFactory(Container container) : base(container)
    {
    }

    protected override void Create()
    {
      EquipmentFactory = container.GetInstance<EquipmentFactory>();
      ScrollsFactory = container.GetInstance<ScrollsFactory>();
      MiscLootFactory = container.GetInstance<MiscLootFactory>();
      BooksFactory = container.GetInstance<BooksFactory>();//new BooksFactory(container);

      factories.Add(EquipmentFactory);
      factories.Add(ScrollsFactory);
      factories.Add(MiscLootFactory);
      factories.Add(BooksFactory);
    }

    public override Loot GetByAsset(string tagPart)
    {
      foreach (var fac in factories)
      {
        var loot = fac.GetByAsset(tagPart);
        if (loot != null)
          return ReturnLoot(loot);
      }

      return null;
    }

    Loot ReturnLoot(Loot loot)
    {
      var eq = loot as Equipment;
      if (eq != null)
        eq.Identified += Eq_Identified;
      return loot;
    }

    private void Eq_Identified(object sender, Loot e)
    {
      container.GetInstance<EventsManager>().AppendAction(new Events.LootAction(e, null)
      {
        Kind = Events.LootActionKind.Identified
      });
    }

    public override Loot GetByName(string name)
    {
      foreach (var fac in factories)
      {
        var loot = fac.GetByName(name);
        if (loot != null)
          return ReturnLoot(loot);
      }

      return null;
    }

    public override Loot GetRandom(int level)
    {
      var subFac = factories.Where(i => i != BooksFactory).ToList();

      var index = RandHelper.GetRandomInt(subFac.Count);
      var lootCreator = subFac[index];
      return lootCreator.GetRandom(level);
    }

    public override IEnumerable<Loot> GetAll()
    {
      List<Loot> res = new List<Loot>();
      foreach (var fac in factories)
      {
        res.AddRange(fac.GetAll());
      }
      return res;
    }
  }
}
