﻿
using Microsoft.Xna.Framework;
using System;
using Helper.Physics;
using Helper.Objects;

namespace Helper.Camera.Cameras
{
    public class FirstPersonCamera : UprightCamera
    {
        public FirstPersonCamera()
        {
            positionLagFactor = .1f;
            lookAtLagFactor = .1f;
        }
        
        public override Matrix GetViewMatrix()
        {
            return RhsViewMatrix;
        }

        public override void Update()
        {
            base.Update();
            Vector3 orientAdjustment = Vector3.Zero;
            Vector3 positionAdjustment = Vector3.Zero;
            
            Gobject gob = GetFirstGobject();
            if (gob == null) return;
            
            
            int assetname = gob.type;
            // if this camera has a profile for this asset,
            if (profiles.ContainsKey(assetname))
            {
                // get the adjustment value from the profile
                orientAdjustment = profiles[assetname].OrientationOffset;
                // get the adjustment value from the profile
                positionAdjustment = profiles[assetname].PositionOffset;
            }


            // create an adjustment quat for the orientation
            Quaternion orientOffset = Quaternion.CreateFromYawPitchRoll(orientAdjustment.Y, orientAdjustment.X, orientAdjustment.Z);// YXZ
            // combine body orientation and adjustment quat
            Quaternion newOrientation = Quaternion.CreateFromRotationMatrix( gob.BodyOrientation()) * orientOffset;
            // update the orientation
            SetTargetOrientation(newOrientation);

            // put the adjustment vector for the position into body coordinates
            positionAdjustment = Vector3.Transform(positionAdjustment, newOrientation);
            // update the position
            CurrentPosition = gob.BodyPosition() + positionAdjustment;

        }
    }
}
