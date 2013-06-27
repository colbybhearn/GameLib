using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using GenEntityConfigTypes;

namespace GameHelper.Objects
{
    public class EntityType
    {
        #region Fields
        public int Id;
        public string Name;
        Entity ControllerClass;
        public SortedList<string, EntityConfig> PrototypeAssets = new SortedList<string, EntityConfig>();
        public Type gobjectType;
        #endregion


        public EntityType(Enum e, Entity controller, Type typeOfGobject)
        {
            Name = e.ToString();
            Id = (int)Convert.ChangeType(e, e.GetTypeCode());
            ControllerClass = controller;
            gobjectType = typeOfGobject;
        }

        public Entity GetNewGobject(EntityConfig ec, SortedList<string, Model> models)
        {
            object o = Activator.CreateInstance(gobjectType);
            if (!(o is Entity))
                return null;
            Entity g = o as Entity;
            g.aType = this;
            if (PrototypeAssets.Count > 0)
            {
                //EntityConfig aConfig = ec;
                g.ApplyConfig(ec, models);
                //g.Model = aConfig.model;
            }
            return g;
        }

        internal void LoadConfigFromFile(string file)
        {
            
            EntityConfig ec = EntityConfigHelper.Load(file);
            if(!PrototypeAssets.ContainsKey(ec.Name))
                PrototypeAssets.Add(ec.Name, ec);
        }
    }
}
