﻿
using GameHelper.Physics;
using Microsoft.Xna.Framework;
using System;
using GameHelper.Objects;
namespace GameHelper.Camera.Cameras
{
    public class ChaseCameraRelative : BaseCamera
    {

        public ChaseCameraRelative()
        {
            positionLagFactor = .25f;
            lookAtLagFactor = .2f;
        }

        
        public override Matrix GetViewMatrix()
        {
            return RhsViewMatrix;
        }

        public Matrix RhsViewMatrix
        {
            get
            {
                Vector3 camRotation = Matrix.CreateFromQuaternion(Orientation).Forward;
                // Side x camRotation gives the correct Up vector WITHOUT roll, if you do -Z,0,X instead, you will be upsidedown
                // There is still an issue when nearing a "1" in camRotation in the positive or negative Y, in that it rotates weird,
                // This does not appear to be related to the up vector.
                Vector3 cameraRotatedUpVector = Vector3.Transform(Vector3.Up, Orientation);
                return Matrix.CreateLookAt(
                    CurrentPosition,
                    CurrentPosition + camRotation,
                    cameraRotatedUpVector);
            }
        }
        

        public override void Update()
        {
            base.Update();
            Entity gob = GetFirstGobject();
            if (gob == null) return;
            // bodyPosition is the physical location of the body
            Vector3 bodyPosition = gob.Position;
            Matrix bodyOrientation = gob.Orientation;
            try
            {
                // the location of where it's headed
                Vector3 ObjectDirection = gob.Velocity; // this * 2 value is pointless I think

                if (ObjectDirection.Length() < 2) // this may be here just to prevent slow velocities from making a stalker camera
                    ObjectDirection = gob.Orientation.Forward;

                Vector3 WhereItsHeaded = bodyPosition + ObjectDirection;
                
                // a vector pointing toward the direction of travel
                //Vector3 Direction = (WhereItsHeaded - bodyPosition);
                ObjectDirection.Normalize();
                ObjectDirection *= 10f; // this may need to be adjustable per object (planes go faster than cars)
                Vector3 WhereItCameFrom = bodyPosition - (ObjectDirection);

                Vector3 offset = new Vector3(0, 2, 0);
                if(profiles.ContainsKey(gob.aType.Id))
                    offset = profiles[gob.aType.Id].PositionOffset;
                offset = Vector3.Transform(offset, bodyOrientation); 
                // get the correction value from the profile
                WhereItCameFrom += offset; 
                TargetPosition = WhereItCameFrom; // this line caused a problem at one time.
                TargetLookAt = WhereItsHeaded;
            }
            catch (Exception E)
            {
                System.Diagnostics.Debug.WriteLine(E.StackTrace);
            }
        }
    }
}
