﻿#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using JigLibX.Vehicles;
#endregion

namespace Helper.Physics.PhysicsObjects
{
    /// <summary>
    /// Class Car
    /// </summary>
    public class Rover : RollingVehicle
    {
        enum WheelId
        {
            WheelBR = 0,
            WheelMR = 1,
            WheelFR = 2,
            WheelBL = 3,
            WheelML = 4,
            WheelFL = 5,
            MaxWheels = 6
        }

        #region private fields

        private bool fWDrive;
        private bool bWDrive;
        private float maxSteerAngle;
        private float steerRate;
        private float wheelSideFriction;
        private float wheelFwdFriction;
        private float wheelTravel;
        private float wheelRadius;
        private float wheelZOffset;
        private float wheelRestingFrac;
        private float wheelDampingFrac;
        private int wheelNumRays;
        private float driveTorque;
        private float gravity;

        // control stuff
        private float destSteering = 0.0f; // +1 for left, -1 for right
        private float destAccelerate = 0.0f; // +1 for acc, -1 for brake

        private float steering = 0.0f;
        private float accelerate = 0.0f;
        private float hBrake = 0.0f;
        #endregion

        /// <summary>
        /// On construction the physical/collision objects are created, but
        /// not registered
        /// </summary>
        /// <param name="FWDrive"></param>
        /// <param name="RWDrive"></param>
        /// <param name="maxSteerAngle"></param>
        /// <param name="steerRate"></param>
        /// <param name="wheelSideFriction"></param>
        /// <param name="wheelFwdFriction"></param>
        /// <param name="wheelTravel"></param>
        /// <param name="wheelRadius"></param>
        /// <param name="wheelZOffset"></param>
        /// <param name="wheelRestingFrac"></param>
        /// <param name="wheelDampingFrac"></param>
        /// <param name="wheelNumRays"></param>
        /// <param name="driveTorque"></param>
        /// <param name="gravity"></param>
        public Rover(bool FWDrive, bool RWDrive, float maxSteerAngle, float steerRate, float wheelSideFriction,
                float wheelFwdFriction, float wheelTravel, float wheelRadius, float wheelZOffset, float wheelRestingFrac,
                float wheelDampingFrac, int wheelNumRays, float driveTorque, float gravity)
        {
            this.fWDrive = FWDrive;
            this.bWDrive = RWDrive;
            this.maxSteerAngle = maxSteerAngle;
            this.steerRate = steerRate;
            this.wheelSideFriction = wheelSideFriction;
            this.wheelFwdFriction = wheelFwdFriction;
            this.wheelTravel = wheelTravel;
            this.wheelRadius = wheelRadius;
            this.wheelZOffset = wheelZOffset;
            this.wheelRestingFrac = wheelRestingFrac;
            this.wheelDampingFrac = wheelDampingFrac;
            this.wheelNumRays = wheelNumRays;
            this.driveTorque = driveTorque;
            this.gravity = gravity;

            chassis = new RoverChassis(this);
            // TODO: still need to fix ground clearance in chassis!
            chassis.SetDims(new Vector3(-1, -.7f, -1.45f), new Vector3(1, .3f, 1.45f)); 
            SetupDefaultWheels();
        }

        /// <summary>
        /// Sets up some sensible wheels based on the chassis
        /// </summary>
        public override void SetupDefaultWheels()
        {
            if (chassis == null)
                return; // happens in constructor of tChassis

            Vector3 min, max;
            chassis.GetDims(out min, out max);

            float mass = chassis.Body.Mass;
            float mass4 = 0.25f * mass;

            Vector3 axis = Vector3.Up; // TODO: check this
            // set the resting position to be restingFrac * mWheelTravel
            // todo how do we get gravity before the car is registered with physics?!
            float spring = mass4 * gravity / (wheelRestingFrac * wheelTravel);

            // inertia = 0.5 * m * r^2
            float wheelMass = 0.03f * mass;
            float inertia = 0.5f * (wheelRadius * wheelRadius) * wheelMass;

            // critical damping from (d = damping, k = spring, x = displacement, v = displacement vel, a = displacement acc):
            // a + (d/m) * v + (k/m) * x = 0
            // beta = d/m   w0^2 = k/m
            // critical if sq(beta) = 4*sq(w0)
            // so d = 2 * sqrt(k*m)
            float damping = 2.0f * (float)System.Math.Sqrt(System.Math.Abs(spring * mass));
            damping *= 0.25f; // assume wheels act together
            damping *= wheelDampingFrac;  // a bit bouncy
            /*
            // the wheels aren't quite at the corners
            min.X += 3.0f * wheelRadius;
            max.X -= 3.1f * wheelRadius;
            min.Z += wheelRadius * 0.35f;
            max.Z -= wheelRadius * 0.35f;
            */
            Vector3 delta = max - min;

            min.Y += wheelZOffset;

            Vector3 BRPos = min;
            Vector3 MRPos = min + new Vector3(delta.X  / 2.0f, 0.0f, 0.0f);
            Vector3 FRPos = min + new Vector3(delta.X, 0.0f, 0.0f);
            Vector3 BLPos = min + new Vector3(0.0f, 0.0f, delta.Z);
            Vector3 MLPos = min + new Vector3(delta.X / 2.0f, 0.0f, delta.Z);
            Vector3 FLPos = min + new Vector3(delta.X, 0.0f, delta.Z);

            if (wheels == null)
                wheels = new List<Wheel>();

            if (wheels.Count == 0)
            {
                wheels.Add(new Wheel());
                wheels.Add(new Wheel());
                wheels.Add(new Wheel());
                wheels.Add(new Wheel());
                wheels.Add(new Wheel());
                wheels.Add(new Wheel());
            }


            wheels[(int)WheelId.WheelBR].Setup(this,
                            BRPos,
                            axis,
                            spring,
                            wheelTravel,
                            inertia,
                            wheelRadius,
                            wheelSideFriction,
                            wheelFwdFriction,
                            damping,
                            wheelNumRays);

            wheels[(int)WheelId.WheelMR].Setup(this,
                            MRPos,
                            axis,
                            spring,
                            wheelTravel,
                            inertia,
                            wheelRadius,
                            wheelSideFriction,
                            wheelFwdFriction,
                            damping,
                            wheelNumRays);

            wheels[(int)WheelId.WheelFR].Setup(this,
                            FRPos,
                            axis,
                            spring,
                            wheelTravel,
                            inertia,
                            wheelRadius,
                            wheelSideFriction,
                            wheelFwdFriction,
                            damping,
                            wheelNumRays);

            wheels[(int)WheelId.WheelBL].Setup(this,
                            BLPos,
                            axis,
                            spring,
                            wheelTravel,
                            inertia,
                            wheelRadius,
                            wheelSideFriction,
                            wheelFwdFriction,
                            damping,
                            wheelNumRays);

            wheels[(int)WheelId.WheelML].Setup(this,
                            MLPos,
                            axis,
                            spring,
                            wheelTravel,
                            inertia,
                            wheelRadius,
                            wheelSideFriction,
                            wheelFwdFriction,
                            damping,
                            wheelNumRays);


            wheels[(int)WheelId.WheelFL].Setup(this,
                            FLPos,
                            axis,
                            spring,
                            wheelTravel,
                            inertia,
                            wheelRadius,
                            wheelSideFriction,
                            wheelFwdFriction,
                            damping,
                            wheelNumRays);
        }

        /// <summary>
        /// Register with physics
        /// </summary>
        public void EnableCar()
        {
            if (chassis != null)
                chassis.EnableChassis();

        }

        /// <summary>
        /// Remove from the physics system
        /// </summary>
        public void DisableCar()
        {
            if (chassis != null)
                chassis.DisableChassis();
        }

        /// <summary>
        /// Update stuff at the end of physics
        /// </summary>
        /// <param name="dt"></param>
        public override void PostPhysics(float dt)
        {
            for (int i = 0; i < wheels.Count; i++)
                wheels[i].Update(dt);

            // control inputs
            float deltaAccelerate = dt * 4.0f;
            float deltaSteering = dt * steerRate;

            // update the actual values
            float dAccelerate = destAccelerate - accelerate;
            dAccelerate = MathHelper.Clamp(dAccelerate, -deltaAccelerate, deltaAccelerate);

            accelerate += dAccelerate;

            float dSteering = destSteering - steering;
            dSteering = MathHelper.Clamp(dSteering, -deltaSteering, deltaSteering);

            steering += dSteering;

            // apply these inputs
            float maxTorque = driveTorque;

            if (fWDrive && bWDrive)
                maxTorque *= 0.5f;

            wheels[(int)WheelId.WheelFL].AddTorque(maxTorque * accelerate);
            wheels[(int)WheelId.WheelML].AddTorque(maxTorque * accelerate);
            wheels[(int)WheelId.WheelBL].AddTorque(maxTorque * accelerate);
            
            wheels[(int)WheelId.WheelFR].AddTorque(maxTorque * accelerate);            
            wheels[(int)WheelId.WheelMR].AddTorque(maxTorque * accelerate);
            wheels[(int)WheelId.WheelBR].AddTorque(maxTorque * accelerate);

            wheels[(int)WheelId.WheelBL].Lock = (hBrake > 0.5f);
            wheels[(int)WheelId.WheelBR].Lock = (hBrake > 0.5f);

            // steering angle applies to the inner wheel. The outer one needs to match it
            int inner, outer, inner2, outer2;

            if (steering > 0.0f)
            {
                inner = (int)WheelId.WheelFL;
                outer = (int)WheelId.WheelFR;
                inner2 = (int)WheelId.WheelBL;
                outer2 = (int)WheelId.WheelBR;
            }
            else
            {
                inner = (int)WheelId.WheelFR;
                outer = (int)WheelId.WheelFL;
                inner2 = (int)WheelId.WheelBR;
                outer2 = (int)WheelId.WheelBL;
            }

            float alpha = System.Math.Abs(maxSteerAngle * steering);
            float angleSgn = steering > 0.0f ? 1.0f : -1.0f;

            wheels[outer].SteerAngle = (angleSgn * alpha);
            wheels[outer2].SteerAngle = (angleSgn * -alpha);

            float beta;

            if (alpha == 0.0f)
            {
                beta = alpha;
            }
            else
            {
                float dx = (wheels[(int)WheelId.WheelFR].Pos.X - wheels[(int)WheelId.WheelBR].Pos.X);
                float dy = (wheels[(int)WheelId.WheelFL].Pos.Z - wheels[(int)WheelId.WheelFR].Pos.Z);
                //beta = ATan2Deg(dy, dx + (dy / TanDeg(alpha)));
                beta = (float)System.Math.Atan2(MathHelper.ToRadians(dy), MathHelper.ToRadians(dx + (dy / (float)System.Math.Tan(MathHelper.ToRadians(alpha)))));
                beta = MathHelper.ToDegrees(beta);
            }
            wheels[inner].SteerAngle = (angleSgn * beta);
            wheels[inner2].SteerAngle = (angleSgn * -beta);
        }

        /// <summary>
        /// Gets or Sets chassis (There will always be a chassis)
        /// </summary>
        public Chassis Chassis
        {
            get { return chassis; }
            set { chassis = value; }
        }


        /// <summary>
        /// Accelerate control - values -1/0 to 1
        /// </summary>
        public float Accelerate
        {
            get { return destAccelerate; }
            set { destAccelerate = value; }
        }

        /// <summary>
        /// Steer control - values -1/0 to 1
        /// </summary>
        public float Steer
        {
            get { return destSteering; }
            set { destSteering = value; }
        }

        /// <summary>
        /// Gets count NumWheelsOnFloor
        /// </summary>
        public int NumWheelsOnFloor
        {
            get
            {
                int count = 0;

                for (int i = 0; i < wheels.Count; i++)
                {
                    if (wheels[i].OnFloor)
                        count++;
                }
                return count;
            }
        }
    }
}
