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
        public AssetConfig config;

        public Asset(string name, Vector3 scale, Model m)
        {
            Name = name;
            model = m;
            Scale = scale;
            Color = Color.Gray;
        }

        /// <summary>
        /// this method should be overloaded in the specific asset config class for each asset and load the setting in the file into memory
        /// </summary>
        /// <param name="file"></param>
        public virtual void LoadConfigFromFile(string file)
        {
            config.LoadFromFile(file);
        }
    }
}
