using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using GameHelper.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameHelper.Objects
{
    public class AssetConfig
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

        public AssetConfig()
        {
        }

        public AssetConfig(string name)
        {
            this.name = name;
        }
        //public AssetConfig config;

        //public AssetConfig(string name, Vector3 scale, Model m)
        //{
        //    Name = name;
        //    model = m;
        //    Scale = scale;
        //    Color = Color.Gray;
        //}

        /// <summary>
        /// this method should be overloaded in the specific asset config class for each asset and load the setting in the file into memory
        /// </summary>
        /// <param name="file"></param>
        public virtual bool LoadFromFile(string file)
        {
            Scale = new Vector3(1.2f, 1.3f, 1.4f);
            string text = File.ReadAllText(file);
            XmlDocument d = new XmlDocument();
            d.LoadXml(text);

            XmlNode n = XmlUtil.GetFirstElementWithTagName(d, "AssetName");
            if (n == null)
                return false;
            AssetName = n.InnerText;

            n= XmlUtil.GetFirstElementWithTagName(d, "AssetTypeName");
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
