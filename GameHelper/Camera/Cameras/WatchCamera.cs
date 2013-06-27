
using Microsoft.Xna.Framework;
using GameHelper.Physics;
using GameHelper.Objects;

namespace GameHelper.Camera.Cameras
{
    public class WatchCamera : UprightCamera
    {
        public WatchCamera()
        {

        }

        public override Matrix GetViewMatrix()
        {
            return RhsLevelViewMatrix;
        }

        public override void Update()
        {
            base.Update();
            Entity gob = GetFirstGobject();
            if (gob == null) return;

            LookAtLocation(gob.Position, Vector3.Up);
            
        }
    }
}
