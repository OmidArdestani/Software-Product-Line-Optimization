using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MyPLAOptimization
{
    interface FileConvertor
    {
        List<PLAComponent> ReadFile(string filePath);
        void ExportFile(string filePath, List<PLAComponent> components);
    }
    class XMIConvertor: FileConvertor
    {
        /// <summary>
        /// Read XMI model file.
        /// </summary>
        /// <param name="filePath">The file address</param>
        /// <returns></returns>
        public List<PLAComponent> ReadFile(string filePath)
        {
            var xmlStr = File.ReadAllText(filePath);

            xmlStr = xmlStr.Replace("UML:", "UML");
            // getting parts
            var str = XElement.Parse(xmlStr);
            var baseRoot = str.Elements("XMI.content").
                Select(x => x.Elements("UMLModel")).
                Select(x => x.Elements("UMLNamespace.ownedElement")).
                Select(x => x.Elements("UMLModel")).
                Select(x => x.Elements("UMLNamespace.ownedElement")).ToList();
            var componenets = baseRoot.
                Select(x => x.Elements("UMLComponent")).ToList();

            var classes = baseRoot.
                Select(x => x.Elements("UMLPackage")).
                Select(x => x.Elements("UMLNamespace.ownedElement")).
                Select(x => x.Elements("UMLClass")).ToList();

            var relationships_realization = baseRoot.
                Select(x => x.Elements("UMLAbstraction")).ToList();

            var relationships_dependencie = baseRoot.
                Select(x => x.Elements("UMLDependency")).ToList();
            // create Model
            return new List<PLAComponent>();
        }

        public void ExportFile(string filePath,List<PLAComponent> components)
        {

        }
    }

    class XMLonvertor: FileConvertor
    {
        /// <summary>
        /// Read XML model file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public List<PLAComponent>ReadFile(string filePath)
        {
            var xmlStr = File.ReadAllText(filePath);
            var str = XElement.Parse(xmlStr);
            // getting parts
            var componenets = str.Elements("Models").
                Select(x => x.Elements("Model")).
                Select(x => x.Elements("ModelChildren")).
                Select(x => x.Elements("Component")).ToList();

            var classes = str.Elements("Models").
                Select(x => x.Elements("Model")).
                Select(x => x.Elements("ModelChildren")).
                Select(x => x.Elements("Class")).ToList();

            var relationships_realization = str.Elements("Models").
                Select(x => x.Elements("ModelRelationshipContainer")).
                Select(x => x.Elements("ModelChildren")).
                Select(x => x.Elements("ModelRelationshipContainer")).
                Select(x => x.Elements("ModelChildren")).
                Select(x => x.Elements("Realization")).ToList();

            var relationships_dependencie = str.Elements("Models").
                Select(x => x.Elements("ModelRelationshipContainer")).
                Select(x => x.Elements("ModelChildren")).
                Select(x => x.Elements("ModelRelationshipContainer")).
                Select(x => x.Elements("ModelChildren")).
                Select(x => x.Elements("Dependency")).ToList();
            // create model
            return new List<PLAComponent>();
        }

        public void ExportFile(string filePath, List<PLAComponent> components)
        {

        }
    }
}
