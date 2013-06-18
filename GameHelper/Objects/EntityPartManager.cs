﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using JigLibX.Physics;
using JigLibX.Collision;

namespace GameHelper.Objects
{
    public class EntityPartManager
    {
        SortedList<int, EntityPart> parts = new SortedList<int,EntityPart>();
        public CollisionCallbackFn collisionCallback;
        public EntityPartManager()
        {
        }

        public EntityPartManager(Body body)
        {
        }

        public Matrix GetOrientationMatrix(int id)
        {
            if (!parts.ContainsKey(id))
                return Matrix.Identity;
            return parts[id].GetOrientation();
        }

        public Matrix GetTranformationMatrix(int id)
        {
            if (!parts.ContainsKey(id))
                return Matrix.Identity;
            return parts[id].GetTransformation(true);
        }

        public Matrix[] GetModelTransforms(int id)
        {
            if (!parts.ContainsKey(id))
                return null;
            return parts[id].ModelTranforms;
        }

        public void AdjustYawPitchRoll(int id, float yaw, float pitch, float roll)
        {
            if (!parts.ContainsKey(id))
                return;
            parts[id].AdjustYawPitchRoll(yaw, pitch, roll);
        }
        public void SetYawPitchRoll(int id, float yaw, float pitch, float roll)
        {
            if (!parts.ContainsKey(id))
                return;
            parts[id].SetYawPitchRoll(yaw, pitch, roll);
        }

        public void AddPart(int parentId, EntityPart child)
        {
            if (!parts.ContainsKey(child.Id))
                parts.Add(child.Id, child);
            child.body.CollisionSkin.callbackFn += new CollisionCallbackFn(PartCollisioncallbackFn);
            if (parts.ContainsKey(parentId))
            {
                EntityPart parent = parts[parentId];
                parent.AddPart(ref child);
                child.fParentPart = parent;
            }
        }

        bool PartCollisioncallbackFn(CollisionSkin skin0, CollisionSkin skin1)
        {
            
            if (collisionCallback == null)
                return true;
            return collisionCallback(skin0, skin1);
        }

        public void AddPart(int id, Model m, Vector3 scale, Vector3 orientYPR, Vector3 RelativeOrigin, Vector3 modelOrientCorrection, Vector3 modelOriginCorrection)
        {
            EntityPart rb = new EntityPart(id, m, scale, orientYPR, RelativeOrigin, modelOrientCorrection, modelOriginCorrection);
            parts.Add(id, rb);
        }
        public void AddPart(int id, Model m, float scale, Vector3 orientYPR, Vector3 RelativeOrigin, Vector3 modelOrientCorrection, Vector3 modelOriginCorrection)
        {
            AddPart(id, m, new Vector3(scale, scale, scale), orientYPR, RelativeOrigin, modelOrientCorrection, modelOriginCorrection);
        }
        public void AddPart(int id, Model m, Vector3 orientYPR, Vector3 RelativeOrigin, Vector3 modelOrientCorrection,Vector3 modelOriginCorrection)
        {
            AddPart(id, m, 1.0f, orientYPR, RelativeOrigin, modelOrientCorrection, modelOriginCorrection);
        }


        internal void EnableBody()
        {
            foreach (EntityPart ep in parts.Values)
            {
                ep.EnableBody();
            }
        }

        internal void DrawWireframe(GraphicsDevice Graphics, Matrix View, Matrix Projection)
        {
            foreach (EntityPart p in parts.Values)
            {
                p.DrawWireframe(Graphics, View, Projection);
            }
        }

        internal void Draw(ref Matrix View, ref Matrix Projection)
        {
            foreach (EntityPart p in parts.Values)
            {
                p.Draw(ref View, ref Projection);
            }
        }
    }
}