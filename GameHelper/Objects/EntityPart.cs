using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using JigLibX.Physics;
using JigLibX.Collision;

namespace GameHelper.Objects
{
    public class EntityPart
    {
        public int Id;
        Model model;
        public Body body;
        CollisionSkin Skin;
        Vector3 Scale = new Vector3(1, 1, 1);
        
        public Quaternion Orient
        {
            get
            {
                return Quaternion.CreateFromYawPitchRoll(RelativeOrientYPR.X, RelativeOrientYPR.Y, RelativeOrientYPR.Z);
            }
        }
        public Quaternion OrientCorrection
        {
            get
            {
                return Quaternion.CreateFromYawPitchRoll(ModelOrientCorrectionYPR.X, ModelOrientCorrectionYPR.Y, ModelOrientCorrectionYPR.Z);
            }
        }
        Vector3 ModelScaleCorrection = new Vector3(1, 1, 1);
        Vector3 ModelOriginCorrection = Vector3.Zero;
        Vector3 ModelOrientCorrectionYPR = Vector3.Zero;

        // Relative to the parent
        Vector3 RelativeOrigin = Vector3.Zero;
        Vector3 RelativeOrientYPR = Vector3.Zero;
        
        public EntityPart fParentPart;
        public SortedList<int, EntityPart> parts = new SortedList<int, EntityPart>();

        public EntityPart(int id, Model m, Vector3 scale, Vector3 relOrientYPR, Vector3 relOrigin, Vector3 modelOrientCorrection, Vector3 modelOriginCorrection)
        {
            Id = id;
            model = m;
            ModelScaleCorrection = scale;
            RelativeOrientYPR = relOrientYPR;
            RelativeOrigin = relOrigin;
            ModelOrientCorrectionYPR = modelOrientCorrection;
            ModelOriginCorrection = modelOriginCorrection;
            
            body = new Body();
            Skin = new CollisionSkin(body);
            body.CollisionSkin = Skin;
            body.ExternalData = this;
            body.CollisionSkin.callbackFn += new CollisionCallbackFn(CollisionSkin_callbackFn);
        }

        bool CollisionSkin_callbackFn(CollisionSkin skin0, CollisionSkin skin1)
        {
            return true; // let the physics system handle the collision
        }

        public void AdjustYawPitchRoll(float yaw, float pitch, float roll)
        {
            RelativeOrientYPR.X += yaw;
            RelativeOrientYPR.Y += pitch;
            RelativeOrientYPR.Z += roll;
        }

        public void AddPart(ref EntityPart rb)
        {
            parts.Add(rb.Id, rb);            
        }

        public static EntityPart GetPart(int id, Model m, Vector3 scale, Vector3 orientYPR, Vector3 RelativeOrigin, Vector3 modelOrientCorrection, Vector3 modelOriginCorrection)
        {
            EntityPart rb = new EntityPart(id, m, scale, orientYPR, RelativeOrigin,modelOrientCorrection, modelOriginCorrection);
            return rb;
        }
        public static EntityPart GetPart(int id, Model m, float scale, Vector3 orientYPR, Vector3 RelativeOrigin, Vector3 modelOrientCorrection, Vector3 modelOriginCorrection)
        {
            return GetPart(id, m, new Vector3(scale, scale, scale), orientYPR, RelativeOrigin, modelOrientCorrection, modelOriginCorrection);
        }
        public static EntityPart GetPart(int id, Model m, Vector3 orientYPR, Vector3 RelativeOrigin, Vector3 modelOrientCorrection, Vector3 modelOriginCorrection)
        {
            return GetPart(id, m, 1.0f, orientYPR, RelativeOrigin,modelOrientCorrection,  modelOriginCorrection);
        }
        /// <summary>
        /// Used for iterating through the meshes
        /// </summary>
        public Matrix[] ModelTranforms
        {
            get
            {
                Matrix[] tranforms = new Matrix[model.Bones.Count];
                model.CopyAbsoluteBoneTransformsTo(tranforms);
                return tranforms;
            }
        }
        /// <summary>
        /// Retrieves all transforms related to this bone's model
        /// Some Transformations are not used (because they are insignificant) for this bone's parents (scale / orientation correction)
        /// </summary>
        /// <param name="isLeaf"></param>
        /// <returns></returns>
        public Matrix GetTransformation(bool isLeaf)
        {
            Matrix transform = Matrix.Identity;

            if (isLeaf) // if this is the leaf model,
                transform*= Matrix.CreateScale(ModelScaleCorrection); // include the scale correction

            transform *= Matrix.CreateTranslation(ModelOriginCorrection); // shift the Model away from its native origin here

            Quaternion qRot = Orient; // represents orientation of the model based on animation / user input
            if (isLeaf) // if this is the leaf model,
                qRot *= OrientCorrection; // represent shifting the model away from native orientation 

            // the rot variable represents all manipulations in a single vector for Yaw, Pitch, Roll
            // Merging Yaw/Pitch/Roll vectors, then getting a Quaternion, and then a Matrix works
            // Multiplying two Quaternions and then getting a Matrix would work
            // Multiplying to Matrices does not work because the first changes the affect of the second.
            Matrix mRot = Matrix.CreateFromQuaternion(qRot); //

            transform *= mRot; // Rotate the model
            //X:-0.35 Y:0.1 Z:-0.26
            transform*=Matrix.CreateTranslation(RelativeOrigin); // Move the model
                
            if(fParentPart!=null)
                transform *= fParentPart.GetTransformation(false); // Apply parent's transforms

            return transform;
        }

        /// <summary>
        /// Returns just the transforms related to model the orientation caused by a series of bones
        /// </summary>
        /// <returns></returns>
        public Matrix GetOrientation()
        {
            Matrix orient = Matrix.CreateFromQuaternion(Orient); // include animation or user-input-based orientation transformation only
            if(fParentPart != null) // if there is a parent,
                orient *= fParentPart.GetOrientation(); // include their orientation transform
            return orient;
        }

        public void SetYawPitchRoll(float yaw, float pitch, float roll)
        {
            RelativeOrientYPR.X = yaw;
            RelativeOrientYPR.Y = pitch;
            RelativeOrientYPR.Z = roll;
        }

        internal void EnableBody()
        {
            body.EnableBody();
        }

        public virtual void Draw(ref Matrix View, ref Matrix Projection)
        {
            if (model == null)
                return;
            Matrix[] transforms = new Matrix[model.Bones.Count];

            model.CopyAbsoluteBoneTransformsTo(transforms);
            Matrix worldMatrix = GetWorldMatrix();

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    //if (Selected)
                        //effect.AmbientLightColor = Color.Red.ToVector3();
                    effect.World = transforms[mesh.ParentBone.Index] * worldMatrix;
                    effect.View = View;
                    effect.Projection = Projection;
                }
                mesh.Draw();
            }
        }
        internal BasicEffect Effect { get; set; }
        public virtual void DrawWireframe(GraphicsDevice Graphics, Matrix View, Matrix Projection)
        {
            try
            {
                VertexPositionColor[] wireFrame = Skin.GetLocalSkinWireframe();
                body.TransformWireframe(wireFrame);
                if (Effect == null)
                {
                    Effect = new BasicEffect(Graphics);
                    Effect.VertexColorEnabled = true;
                }
                Effect.TextureEnabled = false;
                Effect.LightingEnabled = false;
                Effect.View = View;
                Effect.Projection = Projection;

                foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Graphics.DrawUserPrimitives<VertexPositionColor>(
                        Microsoft.Xna.Framework.Graphics.PrimitiveType.LineStrip,
                        wireFrame, 0, wireFrame.Length - 1);
                }

                VertexPositionColor[] Velocity = new VertexPositionColor[2];
                Velocity[0] = new VertexPositionColor(body.Position, Color.Blue);
                Velocity[1] = new VertexPositionColor(body.Position + body.Velocity, Color.Red);

                foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Graphics.DrawUserPrimitives<VertexPositionColor>(
                        Microsoft.Xna.Framework.Graphics.PrimitiveType.LineStrip,
                        Velocity, 0, Velocity.Length - 1);
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.StackTrace);
            }
        }
        /// <summary>
        /// only used for the model
        /// </summary>
        /// <returns></returns>
        public Matrix GetWorldMatrix()
        {
            return Matrix.CreateScale(Scale) * Skin.GetPrimitiveLocal(0).Transform.Orientation * body.Orientation * Matrix.CreateTranslation(body.Position);
        }
    }
}
