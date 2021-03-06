﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using GameHelper.Physics;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Xml;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Content;
using GenEntityConfigTypes;

namespace GameHelper.Objects
{

    /*
     * Vision:
     * game assets have config files
     * one file for each possible asset
     * The game operates on asset types
     *  Example types: Fighters, Cruisers, Feathers
     * 
     * At runtime, assets of the appropriate type are used
     *  Example assets: FighterA, FighterB, CuiserA, Feather1, Feather2
     *  
     * the asset manager should be prepped with the types to accept.
     * It should then try loading everything in a specified directory
     * Files successfully parsed can be processed.
     * 
     * File processing means validating asset types against those allowed
     * Store those assets validated.
     * Store a name, a model filename, the asset type, movement parameters, scale     
     * 
     * Extended asset properties should be suppored:
     *  Health
     *  Ammo
     *  Armor
     *  MovementEnergy
     *  OffenseEnergy
     *  DefenseEnergy
     *  
     * Peroperties go into a namevaluecollection / sortedlist for extraction
     * A class shall exist for an assetType, such that those properties used by the game are correctly assigned when loading an asset of that type.
     * 
     * Asset manager can put the information from the files into the an asset config array
     * Each asset Type needs its initialization class
     * That class should inherit a base class for all asset Types
     * The Gobject shall 
     * Asset manager should allow a unit with those specs to be created.
     * When an asset is requested from the asset manager, it should create a gobject with the correct config from the config array
     * All gobjects subclasses need the base asset parameter class as a field.
     * 
     * Round 2
     * 
     * Manager is prepped with types to expect
     * Load the files from the place.
     * Update types with their config values from the files.
     * 
     * 
     * 
     */
    public class EntityManager
    {
        /*
         * (Object -> Client) Gobject should have an owner field for the client. 
         * (Client -> Object) AssetManager needs a sortedList<id, List<int>>
         * (Object owned?) Gobject owner == -1;
         */
        
        private SortedList<int, Entity> gameObjects;
        private SortedList<int, Entity> objectsToAdd;
        private List<int> objectsToDelete;
        private SortedList<int, List<int>> ObjectIdsByOwningClient = new SortedList<int, List<int>>();
        private SortedList<int, int> OwningClientsByObjectId = new SortedList<int, int>();
        private string assetConfigDirectory;
        private string processedModelDirectory;
        private SortedList<string, SortedList<string, Model>> models = new SortedList<string, SortedList<string, Model>>();

        private SortedList<string, EntityType> AssetTypesByName = new SortedList<string, EntityType>();
        private SortedList<int, EntityType> AssetTypesById = new SortedList<int, EntityType>();
        private SortedList<int, List<EntityConfig>> ConfigByType = new SortedList<int, List<EntityConfig>>();

        public EntityManager(ref SortedList<int, Entity> gObjects, ref SortedList<int, Entity> nObjects, ref List<int> dObjects)
            : this(ref gObjects, ref nObjects, ref dObjects, System.Windows.Forms.Application.StartupPath)
        {
        }

        public EntityManager(ref SortedList<int, Entity> gObjects, ref SortedList<int, Entity> nObjects, ref List<int> dObjects, string Root)
        {
            gameObjects = gObjects;
            objectsToAdd = nObjects;
            objectsToDelete = dObjects;
            assetConfigDirectory = Root + @"\Assets";
            processedModelDirectory = Root + @"\Content";
            
        }

        private void LoadCompiledAssets(ContentManager cm)
        {
            foreach (EntityType at in AssetTypesByName.Values)
            {
                foreach (EntityConfig ac in at.PrototypeAssets.Values)
                {
                    LoadParts(ac.Name, cm, ac.Parts);
                }
            }
        }

        private void LoadParts(string entityName, ContentManager cm, GenEntityConfigTypes.Part[] parts)
        {
            if (parts == null)
                return;
            foreach (Part part in parts)
            {                
                LoadPart(entityName, cm, part);

                LoadParts(entityName, cm, part.Parts);
            }
        }

        private void LoadPart(string entityName, ContentManager cm, Part part)
        {
            if (!models.ContainsKey(entityName))
                models.Add(entityName, new SortedList<string,Model>());
            
            SortedList<string, Model> mods = models[entityName];

            string cleanedName = CleanPath(part.Model.relFilepath);
            if(mods.ContainsKey(cleanedName))
                return;
            try
            {
                Model m =cm.Load<Model>(cleanedName);
                mods.Add(cleanedName, m);
            }
            catch (Exception E)
            {
                System.Diagnostics.Debug.WriteLine(E.StackTrace);
            }
        }

        public void LoadAssetConfigFiles()
        {
            string[] files = Directory.GetFiles(assetConfigDirectory, "*.xml");
            foreach (string file in files)
                LoadAssetConfigFile(file);
        }

        private void LoadAssetConfigFile(string file)
        {
            // make an instance of the generic loader to determine which specific loader to use
            EntityConfig ec = EntityConfigHelper.Load(file);
            //ec.LoadFromFile(file);
            if (!AssetTypesByName.ContainsKey(ec.Type))
                return;

            int id = AssetTypesByName[ec.Type].Id;
            if (!ConfigByType.ContainsKey(id))
                ConfigByType.Add(id, new List<EntityConfig>());

            List<EntityConfig> configs = ConfigByType[id];
            configs.Add(ec);    
            
            // run it through the more specific loader
            EntityType at = AssetTypesByName[ec.Type];
            at.LoadConfigFromFile(file); 
            //assetConfigs.Add(ac.AssetName, ac);
        }


        #region Compilation

        private void CheckPart(Part[] partlist)
        {
            if (partlist == null)
                return;
            foreach (Part part in partlist)
            {
                if (NeedsCompiling(part.Model))
                    contentBuilder.Add(part.Model.relFilepath, CleanPath(part.Model.relFilepath), "FbxImporter", "ModelProcessor");
                else
                    Trace.WriteLine("Skipping compilation of asset \"" + part.Model.relFilepath + "\"");

                CheckPart(part.Parts);

            }
        }

        ContentBuilder contentBuilder = new ContentBuilder();
        private bool CompileAssets()
        {
            
            foreach (EntityType at in AssetTypesByName.Values)
                foreach (EntityConfig ac in at.PrototypeAssets.Values)
//<<<<<<< HEAD
                    CheckPart(ac.Parts);                    
//=======
//                {
//                    if (NeedsCompiling(ac))
//                        contentBuilder.Add(ac.fbxModelFilepath, ac.AssetName, "FbxImporter", "ModelProcessor");
//                    else
//                        Debug.WriteLine("Skipping compilation of asset \"" + ac.AssetName + "\"");
//                }
//            }
//>>>>>>> 2dbeb8655797c9cd196612178f8ddca744fa620c

            if (!contentBuilder.hasContent)
                return true;

            // kick-off the build
            string error = contentBuilder.Build();
            
            if (!String.IsNullOrEmpty(error))
            {
                MessageBox.Show(error);
                return false;
            }
            
            // move the output from the temp directory to the destination directory
            string tempPath = contentBuilder.OutputDirectory;
            if (!Directory.Exists(processedModelDirectory))
                Directory.CreateDirectory(processedModelDirectory);

            string[] files = Directory.GetFiles(tempPath, "*.xnb");
            
            foreach (string file in files)
                System.IO.File.Copy(file, Path.Combine(processedModelDirectory, Path.GetFileName(file)), true);
            
            MessageBox.Show("Files compiled successfully.");
            return true;
        }
        private bool NeedsCompiling(ecModel ac)
        {
            if (!File.Exists(ac.relFilepath))
                return false;

            string compiledFile = Path.Combine(processedModelDirectory, CleanPath(ac.relFilepath) + ".xnb");
            if (!File.Exists(compiledFile))
                return true;

            DateTime dtSource = File.GetLastWriteTimeUtc(CleanPath(ac.relFilepath));
            DateTime dtCompiled = File.GetLastWriteTimeUtc(compiledFile);
            return dtSource > dtCompiled;
        }
        #endregion

        private string CleanPath(string path)
        {
            return path.Replace("\\",string.Empty).Replace(".",string.Empty);
        }

        /// <summary>
        /// Adds an asset type
        /// </summary>
        /// <param name="name"></param>
        /// <param name="CreateCallback"></param>
        /// <param name="scale"></param>
        public void AddAssetType(Enum e, Vector3 scale, Type typeOfGobject)
        {
            if (AssetTypesByName.ContainsKey(e.ToString()))
                return;

            EntityType at = new EntityType(e, null, typeOfGobject);
            //AssetType at = new AssetType(e, null, CreateCallback);
            int id = (int)Convert.ChangeType(e, e.GetTypeCode());
            AssetTypesById.Add(id, at);
            AssetTypesByName.Add(e.ToString(), at);
        }

        /// <summary>
        /// Adds an asset with a scale of X = Y = Z = scale
        /// </summary>
        /// <param name="name"></param>
        /// <param name="CreateCallback"></param>
        /// <param name="scale"></param>
        public void AddAssetType(Enum e, float scale, Type typeOfGobject)
        {
            AddAssetType(e, new Vector3(scale, scale, scale), typeOfGobject);
        }

        /// <summary>
        /// Adds an asset with a default scale of 1
        /// </summary>
        /// <param name="name"></param>
        /// <param name="CreateCallback"></param>
        public void AddAssetType(Enum e, Type typeOfGobject)
        {
            AddAssetType(e, 1.0f, typeOfGobject);
        }

        /// <summary>
        /// returns an instance of the specified asset type
        /// Assumes no client owns the object
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public Entity GetNewInstance(Enum e)
        {
            return GetNewInstanceOfType(e, -1);
        }
        
        /// <summary>
        /// returns an instance of the specified asset type
        /// </summary>
        /// <param name="e"></param>
        /// <param name="owningClientId"></param>
        /// <returns></returns>
        public Entity GetNewInstanceOfType(Enum e, int owningClientId)
        {
            int id = (int)Convert.ChangeType(e, e.GetTypeCode());
            return GetNewInstanceOfType(id, owningClientId);
        }

        /// <summary>
        /// returns an instance of the specified asset type
        /// </summary>
        /// <param name="e"></param>
        /// <param name="owningClientId"></param>
        /// <returns></returns>
        private Entity GetNewInstanceOfType(int id, int owningClientId)
        {
            if (!AssetTypesById.ContainsKey(id))
            {
                Debug.WriteLine("Aborting instantiation of asset Type. Unkown to AssetManager: " + id);
                return null;
            }

            EntityType at  = AssetTypesById[id];

            if (!ConfigByType.ContainsKey(at.Id))
            {
                Trace.WriteLine("Unable to instantiate asset Type. Config not loaded. " + id);
                return null;
            }

            EntityConfig ec = ConfigByType[at.Id][0];

            Entity go = at.GetNewGobject(ec, models[ec.Name]);
            go.OwningClientId = owningClientId;
            go.ID = GetAvailableObjectId();

            // we have to already have the setting ready, here.
            if (!ObjectIdsByOwningClient.ContainsKey(owningClientId))
                ObjectIdsByOwningClient.Add(owningClientId, new List<int>());

            List<int> ownedIds = ObjectIdsByOwningClient[owningClientId];
            ownedIds.Add(go.ID);
            if(!OwningClientsByObjectId.ContainsKey(go.ID))
                OwningClientsByObjectId.Add(go.ID, owningClientId);

            // we should centralize all object addition here. (but still return the Gobject)
            // Maybe the baseGame should call PostIntegrate and PreIntegrate methods overridden in the specific game so that Specific-Game can call AssetManager.GetNewInstance() at the appropriate time.
            return go;
        }

        public void RemoveInstance(int objectId)
        {
            if (isObjectOwnedByAnyClient(objectId))
                RemoveObjectIdFromOwnerList(objectId);

            if (OwningClientsByObjectId.ContainsKey(objectId))
                OwningClientsByObjectId.Remove(objectId);

            // we should centralize all object removal here
            // Maybe the baseGame should call PostIntegrate and PreIntegrate methods overridden in the specific game so that Specific-Game can call AssetManager.RemoveInstance() at the appropriate time.
        }

        /// <summary>
        /// Selects an unused object ID
        /// </summary>
        /// <returns></returns>
        public int GetAvailableObjectId()
        {
            int id = 1;
            bool found = true;
            // locks are expensive, I think.
            // We probably don't want to lock inside a loop.
            lock (gameObjects)
            {
                lock (objectsToAdd)
                {
                    lock (objectsToDelete)
                    {
                        while (found)
                        {
                            if (isObjectIdInUse_Unprotected(id))
                                id++;
                            else
                                found = false;
                        }
                    }
                }
            }
            return id;
        }

        /// <summary>
        /// Does NOT lock on purpose. Must only be called from a method that locks gameObjects, ObjectsToAdd, and then ObjectsToDelete
        /// Allows for good performance when being called iteratively. 
        /// Allows for centralized logic to be called from methods that do the required locking 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool isObjectIdInUse_Unprotected(int id)
        {
            return gameObjects.ContainsKey(id) || objectsToAdd.ContainsKey(id) || objectsToDelete.Contains(id); ;
        }

        /// <summary>
        /// Checks to see if an id is in use already
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool isObjectIdInUse(int id)
        {
            lock (gameObjects)
            {
                lock (objectsToAdd)
                {
                    lock (objectsToDelete)
                    {
                        return isObjectIdInUse_Unprotected(id);
                    }
                }
            }
        }

        public bool isObjectOwnedByAnyClient(int objectId)
        {
            if (!gameObjects.ContainsKey(objectId))
                return false;

            if (gameObjects[objectId].OwningClientId == -1)
                return false;
            return true;
        }

        public bool isObjectOwnedByClient(int objectId, int clientId)
        {
            if(!OwningClientsByObjectId.ContainsKey(objectId))
                return false;
            if (OwningClientsByObjectId[objectId] == clientId)
                return true;
            return false;
        }

        public List<int> GetObjectsOwnedByClient(int clientId)
        {
            if(!ObjectIdsByOwningClient.ContainsKey(clientId))
                return new List<int>();
            return ObjectIdsByOwningClient[clientId];
        }

        private void RemoveObjectIdFromOwnerList(int id)
        {
            foreach (List<int> idList in ObjectIdsByOwningClient.Values)
                if (idList.Contains(id))
                    idList.Remove(id);
        }

        public void LoadAssets(ContentManager cm)
        {
            if (!Directory.Exists(assetConfigDirectory))
                Directory.CreateDirectory(assetConfigDirectory);

            LoadAssetConfigFiles();
            CompileAssets();
            LoadCompiledAssets(cm);
        }
    }
}
