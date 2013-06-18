using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GameHelper.Objects
{
    public class EntityType
    {
        public int Id;
        public string Name;
        Entity ControllerClass;
        public SortedList<string, EntityConfig> PrototypeAssets = new SortedList<string, EntityConfig>();
        public Type gobjectType;
        

        public EntityType(Enum e, Entity controller, Type typeOfGobject)
        {
            Name = e.ToString();
            Id = (int)Convert.ChangeType(e, e.GetTypeCode());
            ControllerClass = controller;
            gobjectType = typeOfGobject;
        }

        public Entity GetNewGobject()
        {
            object o = Activator.CreateInstance(gobjectType);
            if (!(o is Entity))
                return null;
            Entity g = o as Entity;
            g.aType = this;
            if (PrototypeAssets.Count > 0)
            {
                EntityConfig aConfig = this.PrototypeAssets.Values[0];
                g.ApplyConfig(aConfig);
                g.assetName = aConfig.AssetName;
                g.Model = aConfig.model;
            }
            return g;
        }

        internal void LoadConfigFromFile(string file)
        {
            Entity g = GetNewGobject();
            if (g == null)
                return;
            EntityConfig ac = g.LoadConfig(file);

            PrototypeAssets.Add(ac.AssetName, ac);
        }
    }
}
