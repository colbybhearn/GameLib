using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace GameHelper.Utility
{
    public class XmlValidator
    {
        string xmlConfigFilePath;
        List<string> xmlSchemaContent;
        private StringBuilder fValidationResult = new StringBuilder();
        private MemoryStream xmlMStream = null;
        public string ValidationResult
        {
            get
            {
                return fValidationResult.ToString();
            }
        }

        public XmlValidator(Stream xmlImportStream, List<string> schemaContent)
        {
            xmlMStream = new MemoryStream();
            StreamWriter sw = new StreamWriter(xmlMStream);
            StreamReader sr = new StreamReader(xmlImportStream);
            sw.Write(sr.ReadToEnd());
            sw.Flush();
            sr.Close();
            xmlMStream.Position = 0;
            xmlSchemaContent = schemaContent;
        }

        public XmlValidator(string xmlFilePath, List<string> schemaContent)
        {
            xmlConfigFilePath = xmlFilePath;
            xmlSchemaContent = schemaContent;
        }

        private XmlSchema GetSchemaFromFilePath(string xmlSchemaFilePath)
        {
            string schemacontent = string.Empty;
            FileStream schemafs = File.OpenRead(xmlSchemaFilePath);
            StreamReader schemasr = new StreamReader(schemafs);
            schemacontent = schemasr.ReadToEnd();
            schemafs.Close();
            schemafs = File.OpenRead(xmlSchemaFilePath);
            schemasr = new StreamReader(schemafs);
            return GetSchemaFromString(schemacontent);
        }

        /// <summary>
        /// Creates an XmlSchema from the string content of an .xsd file
        /// </summary>
        /// <param name="schemacontent"></param>
        /// <returns></returns>
        private XmlSchema GetSchemaFromString(string schemacontent)
        {
            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);
            sw.Write(schemacontent);
            sw.Flush();
            ms.Position = 0;
            StreamReader sr = new StreamReader(ms);
            Debug.WriteLine(schemacontent);
            XmlSchema schema = XmlSchema.Read(sr.BaseStream, rdrSettings_ValidationEventHandlerSchema);
            sr.Close();
            return schema;
        }

        /// <summary>
        /// Validates an xml file at a path according to an xsd file (in a string)
        /// Any and all validation errors will accumulate in ValidationResult
        /// </summary>
        public bool isValid()
        {
            FileStream fsconfig = null;
            // create xml reader
            XmlReaderSettings rdrSettings = new XmlReaderSettings();
            rdrSettings.ValidationFlags = rdrSettings.ValidationFlags | XmlSchemaValidationFlags.ReportValidationWarnings;
            // add all validation schemas to the reader
            foreach (string schemaContent in xmlSchemaContent)
                rdrSettings.Schemas.Add(GetSchemaFromString(schemaContent));
            // use schema validation
            rdrSettings.ValidationType = ValidationType.Schema;
            // hook into validation error handler
            rdrSettings.ValidationEventHandler += rdrSettings_ValidationEventHandler;
            XmlReader config = null;
            try
            {
                // create an xml reader with the validation settings
                if (xmlMStream != null)
                    config = XmlReader.Create(xmlMStream, rdrSettings);
                else
                {                    
                    fsconfig = File.OpenRead(xmlConfigFilePath);
                    config = XmlReader.Create(fsconfig, rdrSettings);
                }
            }
            catch (Exception E)
            {
                fValidationResult.AppendLine(E.Message);
            }

            if (config == null)
            {
                if (fsconfig != null)
                    fsconfig.Close();
                
                return false;
            }

            try
            {
                // read the configuration file, watching purely for validation error events.
                while (config.Read()) { }
            }
            catch (Exception E)
            {
                fValidationResult.AppendLine(E.Message);
            }

            if (config != null)
                    config.Close();
                    
            if (fsconfig != null)
            {
                fsconfig.Dispose();
            }

            // validation messages will accumulate in fValidationResult
            if (fValidationResult.ToString().Length < 1)
                return true;
            else
                return false;
        }

        /// <summary>
        /// There is a problem with the xsd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void rdrSettings_ValidationEventHandlerSchema(object sender, System.Xml.Schema.ValidationEventArgs e)
        {
            fValidationResult.AppendLine(e.Message);
        }

        /// <summary>
        /// there is a problem with the xml 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void rdrSettings_ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Warning)
            {
                fValidationResult.AppendLine(e.Message);
            }
            else if (e.Severity == XmlSeverityType.Error)
            {
                fValidationResult.AppendLine(e.Message);
            }
        }

        public object GetConfigurationInfo(Type root)
        {
            FileStream fs;
            if (xmlMStream != null)
            {
                xmlMStream.Position = 0;
                XmlSerializer s = new XmlSerializer(root);
                object o = s.Deserialize(xmlMStream);                
                return o;
            }
            else
            {
                fs = new FileStream(xmlConfigFilePath, FileMode.Open);
                XmlSerializer s = new XmlSerializer(root);
                object o = s.Deserialize(fs);
                fs.Close();
                return o;

            }
        }

    }
}


/*
 * 
Regenerate the class types "C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\bin\NETFX 4.0 Tools\xsd" "D:\Programming\GameLib\GameHelper\Objects\EntityConfig.xsd" /classes /language:CS /namespace:GenEntityConfigTypes /outputdir:"D:\Programming\GameLib\GameHelper\Objects"
 * 
 * 

 * 
 */

/* How to generate the CS files from the XSD files


To generate all types into a single file (the last xsd specified), use this...
D:
cd "D:\Project Source\GenWatch3\Development\ConfigHelper\XML Schemas"
"C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\xsd" ".\GetLteReaderSchema.xsd" ".\GetLte2SqlSchema.xsd" ".\GetSnmpMasterAgentSchema.xsd" /classes /language:CS /namespace:XmlSchemaTypesAll /outputdir:"D:\Project Source\GenWatch3\Development\ConfigHelper\Generated Classes"

 * */
