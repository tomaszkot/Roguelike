﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike
{
  public class History
  {
    public Hints.HintHistory Hints { get; set; } = new Hints.HintHistory();
    public LootHistory Looting { get; set; } = new LootHistory();
  }
}
