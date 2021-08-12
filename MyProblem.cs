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
        private PLArchitecture Architecture;
        public MyProblem(PLArchitecture architecture)
        {
            Architecture = architecture;
            // Count number of operators
            int operatorCount = Architecture.Components.Select(c => c.Interfaces.Select(o => o.Operators.Count()).Sum()).Sum();
            operatorCount += Architecture.Components.Select(c => c.DependedInterfaces.Select(o => o.Operators.Count()).Sum()).Sum();
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
            for (int i = 0; i < Architecture.Components.Count; i++)
            {
                for (int j = 0; j < Architecture.Components[i].Interfaces.Count; j++)
                {
                    operators.AddRange(Architecture.Components[i].Interfaces[j].Operators);
                }
                for (int j = 0; j < Architecture.Components[i].DependedInterfaces.Count; j++)
                {
                    operators.AddRange(Architecture.Components[i].DependedInterfaces[j].Operators);
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
            throw new Exception("EvalCohesion not implemented");
        }
        private double EvalCoupling(PLArchitecture pla)
        {
            throw new Exception("EvalCoupling not implemented");
        }
        private double EvalGranularity(PLArchitecture pla)
        {
            throw new Exception("EvalGranularity not implemented");
        }
        private double EvalFeatureScattering(PLArchitecture pla)
        {
            throw new Exception("EvalFeatureScattering not implemented");
        }
        private double EvalFeatureInteraction(PLArchitecture pla)
        {
            throw new Exception("EvalFeatureInteraction not implemented");
        }
    }
}
