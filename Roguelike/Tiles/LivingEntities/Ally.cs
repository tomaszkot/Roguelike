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
    public bool IncreaseStatsDueToDifficulty = true;//too easy ?
    public Ally(Container cont, char symbol = '!') : base(cont, new Point().Invalid(), symbol)
    {
      canAdvanceInExp = true;
      Inventory.InvBasketKind = InvBasketKind.Ally;
      CurrentEquipment.InvBasketKind = InvBasketKind.AllyEquipment;
      Inventory.Capacity = 8;
    }

    protected override bool CanIncreaseStatsDueToDifficulty()
    {
      return IncreaseStatsDueToDifficulty;
    }

    public bool Active { get; set; }

    public AnimalKind AnimalKind { get; set; }

    public AllyKind kind;
    public AllyKind Kind
    {
      get { return kind; }
      set 
      {
        kind = value;
        if (kind == AllyKind.Hound)
          AnimalKind = AnimalKind.Hound;
      }
    }
    override protected bool CanUseAnimalKindEq(Equipment eq)
    {
      if (this.Kind == AllyKind.Hound && eq.MatchingAnimalKind == AnimalKind.Hound)
        return true;
      return base.CanUseAnimalKindEq(eq);
    }

    public override bool CanUseEquipment(Equipment eq, bool autoPutoOn)
    {
      if (eq.EquipmentKind != EquipmentKind.Amulet)
      {
        if (this.AnimalKind != eq.MatchingAnimalKind)
          return false;
      }
      if (IsBowLike(eq))
      {
        return false;//other pose for a skeleton is needed for a bow
      }

      if (Kind == AllyKind.Hound && eq is Armor arm && arm.MatchingAnimalKind != AnimalKind.Hound)
      {
        return false;
      }

      return base.CanUseEquipment(eq, autoPutoOn);
    }

    private static bool IsBowLike(Equipment eq)
    {
      return eq is Weapon wpn && wpn.IsBowLike;
    }

    public Point Point { get => point; set => point = value; }

    public bool TakeLevelFromCaster { get; protected set; }

    public override bool SetLevel(int level, Difficulty? diff = null)
    {
      return base.SetLevel(level, diff);
    }

    public static Ally Spawn<T>(Container cont, char symbol, int level, Difficulty diff) where T : Ally, new()
    {
      var ally = cont.GetInstance<T>();
      ally.InitSpawned(symbol, level, diff);

      return ally;
    }
    public void SetNextExpFromLevel()
    {
      var nle = NextLevelExperience;
      for (int i = 1; i < Level; i++)
        nle = CalcNextLevelExperience((float)nle);

      SetNextLevelExp(nle);
    }

    public void IncreaseStats(Abilities.PassiveAbility ab)
    {
      base.IncreaseStats(1+ (ab.PrimaryStat.Factor)/100, IncreaseStatsKind.Ability);
    }

    public void InitSpawned(char symbol, int level, Difficulty? diff = null) //where T : Ally, new()
    {
      Symbol = symbol;
      
      SetLevel(level, diff);
      SetTag();
      Revealed = true;
      Active = true;

      var abp = 5;
      AbilityPoints = abp;
      for (int i = 0; i < abp; i++)
        this.IncreaseAbility(Roguelike.Abilities.AbilityKind.RestoreHealth);

    }

    public abstract void SetTag();

    public override bool GetGoldWhenSellingTo(IInventoryOwner dest)
    {
      return false;
    }

    public void SetNextLevelExp(double exp)
    {
      NextLevelExperience = exp;
    }

  }

}
