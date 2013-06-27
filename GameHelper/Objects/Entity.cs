using GameHelper.Physics;
using JigLibX.Collision;
using JigLibX.Geometry;
using JigLibX.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using GenEntityConfigTypes;

namespace GameHelper.Objects
{
    public class Entity
    {
        /*
        * Colby is here 2013.06.20
        * 
        * Entites are loading their configs and configging parts accordingly.
        * Problem is all the entity initialization (location / orientation) in the entity, affecting entity properties, and not the ROOT PART.
        * They should affect the root part.
        * Either store a reference to the root part by passing it into the PartManager constructor OR pass through position/orientation calls to the part manager.
         * 
         * 
         * Entity has parts
         *  Those parts can move or have various attributes that the game can hook into.
         * 
         * The problem is:
         *  We don't have specific references into which part does what, like we used to.
         *  That's not to say it can't work by part names -> By name, find a unique part and adjust it a certain way.
         *  In that regard, the entity needs a reference to every part, but we also need the hierarchy of parts for fewer lookups during draw calculations.
         * 
         * So, we keep the master list in the Entity.
         * But what about the "root" from which everything is based.
         * Can it not have a model and not have a skin, but be a body, and not have a name?
        */

        #region Fields
        public int ID;
        public bool isOnServer;
        public bool isOnClient;
        public EntityType aType;
        public GenEntityConfigTypes.EntityConfig config;
        public bool Selected;
        public GameHelper.Input.ActionManager actionManager = new GameHelper.Input.ActionManager();
        public List<Controller> controllers = new List<Controller>();
        public bool hasNotDoneFirstInterpoladation = true; // this object has never processed an update and interpolated (with any factor) 
        public int OwningClientId = -1;
        public int UpdateCountdown = 10;
        public EntityPart root;
        public SortedList<int, EntityPart> partList = new SortedList<int, EntityPart>();
        private bool CollidedRecently = true; // we want the object to update immediately once created
        #endregion
        
        #region Properties
        public Vector3 Position
        {
            get
            {
                return root.body.Position;
            }
            set
            {
                root.body.Position = value;
            }
        }
        public Matrix Orientation
        {
            get
            {
                return root.body.Orientation;
            }
            set
            {
                root.body.Orientation = value;
            }
        }
        public Vector3 Velocity
        {
            get
            {
                return root.body.Velocity;
            }
            set
            {
                //foreach (EntityPart p in partList.Values)
                    //p.body.Velocity = Vector3.Zero;

                root.body.Velocity = value;
                
            }
        }

        internal BasicEffect Effect { get; set; }
        public bool IsActive
        {
            get
            {
                return root.body.IsActive;
            }
        }
        public bool isMoveable
        {
            get
            {
                return !root.body.Immovable;
            }
        }
        
        public bool DidCollideRecently
        {
            get
            {
                if (CollidedRecently)
                {
                    CollidedRecently = false;
                    return true;
                }
                return false;
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Default Constructor
        /// Initalizes the Body and a CollisionSkin
        /// No Primatives are added to the Body
        /// </summary>
        /// <param name="position">Initial Body Position</param>
        /// <param name="scale">Scale</param>
        public Entity()
        {
            root = new EntityPart(true);
            partList.Add(0, root);
        }

        /// <summary>
        /// Single Primitive Constructor with custom MaterialProperty
        /// </summary>
        /// <param name="position">Initial Body Position</param>
        /// <param name="scale">Scale</param>
        /// <param name="primative">Primitive to add to Skin</param>
        /// <param name="prop">Material Properties of Primitive</param>
        public Entity(Vector3 position, Vector3 scale, Primitive primative, MaterialProperties prop, Model model, int asset)
            : this()
        {
            CommonInit(position, scale, model, true, asset);
        }

        /// <summary>
        /// Single Primitive Constructor with predefined MaterialProperty
        /// </summary>
        /// <param name="position">Initial Body Position</param>
        /// <param name="scale">Scale</param>
        /// <param name="primative">Primitive to add to Skin</param>
        /// <param name="propId">Predefined Material Properties of Primitive</param>
        public Entity(Vector3 position, Vector3 scale, Primitive primative, MaterialTable.MaterialID propId, Model model, int asset)
            : this()
        {
            CommonInit(position, scale, model, true, asset);
        }

        /// <summary>
        /// Multiple Primitive Constructor
        /// Each Primitive needs a Material Property
        /// </summary>
        /// <param name="position">Initial Body Position</param>
        /// <param name="scale">Scale</param>
        /// <param name="primatives">Primitives to add to Skin</param>
        /// <param name="props">Material Properties of Primitives to add</param>
        public Entity(Vector3 position, Vector3 scale, List<Primitive> primatives, List<MaterialProperties> props, Model model, int asset)
            : this()
        {
            CommonInit(position, scale, model, true, asset);
        }

        public Entity(Vector3 position, Vector3 scale, Primitive primitive, Model model, bool moveable, int asset)
            : this()
        {
            
            try
            {
                
                CommonInit(position, scale, model, moveable, asset);
            }
            catch (Exception E)
            {
                System.Diagnostics.Debug.WriteLine(E.StackTrace);
            }
        }
        
        public void CommonInit(Vector3 pos, Vector3 scale, Model model, bool moveable, int asset)
        {
            Position = pos;
            //config.
            //Model = model;
            root.body.Immovable = !moveable;
            // Enumerated AssetTypes are the only integers/Enums


            //aType = new AssetType(types.a1, this, );

            // asset names are loaded at runtime
            
            // MOVED TO BEFORE INTEGRATE
            //FinalizeBody();
        }
        public void CommonInit(Vector3 pos, Matrix orient, bool moveable)
        {
            Position = pos;
            Orientation = orient;
            root.body.Immovable = !moveable;
            // MOVED TO BEFORE INTEGRATE
            //FinalizeBody();
        }
        public void BodyInit(Vector3 pos, Matrix orient)
        {
            root.body.MoveTo(pos, orient);
        }
        public void BodyInit(Vector3 pos)
        {
            root.body.MoveTo(pos, Matrix.Identity);
        }
#endregion

        #region Physics
        bool CollisionSkin_callbackFn(CollisionSkin skin0, CollisionSkin skin1)
        {
            CollidedRecently = true; // we just want to know when it collided
            return true; // let the physics system handle the collision
        }
        public void AddCollisionCallback(CollisionCallbackFn cbf)
        {
            this.root.CollisionOccurred+=cbf;
            //this.body.CollisionSkin.callbackFn += cbf;
        }
        public void AddController(Controller c)
        {
            controllers.Add(c);
            PhysicsSystem.CurrentPhysicsSystem.AddController(c);
        }
        public void MoveTo(Vector3 pos, Matrix orient)
        {
            root.body.MoveTo(pos, orient);
        }
        public virtual void FinalizeBody()
        {

            foreach (EntityPart p in partList.Values)
            {
                p.FinalizeBody();
            }
            root.Enable();
        }
        #endregion

        #region Parts
        public void EnableParts()
        {
            root.body.Mass = 100;
            root.Enable();
        }

        public void AddPart(int parentId, EntityPart child)
        {
            if (!partList.ContainsKey(child.Id))
                partList.Add(child.Id, child);
            
            child.body.CollisionSkin.callbackFn += new CollisionCallbackFn(PartCollisioncallbackFn);
            if (partList.ContainsKey(parentId))
            {
                EntityPart parent = partList[parentId];

                /*
                 * collision skin types which should or should not collide with those in another certain group.
                 * 
                 * 
                 */



                if (child.part.Name == "fuselageA")
                {
                    JigLibX.Physics.ConstraintPoint c = new ConstraintPoint(parent.body, new Vector3(3, 3, 3), child.body, new Vector3(-3, -3, 3), 0, 1f);
                    PhysicsSystem.CurrentPhysicsSystem.AddConstraint(c);
                    c.EnableConstraint();
                    JigLibX.Physics.ConstraintPoint c2 = new ConstraintPoint(parent.body, new Vector3(3, 3, -3), child.body, new Vector3(-3, -3, -3), 0, 1f);
                    PhysicsSystem.CurrentPhysicsSystem.AddConstraint(c2);
                    c2.EnableConstraint();
                    JigLibX.Physics.ConstraintPoint c3 = new ConstraintPoint(parent.body, new Vector3(3, -3, 0), child.body, new Vector3(-3, -6, 0), 0, 1f);
                    PhysicsSystem.CurrentPhysicsSystem.AddConstraint(c3);
                    c3.EnableConstraint();
                }
                else if (child.part.Name == "fuselageB")
                {
                    JigLibX.Physics.ConstraintPoint c = new ConstraintPoint(parent.body, new Vector3(-3, 3, 3), child.body, new Vector3(3, -3, 3), 0, 1f);
                    PhysicsSystem.CurrentPhysicsSystem.AddConstraint(c);
                    c.EnableConstraint();
                    JigLibX.Physics.ConstraintPoint c2 = new ConstraintPoint(parent.body, new Vector3(-3, 3, -3), child.body, new Vector3(3, -3, -3), 0, 1f);
                    PhysicsSystem.CurrentPhysicsSystem.AddConstraint(c2);
                    c2.EnableConstraint();
                    JigLibX.Physics.ConstraintPoint c3 = new ConstraintPoint(parent.body, new Vector3(-3, -3, 0), child.body, new Vector3(3, -6, 0), 0, 1f);
                    PhysicsSystem.CurrentPhysicsSystem.AddConstraint(c3);
                    c3.EnableConstraint();
                }

                parent.AddPart(ref child);
                child.fParentPart = parent;
            }
        }

        bool PartCollisioncallbackFn(CollisionSkin skin0, CollisionSkin skin1)
        {
            return true;
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

        private int GetPartId()
        {
            int i = 0;
            while (partList.ContainsKey(++i)) ;
            return i;
        }
        #endregion

        #region Visual
        public virtual void Draw(ref Matrix View, ref Matrix Projection)
        {
            root.Draw(ref View, ref Projection);
            /*
            if (Model == null)
                return;
            Matrix[] transforms = new Matrix[Model.Bones.Count];

            Model.CopyAbsoluteBoneTransformsTo(transforms);
            Matrix worldMatrix = GetWorldMatrix();

            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    if(Selected)
                        effect.AmbientLightColor = Color.Red.ToVector3();
                    effect.World = transforms[mesh.ParentBone.Index] * worldMatrix;
                    effect.View = View;
                    effect.Projection = Projection;
                }
                mesh.Draw();
            }*/
        }
        public virtual void DrawWireframe(GraphicsDevice Graphics, Matrix View, Matrix Projection)
        {
            string entityName = this.aType.Name;
            root.DrawWireframe(Graphics, View, Projection);
            /*
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

                foreach (Controller c in controllers)
                {
                    if (c is BoostController)
                    {
                        VertexPositionColor[] Force = new VertexPositionColor[2];
                        BoostController bc = c as BoostController;
                        Force[0] = new VertexPositionColor(bc.ForcePosition, Color.Green);
                        Force[1] = new VertexPositionColor(bc.ForcePosition + (bc.Force * bc.forceMag), Color.Yellow);
                        if (!bc.worldForce)
                            body.TransformWireframe(Force);

                        VertexBuffer verts = new VertexBuffer(Graphics, VertexPositionColor.VertexDeclaration, Force.Length, BufferUsage.WriteOnly);
                        verts.SetData(Force);

                        foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            Graphics.SetVertexBuffer(verts);
                            Graphics.DrawPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.LineStrip, 0, Force.Length - 1);
                            //Graphics.DrawUserPrimitives<VertexPositionColor>(
                            //    Microsoft.Xna.Framework.Graphics.PrimitiveType.LineStrip,
                            //    Force, 0, Force.Length - 1, LightingVertexFormat.VertexDeclaration);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.StackTrace);
            }*/
        }
        #endregion

        #region Multiplayer
        /// <summary>
        /// should be called after MoveTo
        /// </summary>
        /// <param name="vel"></param>
        public void UpdateVelocity(Vector3 vel)
        {
            root.body.Velocity = vel;
            //Body.UpdateVelocity(vel.Length);
        }
        public void ProcessSimulatedInput(object[] actionvalues)
        {
            actionManager.ProcessActionValues(actionvalues);
        }
        public virtual Vector3 GetPositionAbove()
        {
            Vector3 ret = Position;
            ret.Y += Math.Abs((root.body.CollisionSkin.WorldBoundingBox.Min.Y + root.body.CollisionSkin.WorldBoundingBox.Max.Y) / 2f); // Assume body is halfway in this?
            return ret;
        }

        /// <summary>
        /// CLIENT SIDE
        /// Interpolating Update with the default 50/50 interpolation
        /// </summary>
        /// <param name="vector3"></param>
        /// <param name="matrix"></param>
        /// <param name="vector3_2"></param>
        public void Interpoladate(Vector3 position, Matrix orientation, Vector3 velocity)
        {
            float interFactor = .5f; // assume simple interpolation

            bool VariableInterp=false;            
            if (VariableInterp)
            {
                float magicFactor = 10; // there's some magic number (varrying per game according to map/unit scale) 
                // the further away the object is from where it should be, the more we should make it where it should be
                // the magic number should be enough to counter-act gravity, perhaps.
                float distSq = Vector3.DistanceSquared(position, Position);
                interFactor = distSq / magicFactor;
            }

            // default interpolation factor is 50/50.
            Interpoladate(position, orientation, velocity, interFactor);
        }

        /// <summary>
        /// CLIENT SIDE
        /// Interpolating Update with a specified interpolation factor.
        /// Factor of 1.0 means make it so! 
        /// Factor of 0 means ignore the update. =(
        /// </summary>
        /// <param name="vector3"></param>
        /// <param name="matrix"></param>
        /// <param name="vector3_2"></param>
        public void Interpoladate(Vector3 position, Matrix orientation, Vector3 velocity, float interpFactor)
        {
            if (interpFactor > 1.0f)
                interpFactor = 1.0f;
            if (interpFactor < 0)
                interpFactor = 0;
            //MoveTo(position, orientation);
            //SetVelocity(velocity);
            Vector3 intPosition = Position + (position - Position) * interpFactor;
            Vector3 intvelocity = Velocity + (velocity - Velocity) * interpFactor;
            if(float.IsNaN(intPosition.X) || 
                float.IsNaN(intvelocity.X))
            {
                return;
            }
            MoveTo(intPosition, orientation);
            Velocity=intvelocity;
            hasNotDoneFirstInterpoladation = false;
            //SetVelocity(intvelocity);            
        }

        public virtual void SetNominalInput()
        {
            
        }
        public bool hasAttributeChanged = false;
        public virtual void GetObjectAttributes(out bool[] bv, out int[] iv, out float[] fv)
        {
            bv = null;
            iv = null;
            fv = null;
        }
        public virtual void SetObjectAttributes(bool[] bv, int[] iv, float[] fv, bool mine)
        {
        }

        #endregion 

        #region Config
        public virtual GenEntityConfigTypes.EntityConfig LoadConfig(string file)
        {
            config = EntityConfigHelper.Load(file);
            return config;
        }
        public void SetAngularVelocity(Vector3 av)
        {
            foreach (EntityPart p in partList.Values)
            {
                p.body.AngularVelocity = av;
            }
        }
        internal void ApplyConfig(EntityConfig ec, SortedList<string, Model> models)
        {
            config = ec;
            ApplyConfig(ec.Parts, models);
        }

        internal void ApplyConfig(Part[] parts, SortedList<string, Model> models)
        {
            int id = 0;
            
            AddParts(id, parts, models);
        }

        #endregion
    }
}
