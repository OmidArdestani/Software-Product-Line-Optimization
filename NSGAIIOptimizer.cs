using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPLAOptimization
{
    class NSGAIIOptimizer
    {

        public event Func<bool> OptimizationFinished;
        public enum PopulationType { PT_Real,PT_Integer,PT_Binary}
        private List<XMIComponent> Architecture = null;
        public void SetArchitecture(List<XMIComponent> architecture)
        {

            Architecture = architecture;
        }

        public void Start()
        {

        }

        public void Configuration(int populationSize, PopulationType type,int maxEvaluations)
        {

        }
    }
}
