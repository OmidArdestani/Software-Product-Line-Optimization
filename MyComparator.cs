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

        public int Compare(Solution x, Solution y)
        {
            double xScor = MySelectionOperator.MathSolutionScore(x.Objective);
            double yScor = MySelectionOperator.MathSolutionScore(y.Objective);
            return xScor < yScor ? 1 : 0;
        }
    }
}
