﻿using GameHelper.Objects;
using JigLibX.Collision;
using JigLibX.Geometry;
using JigLibX.Physics;
using JigLibX.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace GameHelper.Physics
{
    public class Terrain : Entity
    {
        VertexPositionNormalTexture[] verts;
        int[] indices;
        Texture2D texture;
        TriangleMesh mesh;
        int numVertsX = 0;
        int numVertsZ = 0;
        int numVerts = 0;
        int numTriX = 0;
        int numTriZ = 0;
        int numTris = 0;        
        List<TriangleVertexIndices> meshIndices = new List<TriangleVertexIndices>();
        List<Vector3> meshVertices = new List<Vector3>();// overlapping verts
        List<Vector3> heightVertices = new List<Vector3>();// overlapping verts

        Vector3[] heightNodes;

        public Terrain( Vector3 posCenter,Vector3 size, int cellsX, int cellsZ, GraphicsDevice g, Texture2D tex) : base()
        {
            numVertsX = cellsX + 1;
            numVertsZ = cellsZ + 1;
            numVerts = numVertsX * numVertsZ;
            numTriX = cellsX * 2;
            numTriZ = cellsZ;
            numTris = numTriX * numTriZ;
            verts = new VertexPositionNormalTexture[numVerts];
            int numIndices = numTris * 3;
            indices = new int[numIndices];
            float cellSizeX = (float)size.X / cellsX;
            float cellSizeZ = (float)size.Z / cellsZ;
            texture = tex;

            Random r = new Random();
            Random r2 = new Random(DateTime.Now.Millisecond);
            double targetHeight = 0;
            double currentHeight = 0;
            // Fill in the vertices
            int count = 0;
            
            // distribute height nodes across the map.
            // for each vert, calculate the height by summing (node height / distance)
            int numHeightNodes = 10;
            heightNodes = new Vector3[numHeightNodes];
            for (int i = 0; i < numHeightNodes; i++)
            {
                heightNodes[i] = new Vector3((float)((-size.X / 2) + (r.NextDouble() * size.X)),
                                                (float)(r.NextDouble() * 0),
                                                (float)((-size.Z / 2) + (r.NextDouble() * size.Z)));
            }

            bool stretchTexture=false;
            float textureTileCount=10000;

            float worldZPosition = - (size.Z / 2);
            for (int z = 0; z < numVertsZ; z++)
            {
                float worldXPosition = - (size.X / 2);
                for (int x = 0; x < numVertsX; x++)
                {

                    if (count % numVertsX == 0)
                        targetHeight = r2.NextDouble();
                    
                    //currentHeight += (targetHeight - currentHeight) * .009f;

                    float height = GetNodeBasedHeight(worldXPosition, worldZPosition);
                    height = 0;
                    verts[count].Position = new Vector3(worldXPosition, height, worldZPosition);
                    verts[count].Normal = Vector3.Zero;

                    if (stretchTexture)
                    {
                        verts[count].TextureCoordinate.X = (float)x / (numVertsX - 1);
                        verts[count].TextureCoordinate.Y = (float)z / (numVertsZ - 1);
                    }
                    else
                    {
                        verts[count].TextureCoordinate.X = ((float)x / (numVertsX - 1))*textureTileCount;
                        verts[count].TextureCoordinate.Y = ((float)z / (numVertsZ - 1))*textureTileCount;
                    }

                    count++;

                    // Advance in x
                    worldXPosition += cellSizeX;
                }

                currentHeight = 0;
                // Advance in z
                worldZPosition += cellSizeZ;
            }

            int index = 0;
            int startVertex = 0;
            for (int cellZ = 0; cellZ < cellsZ; cellZ++)
            {
                for (int cellX = 0; cellX < cellsX; cellX++)
                {
                    indices[index] = startVertex + 0;
                    indices[index + 1] = startVertex + 1;
                    indices[index + 2] = startVertex + numVertsX;
                    SetNormalOfTriangleAtIndices(indices[index], indices[index + 1], indices[index + 2]);
                    meshIndices.Add(new TriangleVertexIndices(index, index + 1, index + 2));
                    meshVertices.Add(verts[indices[index]].Position);
                    meshVertices.Add(verts[indices[index+1]].Position);
                    meshVertices.Add(verts[indices[index+2]].Position);
                    index += 3;

                    indices[index] = startVertex + 1;
                    indices[index + 1] = startVertex + numVertsX + 1;
                    indices[index + 2] = startVertex + numVertsX;
                    SetNormalOfTriangleAtIndices(indices[index], indices[index + 1], indices[index + 2]);
                    meshIndices.Add(new TriangleVertexIndices(index, index + 1, index + 2));
                    meshVertices.Add(verts[indices[index]].Position);
                    meshVertices.Add(verts[indices[index + 1]].Position);
                    meshVertices.Add(verts[indices[index + 2]].Position);
                    index += 3;

                    startVertex++;
                }
                startVertex++;
            }

            try
            {
                Effect = new BasicEffect(g);
                mesh = new TriangleMesh();
                //mesh.CreateMesh(meshVertices, meshIndices, 2, cellSizeX);
            }
            catch (Exception E)
            {
                System.Diagnostics.Debug.WriteLine(E.StackTrace);
            }

            //this.body = new Body();
            //Skin = new CollisionSkin(body);
            //body.CollisionSkin = Skin;
            //body.ExternalData = this;
            float heightf = 0;
            try
            {
            Array2D field = new Array2D(numVertsX, numVertsZ);

            int i = 0;
            for (int c = 0; c < verts.Length; c++)
            {
                int x = c / numVertsX;
                int z = c % numVertsX;
                
                if (i >= verts.Length)
                    i = (i % verts.Length)+1;
                heightf = verts[i].Position.Y + posCenter.Y; // works
                //heightf = verts[i].Position.Y;
                i += numVertsX;
                
                field.SetAt(x,z, heightf);
            }

               // Body.MoveTo(Position, Matrix.Identity);

                Heightmap hm = new Heightmap(field, 
                                                    (-size.X / 2) / (cellsX+0) + cellSizeX/2,
                                                    (-size.Z / 2) / (cellsZ + 0) + cellSizeZ/2, 
                                                    size.X / (cellsX+0), 
                                                    size.Z / (cellsZ+0));
                //Skin.AddPrimitive(hm, new MaterialProperties(0.7f, 0.7f, 0.6f));
                //Skin.AddPrimitive(GetMesh(), new MaterialProperties(0.7f, 0.7f, 0.6f));
                //VertexPositionColor[] wireFrame = Skin.GetLocalSkinWireframe(); // 1200 across before Z changes to from -7.5/-7.35 to -7.35/-7.2
                
                //PhysicsSystem.CurrentPhysicsSystem.CollisionSystem.AddCollisionSkin(Skin);
                CommonInit(posCenter, new Vector3(0,0,0), null, false, 0);
            }
            catch(Exception E)
            {
                System.Diagnostics.Debug.WriteLine(E.StackTrace);
            }

        }

        private float GetNodeBasedHeight(float x, float z)
        {
            float h = 0;
            float dist=0;
            foreach (Vector3 v in heightNodes)
            {
                dist = (float)Math.Sqrt(((v.X - x) * (v.X - x)) + (float)((v.Z - z) * (v.Z - z)));
                float power = dist * v.Y / 10;
                h+= power;
            }

            return h;
        }
        

        private Triangle GetTriangleOfFirstVert(int startVertex)
        {
            int a = startVertex + 0;
            int b = startVertex + 1;
            int c = startVertex + numVertsX;
            Vector3 vA = verts[a].Position;
            Vector3 vB = verts[b].Position;
            Vector3 vC = verts[c].Position;
            return new Triangle(vA, vC, vB);
        }
        private void SetNormalOfTriangleAtIndices(int a, int b, int c)
        {
            Vector3 vA = verts[a].Position;
            Vector3 vB = verts[b].Position;
            Vector3 vC = verts[c].Position;
            Triangle t = new Triangle(vA, vC, vB);
            Vector3 n = t.Normal;
            verts[a].Normal += n;
            verts[b].Normal += n;
            verts[c].Normal += n;
            verts[a].Normal.Normalize();
            verts[b].Normal.Normalize();
            verts[c].Normal.Normalize();
        }
        

        public void Draw(GraphicsDevice g, Matrix view, Matrix projection)
        {
            if (verts == null)
                return;

            try
            {
                this.Effect.View = view;
                Effect.Texture = texture;
                Effect.Projection = projection;
                Effect.EnableDefaultLighting();
                Effect.AmbientLightColor = Color.Gray.ToVector3();
                Effect.DiffuseColor = Color.Green.ToVector3();                
                Effect.TextureEnabled = true;

                VertexPositionNormalTexture[] worldly = TransformVPNTs(verts); // because the verts are relative to the body, but here we need the real deal for drawing in the world
                foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    g.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(Microsoft.Xna.Framework.Graphics.PrimitiveType.TriangleList, worldly, 0, verts.Length, indices, 0, indices.Length / 3);
                }
            }
            catch (Exception E)
            {
                System.Diagnostics.Debug.WriteLine(E.StackTrace);
            }
        }

        private VertexPositionNormalTexture[] TransformVPNTs(VertexPositionNormalTexture[] vpnt)
        {
            VertexPositionNormalTexture[] tverts = new VertexPositionNormalTexture[vpnt.Length];
            for (int i = 0; i < vpnt.Length; i++)
            {
                tverts[i] = vpnt[i];
                tverts[i].Position = Vector3.Transform(vpnt[i].Position,
                                            root.body.Orientation * Matrix.CreateTranslation(root.body.Position));
            }
            return tverts;
        }

        /*
        public override void DrawWireframe(GraphicsDevice Graphics, Matrix View, Matrix Projection)
        {
            try
            {
                VertexPositionColor[] wireFrame = Skin.GetLocalSkinWireframe(); 
                Body.TransformWireframe(wireFrame); // because the wireframed primitives are relative to the body, but we need them in the world
                if (Effect == null)
                {
                    Effect = new BasicEffect(Graphics);
                    Effect.VertexColorEnabled = true;
                }
                Effect.View = View;
                Effect.Projection = Projection;

                Effect.TextureEnabled = false;
                Effect.LightingEnabled = false;

                foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Graphics.DrawUserPrimitives<VertexPositionColor>(
                        Microsoft.Xna.Framework.Graphics.PrimitiveType.LineStrip,
                        wireFrame, 0, wireFrame.Length - 1);

                    foreach (VertexPositionNormalTexture vpc in verts)
                    {
                        VertexPositionColor[] normal = new VertexPositionColor[2];
                        normal[0].Position = vpc.Position;
                        normal[1].Position = vpc.Position+vpc.Normal;
                        Body.TransformWireframe(normal);

                        Graphics.DrawUserPrimitives<VertexPositionColor>(
                            Microsoft.Xna.Framework.Graphics.PrimitiveType.LineStrip,
                            normal, 0, normal.Length - 1);
                    }
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
        }*/

        public TriangleMesh GetMesh()
        {
            return mesh;
        }

    }
}
