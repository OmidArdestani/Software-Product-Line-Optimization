using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MyPLAOptimization
{
    interface FileConvertor
    {
        PLArchitecture ReadFile(string filePath);
        void ExportFile(string filePath, List<PLAComponent> components);
    }
    class XMIConvertor : FileConvertor
    {
        /// <summary>
        /// Read XMI model file.
        /// </summary>
        /// <param name="filePath">The file address</param>
        /// <returns></returns>
        public PLArchitecture ReadFile(string filePath)
        {
            var xmlStr = File.ReadAllText(filePath);

            xmlStr = xmlStr.Replace("UML:", "UML");
            // getting parts
            var str = XElement.Parse(xmlStr);

            var xmiComponenets = str.Element("XMI.content").Element("UMLModel").Element("UMLNamespace.ownedElement").Elements("UMLModel").
                Where(x => x.Attribute("xmi.id").Value == "Component_View").SingleOrDefault().
                Element("UMLNamespace.ownedElement").Elements("UMLComponent");

            var xmiInterfaces = str.Element("XMI.content").Element("UMLModel").Element("UMLNamespace.ownedElement").Elements("UMLModel").
                Where(x => x.Attribute("xmi.id").Value == "Logical_View").SingleOrDefault().
                Element("UMLNamespace.ownedElement").Elements("UMLInterface");

            var relationships_realization = str.Element("XMI.content").Element("UMLModel").Element("UMLNamespace.ownedElement").Elements("UMLModel").
                Where(x => x.Attribute("xmi.id").Value == "Logical_View").SingleOrDefault().
                Element("UMLNamespace.ownedElement").Elements("UMLAbstraction");

            var relationships_dependencie = str.Element("XMI.content").Element("UMLModel").Element("UMLNamespace.ownedElement").Elements("UMLModel").
                Where(x => x.Attribute("xmi.id").Value == "Logical_View").SingleOrDefault().
                Element("UMLNamespace.ownedElement").Elements("UMLDependency");
            // create Model
            // fetch components
            List<PLAComponent> componentList = new List<PLAComponent> { };
            foreach (var item in xmiComponenets)
            {
                PLAComponent cmp = new PLAComponent()
                {
                    Id = item.Attribute("xmi.id").Value,
                    Name = item.Attribute("name").Value,
                    DependedInterfaces = new List<PLAInterface> { },
                    Interfaces = new List<PLAInterface> { }
                };
                componentList.Add(cmp);
            }
            // fetch interfaces and operators
            List<PLAInterface> interfaceList = new List<PLAInterface> { };
            foreach (var item in xmiInterfaces)
            {
                PLAInterface intf = new PLAInterface()
                {
                    Id = item.Attribute("xmi.id").Value,
                    Name = item.Attribute("name").Value
                };
                intf.Operators = new List<PLAOperator> { };
                var xmiOperators = item.Element("UMLClassifier.feature").Elements("UMLOperation");
                foreach (var xmiOperator in xmiOperators)
                {
                    PLAOperator opr = new PLAOperator()
                    {
                        Id = item.Attribute("xmi.id").Value,
                        Name = item.Attribute("name").Value
                    };
                    intf.Operators.Add(opr);
                }
                interfaceList.Add(intf);
            }
            // create relationships
            // realization
            foreach (var realizationItem in relationships_realization)
            {
                string realizeFrom = realizationItem.Attribute("supplier").Value;
                string realizeTo = realizationItem.Attribute("client").Value;
                PLAInterface intf = interfaceList.Where(x => x.Id == realizeFrom).SingleOrDefault();
                PLAComponent comp = componentList.Where(x => x.Id == realizeTo).SingleOrDefault();
                comp.Interfaces.Add(intf);
            }
            // dependecies
            foreach (var dependecieItem in relationships_dependencie)
            {
                string realizeFrom = dependecieItem.Attribute("supplier").Value;
                string realizeTo = dependecieItem.Attribute("client").Value;
                PLAInterface intf = interfaceList.Where(x => x.Id == realizeFrom).SingleOrDefault();
                PLAComponent comp = componentList.Where(x => x.Id == realizeTo).SingleOrDefault();
                comp.DependedInterfaces.Add(intf);
            }

            return new PLArchitecture(componentList);
        }

        public void ExportFile(string filePath, List<PLAComponent> components)
        {

        }
    }

    class XMLConvertor : FileConvertor
    {
        /// <summary>
        /// Read XML model file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public PLArchitecture ReadFile(string filePath)
        {
            var xmlStr = File.ReadAllText(filePath);
            var str = XElement.Parse(xmlStr);
            // getting parts

            var xmlComponenets = str.Element("Models").Elements("Model").
                Where(x => x.Attribute("Name").Value == "Component View").SingleOrDefault().
                Element("ModelChildren").Elements("Component");

            var xmlInterfaces = str.Element("Models").Elements("Model").
                Where(x => x.Attribute("Name").Value == "Logical View").SingleOrDefault().
                Element("ModelChildren").Elements("Class");

            var relationships_realization = str.Element("Models").Elements("ModelRelationshipContainer").
                Where(x => x.Attribute("Name").Value == "relationships").SingleOrDefault().
                Element("ModelChildren").Elements("ModelRelationshipContainer").Where(x=>x.Attribute("Name").Value == "Realization").SingleOrDefault().
                Element("ModelChildren").Elements("Realization");

            var relationships_dependencie = str.Element("Models").Elements("ModelRelationshipContainer").
                Where(x => x.Attribute("Name").Value == "relationships").SingleOrDefault().
                Element("ModelChildren").Elements("ModelRelationshipContainer").Where(x => x.Attribute("Name").Value == "Dependency").SingleOrDefault().
                Element("ModelChildren").Elements("Dependency");
            // create Model
            // fetch components
            List<PLAComponent> componentList = new List<PLAComponent> { };
            foreach (var item in xmlComponenets)
            {
                PLAComponent cmp = new PLAComponent()
                {
                    Id = item.Attribute("Id").Value,
                    Name = item.Attribute("Name").Value,
                    DependedInterfaces=new List<PLAInterface> { },
                    Interfaces=new List<PLAInterface> { }
                };
                componentList.Add(cmp);
            }
            // fetch interfaces and operators
            List<PLAInterface> interfaceList = new List<PLAInterface> { };
            foreach (var item in xmlInterfaces)
            {
                PLAInterface intf = new PLAInterface()
                {
                    Id = item.Attribute("Id").Value,
                    Name = item.Attribute("Name").Value
                };
                intf.Operators = new List<PLAOperator> { };
                var xmlOperators = item.Element("ModelChildren").Elements("Operation");
                foreach (var xmlOperator in xmlOperators)
                {
                    PLAOperator opr = new PLAOperator()
                    {
                        Id = item.Attribute("Id").Value,
                        Name = item.Attribute("Name").Value,
                    };
                    intf.Operators.Add(opr);
                }
                interfaceList.Add(intf);
            }
            // create relationships
            // realization
            foreach (var realizationItem in relationships_realization)
            {
                string realizeFrom = realizationItem.Attribute("From").Value;
                string realizeTo = realizationItem.Attribute("To").Value;
                PLAInterface intf = interfaceList.Where(x => x.Id == realizeFrom).SingleOrDefault();
                PLAComponent comp = componentList.Where(x => x.Id == realizeTo).SingleOrDefault();
                comp.Interfaces.Add(intf);
            }
            // dependecies
            foreach (var dependecieItem in relationships_dependencie)
            {
                string realizeFrom = dependecieItem.Attribute("From").Value;
                string realizeTo = dependecieItem.Attribute("To").Value;
                PLAComponent comp = componentList.Where(x => x.Id == realizeFrom).SingleOrDefault();
                PLAInterface intf = interfaceList.Where(x => x.Id == realizeTo).SingleOrDefault();
                comp.DependedInterfaces.Add(intf);
            }
            return new PLArchitecture(componentList);
        }

        public void ExportFile(string filePath, List<PLAComponent> components)
        {

        }
    }
}
