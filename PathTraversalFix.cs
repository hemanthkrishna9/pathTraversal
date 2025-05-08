
namespace MI.Colos.Modules.XmlDataMapper.Common.Tools
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Security;
    using System.Xml;
    using System.Xml.Schema;
    using MI.Colos.Modules.Library;

    /// <summary>
    /// Validator class to validate an xml file against an xml schema
    /// </summary>
    public class XmlValidator
    {
        /// <summary>
        /// A list of validation errors
        /// </summary>
        private Collection<XmlValidationResult> validationErrors = new Collection<XmlValidationResult>();

        /// <summary>
        /// Reader for the xml file
        /// </summary>
        private XmlReader reader;

        /// <summary>
        /// Validates the XML file.
        /// </summary>
        /// <param name="xmlFilePath">The XML file path.</param>
        /// <returns>Errors if any present otherwise null.</returns>
        public static string ValidateXmlFile(string xmlFilePath)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(File.ReadAllText(xmlFilePath));
            }
            catch (Exception e)
            {
                if (Exceptions.IsCritical(e))
                {
                    throw;
                }

                return e.Message;
            }

            return null;
        }

        /// <summary>
        /// Validates the XML.
        /// </summary>
        /// <param name="xmlFile">The XML file.</param>
        /// <param name="schemaFile">The schema file.</param>
        /// <returns>A list of validation errors</returns>
        public Collection<XmlValidationResult> ValidateXmlFileAgainstSchema(string xmlFile, string schemaFile)
        {

            if (!IsPathSafe(xmlFile))
            {
                this.validationErrors.Add(new XmlValidationResult(XmlSeverityType.Error, "Unsafe XML file path.", 0, 0));
            }

            if (!IsPathSafe(schemaFile))
            {
                this.validationErrors.Add(new XmlValidationResult(XmlSeverityType.Error, "Unsafe schema file path.", 0, 0));
            }

            if (!File.Exists(xmlFile))
            {
                this.validationErrors.Add(
                    new XmlValidationResult(XmlSeverityType.Error, "Xml file doesn't exists", 0, 0));
            }

            if (!File.Exists(schemaFile))
            {
                this.validationErrors.Add(
                    new XmlValidationResult(XmlSeverityType.Error, "Xml Schema file doesn't exists", 0, 0));
            }

            if (this.validationErrors.Count > 0)
            {
                return this.validationErrors;
            }

            return this.ValidateXmlAgainstSchema(File.ReadAllText(xmlFile), File.ReadAllText(schemaFile));
        }

        /// <summary>
        /// Validates the XML.
        /// </summary>
        /// <param name="xmlFile">The XML file.</param>
        /// <param name="schemaFile">The schema file.</param>
        /// <returns>A list of validation errors</returns>
        //public Collection<XmlValidationResult> ValidateXmlFileAgainstSchema(string xmlFile, string schemaFile)
        //{
        //    string xmlRootPath = Path.GetPathRoot(xmlFile);
        //    string schemaRootPath = Path.GetPathRoot(schemaFile);

        //    if (!IsPathSafe(xmlFile, xmlRootPath))
        //    {
        //        this.validationErrors.Add(new XmlValidationResult(XmlSeverityType.Error, "Unsafe XML file path.", 0, 0));
        //    }

        //    if (!IsPathSafe(schemaFile, schemaRootPath))
        //    {
        //        this.validationErrors.Add(new XmlValidationResult(XmlSeverityType.Error, "Unsafe schema file path.", 0, 0));
        //    }

        //    if (!File.Exists(xmlFile))
        //    {
        //        this.validationErrors.Add(
        //            new XmlValidationResult(XmlSeverityType.Error, "Xml file doesn't exists", 0, 0));
        //    }

        //    if (!File.Exists(schemaFile))
        //    {
        //        this.validationErrors.Add(
        //            new XmlValidationResult(XmlSeverityType.Error, "Xml Schema file doesn't exists", 0, 0));
        //    }

        //    if (this.validationErrors.Count > 0)
        //    {
        //        return this.validationErrors;
        //    }

        //    return this.ValidateXmlAgainstSchema(File.ReadAllText(xmlFile), File.ReadAllText(schemaFile));
        //}

        /// <summary>
        /// Validates the inputPath.
        /// </summary>
        /// <param name="inputPath">The XML file.</param>
        /// <param name="baseDirectory">The schema file.</param>
        /// <returns>true or false</returns>
        private static bool IsPathSafe(string inputPath)
        {
            if (string.IsNullOrWhiteSpace(inputPath))
                return false;

            try
            {
                string decodedPath = Uri.UnescapeDataString(inputPath);

                string[] parts = decodedPath.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                                            StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    if (part == "..")
                        return false;
                }

                Path.GetFullPath(decodedPath);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }


        /// <summary>
        /// Validates the inputPath.
        /// </summary>
        /// <param name="inputPath">The XML file.</param>
        /// <param name="baseDirectory">The schema file.</param>
        /// <returns>true or false</returns>
        //private static bool IsPathSafe(string inputPath)
        //{
        //    if (string.IsNullOrWhiteSpace(inputPath))
        //        return false;

        //    string rootPath = Path.GetPathRoot(inputPath);
        //    try
        //    {
        //        string fullPath = Path.GetFullPath(inputPath);
        //        string basePath = Path.GetFullPath(rootPath);

        //        return fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase);
        //    }
        //    catch (ArgumentException)
        //    {
        //        return false;
        //    }
        //}


        /// <summary>
        /// Validates the XML for a given xml text and schema.
        /// </summary>
        /// <param name="xml">The XML text.</param>
        /// <param name="schema">The schema text.</param>
        /// <returns>A list of validation errors</returns>
        public Collection<XmlValidationResult> ValidateXmlAgainstSchema(string xml, string schema)
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.LoadXml(xml);

                XmlSchemaSet xmlSchema = GetSchema(schema);
                this.ParseAndValidate(xmlDoc, xmlSchema);
            }
            catch (XmlException xmlEx)
            {
                this.AddXmlExceptions(xmlEx);
            }
            catch (Exception ex)
            {
                if (Exceptions.IsCritical(ex))
                {
                    throw;
                }

                this.AddXmlExceptions(new XmlException(ex.Message));
            }

            return this.validationErrors;
        }

        /// <summary>
        /// Gets the xml schema.
        /// </summary>
        /// <param name="text">The schema text.</param>
        /// <returns>The xmlSchema object</returns>
        private XmlSchemaSet GetSchema(string text)
        {
            XmlSchemaSet schemaSet = new XmlSchemaSet();

            try
            {
                using (XmlReader xmlReader = XmlReader.Create(new StringReader(text)))
                {
                    schemaSet.Add(null, xmlReader);
                }
            }
            catch (XmlSchemaException e)
            {
                AddXmlSchemaExceptions(e);
            }

            return schemaSet;
        }

        /// <summary>
        /// Parses and validates the xml document.
        /// </summary>
        /// <param name="xml">The XML document.</param>
        /// <param name="schema">The XML schema.</param>
        private void ParseAndValidate(XmlDocument xml, XmlSchemaSet schema)
        {
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.DtdProcessing = DtdProcessing.Parse;
                settings.ValidationType = ValidationType.Schema;
                settings.ValidationFlags =
                    XmlSchemaValidationFlags.ReportValidationWarnings;
                settings.CloseInput = true;
                settings.ConformanceLevel = ConformanceLevel.Auto;
                settings.Schemas = schema;
                settings.ValidationEventHandler +=
                    new ValidationEventHandler(this.ValidationCallback);

                XmlNodeReader nodeReader = new XmlNodeReader(xml);
                this.reader = XmlReader.Create(nodeReader, settings);

                while (this.reader.Read())
                {
                }
            }
            catch (XmlException xmlEx)
            {
                this.AddXmlExceptions(xmlEx);
            }
        }

        /// <summary>
        /// Adds the XML schema exceptions to the collection.
        /// </summary>
        /// <param name="e">The XML schema ex.</param>
        private void AddXmlSchemaExceptions(XmlSchemaException e)
        {
            XmlValidationResult result = new XmlValidationResult
            {
                SeverityType = XmlSeverityType.Error,
                Error = e.Message,
                Row = e.LineNumber,
                Column = e.LinePosition
            };

            validationErrors.Add(result);
        }

        /// <summary>
        /// Adds the XML exceptions to the collection.
        /// </summary>
        /// <param name="xmlEx">The XML ex.</param>
        private void AddXmlExceptions(XmlException xmlEx)
        {
            XmlValidationResult result = new XmlValidationResult();
            result.SeverityType = XmlSeverityType.Error;
            result.Error = xmlEx.Message;
            result.Row = xmlEx.LineNumber;
            result.Column = xmlEx.LinePosition;

            this.validationErrors.Add(result);
        }

        /// <summary>
        /// The call back method of the xml validation.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Xml.Schema.ValidationEventArgs"/> instance containing the event data.</param>
        private void ValidationCallback(object sender, ValidationEventArgs e)
        {
            XmlValidationResult result = new XmlValidationResult();
            result.SeverityType = e.Severity;
            result.Error = e.Message;
            result.Row = e.Exception.LineNumber;
            result.Column = e.Exception.LinePosition;

            this.validationErrors.Add(result);
        }
    }
}
