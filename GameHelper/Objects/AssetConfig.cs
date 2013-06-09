using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using GameHelper.Utility;

namespace GameHelper.Objects
{
    public class AssetConfig
    {
        //name, a model filename, the asset type, movement parameters, scale     
        public string AssetName;
        public string AssetTypeName;
        public string fbxModelFilepath;
        public SortedList<string, string> Params = new SortedList<string, string>();

        public bool LoadFromFile(string file)
        {
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
