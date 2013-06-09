using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameHelper.Objects
{
    public class AssetType
    {
        public int Id;
        public string Name;
        Gobject ControllerClass;
        public GetGobjectDelegate GetNewGobject;

        public AssetType(Enum e, Gobject controller, GetGobjectDelegate callback)
        {
            Name = e.ToString();
            Id = (int)Convert.ChangeType(e, e.GetTypeCode());
            ControllerClass = controller;
            GetNewGobject = callback;
        }
    }
}
