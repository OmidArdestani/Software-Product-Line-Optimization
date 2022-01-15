using JMetalCSharp.Core;
using JMetalCSharp.Encoding.Variable;
using read_feature_model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyPLAOptimization
{

    enum ObjectivSelection
    {
        OS_PLACohesion = 0,
        OS_Coupling = 1,
        OS_Reusability = 2,
        OS_ConventionalCohesion = 3,
        OS_Configurability = 4
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
                UpperLimit[i] = 1.0;
            }

            SolutionType = new MyReal2DSolutionType(this);
        }
        public override void Evaluate(Solution solution)
        {
            // Generate new PLA considering new interface chane
            PLArchitecture currentArchitecture = GenerateArchitecture(solution);
            //----------------------------------------------------------
            //  Calculate all the fitness functions.
            // ATTENTION: The calculation sequence is important.
            //----------------------------------------------------------
            //evaluate Coupling (1)
            fitnessFunctions[(int)ObjectivSelection.OS_Coupling] = EvalCoupling(currentArchitecture);
            //evaluate Cohesion (2)
            fitnessFunctions[(int)ObjectivSelection.OS_PLACohesion] = EvalPLACohesion(currentArchitecture);
            //evaluate PLA-Cohesion (Feature-Scattering) (3)
            fitnessFunctions[(int)ObjectivSelection.OS_ConventionalCohesion] = EvalConventionalCohesion(currentArchitecture);
            //evaluate Reusabulity (4)
            fitnessFunctions[(int)ObjectivSelection.OS_Reusability] = EvalReusability(currentArchitecture);
            //evaluate Configurability (5)
            fitnessFunctions[(int)ObjectivSelection.OS_Configurability] = EvalConfigurability(currentArchitecture);
            // set objectives
            solution.Objective = fitnessFunctions;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="solution"></param>
        /// <returns></returns>
        public PLArchitecture GenerateArchitecture(Solution solution)
        {
            // create interfaces
            int operationCount = LocalOperations.Count;
            List<PLAInterface> interfaces = new List<PLAInterface> { };
            for (int o = 0; o < operationCount; o++)
            {
                double varValue = ((ArrayReal)solution.Variable[0]).Array[o];
                int currentSolutionIndex = (int)(varValue * operationCount);
                PLAInterface currentInterface = interfaces.Where(_interface => _interface.Id == currentSolutionIndex.ToString()).SingleOrDefault();
                if (currentInterface == null)
                {
                    currentInterface = new PLAInterface();
                    currentInterface.Operations = new List<PLAOperation> { };
                    currentInterface.Id = currentSolutionIndex.ToString();
                    interfaces.Add(currentInterface);
                }
                currentInterface.Operations.Add(LocalOperations[o]);
            }
            // create components
            int interfaceCount = interfaces.Count;
            List<PLAComponent> components = new List<PLAComponent> { };
            for (int i = 0; i < interfaceCount; i++)
            {
                double varValue = ((ArrayReal)solution.Variable[1]).Array[i];
                int currentSolutionIndex = (int)(varValue * interfaceCount);
                PLAComponent currentComponent = components.Where(_component => _component.Id == currentSolutionIndex.ToString()).SingleOrDefault();
                if (currentComponent == null)
                {
                    currentComponent = new PLAComponent();
                    currentComponent.Interfaces = new List<PLAInterface> { };
                    currentComponent.DependedInterfaces = new List<PLAInterface> { };
                    currentComponent.Id = currentSolutionIndex.ToString();
                    components.Add(currentComponent);
                }
                currentComponent.Interfaces.Add(interfaces[i]);
            }
            // create dependencies
            for (int idi = 0; idi < operationDependencies.Count; idi++)
            {
                // sweep all key and value dependencies
                for (int idci = 0; idci < operationDependencies[idi].Key.Count; idci++)
                {
                    // find the components, that using the current cheking operator is in any interface.
                    var clientComponents = components.Where(
                        c => c.Interfaces.Find(
                            i => i.Operations.Find(
                                o => o.Id == operationDependencies[idi].Key[idci]) != null) != null).ToList();
                    for (int idsi = 0; idsi < operationDependencies[idi].Value.Count; idsi++)
                    {
                        // find the suplier interface considering the operator relationship matrix from input architecture
                        var suplierInterface = components.Where(
                        c => c.Interfaces.Find(
                            i => i.Operations.Find(
                                o => o.Id == operationDependencies[idi].Value[idsi]) != null) != null).Select(x => x.Interfaces).SingleOrDefault();
                        for (int cci = 0; cci < clientComponents.Count; cci++)
                        {
                            //check suplier interface count
                            if (suplierInterface.Count > 0)
                            {
                                // add dependency interface to client component, if the dependency interface was not added befor.
                                if (clientComponents[cci].DependedInterfaces.Find(l => l.Id == suplierInterface.First().Id) == null)
                                    clientComponents[cci].DependedInterfaces.Add(suplierInterface.First());
                            }
                        }
                    }
                }
            }
            PLArchitecture returnPla = new PLArchitecture(components);
            returnPla.ComponentCount = components.Count();
            returnPla.InterfaceCount = interfaceCount;
            returnPla.OperatorCount = operationCount;
            return returnPla;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pla"></param>
        /// <returns></returns>
        private double EvalCoupling(PLArchitecture pla)
        {
            List<double> couplings = new List<double> { };
            for (int componentIndex = 0; componentIndex < pla.Components.Count; componentIndex++)
            {
                // get all dependency interfaces for each component
                var depInterfaces = pla.Components[componentIndex].DependedInterfaces.ToList();
                List<PLAComponent> depComponents = new List<PLAComponent> { };
                for (int i = 0; i < depInterfaces.Count(); i++)
                {
                    // find the component dependency from the PLA
                    var depComponent = pla.Components.Where(comp => comp.Interfaces.Find(dpInt => dpInt.Id == depInterfaces[i].Id) != null).SingleOrDefault();
                    // if the component was not in list, append that
                    if (depComponents.Find(comp => comp.Id == depComponent.Id) == null)
                    {
                        depComponents.Add(depComponent);
                    }
                }
                // add count of dependencies of the component to the coupling list
                couplings.Add(depComponents.Count);
            }
            // at the end, we have a list of coupling of each component in the PLA
            // final coupling is, sum of coupling values 
            // for normalizing the coupling value, divide the value to all probability dependencies between components. n(n-1)
            double n = pla.ComponentCount;
            double allProbability = n * (n - 1);
            return couplings.Sum() / allProbability;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pla"></param>
        /// <returns></returns>
        private double EvalPLACohesion(PLArchitecture pla)
        {
            // Get component count
            double componentIsRealizingFeature_Count = 0;
            double featureRealizedByComponent_Count = 0;
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
                    var allRels = featureRealationshipMatrix.Where(x => x.RelatedOperation == operation);
                    // Sweep in the got relationships
                    foreach (var item in allRels)
                    {
                        var featureRalatedToOperation = item.RelatedFeature;
                        // Insert ther Operation ID in the dictionary, if the feature is not null and also the ID is not added to the dictionory.
                        if (featureRalatedToOperation != null && !mapOfFeatureAndOperation.ContainsKey(featureRalatedToOperation.ID))
                        {
                            mapOfFeatureAndOperation.Add(featureRalatedToOperation.ID, operation.Id);
                        }
                    }
                }
                // Sum all count of dictionary items
                componentIsRealizingFeature_Count += mapOfFeatureAndOperation.Count();
            }
            //-----------------------------------------------------------------------
            // Calculation count of components that realized each feature.
            //-----------------------------------------------------------------------
            // Get all features from feature model
            var allFeatures = featureModel.GetAllChildrenOf(featureModel.Root);
            // Get all operation in the PLA
            var allOperationsInComponent = new List<PLAOperation> { };
            pla.Components.ForEach(c => c.Interfaces.ForEach(i => allOperationsInComponent.AddRange(i.Operations)));
            // Sweep the feature list
            for (int featureInd = 0; featureInd < allFeatures.Count(); featureInd++)
            {
                // Keep current feature.
                var currentFeature = allFeatures[featureInd];
                // Get all relationships with the current feature.
                var allFeatureRels = featureRealationshipMatrix.Where(x => x.RelatedFeature == currentFeature);
                Dictionary<string, string> mapOfComponentToFeature = new Dictionary<string, string>(); // (Component id as key,Feature id as value)
                // Sweep in the got relationships
                foreach (var rel in allFeatureRels)
                {
                    // Sweep in the all operation in the PLA
                    foreach (var operation in allOperationsInComponent)
                    {
                        // Insert the owner component ID in the dictionary, if the operation equal rel. operation and also the Component ID was not inserted.
                        if (operation == rel.RelatedOperation && !mapOfComponentToFeature.ContainsKey(operation.OwnerInterface.OwnerComponent.Id))
                        {
                            mapOfComponentToFeature.Add(operation.OwnerInterface.OwnerComponent.Id, rel.RelatedFeature.ID);
                        }
                    }
                }
                // Sum all count of dictionary items
                featureRealizedByComponent_Count += mapOfComponentToFeature.Count();
            }
            // value normalization
            double n = 1;
            double normalizedPLACohesion = (componentIsRealizingFeature_Count + featureRealizedByComponent_Count) / n;
            // return result
            return -normalizedPLACohesion;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pla"></param>
        /// <returns></returns>
        private double EvalConventionalCohesion(PLArchitecture pla)
        {
            // List of operation dependensy count
            List<double> operationNormalizedCohesionList = new List<double> { };
            // Get current pla operations
            List<PLAOperation> operations = new List<PLAOperation> { };
            for (int c = 0; c < pla.Components.Count; c++)
            {
                for (int i = 0; i < pla.Components[c].Interfaces.Count; i++)
                {
                    operations.AddRange(pla.Components[c].Interfaces[i].Operations);
                }
            }
            // Check each operation dependensy
            // Select one operation as operationA
            foreach (var operationA in operations)
            {
                double operationADependensies = 0;
                // Select onother operation as operationB
                foreach (var operationB in operations)
                {
                    if (operationA.Id != operationB.Id)
                    {
                        if (operationA.OwnerInterface.OwnerComponent.Id == operationB.OwnerInterface.OwnerComponent.Id)
                            operationADependensies++;
                    }
                }
                // number of all dependensies for 
                double normalizationValue = operationDependencies.Where(x => x.Key.Select(y => y == operationA.Id).Count() > 0).Select(z => z.Key.Count()).Sum();
                operationNormalizedCohesionList.Add(operationADependensies / normalizationValue);
            }
            return -1 * operationNormalizedCohesionList.Average();
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
        private double EvalReusability(PLArchitecture pla)
        {
            //-----------------------------------------------------------------------
            // Calculation reusibility in time = PLACohesion / Coupling
            //-----------------------------------------------------------------------
            double inTime = -fitnessFunctions[(int)ObjectivSelection.OS_PLACohesion] / fitnessFunctions[(int)ObjectivSelection.OS_Coupling];
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
                        interfaceItem.SetPropertie("isOptional", ((SolitaireFeature)featureItem).IsOptional);
                        interfaceItem.SetPropertie("isGroup", false);
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
                        interfaceItem.SetPropertie("isOptional", true);
                        interfaceItem.SetPropertie("isGroup", true);
                        probability += MathChooseProbability(1, featureItem.Parent.ChildCount());
                    }
                }
                // Sum of average of probability
                inSpace += probability / pla.InterfaceCount;
            }
            return inTime + inSpace;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pla"></param>
        /// <returns></returns>
        private double EvalConfigurability(PLArchitecture pla)
        {
            double k_interfaceCount = pla.InterfaceCount;
            // Checking the property named "isOptional", which was set in the Reusability calculation step.
            double optionalInterfaceCount = pla.Components.Select(c => c.Interfaces.Where(i=> Convert.ToBoolean(i.Propertie("isOptional"))).Count()).Sum();
            return optionalInterfaceCount / Math.Pow(2, k_interfaceCount); // 2^k is if all interfaces was optional.
        }
    }
}
