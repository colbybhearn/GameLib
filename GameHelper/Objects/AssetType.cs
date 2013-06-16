using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GameHelper.Objects
{
    public class AssetType
    {
        public int Id;
        public string Name;
        Gobject ControllerClass;
        //public GetGobjectDelegate GetNewGobject;
        public SortedList<string, AssetConfig> PrototypeAssets = new SortedList<string, AssetConfig>();
        public Type gobjectType;
        /*
        public AssetType(Enum e, Gobject controller, GetGobjectDelegate callback)
        {
            Name = e.ToString();
            Id = (int)Convert.ChangeType(e, e.GetTypeCode());
            ControllerClass = controller;
            GetNewGobject = callback;
        }*/

        public AssetType(Enum e, Gobject controller, Type typeOfGobject)
        {
            Name = e.ToString();
            Id = (int)Convert.ChangeType(e, e.GetTypeCode());
            ControllerClass = controller;
            gobjectType = typeOfGobject;
        }

        //public void AddAssetConfig(AssetConfig a)
        //{
        //    PrototypeAssets.Add(a.AssetName,a);
        //}

        //private void LoadConfigFromFile(string name, string file)
        //{
        //    //AssetConfig a = new AssetConfig(name, new Vector3(1, 1, 1), null);
        //    AssetConfig a = new AssetConfig(name);
        //    AddAssetConfig(a);
        //    a.LoadFromFile(file);
        //}

        public Gobject GetNewGobject()
        {
            object o = Activator.CreateInstance(gobjectType);
            if (!(o is Gobject))
                return null;
            Gobject g = o as Gobject;
            g.aType = this;
            if (PrototypeAssets.Count > 0)
            {
                AssetConfig aConfig = this.PrototypeAssets.Values[0];
                g.ApplyConfig(aConfig);
                g.assetName = aConfig.AssetName;
                g.Model = aConfig.model;
            }
            return g;
        }

        internal void LoadConfigFromFile(string file)
        {
            Gobject g = GetNewGobject();
            if (g == null)
                return;
            AssetConfig ac = g.LoadConfig(file);

            PrototypeAssets.Add(ac.AssetName, ac);
        }
    }
}
