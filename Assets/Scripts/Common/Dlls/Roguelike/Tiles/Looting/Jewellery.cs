namespace Roguelike.Tiles.Looting
{
  public class Jewellery : Equipment
  {
    public const char AmuletSymbol = '"';
    public const char RingSymbol = '=';
    public const char PendantSymbol = AmuletSymbol;

    public override EquipmentKind EquipmentKind
    {
      get
      {
        return base.EquipmentKind;
      }

      set
      {
        base.EquipmentKind = value;
        var name = "";
        switch (EquipmentKind)
        {
          case EquipmentKind.Ring:
            Symbol = RingSymbol;
            name = "Ring";
            //IncludeDebugDetailsInToString = false;
            break;
          case EquipmentKind.Amulet:
            Symbol = AmuletSymbol;
            name = IsPendant ? "Pendant" : "Amulet";
            //IncludeDebugDetailsInToString = false;
            break;
          default:
            break;
        }

        if (Name != name)//do not override real name!
          Name = name;
      }
    }

    bool isPendant = false;
    public bool IsPendant
    {
      get { return isPendant; }
      //for de-serialization
      set { isPendant = value; }
    }

    public Jewellery() : base(EquipmentKind.Ring)
    {

    }

    public void SetIsPendant(bool isPendant)
    {
      this.isPendant = isPendant;
      if (isPendant)
      {
        tag1 = "pendant";
        Name = "Pendant";
        MakeEnchantable();
      }
    }
  }
}
