using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using JigLibX.Physics;
using JigLibX.Collision;
using GenEntityConfigTypes;
using JigLibX.Geometry;

namespace GameHelper.Objects
{
    public class EntityPart
    {
        #region Fields
        public int Id;
        Model model;
        public Body body;
        CollisionSkin Skin;
        Vector3 Scale = new Vector3(1, 1, 1);
        bool isRoot = false;
        #endregion

        #region Properties
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
        internal BasicEffect Effect { get; set; }

        Vector3 ModelScaleCorrection = new Vector3(1, 1, 1);
        Vector3 ModelOriginCorrection = Vector3.Zero;
        Vector3 ModelOrientCorrectionYPR = Vector3.Zero;

        // Relative to the parent
        Vector3 RelativeOrigin = Vector3.Zero;
        Vector3 RelativeOrientYPR = Vector3.Zero;
        
        public event CollisionCallbackFn CollisionOccurred;

        public EntityPart fParentPart;
        public SortedList<int, EntityPart> parts = new SortedList<int, EntityPart>();
        public GenEntityConfigTypes.Part part;
        #endregion

        #region Initialization
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

        public EntityPart(int id, Model model, GenEntityConfigTypes.Part part)
        {
            // TODO: Complete member initialization
            this.Id = id;
            this.model = model;
            this.part = part;
            body = new Body();
            Skin = new CollisionSkin(body);
            ApplyConfig(part);
            body.CollisionSkin = Skin;

            //use part config here
            
        }

        public EntityPart(bool isroot)
        {
            body = new Body();
            Skin = new CollisionSkin();
            isRoot = isroot;
            if (isroot)
            {
                body.CollisionSkin = new CollisionSkin();
                Skin.AddPrimitive(new Box(new Vector3((float)3 * -.5f, (float)3 * -.5f, (float)3 * -.5f),
                    Matrix.Identity,
                    new Vector3((float)3, (float)3, (float)3)), (int)MaterialTable.MaterialID.NotBouncyNormal);
             
            }
            else
            {
                body.CollisionSkin = Skin;
            }
            body.ExternalData = this;
        }
        #endregion

        #region Config
        public void ApplyConfig(GenEntityConfigTypes.Part part)
        {
            this.RelativeOrigin = new Vector3((float)part.Body.relTranslation.X,
                                                (float)part.Body.relTranslation.Y,
                                                (float)part.Body.relTranslation.Z);
            ApplyBoxConfig(part.Body.Skin.Box);
            ApplyTriangleConfig(part.Body.Skin.Triangles);
        }
       
       
        private void ApplyBoxConfig(ecBox[] boxes)
        {
            if (boxes == null)
                return;
            foreach (ecBox box in boxes)
            {
                Skin.AddPrimitive(new Box(new Vector3((float)box.SideLengths.X * -.5f, (float)box.SideLengths.Y * -.5f, (float)box.SideLengths.Z * -.5f),
                    Matrix.Identity,
                    new Vector3((float)box.SideLengths.X, (float)box.SideLengths.Y, (float)box.SideLengths.Z)), (int)MaterialTable.MaterialID.NotBouncyNormal);
            }
        }
        private void ApplyTriangleConfig(ecTriangle[] tris)
        {
            if (tris == null)
                return;
            foreach (ecTriangle tri in tris)
            {
            }
        }
        #endregion

        #region physics
        internal void Enable()
        {
            body.EnableBody();
            //this.body.DoShockProcessing = false;
            foreach (EntityPart ep in parts.Values)
            {
                ep.body.Mass = 1f;
                ep.Enable();                
            }
        }
        bool CollisionSkin_callbackFn(CollisionSkin skin0, CollisionSkin skin1)
        {
            if (CollisionOccurred != null)
                CollisionOccurred(skin0, skin1);
            return true; // let the physics system handle the collision
        }
        #endregion

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

        #region Visual
        public virtual void Draw(ref Matrix View, ref Matrix Projection)
        {
            foreach(EntityPart ep in parts.Values)
                ep.Draw(ref View, ref Projection);

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
        public virtual void DrawWireframe(GraphicsDevice Graphics, Matrix View, Matrix Projection)
        {
           
            
            
            try
            {

                if (Effect == null)
                {
                    Effect = new BasicEffect(Graphics);
                    Effect.VertexColorEnabled = true;
                }

                
                foreach (EntityPart ep in parts.Values)
                {

                    #region part skeleton
                    ep.DrawWireframe(Graphics, View, Projection);

                    VertexPositionColor[] bone = new VertexPositionColor[2];
                    bone[0] = new VertexPositionColor(Position, Color.Green);
                    bone[1] = new VertexPositionColor(ep.Position, Color.Green);
                    //TransformVectorList(Velocity);

                    foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        Graphics.DrawUserPrimitives<VertexPositionColor>(
                            Microsoft.Xna.Framework.Graphics.PrimitiveType.LineStrip,
                            bone, 0, bone.Length - 1);
                    }
                    #endregion
                }
                




                VertexPositionColor[] wireFrame = Skin.GetLocalSkinWireframe();
                if (wireFrame.Length == 0)
                    return;
                TransformVectorList(wireFrame);

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
                Velocity[0] = new VertexPositionColor(Position, Color.Blue);
                Velocity[1] = new VertexPositionColor(Position + body.Velocity, Color.Red);
                //TransformVectorList(Velocity);

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
        #endregion

        /// <summary>
        /// only used for the model
        /// </summary>
        /// <returns></returns>
        public Matrix GetWorldMatrix()
        {
            Matrix m = Matrix.Identity;
            //if(fParentPart!=null)
                //m = fParentPart.GetWorldMatrix();
            /*
            //ORIGINAL
            if(Skin.NumPrimitives==0)
                return m * Matrix.CreateScale(Scale) * body.Orientation * Matrix.CreateTranslation(body.Position);
            else
                return m * Matrix.CreateScale(Scale) * Skin.GetPrimitiveLocal(0).Transform.Orientation * body.Orientation * Matrix.CreateTranslation(body.Position) * Matrix.CreateTranslation(RelativeOrigin);
            */
            if (Skin.NumPrimitives == 0)
                return m * Matrix.CreateScale(Scale) * body.Orientation * Matrix.CreateTranslation(body.Position);
            else
                return m * Matrix.CreateScale(Scale) * Skin.GetPrimitiveLocal(0).Transform.Orientation * body.Orientation * Matrix.CreateTranslation(body.Position);
             

            /*
             * no good
            if (Skin.NumPrimitives == 0)
                return m * body.Orientation * Matrix.CreateTranslation(body.Position);
            else
                return m * Skin.GetPrimitiveLocal(0).Transform.Orientation * body.Orientation * Matrix.CreateTranslation(body.Position) * Matrix.CreateTranslation(RelativeOrigin);
             */
        }
        public Vector3 Position
        {
            get
            {
                return Vector3.Transform(RelativeOrigin,
                                                GetWorldMatrix());
            }
        }
        public void TransformVectorList(VertexPositionColor[] wireframe)
        {
             for (int i = 0; i < wireframe.Length; i++)
                {
                    wireframe[i].Position = Vector3.Transform(wireframe[i].Position,
                                                GetWorldMatrix());
                }
        }

        internal void FinalizeBody()
        {
            try
            {
                Vector3 com = SetMass(1.0f);

                //body.MoveTo(Position, Orientation);
                Skin.ApplyLocalTransform(new JigLibX.Math.Transform(-com, Matrix.Identity));
            }
            catch (Exception E)
            {
                System.Diagnostics.Debug.WriteLine(E.StackTrace);
            }
        }

        internal Vector3 SetMass(float mass)
        {
            PrimitiveProperties primitiveProperties = new PrimitiveProperties(
                PrimitiveProperties.MassDistributionEnum.Solid,
                PrimitiveProperties.MassTypeEnum.Mass,
                mass);

            float oMass;
            Vector3 com = new Vector3();
            Matrix it, itCom;

            Skin.GetMassProperties(primitiveProperties, out oMass, out com, out it, out itCom);  
            body.BodyInertia = itCom;
            body.Mass = oMass;

            return com;
        }
    }
}
