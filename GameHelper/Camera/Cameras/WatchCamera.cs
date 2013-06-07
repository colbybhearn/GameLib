
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
            Gobject gob = GetFirstGobject();
            if (gob == null) return;

            LookAtLocation(gob.BodyPosition(), Vector3.Up);
            
        }
    }
}
