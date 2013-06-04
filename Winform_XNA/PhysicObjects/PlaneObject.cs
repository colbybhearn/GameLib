using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using JigLibX.Collision;
using JigLibX.Physics;

namespace Winform_XNA.PhysicObjects
{
    class PlaneObject : Gobject
    {

        public PlaneObject(Model model,float d, Vector3 position) : base()
        {
            Body = new Body();
            Skin = new CollisionSkin(null);            
            Skin.AddPrimitive(new JigLibX.Geometry.Plane(Vector3.Up, d), new MaterialProperties(0.2f, 0.7f, 0.6f));
            PhysicsSystem.CurrentPhysicsSystem.CollisionSystem.AddCollisionSkin(Skin);
            CommonInit(position, new Vector3(1,1,1), model, false);
        }       
    }
}
