using JMetalCSharp.Core;
using JMetalCSharp.Encoding.SolutionType;
using JMetalCSharp.Encoding.Variable;
using read_feature_model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MyPLAOptimization
{

    enum ObjectivSelection
    {
        OS_Coupling,
        OS_PLACohesion,
        OS_ConventionalCohesion,
        OS_Commonality,
        OS_Granularity
    }
    class MyProblem : Problem
    {
        private PLArchitecture primaryArchitecture;
        private FeatureModel featureModel;
        private List<FeatureRelationship> featureRealationshipMatrix;
        private List<PLAOperation> LocalOperations = new List<PLAOperation> { };

        private double[] fitnessFunctions;
        private List<KeyValuePair<List<string>, List<string>>> operationDependencies;
        public MyProblem(PLArchitecture architecture, List<KeyValuePair<List<string>, List<string>>> operatorDependencies, FeatureModel featureModel, List<FeatureRelationship> featureRealationshipMatrix)
        {
            this.primaryArchitecture = architecture;
            this.operationDependencies = operatorDependencies;
            this.featureModel = featureModel;
            this.featureRealationshipMatrix = featureRealationshipMatrix;
            // get operators
            LocalOperations.Clear();
            for (int c = 0; c < primaryArchitecture.Components.Count; c++)
            {
                for (int i = 0; i < primaryArchitecture.Components[c].Interfaces.Count; i++)
                {
                    for (int o = 0; o < primaryArchitecture.Components[c].Interfaces[i].Operations.Count; o++)
                    {
                        string operatorId = primaryArchitecture.Components[c].Interfaces[i].Operations[o].Id;
                        // add operator to list if it was not added befor.
                        if (LocalOperations.Where(_o => _o.Id == operatorId).Count() == 0)
                            LocalOperations.Add(primaryArchitecture.Components[c].Interfaces[i].Operations[o]);
                    }
                }
            }
            // Count number of operators
            NumberOfVariables = LocalOperations.Count;
            /* ----------------------
             * 1- Cohesion
             * 2- Coupling
             * 3- Granularity
             * 4- Feature-Scattering
             * 5- Feature-Interaction
             *-----------------------*/
            NumberOfObjectives = 5;
            NumberOfConstraints = 0;
            ProblemName = "MyProblem";
            fitnessFunctions = new double[NumberOfObjectives];
            UpperLimit = new double[NumberOfVariables];
            LowerLimit = new double[NumberOfVariables];
            for (int i = 0; i < NumberOfVariables; i++)
            {
                LowerLimit[i] = 0.0;
                UpperLimit[i] = NumberOfVariables;
            }
            SolutionType = new MyReal2DSolutionType(this);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="solution"></param>
        /// 
        public override void Evaluate(Solution solution)
        {
            fitnessFunctions = new double[NumberOfObjectives];
            // Generate new PLA considering new interface chane
            PLArchitecture currentArchitecture = GenerateArchitecture(solution);
            //----------------------------------------------------------
            //  Calculate all the fitness functions.
            // ATTENTION: The calculation sequence is important.
            //----------------------------------------------------------
            double FM = 0, CM = 0;
            //evaluate Coupling (1)
            var coup = EvalCoupling(currentArchitecture) * 100;
            //evaluate Cohesion (2)
            var plaCOh = EvalPLACohesion(currentArchitecture) * 100;
            //evaluate PLA-Cohesion (Feature-Scattering) (3)
            var conCoh = EvalConventionalCohesion(currentArchitecture) * 100;
            //evaluate Commonality (4)
            var common = Math.Abs(0.5 - EvalCommonality(currentArchitecture));
            //evaluate Granularity (5)
            var gran = EvalGranularityDistance(currentArchitecture);

            //FM = 1 / plaCOh + common;
            //CM = coup + 1 / conCoh + gran;
            //fitnessFunctions[0] = (1 / conCoh) * 100 + coup;
            fitnessFunctions[0] = (1 / conCoh);
            fitnessFunctions[1] = coup;
            fitnessFunctions[2] = 1 / plaCOh;
            fitnessFunctions[3] = common;
            fitnessFunctions[4] = gran;
            // set objectives
            solution.Objective = fitnessFunctions;
            //
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="solution"></param>
        /// <returns></returns>
        public PLArchitecture GenerateArchitecture(Solution solution)
        {
            //
            // create interfaces
            //
            int operationCount = LocalOperations.Count;
            List<PLAInterface> interfaces = new List<PLAInterface> { };
            for (int o = 0; o < operationCount; o++)
            {
                double varValue = ((ArrayReal)solution.Variable[0]).Array[o];
                //int currentSolutionIndex = (int)Math.Round(varValue * operationCount, 0);
                int currentSolutionIndex = (int)Math.Round(varValue, 0);
                PLAInterface currentInterface = interfaces.Where(_interface => _interface.Id == currentSolutionIndex.ToString()).SingleOrDefault();
                if (currentInterface == null)
                {
                    currentInterface = new PLAInterface();
                    currentInterface.Operations = new List<PLAOperation> { };
                    currentInterface.Id = currentSolutionIndex.ToString();
                    interfaces.Add(currentInterface);
                }
                var newOperation = new PLAOperation(LocalOperations[o]);
                newOperation.OwnerInterface = currentInterface;
                currentInterface.Operations.Add(newOperation);
            }
            //
            // create components
            //
            int interfaceCount = interfaces.Count;
            List<PLAComponent> components = new List<PLAComponent> { };
            for (int i = 0; i < interfaceCount; i++)
            {
                double varValue = ((ArrayReal)solution.Variable[1]).Array[i];
                int currentSolutionIndex = (int)Math.Round(varValue * interfaceCount, 0);
                //int currentSolutionIndex = (int)Math.Round(varValue, 0);
                PLAComponent currentComponent = components.Where(_component => _component.Id == currentSolutionIndex.ToString()).SingleOrDefault();
                if (currentComponent == null)
                {
                    currentComponent = new PLAComponent();
                    currentComponent.Interfaces = new List<PLAInterface> { };
                    currentComponent.DependedInterfaces = new List<PLAInterface> { };
                    currentComponent.Id = currentSolutionIndex.ToString();
                    components.Add(currentComponent);
                }
                interfaces[i].OwnerComponent = currentComponent;
                currentComponent.Interfaces.Add(interfaces[i]);
            }
            //
            // create dependencies
            //
            for (int idi = 0; idi < operationDependencies.Count; idi++)
            {
                // sweep all key and value dependencies
                for (int idci = 0; idci < operationDependencies[idi].Key.Count; idci++)
                {
                    // find the components, that using the current cheking operation is in any interfaces.
                    var clientComponents = components.Find(
                        c => c.Interfaces.Find(
                            i => i.Operations.Find(
                                o => o.Id == operationDependencies[idi].Key[idci]) != null) != null);
                    for (int idsi = 0; idsi < operationDependencies[idi].Value.Count; idsi++)
                    {
                        // find the suplier interface considering the operation relationship matrix from input architecture
                        var suplierInterface = interfaces.Find(i => i.Operations.Find(o => o.Id == operationDependencies[idi].Value[idsi]) != null);
                        //check suplier interface count
                        if (suplierInterface != null)
                        {
                            // add dependency interface to client component, if the dependency interface was not added befor.
                            if (clientComponents != null)
                            {
                                if (clientComponents.DependedInterfaces.Find(l => l.Id == suplierInterface.Id) == null)
                                    clientComponents.DependedInterfaces.Add(suplierInterface);
                            }
                        }
                    }
                }
            }
            PLArchitecture returnPla = new PLArchitecture(components);
            returnPla.ComponentCount = components.Count();
            returnPla.InterfaceCount = interfaceCount;
            returnPla.OperatorCount = operationCount;
            //
            // set feature relationships
            //
            List<PLAInterface> allInterfaces = new List<PLAInterface> { };
            components.ForEach(c => allInterfaces.AddRange(c.Interfaces));
            foreach (var interfaceItem in allInterfaces)
            {
                // Store related features
                var relatedFeatureList = new List<FeatureTreeNode> { };
                // Find related features consider to operations
                foreach (var operationItem in interfaceItem.Operations)
                {
                    var relationships = featureRealationshipMatrix.Where(x => x.RelatedOperation == operationItem);
                    foreach (var relItem in relationships)
                    {
                        // Insert into the related list if not inserted befor.
                        if (!relatedFeatureList.Contains(relItem.RelatedFeature))
                        {
                            relatedFeatureList.Add(relItem.RelatedFeature);
                        }
                    }
                }
                // Check Mandatory/Optional or Group of features that related to this interface.
                foreach (var featureItem in relatedFeatureList)
                {
                    // Mandatory/Optional Feature
                    if (featureItem is SolitaireFeature)
                    {
                        interfaceItem.SetPropertie("isGroup", false);
                    }
                    // Check the feature parent is a Group (OR items)
                    else if (featureItem.Parent is FeatureGroup)
                    {
                        interfaceItem.SetPropertie("isGroup", true);
                    }
                }
                bool isOptional = true;
                foreach (var operationItem in interfaceItem.Operations)
                {
                    if (operationItem.Propertie("isOptional") != null)
                    {
                        if (!Convert.ToBoolean(operationItem.Propertie("isOptional")))
                        {
                            interfaceItem.SetPropertie("isOptional", false);
                            isOptional = false;
                            break;
                        }
                    }
                    else
                    {
                        interfaceItem.SetPropertie("isOptional", false);
                        isOptional = false;
                        break;
                    }
                }
                if (isOptional)
                    interfaceItem.SetPropertie("isOptional", true);
            }
            return returnPla;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pla"></param>
        /// <returns></returns>
        public double EvalCoupling(PLArchitecture pla)
        {
            List<double> couplings = new List<double> { };
            // for normalizing the coupling value, divide the value to all probability dependencies between components. n(n-1)
            double n = pla.ComponentCount;
            double allProbability = (n - 1);
            allProbability = allProbability == 0 ? 1 : allProbability;
            for (int componentIndex = 0; componentIndex < pla.Components.Count; componentIndex++)
            {
                Dictionary<PLAComponent, string> componentDependencies = new Dictionary<PLAComponent, string> { };
                // get all component dependencies for each component
                // add to a map to count all of them
                pla.Components[componentIndex].DependedInterfaces.ForEach(i => componentDependencies[i.OwnerComponent] = "1");
                componentDependencies.Remove(pla.Components[componentIndex]);
                // add count of dependencies of the component to the coupling list
                couplings.Add((double)componentDependencies.Count / allProbability);
            }
            // at the end, we have a list of coupling of each component in the PLA
            // final coupling is, sum of coupling values 
            return couplings.Sum() / (double)pla.ComponentCount;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pla"></param>
        /// <returns></returns>
        public double EvalPLACohesion(PLArchitecture pla)
        {
            // Get component count
            double componentIsRealizingFeature_Count = 0;
            double featureRealizedByComponent_Count = 0;
            // Get all features from feature model
            var allFeatures = featureModel.GetAllChildrenOf(featureModel.Root);
            //-----------------------------------------------------------------------
            // Calculation count of features that each component is realizing.
            //-----------------------------------------------------------------------
            for (int cIndex = 0; cIndex < pla.ComponentCount; cIndex++)
            {
                // Get all operation in the current component
                var allOperationsInTheComponent = new List<PLAOperation> { };
                pla.Components[cIndex].Interfaces.ForEach(i => allOperationsInTheComponent.AddRange(i.Operations));
                Dictionary<string, string> mapOfFeatureAndOperation = new Dictionary<string, string>(); // (Feature id as key,Operation id as value)
                // Sweep in all operation
                foreach (var operation in allOperationsInTheComponent)
                {
                    // Find all realationships with the current operation
                    var allRels = featureRealationshipMatrix.Where(x => x.RelatedOperation.Id == operation.Id);
                    // Sweep in the got relationships
                    foreach (var item in allRels)
                    {
                        var featureRalatedToOperation = item.RelatedFeature;
                        // Insert ther Operation ID in the dictionary, if the feature is not null and also the ID is not added to the dictionory.
                        if (featureRalatedToOperation != null)
                        {
                            mapOfFeatureAndOperation[featureRalatedToOperation.ID] = operation.Id;
                        }
                    }
                }
                // Sum all count of dictionary items
                // Percentage of total components
                componentIsRealizingFeature_Count += (double)mapOfFeatureAndOperation.Count() / (double)allFeatures.Count();
            }
            //-----------------------------------------------------------------------
            // Calculation count of components that realized each feature.
            //-----------------------------------------------------------------------
            // Get all operation in the PLA
            var allOperationsInComponent = new List<PLAOperation> { };
            pla.Components.ForEach(c => c.Interfaces.ForEach(i => allOperationsInComponent.AddRange(i.Operations)));
            // Sweep the feature list
            for (int featureInd = 0; featureInd < allFeatures.Count(); featureInd++)
            {
                // Keep current feature.
                var currentFeature = allFeatures[featureInd];
                // Get all relationships with the current feature.
                var allFeatureRels = featureRealationshipMatrix.Where(x => x.RelatedFeature.ID == currentFeature.ID);
                Dictionary<string, string> mapOfComponentToFeature = new Dictionary<string, string>(); // (Component id as key,Feature id as value)
                // Sweep in the got relationships
                foreach (var rel in allFeatureRels)
                {
                    // Sweep in the all operation in the PLA
                    foreach (var operation in allOperationsInComponent)
                    {
                        // Insert the owner component ID in the dictionary, if the operation equal rel. operation and also the Component ID was not inserted.
                        if (operation == rel.RelatedOperation)
                        {
                            mapOfComponentToFeature[operation.OwnerInterface.OwnerComponent.Id] = rel.RelatedFeature.ID;
                        }
                    }
                }
                // Sum all count of dictionary items
                // Percentage of total features
                featureRealizedByComponent_Count += (double)mapOfComponentToFeature.Count() / (double)pla.ComponentCount;
            }
            // -----------------------------------------------------------------------------------------
            // Calculate average of percentages
            featureRealizedByComponent_Count = featureRealizedByComponent_Count / (double)allFeatures.Count();
            componentIsRealizingFeature_Count = componentIsRealizingFeature_Count / (double)pla.ComponentCount;
            // value normalization
            double normalizedPLACohesion = (componentIsRealizingFeature_Count + featureRealizedByComponent_Count) / 2.0;
            // return result
            return normalizedPLACohesion;
        }
        public double EvalCommonality(PLArchitecture pla)
        {
            double numberOfTotalInterface = pla.InterfaceCount;
            double numberOfMandatoryInterface = pla.Components.Select(c => c.Interfaces.Where(i => !Convert.ToBoolean(i.Propertie("isOptional"))).Count()).Sum();
            double commonalityValue = numberOfMandatoryInterface / numberOfTotalInterface;
            return commonalityValue;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pla"></param>
        /// <returns></returns>
        public double EvalConventionalCohesion(PLArchitecture pla)
        {
            // List of operation dependensy count
            List<double> cohesionList = new List<double> { };
            foreach (var component in pla.Components)
            {
                var operationInComponentList = new List<PLAOperation> { };
                component.Interfaces.ForEach(x => operationInComponentList.AddRange(x.Operations));
                int numberOfDependedOperations = component.DependedInterfaces.Select(x => x.Operations.Count()).Sum();
                double sumCo = 0;
                foreach (var perationItem in operationInComponentList)
                {
                    sumCo += operationInComponentList.Count() - 1; // this is same as n(n-1)
                }
                if (sumCo + numberOfDependedOperations == 0)
                    cohesionList.Add(0);
                else
                {
                    double componentCo = sumCo / (sumCo + numberOfDependedOperations);
                    cohesionList.Add(componentCo);
                }
            }
            return cohesionList.Sum() / (pla.ComponentCount);
        }
        private double Factoriel(int value)
        {
            int num = value;
            if (num > 0)
            {
                int n = num;
                for (int i = n - 1; i > 0; i--)
                {
                    n *= i;
                }
                return n;
            }
            else
                return 1;
        }
        private double MathChooseProbability(int k_select, int n_total)
        {
            return Factoriel(n_total) / (Factoriel(k_select) * Factoriel(n_total - k_select));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pla"></param>
        /// <returns></returns>
        public double EvalReusability(PLArchitecture pla)
        {
            //-----------------------------------------------------------------------
            // Calculation reusibility in time = Conventional-Cohesion / Coupling
            //-----------------------------------------------------------------------
            // if coupling and cohesion was calc, use of them, else calc both of them
            double coupling = 0;
            double cohesion = 0;
            //if (fitnessFunctions[(int)ObjectivSelection.OS_Coupling] != 0 && fitnessFunctions.Count() == 5)
            //{
            //    cohesion = -fitnessFunctions[(int)ObjectivSelection.OS_ConventionalCohesion];
            //    coupling = fitnessFunctions[(int)ObjectivSelection.OS_Coupling];
            //}
            //else
            //{
            cohesion = -EvalConventionalCohesion(pla);
            coupling = EvalCoupling(pla);
            //}
            double inTime = (cohesion / Math.Round(coupling, 15)) / 1e15;
            //-----------------------------------------------------------------------
            // Calculation reusability in space = average of interface probability use in products (configurations).
            //-----------------------------------------------------------------------
            var plaInterfaces = new List<PLAInterface> { };
            pla.Components.ForEach(c => plaInterfaces.AddRange(c.Interfaces));
            double inSpace = 0;
            foreach (var interfaceItem in plaInterfaces)
            {
                // Store related features
                var relatedFeatureList = new List<FeatureTreeNode> { };
                // Find related features consider to operations
                foreach (var operationItem in interfaceItem.Operations)
                {
                    var relationships = featureRealationshipMatrix.Where(x => x.RelatedOperation == operationItem);
                    foreach (var relItem in relationships)
                    {
                        // Insert into the related list if not inserted befor.
                        if (!relatedFeatureList.Contains(relItem.RelatedFeature))
                        {
                            relatedFeatureList.Add(relItem.RelatedFeature);
                        }
                    }
                }
                double probability = 0;
                // Check Mandatory/Optional or Group of features that related to this interface.
                foreach (var featureItem in relatedFeatureList)
                {
                    // Mandatory/Optional Feature
                    if (featureItem is SolitaireFeature)
                    {
                        // If the feature is mandatory, the probability of interface use in products is 1, means always in use.
                        if (!((SolitaireFeature)featureItem).IsOptional)
                        {
                            probability = 1;
                            break;
                        }
                        else
                        {
                            probability += 0.5;
                        }
                    }
                    // Check the feature parent is a Group (OR items)
                    else if (featureItem.Parent is FeatureGroup)
                    {
                        probability += MathChooseProbability(1, featureItem.Parent.ChildCount());
                    }
                }
                // Sum of average of probability
                inSpace += probability / pla.InterfaceCount;
            }
            double finalReusability = (inTime + inSpace) / 2.0;
            return finalReusability;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pla"></param>
        /// <returns></returns>
        public double EvalConfigurability(PLArchitecture pla)
        {
            double k_interfaceCount = pla.InterfaceCount;
            // Checking the property named "isOptional", which was set in the Reusability calculation step.
            double optionalInterfaceCount = pla.Components.Select(c => c.Interfaces.Where(i => Convert.ToBoolean(i.Propertie("isOptional"))).Count()).Sum();
            return -(Math.Pow(2, optionalInterfaceCount) / Math.Pow(2, k_interfaceCount)); // 2^k is if all interfaces was optional.
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pla"></param>
        /// <returns></returns>
        public double EvalCompleteness(PLArchitecture pla)
        {
            //CO = Completeness of operations
            double CO = 1 - (primaryArchitecture.OperatorCount - pla.OperatorCount) / primaryArchitecture.OperatorCount;
            List<PLAOperation> allOperationsInput = new List<PLAOperation> { };
            List<PLAOperation> allOperationsOutput = new List<PLAOperation> { };
            // Get all output PLA Operations
            primaryArchitecture.Components.ForEach(c => c.Interfaces.ForEach(i => allOperationsInput.AddRange(i.Operations)));
            // Get all input PLA Operations
            pla.Components.ForEach(c => c.Interfaces.ForEach(i => allOperationsOutput.AddRange(i.Operations)));
            //
            var optionalOperationInput = allOperationsInput.Where(o => Convert.ToBoolean(o.Propertie("isOptional"))).ToList();
            var mandatoryOperationInput = allOperationsInput.Where(o => !Convert.ToBoolean(o.Propertie("isOptional"))).ToList();
            var optionalOperationOutput = allOperationsOutput.Where(o => Convert.ToBoolean(o.Propertie("isOptional"))).ToList();
            var mandatoryOperationOutput = allOperationsOutput.Where(o => !Convert.ToBoolean(o.Propertie("isOptional"))).ToList();
            //
            var optInputInnerJoinOptOutput = optionalOperationInput.Join(optionalOperationOutput,
                                                input => input.Id,
                                                output => output.Id,
                                                (input, output) => new
                                                {
                                                    id = input.Id,
                                                    proprtie = input.Propertie("isOptional") == output.Propertie("isOptional")
                                                }).Where(x => !x.proprtie).ToList();

            var mandInputInnerJoinOptOutput = mandatoryOperationInput.Join(mandatoryOperationOutput,
                                    input => input.Id,
                                    output => output.Id,
                                    (input, output) => new
                                    {
                                        id = input.Id,
                                        proprtie = input.Propertie("isOptional") == output.Propertie("isOptional")
                                    }).Where(x => !x.proprtie).ToList();
            //CMO = Completeness of mandatory operations.
            double CMO = 1 - mandInputInnerJoinOptOutput.Count() / mandatoryOperationInput.Count();
            //COO = Completeness of optional operations.
            double COO = 1 - optInputInnerJoinOptOutput.Count() / optionalOperationInput.Count();
            double completeness = (COO + CMO + CO) / 3;
            return completeness;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pla"></param>
        /// <returns></returns>
        public double EvalGranularityDistance(PLArchitecture pla)
        {
            double C = pla.ComponentCount;
            double H = Math.Log((double)pla.OperatorCount) + 0.577;
            List<double> componentOperations = new List<double> { };
            foreach (var component in pla.Components)
            {
                //Oi: number of operations within component i
                double O = component.Interfaces.Select(i => i.Operations.Count()).Sum();
                componentOperations.Add(Math.Abs(O - H));
            }
            double objGranularity = componentOperations.Sum() / C;
            return objGranularity;
        }
    }
}
