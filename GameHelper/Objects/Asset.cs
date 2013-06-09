﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using GameHelper.Physics;
using Microsoft.Xna.Framework.Graphics;

namespace GameHelper.Objects
{
    public delegate Gobject GetGobjectDelegate();

    // The idea of an asset: It is a unique game element.
    // You can have ObjectA which is a small sphere and ObjectB which is a large sphere
    // they are different game elements and should be distinct asset types with distinct names and distinct default properties with which to be created.
    public class Asset
    {
        public string Name;
        
        public Vector3 Scale;
        public Color Color;
        public Model model;

        public Asset(string name, Vector3 scale)
        {
            Name = name;
            //GetNewGobject = getgobjectcallback;
            Scale = scale;
            Color = Color.Gray;
        }
    }
}
