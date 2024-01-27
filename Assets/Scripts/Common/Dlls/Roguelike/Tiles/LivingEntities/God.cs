using Dungeons.Core;
using Roguelike.Abstract.Tiles;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System.Drawing;

namespace Roguelike.Tiles.LivingEntities
{
  public class God : AdvancedLivingEntity, IAlly
  {
    public AllyBehaviour AllyBehaviour { get; set; }
    public string PowerReleaseSpeach { get; set; }
    public bool Active { get; set; }

    public AllyKind Kind => throw new System.NotImplementedException();

    public Point Point { get => point; set => point = value; }

    public bool TakeLevelFromCaster { get; }
    public bool PendingReturnToCamp { get; set; }

    public God(Container cont) : this(cont, new Point().Invalid(), '0')
    {
    }

    public God(Container cont, Point point, char symbol) : base(cont, point, symbol)
    {
      Alive = false;//TODO, for turn to work
      Stats.SetNominal(Attributes.EntityStatKind.ChanceToPhysicalProjectileHit, 100);
      Stats.SetNominal(Attributes.EntityStatKind.ChanceToCastSpell, 100);
    }

    public virtual Roguelike.Abstract.Spells.ISpell CreateSpell(out Scroll godScroll)
    {
      godScroll = null;
      return null;
    }

    public void SetNextLevelExp(double exp)
    {
      NextLevelExperience = exp;
    }

    public virtual bool MakeTurn()
    {
      return false;
    }

    //public virtual LootBase GetAwakeReward(bool primary)
    //{
    //  return null;
    //}

    //string GetLootAssetName(LootBase loot)
    //{
    //  if (loot.AssetName == "horn_rotated")
    //   return Dziewanna.HiddenGodAwakingLoot;
    //  return loot.AssetName;
    //}

    //public string LootNotReady = "Item is not ready";

    //public virtual bool AcceptGift(LootBase loot, float rx, float ry, float rz, ref string err)
    //{
    //  if (loot.AssetName == "horn_rotated")
    //  {
    //    if (rx != 0 || ry != 0 || rz != 0)
    //    {
    //      err = LootNotReady;
    //      return false;
    //    }
    //    var name = GetLootAssetName(loot);
    //    return Accept(name, ref err);
    //  }
    //  return false;
    //}

    //public virtual bool AcceptGift(LootBase loot, ref string err)
    //{
    //  var name = loot.AssetName;// GetLootAssetName(loot);
    //  return Accept(name, ref err);
    //}

    //bool Accept(string name, ref string err)
    //{
    //  if (!Awoken)
    //  {
    //    if (AwakingLoot.Contains(name))
    //    {
    //      if (AcceptedLoot.Contains(name))
    //      {
    //        err = "This gift was already accepted";
    //        return false;
    //      }
    //      AcceptedLoot.Add(name);
    //      var info = GameManager.Instance.AlreadyGeneratedStuff;
    //      info.AcceptedGodGifts.Add(name);
    //    }
    //    else
    //      return false;
    //  }
    //  CalcAwoken();
    //  return true;
    //}

    //private void CalcAwoken()
    //{
    //  if (!Awoken)
    //  {
    //    if (AcceptedLoot.Contains(AwakingGift))
    //    {
    //      Awoken = true;
    //      return;
    //    }
    //    var shallAwake = awakingLoot.All(i => AcceptedLoot.Contains(i));
    //    if (shallAwake)
    //      Awoken = true;
    //  }
    //}



    //public List<string> AwakingLoot
    //{
    //  get
    //  {
    //    return awakingLoot;
    //  }

    //  set
    //  {
    //    awakingLoot = value;
    //  }
    //}
    //public List<LootBase> fromHero = new List<LootBase>();
    //public List<LootBase> FromHero
    //{
    //  get
    //  {
    //    return fromHero;
    //  }

    //  set
    //  {
    //    fromHero = value;
    //  }
    //}

    //public bool HasAcceptedGift(string giftName)
    //{
    //  return AcceptedLoot.Contains(giftName);
    //}

    //public string HiddenAwakingLoot = "";
    //}



    //  }
    //  public Jarowit(Point point) : base(point, '4')
    //  {
    //    PowerReleaseSpeach = "It's time of the Chosen One!";
    //    Name = "Jarowit";
    //    AwakingGift = "JarowitsShield";
    //    AwakingLoot.Add("ruby_medium");
    //    AwakingLoot.Add("emerald_medium");
    //    AwakingLoot.Add("diamond_medium");
    //  }

    //  //power: hits random monsters with weaken spell, hero with iron skin
    //  public override string GetPrimaryStatDescription()
    //  {
    //    return "God of War";
    //  }
    //}

    //public class Swiatowit : God
    //{
    //  public static string SwiatowitHiddenAwakingLoot = "swiatowid_sword_part";
    //  public Swiatowit() : this(Point.Invalid)
    //  {

    //  }
    //  public Swiatowit(Point point) : base(point, '5')
    //  {
    //    HiddenAwakingLoot = SwiatowitHiddenAwakingLoot;
    //    PowerReleaseSpeach = "Obey god's will!";
    //    Name = "Swiatowit";
    //    AwakingGift = "swiatowid_sword";
    //    AwakingLoot.Add("swiatowid_horn");
    //    AwakingLoot.Add(HiddenAwakingLoot);
    //  }

    //  public override LootBase GetAwakeReward(bool primary)
    //  {
    //    if (primary)
    //      return new Gem(11);
    //    return new Gem(11);
    //  }

    //  //power: hits random monsters with random spell, spell causes effect 50%
    //  public override string GetPrimaryStatDescription()
    //  {
    //    return "Highest God";
    //  }
  }
}
