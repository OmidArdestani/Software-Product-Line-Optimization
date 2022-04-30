using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(xmlStr);
            XmlNodeList xmlComponentList = xdoc.SelectNodes("//UMLComponent");
            XmlNodeList xmlInterfaceList = xdoc.SelectNodes("//UMLInterface");
            XmlNodeList xmlRealizationList = xdoc.SelectNodes("//UMLAbstraction");
            XmlNodeList xmlDependencieList = xdoc.SelectNodes("//UMLDependency");
            // create Model
            // fetch components
            List<PLAComponent> componentList = new List<PLAComponent> { };
            foreach (XmlNode item in xmlComponentList)
            {
                if (item.Attributes.GetNamedItem("xmi.id") != null)
                {
                    PLAComponent cmp = new PLAComponent()
                    {
                        Id = item.Attributes.GetNamedItem("xmi.id").Value,
                        Name = item.Attributes.GetNamedItem("name").Value,
                        DependedInterfaces = new List<PLAInterface> { },
                        Interfaces = new List<PLAInterface> { }
                    };
                    componentList.Add(cmp);
                }
            }
            ComponentCount = componentList.Count;
            // fetch interfaces and operators
            List<PLAInterface> interfaceList = new List<PLAInterface> { };
            foreach (XmlNode item in xmlInterfaceList)
            {
                if (item.Attributes.GetNamedItem("xmi.id") != null)
                {
                    PLAInterface intf = new PLAInterface()
                    {
                        Id = item.Attributes.GetNamedItem("xmi.id").Value,
                        Name = item.Attributes.GetNamedItem("name").Value
                    };
                    intf.Operations = new List<PLAOperation> { };
                    var xmiOperators = item.SelectNodes("UMLClassifier.feature/UMLOperation");
                    foreach (XmlNode xmiOperator in xmiOperators)
                    {
                        PLAOperation opr = new PLAOperation()
                        {
                            Id = xmiOperator.Attributes.GetNamedItem("xmi.id").Value,
                            Name = xmiOperator.Attributes.GetNamedItem("name").Value
                        };
                        opr.OwnerInterface = intf;
                        intf.Operations.Add(opr);
                    }
                    interfaceList.Add(intf);
                }
            }
            InterfaceCount = interfaceList.Count;
            OperatorCount = interfaceList.Select(i => i.Operations.Count).Sum();
            // create relationships
            // realization
            foreach (XmlNode realizationItem in xmlRealizationList)
            {
                bool valueFound = false;
                string realizeFrom = "";
                string realizeTo = "";
                if (realizationItem.Attributes.GetNamedItem("supplier") != null)
                {
                    if (realizationItem.Attributes.GetNamedItem("client") != null)
                    {
                        realizeFrom = realizationItem.Attributes.GetNamedItem("supplier").Value;
                        realizeTo = realizationItem.Attributes.GetNamedItem("client").Value;
                        valueFound = true;
                    }
                }
                else
                {
                    foreach (XmlNode node in realizationItem.ChildNodes)
                    {
                        if (node.Name == "UMLDependency.client")
                        {
                            realizeTo = node.ChildNodes.Item(0).Attributes.GetNamedItem("xmi.idref").Value;
                            valueFound = true;
                            continue;
                        }
                        else if (node.Name == "UMLDependency.supplier")
                        {
                            realizeFrom = node.ChildNodes.Item(0).Attributes.GetNamedItem("xmi.idref").Value;
                            continue;
                        }
                    }
                }
                if (valueFound)
                {
                    PLAInterface intf = interfaceList.Where(x => x.Id == realizeFrom).SingleOrDefault();
                    PLAComponent comp = componentList.Where(x => x.Id == realizeTo).SingleOrDefault();
                    intf.OwnerComponent = comp;
                    comp.Interfaces.Add(intf);
                }
            }
            // dependecies
            foreach (XmlNode dependecieItem in xmlDependencieList)
            {
                bool valueFound = false;
                string realizeFrom = "";
                string realizeTo = "";

                if (dependecieItem.Attributes.GetNamedItem("supplier") != null)
                {
                    if (dependecieItem.Attributes.GetNamedItem("client") != null)
                    {
                        realizeFrom = dependecieItem.Attributes.GetNamedItem("supplier").Value;
                        realizeTo = dependecieItem.Attributes.GetNamedItem("client").Value;
                        valueFound = true;
                    }
                }
                else
                {
                    foreach (XmlNode node in dependecieItem.ChildNodes)
                    {
                        if (node.Name == "UMLDependency.client")
                        {
                            realizeTo = node.ChildNodes.Item(0).Attributes.GetNamedItem("xmi.idref").Value;
                            valueFound = true;
                            continue;
                        }
                        else if (node.Name == "UMLDependency.supplier")
                        {
                            realizeFrom = node.ChildNodes.Item(0).Attributes.GetNamedItem("xmi.idref").Value;
                            continue;
                        }
                    }
                }
                if (valueFound)
                {
                    PLAInterface intf = interfaceList.Where(x => x.Id == realizeFrom).SingleOrDefault();
                    PLAComponent comp = componentList.Where(x => x.Id == realizeTo).SingleOrDefault();
                    comp.DependedInterfaces.Add(intf);
                }
            }
            var returnPLA = new PLArchitecture(componentList);
            returnPLA.InterfaceCount = InterfaceCount;
            returnPLA.OperatorCount = OperatorCount;
            returnPLA.ComponentCount = ComponentCount;
            return returnPLA;
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
            // getting parts
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(xmlStr);
            XmlNodeList xmlComponentList = xdoc.SelectNodes("//Component");
            XmlNodeList xmlInterfaceList = xdoc.SelectNodes("//Class");
            XmlNodeList xmlRealizationList = xdoc.SelectNodes("//Realization");
            XmlNodeList xmlDependencieList = xdoc.SelectNodes("//Dependency");
            // create Model
            // fetch components
            List<PLAComponent> componentList = new List<PLAComponent> { };
            foreach (XmlNode item in xmlComponentList)
            {
                if (componentList.Find(c => c.Name == item.Attributes.GetNamedItem("Name").Value) == null)
                {
                    PLAComponent cmp = new PLAComponent()
                    {
                        Id = item.Attributes.GetNamedItem("Id").Value,
                        Name = item.Attributes.GetNamedItem("Name").Value,
                        DependedInterfaces = new List<PLAInterface> { },
                        Interfaces = new List<PLAInterface> { }
                    };
                    componentList.Add(cmp);
                }
            }
            ComponentCount = componentList.Count;
            // fetch interfaces and operators
            List<PLAInterface> interfaceList = new List<PLAInterface> { };
            foreach (XmlNode item in xmlInterfaceList)
            {
                if (item.ChildNodes.Count >= 2)
                {
                    if (item.Attributes.GetNamedItem("Id") != null)
                    {
                        var xmlOperators = item.SelectNodes("ModelChildren/Operation");
                        if (xmlOperators.Count > 0)
                        {
                            PLAInterface intf = new PLAInterface()
                            {
                                Id = item.Attributes.GetNamedItem("Id").Value,
                                Name = item.Attributes.GetNamedItem("Name").Value
                            };
                            intf.Operations = new List<PLAOperation> { };
                            foreach (XmlNode xmlOperator in xmlOperators)
                            {
                                if (xmlOperator.Attributes.GetNamedItem("Id") != null)
                                {
                                    PLAOperation opr = new PLAOperation()
                                    {
                                        Id = xmlOperator.Attributes.GetNamedItem("Id").Value,
                                        Name = xmlOperator.Attributes.GetNamedItem("Name").Value,
                                    };
                                    opr.OwnerInterface = intf;
                                    intf.Operations.Add(opr);
                                }
                            }
                            interfaceList.Add(intf);
                        }
                    }
                }
            }
            InterfaceCount = interfaceList.Count;
            OperatorCount = interfaceList.Select(i => i.Operations.Count).Sum();
            // create relationships
            // realization
            foreach (XmlNode realizationItem in xmlRealizationList)
            {
                if (realizationItem.Attributes.GetNamedItem("From") != null)
                {
                    string realizeFrom = realizationItem.Attributes.GetNamedItem("From").Value;
                    string realizeTo = realizationItem.Attributes.GetNamedItem("To").Value;
                    PLAInterface intf = interfaceList.Where(x => x.Id == realizeFrom).SingleOrDefault();
                    PLAComponent comp = componentList.Where(x => x.Id == realizeTo).SingleOrDefault();
                    if (comp != null && intf != null)
                    {
                        intf.OwnerComponent = comp;
                        comp.Interfaces.Add(intf);
                    }
                }
            }
            // dependecies
            foreach (XmlNode dependecieItem in xmlDependencieList)
            {
                if (dependecieItem.Attributes.GetNamedItem("From") != null)
                {
                    string realizeFrom = dependecieItem.Attributes.GetNamedItem("From").Value;
                    string realizeTo = dependecieItem.Attributes.GetNamedItem("To").Value;
                    PLAComponent comp = componentList.Where(x => x.Id == realizeFrom).SingleOrDefault();
                    PLAInterface intf = interfaceList.Where(x => x.Id == realizeTo).SingleOrDefault();
                    if (comp != null && intf != null)
                    {
                        comp.DependedInterfaces.Add(intf);
                    }
                }
            }
            var returnPLA = new PLArchitecture(componentList);
            returnPLA.InterfaceCount = InterfaceCount;
            returnPLA.OperatorCount = OperatorCount;
            returnPLA.ComponentCount = ComponentCount;
            return returnPLA;
        }
        #region Project info for XML header
        private XElement ProjectInfo()
        {
            XElement WarningOptions = new XElement("WarningOptions");
            WarningOptions.SetAttributeValue("CreateORMClassInDefaultPackage", "true");
            //
            XElement StateCodeEngineOptions = new XElement("StateCodeEngineOptions");
            StateCodeEngineOptions.SetAttributeValue("AutoCreateInitialStateInStateDiagram", "true");
            StateCodeEngineOptions.SetAttributeValue("AutoCreateTransitionMethods", "true");
            StateCodeEngineOptions.SetAttributeValue("DefaultInitialStateLocationX", -1);
            StateCodeEngineOptions.SetAttributeValue("DefaultInitialStateLocationY", -1);
            StateCodeEngineOptions.SetAttributeValue("GenerateDebugMessage", "false");
            StateCodeEngineOptions.SetAttributeValue("GenerateSample", "true");
            StateCodeEngineOptions.SetAttributeValue("GenerateTryCatch", "true");
            StateCodeEngineOptions.SetAttributeValue("Language", "\\u0000");
            StateCodeEngineOptions.SetAttributeValue("RegenerateTransitionMethods", "false");
            StateCodeEngineOptions.SetAttributeValue("SyncTransitionMethods", "true");
            //
            XElement RequirementDiagramOptions = new XElement("RequirementDiagramOptions");
            RequirementDiagramOptions.SetAttributeValue("DefaultWrapMember", "true");
            RequirementDiagramOptions.SetAttributeValue("ShowAttributes", "\\u0001");
            RequirementDiagramOptions.SetAttributeValue("SupportHTMLAttribute", "false");
            //
            XElement ORMOptions = new XElement("ORMOptions");
            ORMOptions.SetAttributeValue("DecimalPrecision", 19);
            ORMOptions.SetAttributeValue("DecimalScale", 0);
            ORMOptions.SetAttributeValue("ExportCommentToDatabase", "true");
            ORMOptions.SetAttributeValue("FormattedSQL", "false");
            ORMOptions.SetAttributeValue("GenerateAssociationWithAttribute", "false");
            ORMOptions.SetAttributeValue("GenerateDiagramFromORMWizards", "true");
            ORMOptions.SetAttributeValue("GetterSetterVisibility", "\\u0000");
            ORMOptions.SetAttributeValue("IdGeneratorType", "native");
            ORMOptions.SetAttributeValue("MappingFileColumnOrder", "\\u0000");
            ORMOptions.SetAttributeValue("NumericToClassType", "\\u0000");
            ORMOptions.SetAttributeValue("QuoteSQLIdentifier", "\\u0000");
            ORMOptions.SetAttributeValue("RecreateShapeWhenSync", "false");
            ORMOptions.SetAttributeValue("SyncToClassDiagramAttributeName", "\\u0001");
            ORMOptions.SetAttributeValue("SyncToClassDiagramAttributeNamePrefix", "");
            ORMOptions.SetAttributeValue("SyncToClassDiagramAttributeNameSuffix", "");
            ORMOptions.SetAttributeValue("SyncToClassDiagramClassName", "\\u0001");
            ORMOptions.SetAttributeValue("SyncToClassDiagramClassNamePrefix", "");
            ORMOptions.SetAttributeValue("SyncToClassDiagramClassNameSuffix", "");
            ORMOptions.SetAttributeValue("SyncToERDColumnName", "\\u0000");
            ORMOptions.SetAttributeValue("SyncToERDColumnNamePrefix", "");
            ORMOptions.SetAttributeValue("SyncToERDColumnNameSuffix", "");
            ORMOptions.SetAttributeValue("SyncToERDTableName", "\\u0004");
            ORMOptions.SetAttributeValue("SyncToERDTableNamePrefix", "");
            ORMOptions.SetAttributeValue("SyncToERDTableNameSuffix", "");
            ORMOptions.SetAttributeValue("SynchronizeDefaultValueToColumn", "false");
            ORMOptions.SetAttributeValue("SynchronizeName", "\\u0002");
            ORMOptions.SetAttributeValue("TablePerSubclassFKMapping", "\\u0000");
            ORMOptions.SetAttributeValue("UpperCaseSQL", "true");
            ORMOptions.SetAttributeValue("UseDefaultDecimal", "true");
            ORMOptions.SetAttributeValue("WrappingServletRequest", "\\u0001");
            //
            XElement ModelQualityOptions = new XElement("ModelQualityOptions");
            ModelQualityOptions.SetAttributeValue("EnableModelQualityChecking", "false");
            //
            XElement InstantReverseOptions = new XElement("InstantReverseOptions");
            InstantReverseOptions.SetAttributeValue("CalculateGeneralizationAndRealization", "false");
            InstantReverseOptions.SetAttributeValue("CreateShapeForParentModelOfDraggedClassPackage", "false");
            InstantReverseOptions.SetAttributeValue("ReverseGetterSetter", "\\u0000");
            InstantReverseOptions.SetAttributeValue("ReverseOperationImplementation", "false");
            InstantReverseOptions.SetAttributeValue("ShowPackageForNewDiagram", "\\u0001");
            InstantReverseOptions.SetAttributeValue("ShowPackageOwner", "\\u0000");
            //
            XElement GeneralOptions = new XElement("GeneralOptions");
            GeneralOptions.SetAttributeValue("ConfirmSubLevelIdWithDot", "true");
            GeneralOptions.SetAttributeValue("QuickAddGlossaryTermParentModelId", "default");
            //
            XElement DefaultHtmlDocFont = new XElement("DefaultHtmlDocFont");
            DefaultHtmlDocFont.SetAttributeValue("Family", "Dialog");
            DefaultHtmlDocFont.SetAttributeValue("Size", 12);
            DefaultHtmlDocFont.SetAttributeValue("Style", 0);
            //
            XElement DiagramOptions = new XElement("DiagramOptions");
            DiagramOptions.SetAttributeValue("ActivityDiagramControlFlowDisplayOption", "\\u0000");
            DiagramOptions.SetAttributeValue("ActivityDiagramShowActionCallBehaviorOption", "\\u0000");
            DiagramOptions.SetAttributeValue("ActivityDiagramShowActivityEdgeWeight", "true");
            DiagramOptions.SetAttributeValue("ActivityDiagramShowObjectNodeType", "true");
            DiagramOptions.SetAttributeValue("ActivityDiagramShowPartitionHandle", "\\u0000");
            DiagramOptions.SetAttributeValue("AddDataStoresExtEntitiesToDecomposedDFD", "\\u0002");
            DiagramOptions.SetAttributeValue("AlignColumnProperties", "true");
            DiagramOptions.SetAttributeValue("AllowConfigShowInOutFlowButtonsInDataFlowDiagram", "false");
            DiagramOptions.SetAttributeValue("AutoGenerateRoleName", "false");
            DiagramOptions.SetAttributeValue("AutoSetAttributeType", "true");
            DiagramOptions.SetAttributeValue("AutoSetColumnType", "true");
            DiagramOptions.SetAttributeValue("AutoSyncRoleName", "true");
            DiagramOptions.SetAttributeValue("BpdAutoStretchPools", "true");
            DiagramOptions.SetAttributeValue("BpdConnectGatewayWithFlowObjectInDifferentPool", "\\u0000");
            DiagramOptions.SetAttributeValue("BpdDefaultConnectionPointStyle", "\\u0001");
            DiagramOptions.SetAttributeValue("BpdDefaultConnectorStyle", "\\u0005");
            DiagramOptions.SetAttributeValue("BpdDhowIdOption", "\\u0000");
            DiagramOptions.SetAttributeValue("BpdShowActivitiesTypeIcon", "true");
            DiagramOptions.SetAttributeValue("BusinessProcessDiagramDefaultLanguage", "English");
            DiagramOptions.SetAttributeValue("ClassVisibilityStyle", "\\u0001");
            DiagramOptions.SetAttributeValue("ConnectorLabelOrientation", "\\u0000");
            DiagramOptions.SetAttributeValue("CreateOneMessagePerDirection", "true");
            DiagramOptions.SetAttributeValue("DecisionMergeNodeConnectionPointStyle", "\\u0000");
            DiagramOptions.SetAttributeValue("DefaultAssociationEndNavigable", "\\u0001");
            DiagramOptions.SetAttributeValue("DefaultAssociationEndVisibility", "\\u0000");
            DiagramOptions.SetAttributeValue("DefaultAssociationShowFromMultiplicity", "true");
            DiagramOptions.SetAttributeValue("DefaultAssociationShowFromRoleName", "true");
            DiagramOptions.SetAttributeValue("DefaultAssociationShowFromRoleVisibility", "true");
            DiagramOptions.SetAttributeValue("DefaultAssociationShowStereotypes", "true");
            DiagramOptions.SetAttributeValue("DefaultAssociationShowToMultiplicity", "true");
            DiagramOptions.SetAttributeValue("DefaultAssociationShowToRoleName", "true");
            DiagramOptions.SetAttributeValue("DefaultAssociationShowToRoleVisibility", "true");
            DiagramOptions.SetAttributeValue("DefaultAttributeMultiplicity", "false");
            DiagramOptions.SetAttributeValue("DefaultAttributeType", "");
            DiagramOptions.SetAttributeValue("DefaultAttributeVisibility", "\\u0001");
            DiagramOptions.SetAttributeValue("DefaultClassAttributeMultiplicity", "");
            DiagramOptions.SetAttributeValue("DefaultClassAttributeMultiplicityOrdered", "false");
            DiagramOptions.SetAttributeValue("DefaultClassAttributeMultiplicityUnique", "true");
            DiagramOptions.SetAttributeValue("DefaultClassInterfaceBall", "false");
            DiagramOptions.SetAttributeValue("DefaultClassVisibility", "\\u0004");
            DiagramOptions.SetAttributeValue("DefaultColumnType", "integer(10)");
            DiagramOptions.SetAttributeValue("DefaultConnectionPointStyle", "\\u0000");
            DiagramOptions.SetAttributeValue("DefaultConnectorStyle", "\\u0001");
            DiagramOptions.SetAttributeValue("DefaultDiagramBackground", "rgb(255, 255, 255)");
            DiagramOptions.SetAttributeValue("DefaultDisplayAsRobustnessAnalysisIcon", "true");
            DiagramOptions.SetAttributeValue("DefaultDisplayAsRobustnessAnalysisIconInSequenceDiagram", "true");
            DiagramOptions.SetAttributeValue("DefaultDisplayAsStereotypeIcon", "false");
            DiagramOptions.SetAttributeValue("DefaultFontColor", "rgb(0, 0, 0)");
            DiagramOptions.SetAttributeValue("DefaultGenDiagramTypeFromScenario", "\\u0000");
            DiagramOptions.SetAttributeValue("DefaultHtmlDocFontColor", "rgb(0, 0, 0)");
            DiagramOptions.SetAttributeValue("DefaultLineJumps", "\\u0000");
            DiagramOptions.SetAttributeValue("DefaultOperationVisibility", "\\u0004");
            DiagramOptions.SetAttributeValue("DefaultParameterDirection", "\\u0002");
            DiagramOptions.SetAttributeValue("DefaultShowAttributeInitialValue", "true");
            DiagramOptions.SetAttributeValue("DefaultShowAttributeOption", "\\u0001");
            DiagramOptions.SetAttributeValue("DefaultShowClassMemberStereotype", "true");
            DiagramOptions.SetAttributeValue("DefaultShowDirection", "false");
            DiagramOptions.SetAttributeValue("DefaultShowMultiplicityConstraints", "false");
            DiagramOptions.SetAttributeValue("DefaultShowOperationOption", "\\u0001");
            DiagramOptions.SetAttributeValue("DefaultShowOperationSignature", "true");
            DiagramOptions.SetAttributeValue("DefaultShowOrderedMultiplicityConstraint", "true");
            DiagramOptions.SetAttributeValue("DefaultShowOwnedAssociationEndAsAttribute", "true");
            DiagramOptions.SetAttributeValue("DefaultShowOwner", "false");
            DiagramOptions.SetAttributeValue("DefaultShowOwnerSkipModelInFullyQualifiedOwnerSignature", "true");
            DiagramOptions.SetAttributeValue("DefaultShowReceptionOption", "\\u0001");
            DiagramOptions.SetAttributeValue("DefaultShowTemplateParameter", "true");
            DiagramOptions.SetAttributeValue("DefaultShowTypeOption", "\\u0001");
            DiagramOptions.SetAttributeValue("DefaultShowUniqueMultiplicityConstraint", "true");
            DiagramOptions.SetAttributeValue("DefaultTypeOfSubProcess", "\\u0000");
            DiagramOptions.SetAttributeValue("DefaultWrapClassMember", "false");
            DiagramOptions.SetAttributeValue("DrawTextAnnotationOpenRectangleFollowConnectorEnd", "true");
            DiagramOptions.SetAttributeValue("EnableMinimumSize", "true");
            DiagramOptions.SetAttributeValue("EntityColumnConstraintsPresentation", "\\u0002");
            DiagramOptions.SetAttributeValue("ErdIndexNumOfDigits", "-1");
            DiagramOptions.SetAttributeValue("ErdIndexPattern", "{table_name}");
            DiagramOptions.SetAttributeValue("ErdIndexPatternSyncAutomatically", "true");
            DiagramOptions.SetAttributeValue("ErdManyToManyJoinTableDelimiter", "_");
            DiagramOptions.SetAttributeValue("EtlTableDiagramFontSize", "14");
            DiagramOptions.SetAttributeValue("ExpandedSubProcessDiagramContent", "\\u0001");
            DiagramOptions.SetAttributeValue("ForeignKeyArrowHeadSize", "\\u0002");
            DiagramOptions.SetAttributeValue("ForeignKeyConnectorEndPointAssociatedColumn", "false");
            DiagramOptions.SetAttributeValue("ForeignKeyNamePattern", "{reference_table_name}{reference_column_name}");
            DiagramOptions.SetAttributeValue("ForeignKeyNamePatternCaseHandling", "0");
            DiagramOptions.SetAttributeValue("ForeignKeyRelationshipPattern", "{association_name}");
            DiagramOptions.SetAttributeValue("FractionalMetrics", "true");
            DiagramOptions.SetAttributeValue("GeneralizationSetNotation", "\\u0001");
            DiagramOptions.SetAttributeValue("GraphicAntiAliasing", "true");
            DiagramOptions.SetAttributeValue("GridDiagramFontSize", "14");
            DiagramOptions.SetAttributeValue("LineJumpSize", "\\u0000");
            DiagramOptions.SetAttributeValue("ModelElementNameAlignment", "\\u0004");
            DiagramOptions.SetAttributeValue("MultipleLineClassName", "\\u0001");
            DiagramOptions.SetAttributeValue("PaintConnectorThroughLabel", "false");
            DiagramOptions.SetAttributeValue("PointConnectorEndToCompartmentMember", "true");
            DiagramOptions.SetAttributeValue("PrimaryKeyConstraintPattern", "");
            DiagramOptions.SetAttributeValue("PrimaryKeyNamePattern", "ID");
            DiagramOptions.SetAttributeValue("RenameConstructorAfterRenameClass", "\\u0000");
            DiagramOptions.SetAttributeValue("RenameExtensionPointToFollowExtendUseCase", "\\u0000");
            DiagramOptions.SetAttributeValue("ShapeAutoFitSize", "false");
            DiagramOptions.SetAttributeValue("ShowActivationsInSequenceDiagram", "true");
            DiagramOptions.SetAttributeValue("ShowActivityStateNodeCaption", "524287");
            DiagramOptions.SetAttributeValue("ShowArtifactOption", "\\u0002");
            DiagramOptions.SetAttributeValue("ShowAssociatedDiagramNameOfInteraction", "false");
            DiagramOptions.SetAttributeValue("ShowAssociationRoleStereotypes", "true");
            DiagramOptions.SetAttributeValue("ShowAttributeGetterSetter", "false");
            DiagramOptions.SetAttributeValue("ShowBSElementCode", "true");
            DiagramOptions.SetAttributeValue("ShowClassEmptyCompartments", "false");
            DiagramOptions.SetAttributeValue("ShowColumnDefaultValue", "false");
            DiagramOptions.SetAttributeValue("ShowColumnNullable", "true");
            DiagramOptions.SetAttributeValue("ShowColumnType", "true");
            DiagramOptions.SetAttributeValue("ShowColumnUniqueConstraintName", "false");
            DiagramOptions.SetAttributeValue("ShowColumnUserType", "false");
            DiagramOptions.SetAttributeValue("ShowComponentOption", "\\u0002");
            DiagramOptions.SetAttributeValue("ShowExtraColumnProperties", "true");
            DiagramOptions.SetAttributeValue("ShowInOutFlowButtonsInDataFlowDiagram", "false");
            DiagramOptions.SetAttributeValue("ShowInOutFlowsInSubLevelDiagram", "true");
            DiagramOptions.SetAttributeValue("ShowMessageOperationSignatureForSequenceAndCommunicationDiagram", "true");
            DiagramOptions.SetAttributeValue("ShowMessageStereotypeInSequenceAndCommunicationDiagram", "true");
            DiagramOptions.SetAttributeValue("ShowNumberInCollaborationDiagram", "true");
            DiagramOptions.SetAttributeValue("ShowNumberInSequenceDiagram", "true");
            DiagramOptions.SetAttributeValue("ShowPackageNameStyle", "\\u0000");
            DiagramOptions.SetAttributeValue("ShowParameterNameInOperationSignature", "true");
            DiagramOptions.SetAttributeValue("ShowRowGridLineWithinCompClassDiagram", "false");
            DiagramOptions.SetAttributeValue("ShowRowGridLineWithinCompERD", "true");
            DiagramOptions.SetAttributeValue("ShowRowGridLineWithinORMDiagram", "true");
            DiagramOptions.SetAttributeValue("ShowSchemaNameInERD", "true");
            DiagramOptions.SetAttributeValue("ShowTransitionTrigger", "\\u0000");
            DiagramOptions.SetAttributeValue("ShowUseCaseExtensionPoint", "true");
            DiagramOptions.SetAttributeValue("ShowUseCaseID", "false");
            DiagramOptions.SetAttributeValue("SnapConnectorsAfterZoom", "false");
            DiagramOptions.SetAttributeValue("StateShowParametersOfInternalActivities", "false");
            DiagramOptions.SetAttributeValue("StateShowPrePostConditionAndBodyOfInternalActivities", "true");
            DiagramOptions.SetAttributeValue("StopTargetLifelineOnCreateDestroyMessage", "\\u0002");
            DiagramOptions.SetAttributeValue("SupportHtmlTaggedValue", "false");
            DiagramOptions.SetAttributeValue("SupportMultipleLineAttribute", "true");
            DiagramOptions.SetAttributeValue("SuppressImpliedMultiplicityForAttributeAssociationEnd", "false");
            DiagramOptions.SetAttributeValue("SyncAssociationNameWithAssociationClass", "\\u0000");
            DiagramOptions.SetAttributeValue("SyncAssociationRoleNameWithReferencedAttributeName", "true");
            DiagramOptions.SetAttributeValue("SyncDocOfInterfaceToSubClass", "\\u0000");
            DiagramOptions.SetAttributeValue("TextAntiAliasing", "true");
            DiagramOptions.SetAttributeValue("TextualAnalysisGenerateRequirementTextOption", "\\u0001");
            DiagramOptions.SetAttributeValue("TextualAnalysisHighlightOption", "\\u0000");
            DiagramOptions.SetAttributeValue("UnnamedIndexPattern", "{table_name}_{column_name}");
            DiagramOptions.SetAttributeValue("UseStateNameTab", "false");
            DiagramOptions.SetAttributeValue("WireflowDiagramDevice", "0");
            DiagramOptions.SetAttributeValue("WireflowDiagramShowActiveFlowLabel", "true");
            DiagramOptions.SetAttributeValue("WireflowDiagramTheme", "0");
            DiagramOptions.SetAttributeValue("WireflowDiagramWireflowShowPreview", "true");
            DiagramOptions.SetAttributeValue("WireflowDiagramWireflowShowScreenId", "true");
            DiagramOptions.Add(DefaultHtmlDocFont);
            //
            XElement ProjectOptions = new XElement("ProjectOptions");
            ProjectOptions.Add(DiagramOptions);
            ProjectOptions.Add(GeneralOptions);
            ProjectOptions.Add(InstantReverseOptions);
            ProjectOptions.Add(ModelQualityOptions);
            ProjectOptions.Add(ORMOptions);
            ProjectOptions.Add(RequirementDiagramOptions);
            ProjectOptions.Add(StateCodeEngineOptions);
            ProjectOptions.Add(WarningOptions);
            // 
            XElement LogicalView = new XElement("LogicalView");
            //
            XElement ProjectInfo = new XElement("ProjectInfo");
            ProjectInfo.Add(LogicalView);
            ProjectInfo.Add(ProjectOptions);
            return ProjectInfo;
        }
        #endregion
        public void ExportFile(string filePath, List<PLAComponent> components)
        {
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
                        XElement Stereotypes = new XElement("Stereotypes");
                        XElement Stereotype_ = new XElement("Stereotype");
                        XElement StereotypePLA_ = new XElement("Stereotype");
                        Stereotype_.SetAttributeValue("Idref", "ig9wjN6GAqA0AQan");
                        Stereotype_.SetAttributeValue("Name", "Interface");
                        try
                        {
                            if ((bool)interface_.Propertie("isOptional"))
                            {
                                StereotypePLA_.SetAttributeValue("Idref", "ig9wjN6GAqA0AOan");
                                StereotypePLA_.SetAttributeValue("Name", "Optional");
                            }
                            else
                            {
                                StereotypePLA_.SetAttributeValue("Idref", "ig9wjN6GAqA0AAan");
                                StereotypePLA_.SetAttributeValue("Name", "Mandatory");
                            }
                        }
                        catch
                        {

                            StereotypePLA_.SetAttributeValue("Idref", "ig9wjN6GAqA0AAan");
                            StereotypePLA_.SetAttributeValue("Name", "Mandatory");
                        }
                        Stereotypes.Add(Stereotype_);
                        Stereotypes.Add(StereotypePLA_);
                        classElement.Add(Stereotypes);
                        //
                        List<XElement> operatorElements = new List<XElement> { };
                        for (int o = 0; o < interface_.Operations.Count; o++)
                        {
                            PLAOperation operator_ = component.Interfaces[i].Operations[o];
                            XElement operatorElement = new XElement("Operation");
                            operatorElement.SetAttributeValue("Name", operator_.Name);
                            operatorElement.SetAttributeValue("Id", operator_.Id);
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
            //
            XElement ModelChildrenClass = new XElement("ModelChildren");
            XElement ModelChildrenComponent = new XElement("ModelChildren");
            XElement ModelChildrenModelClass = new XElement("Model");
            XElement ModelChildrenModelComponent = new XElement("Model");
            XElement ModelChildrenModelModels = new XElement("Models");
            //
            ModelChildrenModelComponent.SetAttributeValue("Name", "Component View");
            ModelChildrenModelComponent.Add(ModelChildrenComponent);
            ModelChildrenModelClass.SetAttributeValue("Name", "Logical View");
            ModelChildrenModelClass.Add(ModelChildrenClass);
            ModelChildrenComponent.Add(componentElements);
            ModelChildrenClass.Add(interfaceElements);
            //
            XElement Stereotype = new XElement("Stereotype");
            Stereotype.SetAttributeValue("Abstract", "false");
            Stereotype.SetAttributeValue("BacklogActivityId", "0");
            Stereotype.SetAttributeValue("BaseType", "Class");
            Stereotype.SetAttributeValue("Documentation_plain", "");
            Stereotype.SetAttributeValue("IconPath", "");
            Stereotype.SetAttributeValue("Id", "ig9wjN6GAqA0AQan");
            Stereotype.SetAttributeValue("Leaf", "false");
            Stereotype.SetAttributeValue("Name", "Interface");
            Stereotype.SetAttributeValue("PmAuthor", "OmidHome");
            Stereotype.SetAttributeValue("PmCreateDateTime", "2021-08-10T21:39:22.387");
            Stereotype.SetAttributeValue("PmLastModified", "2021-08-10T21:44:28.387");
            Stereotype.SetAttributeValue("QualityReason_IsNull", "true");
            Stereotype.SetAttributeValue("QualityScore", "-1");
            Stereotype.SetAttributeValue("Root", "false");
            Stereotype.SetAttributeValue("UserIDLastNumericValue", "0");
            Stereotype.SetAttributeValue("UserID_IsNull", "true");

            XElement StereotypeOp = new XElement("Stereotype");
            StereotypeOp.SetAttributeValue("Abstract", "false");
            StereotypeOp.SetAttributeValue("BacklogActivityId", "0");
            StereotypeOp.SetAttributeValue("BaseType", "Class");
            StereotypeOp.SetAttributeValue("Documentation_plain", "");
            StereotypeOp.SetAttributeValue("IconPath", "");
            StereotypeOp.SetAttributeValue("Id", "ig9wjN6GAqA0AOan");
            StereotypeOp.SetAttributeValue("Leaf", "false");
            StereotypeOp.SetAttributeValue("Name", "Optional");
            StereotypeOp.SetAttributeValue("PmAuthor", "OmidHome");
            StereotypeOp.SetAttributeValue("PmCreateDateTime", "2021-08-10T21:39:22.387");
            StereotypeOp.SetAttributeValue("PmLastModified", "2021-08-10T21:44:28.387");
            StereotypeOp.SetAttributeValue("QualityReason_IsNull", "true");
            StereotypeOp.SetAttributeValue("QualityScore", "-1");
            StereotypeOp.SetAttributeValue("Root", "false");
            StereotypeOp.SetAttributeValue("UserIDLastNumericValue", "0");
            StereotypeOp.SetAttributeValue("UserID_IsNull", "true");


            XElement StereotypeMan = new XElement("Stereotype");
            StereotypeMan.SetAttributeValue("Abstract", "false");
            StereotypeMan.SetAttributeValue("BacklogActivityId", "0");
            StereotypeMan.SetAttributeValue("BaseType", "Class");
            StereotypeMan.SetAttributeValue("Documentation_plain", "");
            StereotypeMan.SetAttributeValue("IconPath", "");
            StereotypeMan.SetAttributeValue("Id", "ig9wjN6GAqA0AAan");
            StereotypeMan.SetAttributeValue("Leaf", "false");
            StereotypeMan.SetAttributeValue("Name", "Mandatory");
            StereotypeMan.SetAttributeValue("PmAuthor", "OmidHome");
            StereotypeMan.SetAttributeValue("PmCreateDateTime", "2021-08-10T21:39:22.387");
            StereotypeMan.SetAttributeValue("PmLastModified", "2021-08-10T21:44:28.387");
            StereotypeMan.SetAttributeValue("QualityReason_IsNull", "true");
            StereotypeMan.SetAttributeValue("QualityScore", "-1");
            StereotypeMan.SetAttributeValue("Root", "false");
            StereotypeMan.SetAttributeValue("UserIDLastNumericValue", "0");
            StereotypeMan.SetAttributeValue("UserID_IsNull", "true");

            ModelChildrenModelModels.Add(ModelChildrenModelComponent);
            ModelChildrenModelModels.Add(ModelChildrenModelClass);
            ModelChildrenModelModels.Add(Stereotype);
            ModelChildrenModelModels.Add(StereotypeMan);
            ModelChildrenModelModels.Add(StereotypeOp);
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
            XElement Project = new XElement("Project");
            Project.SetAttributeValue("Author", "OmidHome");
            Project.SetAttributeValue("CommentTableSortAscending", "false");
            Project.SetAttributeValue("CommentTableSortColumn", "Date Time");
            Project.SetAttributeValue("DocumentationType", "html");
            Project.SetAttributeValue("ExportedFromDifferentName", "false");
            Project.SetAttributeValue("ExporterVersion", "16.1.1");
            Project.SetAttributeValue("Name", "untitled");
            Project.SetAttributeValue("TextualAnalysisHighlightOptionCaseSensitive", "false");
            Project.SetAttributeValue("UmlVersion", "2.x");
            Project.SetAttributeValue("Xml_structure", "simple");
            Project.Add(ProjectInfo());
            Project.Add(ModelChildrenModelModels);
            XDocument doc = new XDocument(Project);
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
