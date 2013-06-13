using System;
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
     * 
     */
    public class AssetManager
    {
        /*
         * (Object -> Client) Gobject should have an owner field for the client. 
         * (Client -> Object) AssetManager needs a sortedList<id, List<int>>
         * (Object owned?) Gobject owner == -1;
         * 
         */
        
        public SortedList<string, Asset> Assets = new SortedList<string, Asset>();
        private SortedList<int, Gobject> gameObjects;
        private SortedList<int, Gobject> objectsToAdd;
        private List<int> objectsToDelete;
        private SortedList<int, List<int>> ObjectIdsByOwningClient = new SortedList<int, List<int>>();
        private SortedList<int, int> OwningClientsByObjectId = new SortedList<int, int>();
        private string assetConfigDirectory;
        private string processedModelDirectory;

        private SortedList<string, AssetType> AssetTypesByName = new SortedList<string, AssetType>();
        private SortedList<int, AssetType> AssetTypesById = new SortedList<int, AssetType>();
        private SortedList<int, List<Asset>> AssetsByType = new SortedList<int, List<Asset>>();
        SortedList<string, AssetConfig> assetConfigs = new SortedList<string, AssetConfig>();

        public AssetManager(ref SortedList<int, Gobject> gObjects, ref SortedList<int, Gobject> nObjects, ref List<int> dObjects)
            : this(ref gObjects, ref nObjects, ref dObjects, System.Windows.Forms.Application.StartupPath)
        {
        }

        public AssetManager(ref SortedList<int, Gobject> gObjects, ref SortedList<int, Gobject> nObjects, ref List<int> dObjects, string Root)
        {
            gameObjects = gObjects;
            objectsToAdd = nObjects;
            objectsToDelete = dObjects;
            assetConfigDirectory = Root + @"\Assets";
            processedModelDirectory = Root + @"\Content";
            
        }

        private void LoadCompiledAssets(ContentManager cm)
        {
            foreach (AssetConfig ac in assetConfigs.Values)
            {
                try
                {
                    Model m = cm.Load<Model>(ac.AssetName);
                    Asset a = new Asset(ac.AssetName, new Vector3(1, 1, 1), m);                    
                    AddAsset(a, ac);
                }
                catch (Exception E)
                {
                    System.Diagnostics.Debug.WriteLine(E.StackTrace);
                }
            }
        }

        private void AddAsset(Asset a, AssetConfig ac)
        {
            if (Assets.ContainsKey(a.Name))
                return;
            Assets.Add(a.Name, a);

            if (AssetTypesByName.ContainsKey(ac.AssetTypeName))
            {
                AssetType at = AssetTypesByName[ac.AssetTypeName];
                if (!AssetsByType.ContainsKey(at.Id))
                {
                    AssetsByType.Add(at.Id, new List<Asset>());
                }
                List<Asset> assets = AssetsByType[at.Id];
                assets.Add(a);
            }

        }

        /// <summary>
        /// Adds an asset
        /// </summary>
        /// <param name="a"></param>
        public void AddAsset(Asset a)
        {
            if (Assets.ContainsKey(a.Name))
                return;
            Assets.Add(a.Name, a);
        }

        public void LoadAssetConfigFiles()
        {
            string[] files = Directory.GetFiles(assetConfigDirectory, "*.xml");
            foreach (string file in files)
            {
                LoadAssetConfigFile(file);
                
            }
        }

        public Gobject GetAssetOfType(Enum e)
        {
            if (!AssetTypesByName.ContainsKey(e.ToString()))
                return null;
            return AssetTypesByName[e.ToString()].GetNewGobject();
        }

        private void LoadAssetConfigFile(string file)
        {
            AssetConfig ac = new AssetConfig();
            ac.LoadFromFile(file);
            assetConfigs.Add(ac.AssetName, ac);

            
        }

        private bool CompileAssets()
        {
            if (assetConfigs.Count == 0)
                return true;

            ContentBuilder contentBuilder = new ContentBuilder();
            foreach (AssetConfig ac in assetConfigs.Values)
            {
                if (NeedsCompiling(ac))
                    contentBuilder.Add(ac.fbxModelFilepath, ac.AssetName, "FbxImporter", "ModelProcessor");
                else
                    Trace.WriteLine("Skipping compilation of asset \"" + ac.AssetName+"\"");
            }

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

        private bool NeedsCompiling(AssetConfig ac)
        {            
            if (!File.Exists(ac.fbxModelFilepath))
                return false;

            string compiledFile = Path.Combine(processedModelDirectory, ac.AssetName + ".xnb");
            if (!File.Exists(compiledFile))
                return true;

            DateTime dtSource = File.GetLastWriteTimeUtc(ac.fbxModelFilepath);
            DateTime dtCompiled = File.GetLastWriteTimeUtc(compiledFile);
            return dtSource > dtCompiled;
        }

        /// <summary>
        /// Adds an asset type
        /// </summary>
        /// <param name="name"></param>
        /// <param name="CreateCallback"></param>
        /// <param name="scale"></param>
        public void AddAssetType(Enum e, GetGobjectDelegate CreateCallback, Vector3 scale)
        {
            if (AssetTypesByName.ContainsKey(e.ToString()))
                return;

            AssetType at = new AssetType(e, null, CreateCallback);
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
        public void AddAssetType(Enum e, GetGobjectDelegate CreateCallback, float scale)
        {
            AddAssetType(e, CreateCallback, new Vector3(scale, scale, scale));
        }

        /// <summary>
        /// Adds an asset with a default scale of 1
        /// </summary>
        /// <param name="name"></param>
        /// <param name="CreateCallback"></param>
        public void AddAssetType(Enum e, GetGobjectDelegate CreateCallback)
        {
            AddAssetType(e, CreateCallback, 1.0f);
        }
        /// <summary>
        /// returns an instance of the specified asset type
        /// Assumes no client owns the object
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public Gobject GetNewInstance(Enum e)
        {
            return GetNewInstanceOfType(e, -1);
        }
        
        /// <summary>
        /// returns an instance of the specified asset type
        /// </summary>
        /// <param name="e"></param>
        /// <param name="owningClientId"></param>
        /// <returns></returns>
        public Gobject GetNewInstanceOfType(Enum e, int owningClientId)
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
        private Gobject GetNewInstanceOfType(int id, int owningClientId)
        {
            if (!AssetTypesById.ContainsKey(id))
            {
                Trace.WriteLine("Aborting instantiation of asset Type. Unkown to AssetManager: " + id);
                return null;
            }

            AssetType at  = AssetTypesById[id];
            Asset asset = AssetsByType[id][0];
            Gobject go = at.GetNewGobject();
            go.OwningClientId = owningClientId;

            go.assetName = asset.Name;
            go.Model = asset.model;

            go.aType = AssetTypesById[id];
            go.ID = GetAvailableObjectId();

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

        public Model GetModel(string id)
        {
            Asset a = Assets[id];
            if (a == null)
                return null;
            return a.model;
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
