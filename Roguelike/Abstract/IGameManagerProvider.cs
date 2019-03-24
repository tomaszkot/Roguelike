﻿using Roguelike.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Abstract
{
  public interface IGameManagerProvider
  {
    GameManager GameManager
    {
      get;
      //set;
    }
  }
}
