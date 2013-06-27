using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using JigLibX.Physics;
using JigLibX.Collision;
using GenEntityConfigTypes;

namespace GameHelper.Objects
{
    public class EntityPartManager
    {
        SortedList<int, EntityPart> parts = new SortedList<int,EntityPart>();
        public CollisionCallbackFn collisionCallback;
        public EntityPartManager()
        {
        }

        public EntityPartManager(ref Body body)
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

        public event CollisionCallbackFn CollisionOccurred;
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
                ep.Enable();
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

        internal void ApplyConfig(GenEntityConfigTypes.Part[] parts)
        {
            
        }

        private int GetPartId()
        {
            int i = 0;
            while (parts.ContainsKey(++i)) ;
            return i;
        }

        internal void ApplyConfig(Part[] parts, SortedList<string, Model> models)
        {
            int id = 0;
            AddParts(id, parts, models);
        }

        private void AddParts(int pid, Part[] parts, SortedList<string, Model> models)
        {
            if (parts == null)
                return;
            foreach (Part part in parts)
                 AddPart(pid, part, models);
        }

        private void AddPart(int pid, Part part, SortedList<string, Model> models)
        {
            int id = GetPartId();
            EntityPart ep = new EntityPart(id, models[CleanPath(part.Model.relFilepath)], part);
            // add this parts under a parent
            AddPart(pid, ep);

            // add this parts' children
            AddParts(id, part.Parts, models);
        }

        private string CleanPath(string path)
        {
            return path.Replace("\\", string.Empty).Replace(".", string.Empty);
        }
    }
}
