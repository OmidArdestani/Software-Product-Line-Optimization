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
    class MyProblem : Problem
    {
        private List<PLAComponent> Architecture;
        public MyProblem(List<PLAComponent> architecture)
        {
            Architecture = architecture;
            // Count number of operators
            int operatorCount = Architecture.Select(c => c.Interfaces.Select(o => o.Operators.Count()).Sum()).Sum();
            operatorCount += Architecture.Select(c => c.DependedInterfaces.Select(o => o.Operators.Count()).Sum()).Sum();
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
                UpperLimit[i] = NumberOfVariables;
            }

            SolutionType = new RealSolutionType(this);
        }
        public override void Evaluate(Solution solution)
        {
            double[] f = new double[NumberOfObjectives];
            XReal currentSolution = new XReal(solution);
            List<PLAComponent> currentArchitecture = GenerateArchitecture(currentSolution);
            //evaluate Cohesion
            f[0] = EvalCohesion(currentArchitecture);
            //evaluate Coupling
            f[1] = EvalCoupling(currentArchitecture);
            //evaluate Granularity
            f[2] = EvalGranularity(currentArchitecture);
            //evaluate Feature-Scattering
            f[3] = EvalFeatureScattering(currentArchitecture);
            //evaluate Feature-Interaction
            f[4] = EvalFeatureInteraction(currentArchitecture);
            // set objectives
            solution.Objective = f;

        }
        private List<PLAComponent> GenerateArchitecture(XReal solution)
        {
            throw new Exception("GenerateArchitecture not implemented");
        }
        private double EvalCohesion(List<PLAComponent> pla)
        {
            throw new Exception("EvalCohesion not implemented");
        }
        private double EvalCoupling(List<PLAComponent> pla)
        {
            throw new Exception("EvalCoupling not implemented");
        }
        private double EvalGranularity(List<PLAComponent> pla)
        {
            throw new Exception("EvalGranularity not implemented");
        }
        private double EvalFeatureScattering(List<PLAComponent> pla)
        {
            throw new Exception("EvalFeatureScattering not implemented");
        }
        private double EvalFeatureInteraction(List<PLAComponent> pla)
        {
            throw new Exception("EvalFeatureInteraction not implemented");
        }
    }
}
