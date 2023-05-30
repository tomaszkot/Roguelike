using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace God4_1.Entities
{
  public partial class Entity : Node
  {
  public void LoadTexture(string path)
  {
    if (GetChild(0) is Sprite2D) 
    {
      var spr = (Sprite2D)GetChild(0);
      spr.Texture = ResourceLoader.Load(path) as Texture2D;
    }
  }
  }
}
