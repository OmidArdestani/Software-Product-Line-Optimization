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
            double xScor = x.Objective[0];
            double yScor = y.Objective[0];
            return xScor < yScor ? 1 : 0;
        }
    }
}
