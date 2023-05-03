using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII.LootContainers
{
  public class ShortcutsBarContainer
  {
    List<ShortcutsBarContainerLine> lines;

    public ShortcutsBarContainer()
    {
      lines = new List<ShortcutsBarContainerLine>();
      lines.Add(new ShortcutsBarContainerLine());
      lines.Add(new ShortcutsBarContainerLine());
    }

    public int CurrentIndex { get; set; }

    [JsonIgnore]
    public ShortcutsBarContainerLine CurrentLine
    {
      get { return lines[CurrentIndex]; }
      set { lines[CurrentIndex] = value; }
    }
    public List<ShortcutsBarContainerLine> Lines
    {
      get => lines;
      set => lines = value;
    }



  }
}
