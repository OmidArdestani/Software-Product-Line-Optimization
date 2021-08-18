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
    interface IFileConvertor
    {
        int GetComponentCount();
        int GetInterfaceCount();
        int GetOperatorCount();
        PLArchitecture ReadFile(string filePath);
        void ExportFile(string filePath, List<PLAComponent> components);
    }
    class XMIConvertor : IFileConvertor
    {
        private int ComponentCount = 0;
        private int InterfaceCount = 0;
        private int OperatorCount = 0;
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
            ComponentCount = componentList.Count;
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
                        Id = xmiOperator.Attribute("xmi.id").Value,
                        Name = xmiOperator.Attribute("name").Value
                    };
                    intf.Operators.Add(opr);
                }
                interfaceList.Add(intf);
            }
            InterfaceCount = interfaceList.Count;
            OperatorCount = interfaceList.Select(i => i.Operators.Count).Sum();
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

        public int GetComponentCount()
        {
            return ComponentCount;
        }

        public int GetInterfaceCount()
        {
            return InterfaceCount;
        }

        public int GetOperatorCount()
        {
            return OperatorCount;
        }
    }

    class XMLConvertor : IFileConvertor
    {
        private int ComponentCount = 0;
        private int InterfaceCount = 0;
        private int OperatorCount = 0;
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
                Element("ModelChildren").Elements("ModelRelationshipContainer").Where(x => x.Attribute("Name").Value == "Realization").SingleOrDefault().
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
                    DependedInterfaces = new List<PLAInterface> { },
                    Interfaces = new List<PLAInterface> { }
                };
                componentList.Add(cmp);
            }
            ComponentCount = componentList.Count;
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
                        Id = xmlOperator.Attribute("Id").Value,
                        Name = xmlOperator.Attribute("Name").Value,
                    };
                    intf.Operators.Add(opr);
                }
                interfaceList.Add(intf);
            }
            InterfaceCount = interfaceList.Count;
            OperatorCount = interfaceList.Select(i => i.Operators.Count).Sum();
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
            //<Project Author="OmidHome" CommentTableSortAscending="false" CommentTableSortColumn="Date Time" DocumentationType="html" ExportedFromDifferentName="false" ExporterVersion="16.1.1" Name="untitled" TextualAnalysisHighlightOptionCaseSensitive="false" UmlVersion="2.x" Xml_structure="simple">
            List<XElement> componentElements = new List<XElement> { };
            List<XElement> interfaceElements = new List<XElement> { };
            List<XElement> relationship_Realization = new List<XElement> { };
            List<XElement> relationship_Dependency = new List<XElement> { };
            for (int c = 0; c < components.Count(); c++)
            {
                PLAComponent component = components[c];
                XElement componentElement = new XElement("Component");
                componentElement.SetAttributeValue("Id", "Component_" + component.Id);
                componentElement.SetAttributeValue("Name", "Component" + c);
                componentElements.Add(componentElement);
                for (int i = 0; i < component.Interfaces.Count; i++)
                {
                    PLAInterface interface_ = component.Interfaces[i];
                    XElement Realization = new XElement("Realization");
                    if (interfaceElements.Where(i_ => i_.Attribute("Id").ToString() == interface_.Id).Count() == 0)
                    {
                        XElement classElement = new XElement("Class");
                        classElement.SetAttributeValue("Id", "Interface_" + interface_.Id);
                        classElement.SetAttributeValue("Name", "Class" + i);
                        List<XElement> operatorElements = new List<XElement> { };
                        for (int o = 0; o < interface_.Operators.Count; o++)
                        {
                            PLAOperator operator_ = component.Interfaces[i].Operators[o];
                            XElement operatorElement = new XElement("Operation");
                            operatorElement.SetAttributeValue("Id", operator_.Id);
                            operatorElement.SetAttributeValue("Name", "Operator" + o);
                            operatorElements.Add(operatorElement);
                        }
                        XElement ModelChildren = new XElement("ModelChildren");
                        ModelChildren.Add(operatorElements);
                        classElement.Add(ModelChildren);
                        interfaceElements.Add(classElement);
                        //
                        Realization.SetAttributeValue("From", "Interface_" + interface_.Id);
                        Realization.SetAttributeValue("To", "Component_" + component.Id);
                        relationship_Realization.Add(Realization);
                    }
                }
                for (int d = 0; d < component.DependedInterfaces.Count; d++)
                {
                    PLAInterface componentDepedency = component.DependedInterfaces[d];
                    XElement Dependency = new XElement("Dependency");
                    Dependency.SetAttributeValue("From", "Component_" + component.Id);
                    Dependency.SetAttributeValue("To", "Interface_" + componentDepedency.Id);
                    relationship_Dependency.Add(Dependency);
                }
            }
            /*
            var relationships_realization = str.Element("Models").Elements("ModelRelationshipContainer").
                Where(x => x.Attribute("Name").Value == "relationships").SingleOrDefault().
                Element("ModelChildren").Elements("ModelRelationshipContainer").Where(x => x.Attribute("Name").Value == "Realization").SingleOrDefault().
                Element("ModelChildren").Elements("Realization");

            var relationships_dependencie = str.Element("Models").Elements("ModelRelationshipContainer").
                Where(x => x.Attribute("Name").Value == "relationships").SingleOrDefault().
                Element("ModelChildren").Elements("ModelRelationshipContainer").Where(x => x.Attribute("Name").Value == "Dependency").SingleOrDefault().
                Element("ModelChildren").Elements("Dependency");*/

            //
            XElement ModelChildrenClass = new XElement("ModelChildren");
            XElement ModelChildrenComponent = new XElement("ModelChildren");
            XElement ModelChildrenModelClass = new XElement("Model");
            XElement ModelChildrenModelComponent = new XElement("Model");
            XElement ModelChildrenModelModels = new XElement("Models");
            XElement ModelChildrenModelProject = new XElement("Project");
            //
            ModelChildrenModelComponent.SetAttributeValue("Name", "Component View");
            ModelChildrenModelComponent.Add(ModelChildrenComponent);
            ModelChildrenModelClass.SetAttributeValue("Name", "Logical View");
            ModelChildrenModelClass.Add(ModelChildrenClass);
            ModelChildrenComponent.Add(componentElements);
            ModelChildrenClass.Add(interfaceElements);
            ModelChildrenModelModels.Add(ModelChildrenModelComponent);
            ModelChildrenModelModels.Add(ModelChildrenModelClass);
            //
            XElement ModelChildrenModelModelRelationshipDep = new XElement("ModelRelationshipContainer");
            XElement ModelChildrenModelModelRelationshipReal = new XElement("ModelRelationshipContainer");
            XElement ModelChildrenModelModelRelationshipCont = new XElement("ModelRelationshipContainer");
            ModelChildrenModelModelRelationshipCont.SetAttributeValue("Name", "relationships");
            XElement ModelChildrenDep = new XElement("ModelChildren");
            XElement ModelChildrenRel = new XElement("ModelChildren");
            XElement ModelChildrenCont = new XElement("ModelChildren");
            ModelChildrenDep.Add(relationship_Dependency);
            ModelChildrenRel.Add(relationship_Realization);
            ModelChildrenModelModelRelationshipDep.Add(ModelChildrenDep);
            ModelChildrenModelModelRelationshipDep.SetAttributeValue("Name", "Dependency");
            ModelChildrenModelModelRelationshipReal.Add(ModelChildrenRel);
            ModelChildrenModelModelRelationshipReal.SetAttributeValue("Name", "Realization");
            ModelChildrenCont.Add(ModelChildrenModelModelRelationshipDep);
            ModelChildrenCont.Add(ModelChildrenModelModelRelationshipReal);
            ModelChildrenModelModelRelationshipCont.Add(ModelChildrenCont);
            ModelChildrenModelModels.Add(ModelChildrenModelModelRelationshipCont);
            //
            ModelChildrenModelProject.Add(ModelChildrenModelModels);
            XDocument doc = new XDocument(ModelChildrenModelProject);
            doc.Save(filePath);
        }

        public int GetComponentCount()
        {
            return ComponentCount;
        }

        public int GetInterfaceCount()
        {
            return InterfaceCount;
        }

        public int GetOperatorCount()
        {
            return OperatorCount;
        }
    }
}
