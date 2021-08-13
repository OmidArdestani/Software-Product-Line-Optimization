using JMetalCSharp.Core;
using JMetalCSharp.Operators.Selection;
using JMetalCSharp.Utils.Comparators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPLAOptimization
{
    class MySelectionOperator : Selection
    {
        public MySelectionOperator(Dictionary<string, object> parameters) : base(parameters)
        {
        }
        private double MathSolutionScore(double[] objectivs)
        {
            double score = 0;

            score += objectivs[(int)ObjectivSelection.OS_Cohesion];
            score -= objectivs[(int)ObjectivSelection.OS_Coupling];
            score += objectivs[(int)ObjectivSelection.OS_Granularity]; //?
            score += objectivs[(int)ObjectivSelection.OS_FeatureScattering];
            score += objectivs[(int)ObjectivSelection.OS_FeatureInteraction];

            return score;
        }
        public override object Execute(object obj)
        {
            SolutionSet solutionSet = (SolutionSet)obj;

            if (solutionSet.Size() == 0)
            {
                return null;
            }
            double bestScore = -1;
            Solution bestSolution = null;

            for (int i = 1; i < solutionSet.Size(); i++)
            {
                Solution currentSolution = solutionSet.Get(i);
                // calc score fot current solution
                double scoreTemp = MathSolutionScore(currentSolution.Objective);
                // if this score is more than last score, save the new solution and score
                if (scoreTemp > bestScore)
                {
                    bestScore = scoreTemp;
                    bestSolution = currentSolution;
                }
            }
            return bestSolution;
        }
    }
}
