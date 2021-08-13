using JMetalCSharp.Core;
using JMetalCSharp.Encoding.SolutionType;
using JMetalCSharp.Utils;
using JMetalCSharp.Utils.Wrapper;
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
        OS_Granularity = 2,
        OS_FeatureScattering = 3,
        OS_FeatureInteraction = 4
    }
    class MyProblem : Problem
    {
        private PLArchitecture PrimaryArchitecture;
        public MyProblem(PLArchitecture architecture)
        {
            PrimaryArchitecture = architecture;
            // Count number of operators
            int operatorCount = PrimaryArchitecture.Components.Select(c => c.Interfaces.Select(o => o.Operators.Count()).Sum()).Sum();
            operatorCount += PrimaryArchitecture.Components.Select(c => c.DependedInterfaces.Select(o => o.Operators.Count()).Sum()).Sum();
            NumberOfVariables = operatorCount;
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

            UpperLimit = new double[NumberOfVariables];
            LowerLimit = new double[NumberOfVariables];

            for (int i = 0; i < NumberOfVariables; i++)
            {
                LowerLimit[i] = 0.0;
                UpperLimit[i] = 1.0;
            }

            SolutionType = new RealSolutionType(this);
        }
        public override void Evaluate(Solution solution)
        {
            double[] f = new double[NumberOfObjectives];
            XReal currentSolution = new XReal(solution);
            PLArchitecture currentArchitecture = GenerateArchitecture(currentSolution);
            //evaluate Cohesion
            f[(int)ObjectivSelection.OS_Cohesion] = EvalCohesion(currentArchitecture);
            //evaluate Coupling
            f[(int)ObjectivSelection.OS_Coupling] = EvalCoupling(currentArchitecture);
            //evaluate Granularity
            f[(int)ObjectivSelection.OS_Granularity] = EvalGranularity(currentArchitecture);
            //evaluate Feature-Scattering
            f[(int)ObjectivSelection.OS_FeatureScattering] = EvalFeatureScattering(currentArchitecture);
            //evaluate Feature-Interaction
            f[(int)ObjectivSelection.OS_FeatureInteraction] = EvalFeatureInteraction(currentArchitecture);
            // set objectives
            solution.Objective = f;

        }
        private PLArchitecture GenerateArchitecture(XReal solution)
        {
            // get operators
            List<PLAOperator> operators = new List<PLAOperator> { };
            for (int i = 0; i < PrimaryArchitecture.Components.Count; i++)
            {
                for (int j = 0; j < PrimaryArchitecture.Components[i].Interfaces.Count; j++)
                {
                    operators.AddRange(PrimaryArchitecture.Components[i].Interfaces[j].Operators);
                }
                for (int j = 0; j < PrimaryArchitecture.Components[i].DependedInterfaces.Count; j++)
                {
                    operators.AddRange(PrimaryArchitecture.Components[i].DependedInterfaces[j].Operators);
                }
            }
            // create interfaces
            int operatorCount = operators.Count;
            List<PLAInterface> interfaces = new List<PLAInterface> { };
            for (int o = 0; o < operators.Count; o++)
            {
                int currentSolutionIndex = (int)(solution.GetValue(o) * operatorCount);
                PLAInterface currentInterface = interfaces.Where(_interface => _interface.Id == currentSolutionIndex).SingleOrDefault();
                if (currentInterface == null)
                {
                    currentInterface = new PLAInterface();
                    currentInterface.Operators = new List<PLAOperator> { };
                    currentInterface.Id = currentSolutionIndex;
                    interfaces.Add(currentInterface);
                }
                currentInterface.Operators.Add(operators[o]);
            }
            // create components
            int interfaceCount = interfaces.Count;
            List<PLAComponent> components = new List<PLAComponent> { };
            for (int i = 0; i < interfaces.Count; i++)
            {
                int currentSolutionIndex = (int)(solution.GetValue(i) * interfaceCount);
                PLAComponent currentComponent = components.Where(_component => _component.Id == currentSolutionIndex).SingleOrDefault();
                if (currentComponent == null)
                {
                    currentComponent = new PLAComponent();
                    currentComponent.Interfaces = new List<PLAInterface> { };
                    currentComponent.Id = currentSolutionIndex;
                    components.Add(currentComponent);
                }
                currentComponent.Interfaces.Add(interfaces[i]);
            }
            return new PLArchitecture(components);
        }
        private double EvalCohesion(PLArchitecture pla)
        {
            // Definition Ref [Colanzi, Vergilio - 2014]
            // Definition : Average number of internal relationships per class in a component. 
            // get all interfaces count
            double totalDependecies = pla.Components.Select(c => c.Interfaces.Count()).Sum();
            // get component count
            double totalComponents = pla.Components.Count();
            // return average of interface per componenet.
            return totalDependecies / totalComponents;
        }
        private double EvalCoupling(PLArchitecture pla)
        {
            // Definition Ref [Colanzi, Vergilio - 2014]
            //// Definition : Number of packages on which classes and interfaces of this component depend.
            //double DepPack = 0;
            //// Definition : Number of elements that depend on this class.
            //double CDepIn = 0;
            //// Definition : Number of elements on which this class depends.
            //double CDepOut = 0;
            //// Definition : Number of UML dependencies where the package is the supplier.
            //double DepIn = 0;
            // Definition : Number of UML dependencies where the package is the client.
            try
            {
                double DepOut = pla.Components.Select(c => c.DependedInterfaces.Count()).Sum();
                return DepOut;
            }
            catch
            {
                return 0;
            }
        }
        private double EvalGranularity(PLArchitecture pla)
        {
            // Average of works that a component will do,
            // this mean, all count of operations that a component will throw,
            double avgWorks = pla.Components.Select(c => c.Interfaces.Select(i => i.Operators.Count()).Sum()).Sum();
            return avgWorks;
        }
        private double EvalFeatureScattering(PLArchitecture pla)
        {
            // Definition Ref [Colanzi, Vergilio - 2014]
            //Number of architectural components which contributes to the realization of a certain feature
            double CDAC = pla.Components.Count();
            //Number of interfaces in the system architecture which contributes to the realization of a certain feature
            double CDAI = pla.Components.Select(c => c.Interfaces.Count()).Sum();
            //Number of operations in the system architecture which contributes to the realization of a certain feature
            double CDAO = pla.Components.Select(c => c.Interfaces.Select(i => i.Operators.Count()).Sum()).Sum();
            return (CDAC + CDAI + CDAO);
        }
        private double EvalFeatureInteraction(PLArchitecture pla)
        {
            // Definition Ref [Colanzi, Vergilio - 2014]
            //Number of features with which the assessed feature share at least a component
            double CIBC = 0;
            //Number of features with which the assessed feature share at least an interface
            double IIBC = 0;
            //Number of features with which the assessed feature share at least an operation
            double OOBC = 0;
            return 0;
        }
    }
}
