using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace GameHelper.Input
{
    // DataContract serialization allows IDictionary implementers to be serialized! WooHoo
    [DataContract]
    public class ButtonMapCollection
    {
        [DataMember]
        public string Game;
        [DataMember]
        public SortedList<string, ButtonMap> buttonMaps = new SortedList<string, ButtonMap>(); // this can't be serialized with XMLSerializer, so DataContract serialization has been used.

        private string FilePath
        {
            get
            {
                return GetSettingsPath(Game);
            }
        }

        public ButtonMapCollection()
        {
        }

        public ButtonMapCollection(ButtonMapCollection other)
        {
            Game = other.Game;
            buttonMaps = new SortedList<string, ButtonMap>(other.buttonMaps);
        }

        public ButtonMapCollection(SortedList<string, ButtonMap> maps)
        {
            buttonMaps = maps;
        }

        /// <summary>
        /// Adds a keymap to the collection
        /// </summary>
        /// <param name="keyMap"></param>
        public void AddMap(ButtonMap keyMap)
        {
            string alias = keyMap.Alias;
            if (buttonMaps.ContainsKey(alias))
                throw new ArgumentException("A Button Map group by the name " + alias + " already exists. Use a different name.");
            buttonMaps.Add(alias, keyMap);
        }

        public void DisableAllButtonMaps()
        {
            foreach (ButtonMap map in buttonMaps.Values)
                map.Enabled = false;
        }

        public void EnableButtonMap(string alias)
        {
            if (buttonMaps.ContainsKey(alias))
                buttonMaps[alias].Enabled = true;
        }

        public void DisableButtonMap(string alias)
        {
            if (buttonMaps.ContainsKey(alias))
                buttonMaps[alias].Enabled = false;
        }

        public static void Save(ButtonMapCollection bmc)
        {
            DataContractSerializer x = new DataContractSerializer(typeof(ButtonMapCollection));
            StreamWriter stm = null;
            StringWriter sw = new StringWriter();
            XmlTextWriter tw = new XmlTextWriter(sw);
            try
            {
                string filepath = bmc.FilePath;                
                if (!Directory.Exists(filepath))
                {
                    string dirpath = Path.GetDirectoryName(filepath);
                    Directory.CreateDirectory(dirpath);
                }
                stm = new StreamWriter(filepath);
                tw.Formatting = Formatting.Indented; // Make it human readable!
                x.WriteObject(tw, bmc);
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

        public static ButtonMapCollection Load(string game, ButtonMapCollection defaultButtonMap)
        {
            DataContractSerializer x = new DataContractSerializer(typeof(ButtonMapCollection));
            ButtonMapCollection finalCollection = new ButtonMapCollection(defaultButtonMap);
            StreamReader stm = null;
            try
            {
                string filepath = ButtonMapCollection.GetSettingsPath(game);
                stm = new StreamReader(filepath);

                ButtonMapCollection savedCollection = (ButtonMapCollection)x.ReadObject(stm.BaseStream);

                foreach (ButtonMap finalkm in finalCollection.buttonMaps.Values)
                    // if tha keymap by this name exists on disk
                    if (savedCollection.buttonMaps.ContainsKey(finalkm.Alias))
                    {
                        ButtonMap savedkm = savedCollection.buttonMaps[finalkm.Alias];
                        // load those preferences, overriding the defaults
                        finalkm.LoadOverrides(savedkm);
                    }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error in LoadKeyMap " + e.StackTrace);
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
