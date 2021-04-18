using Dungeons.Core;
using Roguelike.Abstract.Tiles;
using SimpleInjector;
using System.Drawing;

namespace Roguelike.Tiles.LivingEntities
{
  public class God : AdvancedLivingEntity, IAlly
    //IDescriptable, 
  {
    //Inventory inventory;
    //public bool Awoken { get; set; }
    //List<string> acceptedLoot = new List<string>();
    //List<string> awakingLoot = new List<string>();
    //public string AwakingGift { get; set; }
    public string PowerReleaseSpeach { get; set; }
    public bool Active { get; set ; }

    public AllyKind Kind => throw new System.NotImplementedException();

    public Point Point { get => point; set => point = value; }

    public God(Container cont) : this(cont, new Point().Invalid(), '0')
    {
     
    }

    public God(Container cont, Point point, char symbol) : base(cont, point, symbol)
    {
      Alive = true;//for turn to work
      //HeroAlly = true;
      //inventory = new InventoryGod();
      //inventory.InvType = InvType.God; //InvType.Merchant;
      //inventory.PriceFactor = 1;

      //SetMagicValue();
    }

    //public virtual int GetMagicFactor()
    //{
    //  return 1;
    //}

    //public virtual int GetMagicAddition()
    //{
    //  return 0;
    //}

    //public void SetMagicValue()
    //{
    //  var magicValue = BaseMagic.NominalValue;
    //  if (GameManager.Instance.Level != null)
    //  {
    //    magicValue *= (GameManager.Instance.Level.LevelIndex / 2 + GetMagicFactor());
    //    magicValue += GetMagicAddition();
    //  }
    //  Stats.SetNominal(EntityStatKind.Magic, magicValue);
    //  var ownerMagicAmount = Stats.GetCurrentValue(EntityStatKind.Magic);
    //  //int k = 0;
    //}

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

    //public Inventory Inventory
    //{
    //  get
    //  {
    //    return inventory;
    //  }

    //  set
    //  {
    //    inventory = value; 
    //  }
    //}

    //public List<string> AcceptedLoot
    //{
    //  get
    //  {
    //    return acceptedLoot;
    //  }

    //  set
    //  {
    //    acceptedLoot = value;
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

    //public bool HasRoomInBackpack()
    //{
    //  return true;//TODO
    //}

    //public bool HasAcceptedGift(string giftName)
    //{
    //  return AcceptedLoot.Contains(giftName);
    //}

    //public string HiddenAwakingLoot = "";
  }

  //public class Swarog : God
  //{
  //  public Swarog():this(Point.Invalid)
  //  {

  //  }

  //  public Swarog(Point point) : base(point, '1')
  //  {
  //    PowerReleaseSpeach = "Darkness surround us!";
  //    Name = "Swarog";
  //    AwakingGift = "swarog_hammer";
  //    AwakingLoot.Add("swarog_hammer_wooden_part1");
  //    AwakingLoot.Add("swarog_hammer_wooden_part2");
  //  }

  //  //power: makes dark causing random monsters to hit each other
  //  public override string GetPrimaryStatDescription()
  //  {
  //    return "God of Sun, Fire and Smithing";
  //  }
  //}

  //public class Perun : God
  //{
  //  public static string HiddenGodAwakingLoot = "perun_sign_2";

  //  public Perun():this(Point.Invalid)
  //  {
      
  //  }

  //  public override int GetMagicFactor()
  //  {
  //    return 1;
  //  }
  //  public override int GetMagicAddition()
  //  {
  //    return 4;
  //  }

  //  public Perun(Point point) : base(point, '2')
  //  {
  //    PowerReleaseSpeach = "Discover power of thunders";
  //    Name = "Perun";
  //    AwakingGift = "lighting_scroll";
  //    AwakingLoot.Add("perun_sign_1");
  //    AwakingLoot.Add("perun_sign_2");
  //    HiddenAwakingLoot = HiddenGodAwakingLoot;
  //    //
  //  }

  //  //power: hits random monsters with lightball spell
  //  public override string GetPrimaryStatDescription()
  //  {
  //    return "God of Lightning";
  //  }
  //}

  //public class Dziewanna : God
  //{
  //  public static string HiddenGodAwakingLoot = "dziewanna_horn";
  //  public Dziewanna():this(Point.Invalid)
  //  {

  //  }

  //  public Dziewanna(Point point) : base(point, '3')
  //  {
  //    HiddenAwakingLoot = HiddenGodAwakingLoot;
  //    PowerReleaseSpeach = "Taste my juicy fruits!";
  //    Name = "Dziewanna";
  //    AwakingGift = "";//SpecialPotion
  //    AwakingLoot.Add("dziewanna_flower");
  //    AwakingLoot.Add(HiddenAwakingLoot);
  //  }
        
  //  public override LootBase GetAwakeReward(bool primary)
  //  {
  //    if(primary)
  //      return  CommonRandHelper.GetRandomDouble() > .5f ? new SpecialPotion(SpecialPotionKind.Strength, true) :
  //        new SpecialPotion(SpecialPotionKind.Magic, true);
  //    return base.GetAwakeReward(primary);
  //  }
  //  //power: hits random monsters with poison arrow (or gives poisoned apple)
  //  public override string GetPrimaryStatDescription()
  //  {
  //    return "God of Nature";
  //  }
    
  //}

  //public class Jarowit : God
  //{
  //  public Jarowit() : this(Point.Invalid)
  //  {

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
