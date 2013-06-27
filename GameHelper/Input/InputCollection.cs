using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace GameHelper.Input
{
    // DataContract serialization allows IDictionary implementers to be serialized! WooHoo
    [DataContract]
    public class InputCollection
    {
        [DataMember]
        public string Game;
        [DataMember]
        public SortedList<string, InputMap> inputMaps = new SortedList<string, InputMap>(); // this can't be serialized with XMLSerializer, so DataContract serialization has been used.

        private string FilePath
        {
            get
            {
                return GetSettingsPath(Game);
            }
        }

        public InputCollection()
        {
        }

        public InputCollection(InputCollection other)
        {
            Game = other.Game;
            inputMaps = new SortedList<string, InputMap>(other.inputMaps);
        }

        public InputCollection(SortedList<string, InputMap> maps)
        {
            inputMaps = maps;
        }

        /// <summary>
        /// Adds a keymap to the collection
        /// </summary>
        /// <param name="keyMap"></param>
        public void AddMap(InputMap keyMap)
        {
            string alias = keyMap.Alias;
            if (inputMaps.ContainsKey(alias))
                throw new ArgumentException("A Button Map group by the name " + alias + " already exists. Use a different name.");
            inputMaps.Add(alias, keyMap);
        }

        public void SetAllMapsState(bool enable)
        {
            foreach (InputMap map in inputMaps.Values)
                map.Enabled = enable;
        }

        public void EnableMap(string alias)
        {
            if (inputMaps.ContainsKey(alias))
                inputMaps[alias].Enabled = true;
        }

        public void DisableMap(string alias)
        {
            if (inputMaps.ContainsKey(alias))
                inputMaps[alias].Enabled = false;
        }

        public static void Save(InputCollection ic)
        {
            DataContractSerializer x = new DataContractSerializer(typeof(InputCollection));
            StreamWriter stm = null;
            StringWriter sw = new StringWriter();
            XmlTextWriter tw = new XmlTextWriter(sw);
            try
            {
                string filepath = ic.FilePath;                
                if (!Directory.Exists(filepath))
                {
                    string dirpath = Path.GetDirectoryName(filepath);
                    Directory.CreateDirectory(dirpath);
                }
                stm = new StreamWriter(filepath);
                tw.Formatting = Formatting.Indented; // Make it human readable!
                x.WriteObject(tw, ic);
                tw.Flush();
                stm.Write(sw.ToString());
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error in SaveKeyMap " + e.StackTrace);
            }
            finally
            {
                if(tw!=null)
                    tw.Close();
                if (stm != null)
                    stm.Close();
            }
        }

        public static InputCollection Load(string game, InputCollection defaultButtonMap)
        {
            DataContractSerializer x = new DataContractSerializer(typeof(InputCollection));
            InputCollection finalCollection = new InputCollection(defaultButtonMap);
            StreamReader stm = null;
            try
            {
                string filepath = InputCollection.GetSettingsPath(game);
                stm = new StreamReader(filepath);

                InputCollection savedCollection = (InputCollection)x.ReadObject(stm.BaseStream);

                foreach (InputMap finalim in finalCollection.inputMaps.Values)
                    // if tha keymap by this name exists on disk
                    if (savedCollection.inputMaps.ContainsKey(finalim.Alias))
                    {
                        InputMap savedkm = savedCollection.inputMaps[finalim.Alias];
                        // load those preferences, overriding the defaults
                        finalim.Load(savedkm);
                    }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error in loading an InputMap " + e.StackTrace);
            }
            finally
            {
                if (stm != null)
                    stm.Close();
            }
            return finalCollection;
        }

        /// <summary>
        /// Centralizes the settings path for optimum maintainability.
        /// Static for flexibility.
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public static string GetSettingsPath(string game)
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\CnJ Xna Physics\\KeyBindings\\" + game + ".xml";
        }
    }
}
