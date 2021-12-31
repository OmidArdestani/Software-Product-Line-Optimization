using JMetalCSharp.Core;
using JMetalCSharp.Encoding.SolutionType;
using JMetalCSharp.Encoding.Variable;
using JMetalCSharp.Utils;
using JMetalCSharp.Utils.Wrapper;
using read_feature_model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPLAOptimization
{

    enum ObjectivSelection
    {
        OS_Cohesion = 0,
        OS_Coupling = 1,
        OS_Reusability = 2,
        OS_PLACohesion = 3,
    }
    class MyProblem : Problem
    {
        private PLArchitecture primaryArchitecture;
        private FeatureModel featureModel;
        private List<PLAOperation> LocalOperations = new List<PLAOperation> { };

        private double[] fitnessFunctions;
        private List<KeyValuePair<List<string>, List<string>>> interfaceDependencies;
        public MyProblem(PLArchitecture architecture, List<KeyValuePair<List<string>, List<string>>> operatorDependencies, FeatureModel featureModel)
        {
            this.primaryArchitecture = architecture;
            this.interfaceDependencies = operatorDependencies;
            this.featureModel = featureModel;
            // get operators
            LocalOperations.Clear();
            for (int c = 0; c < primaryArchitecture.Components.Count; c++)
            {
                for (int i = 0; i < primaryArchitecture.Components[c].Interfaces.Count; i++)
                {
                    for (int o = 0; o < primaryArchitecture.Components[c].Interfaces[i].Operation.Count; o++)
                    {
                        string operatorId = primaryArchitecture.Components[c].Interfaces[i].Operation[o].Id;
                        // add operator to list if it was not added befor.
                        if (LocalOperations.Where(_o => _o.Id == operatorId).Count() == 0)
                            LocalOperations.Add(primaryArchitecture.Components[c].Interfaces[i].Operation[o]);
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
            NumberOfObjectives = 4;
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
            //XReal currentSolution = new XReal(solution);
            PLArchitecture currentArchitecture = GenerateArchitecture(solution);
            //evaluate Cohesion
            fitnessFunctions[(int)ObjectivSelection.OS_Cohesion] = EvalCohesion(currentArchitecture);
            //evaluate Coupling
            fitnessFunctions[(int)ObjectivSelection.OS_Coupling] = EvalCoupling(currentArchitecture);
            //evaluate Granularity
            //f[(int)ObjectivSelection.OS_Granularity] = EvalGranularity(currentArchitecture);
            //evaluate Reusabulity
            fitnessFunctions[(int)ObjectivSelection.OS_Reusability] = EvalReusability(currentArchitecture);
            //evaluate PLA-Cohesion (Feature-Scattering)
            fitnessFunctions[(int)ObjectivSelection.OS_PLACohesion] = EvalPLACohesion(currentArchitecture);
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
                    currentInterface.Operation = new List<PLAOperation> { };
                    currentInterface.Id = currentSolutionIndex.ToString();
                    interfaces.Add(currentInterface);
                }
                currentInterface.Operation.Add(LocalOperations[o]);
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
            for (int idi = 0; idi < interfaceDependencies.Count; idi++)
            {
                // sweep all key and value dependencies
                for (int idci = 0; idci < interfaceDependencies[idi].Key.Count; idci++)
                {
                    // find the components, that using the current cheking operator is in any interface.
                    var clientComponents = components.Where(
                        c => c.Interfaces.Find(
                            i => i.Operation.Find(
                                o => o.Id == interfaceDependencies[idi].Key[idci]) != null) != null).ToList();
                    for (int idsi = 0; idsi < interfaceDependencies[idi].Value.Count; idsi++)
                    {
                        // find the suplier interface considering the operator relationship matrix from input architecture
                        var suplierInterface = components.Where(
                        c => c.Interfaces.Find(
                            i => i.Operation.Find(
                                o => o.Id == interfaceDependencies[idi].Value[idsi]) != null) != null).Select(x => x.Interfaces).SingleOrDefault();
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
        private double EvalCohesion(PLArchitecture pla)
        {
            // Definition Ref [Colanzi, Vergilio - 2014]
            // Definition : Average number of internal relationships per class in a component. 
            // get all interfaces count
            double totalDependecies = pla.Components.Select(c => c.Interfaces.Count()).Sum();
            // get component count
            double totalComponents = pla.Components.Count();
            // return average of interface per componenet.
            return -totalDependecies / totalComponents;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pla"></param>
        /// <returns></returns>
        private double EvalCoupling(PLArchitecture pla)
        {
            List<double> coupllings = new List<double> { };
            for (int componentIndex = 0; componentIndex < pla.Components.Count; componentIndex++)
            {
                var depInterfaces = pla.Components[componentIndex].DependedInterfaces.ToList();
                List<PLAComponent> depComponents = new List<PLAComponent> { };
                for (int i = 0; i < depInterfaces.Count(); i++)
                {
                    var depComponent = pla.Components.Where(comp => comp.Interfaces.Find(dpInt => dpInt.Id == depInterfaces[i].Id) != null).SingleOrDefault();
                    if (depComponents.Find(comp => comp.Id == depComponent.Id) == null)
                    {
                        depComponents.Add(depComponent);
                    }
                }
                coupllings.Add(depComponents.Count);
            }
            return coupllings.Sum() / pla.ComponentCount;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pla"></param>
        /// <returns></returns>
        private double EvalPLACohesion(PLArchitecture pla)
        {
            // List of operation dependensy count
            List<int> operationDependensyList = new List<int> { };
            // Get current pla operations
            List<PLAOperation> operations = new List<PLAOperation> { };
            for (int c = 0; c < pla.Components.Count; c++)
            {
                for (int i = 0; i < pla.Components[c].Interfaces.Count; i++)
                {
                    operations.AddRange(pla.Components[c].Interfaces[i].Operation);
                }
            }
            // Check each operation dependensy
            // Select one operation as operationA
            foreach (var operationA in operations)
            {
                int operationADependensies = 0;
                // Select onother operation as operationB
                foreach (var operationB in operations)
                {
                    // Get all dependent interfaces from the owner component of the OperationA.
                    foreach (var currentInterface in operationA.OwnerInterface.OwnerComponent.DependedInterfaces)
                    {
                        // Check each opration in each interface
                        // If operationB.id is the same as one of the interface operations,
                        //      the dependency between the OperationA and OperationB satisfies in the same component.
                        foreach (var currentOepration in currentInterface.Operation)
                        {
                            if (currentOepration.Id != operationB.Id)
                                operationADependensies++;
                        }
                    }
                }
                operationDependensyList.Add(operationADependensies);
            }
            return operationDependensyList.Sum();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pla"></param>
        /// <returns></returns>
        private double EvalReusability(PLArchitecture pla)
        {
            double inTime = fitnessFunctions[(int)ObjectivSelection.OS_Cohesion] / fitnessFunctions[(int)ObjectivSelection.OS_Coupling];
            double inSpace = 0;
            return inTime + inSpace;
        }
    }
}
