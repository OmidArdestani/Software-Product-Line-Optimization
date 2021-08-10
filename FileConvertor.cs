using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MyPLAOptimization
{
    class XMIConvertor
    {
        public static List<PLAComponent> ReadXMIFile(string filePath)
        {
            return new List<PLAComponent>();
        }

        public static void ExportXMIFile(string filePath,List<PLAComponent> components)
        {

        }
    }

    class XMLonvertor
    {
        public static List<PLAComponent>ReadXMLFIle(string filePath)
        {
            var xmlStr = File.ReadAllText(filePath);
            var str = XElement.Parse(xmlStr);

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

        public static void ExportXMLFile(string filePath, List<PLAComponent> components)
        {

        }
    }
}
