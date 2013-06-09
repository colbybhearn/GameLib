using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace GameHelper.Utility
{
    public class XmlUtil
    {
        public static XmlNode GetFirstElementWithTagName(XmlDocument doc, string name)
        {
            XmlNodeList list = doc.GetElementsByTagName(name);
            if (list == null)
                return null;
            if (list.Count == 0)
                return null;
            return list[0];
        }
    }
}
