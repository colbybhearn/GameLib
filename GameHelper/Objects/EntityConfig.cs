using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using GameHelper.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Forms;

namespace GameHelper.Objects
{
    public class EntityConfig
    {
        //name, a model filename, the asset type, movement parameters, scale     
        public string AssetName;
        public string AssetTypeName;
        public string fbxModelFilepath;
        public SortedList<string, string> Params = new SortedList<string, string>();
        //public string Name;

        public Vector3 Scale;
        public Color Color;
        public Model model;
        private string name;

        public EntityConfig()
        {
        }

        public EntityConfig(string name)
        {
            this.name = name;
        }


        /// <summary>
        /// this method should be overloaded in the specific asset config class for each asset and load the setting in the file into memory
        /// </summary>
        /// <param name="file"></param>
        public virtual bool LoadFromFile(string file)
        {

            XmlValidator config = new XmlValidator(file, new List<string> { Properties.Resources.EntityConfig });

            if(!config.isValid())
            {
                MessageBox.Show(config.ValidationResult);
            }

            object info = config.GetConfigurationInfo(typeof(GenEntityConfigTypes.Entity));

            if (info is GenEntityConfigTypes.Entity)
            {

            }

            Scale = new Vector3(1.2f, 1.3f, 1.4f);
            string text = File.ReadAllText(file);
            XmlDocument d = new XmlDocument();
            d.LoadXml(text);

            XmlNode n = XmlUtil.GetFirstElementWithTagName(d, "AssetName");
            if (n == null)
                return false;
            AssetName = n.InnerText;

            n = XmlUtil.GetFirstElementWithTagName(d, "AssetTypeName");
            if (n == null)
                return false;
            AssetTypeName = n.InnerText;

            n = XmlUtil.GetFirstElementWithTagName(d, "FbxModelFilepath");
            if (n == null)
                return false;
            fbxModelFilepath = n.InnerText;

            n = XmlUtil.GetFirstElementWithTagName(d, "Params");
            if (n == null)
                return false;
            //process parameters here

            return true;
        }
    }
}