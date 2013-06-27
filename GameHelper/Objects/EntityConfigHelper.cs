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
    public class EntityConfigHelper
    {
        public EntityConfigHelper()
        {
        }

        public static GenEntityConfigTypes.EntityConfig Load(string file)
        {
            XmlValidator validator = new XmlValidator(file, new List<string> { Properties.Resources.EntityConfigSchema });

            if(!validator.isValid())
            {
                MessageBox.Show(validator.ValidationResult);
                return null;
            }

            object info = validator.GetConfigurationInfo(typeof(GenEntityConfigTypes.EntityConfig));
            GenEntityConfigTypes.EntityConfig ec = null;
            if (info is GenEntityConfigTypes.EntityConfig)
            {
                ec = info as GenEntityConfigTypes.EntityConfig;
                return ec;
            }
            return null;
        }


    }
}