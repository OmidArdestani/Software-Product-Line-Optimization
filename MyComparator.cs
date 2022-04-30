using JMetalCSharp.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPLAOptimization
{
    public class MyComparator : IComparer<Solution>
    {

        public int Compare(Solution bestKnown, Solution candidateSolution)
        {
            double maxObjectivSolution1 = bestKnown.Objective.Max();
            double maxObjectivSolution2 = candidateSolution.Objective.Max();
            return maxObjectivSolution1 > maxObjectivSolution2 ? 1 : -1;
        }
    }
}
