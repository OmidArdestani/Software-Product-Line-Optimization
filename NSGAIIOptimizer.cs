using JMetalCSharp.Core;
using JMetalCSharp.Operators.Crossover;
using JMetalCSharp.Operators.Mutation;
using JMetalCSharp.Operators.Selection;
using JMetalCSharp.Problems;
using JMetalCSharp.Problems.Kursawe;
using JMetalCSharp.Problems.ZDT;
using JMetalCSharp.QualityIndicator;
using JMetalCSharp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyPLAOptimization
{
    class NSGAIIOptimizer
    {
        public event Func<bool> OptimizationFinished;
        public event Func<string, bool> AlgorithmOutput;
        public enum ChromosomeType { PT_Real, PT_Integer, PT_Binary }
        private List<PLAComponent> Architecture = null;
        private int PopulationSize = 100;
        private int MaxEvaluation = 10;
        private ChromosomeType pType = ChromosomeType.PT_Real;
        /// <summary>
        /// Set the architecture that need optimization
        /// </summary>
        /// <param name="architecture">The Architecture that exported from Model</param>
        public void SetArchitecture(List<PLAComponent> architecture)
        {
            this.Architecture = architecture;
        }
        /// <summary>
        /// Run optimizarion asyncron
        /// </summary>
        public async Task StartAsync()
        {
            await Task.Run(() =>
            {
                Thread.Sleep(1000);
                Problem problem; // The problem to solve
                Algorithm algorithm; // The algorithm to use
                Operator crossover; // Crossover operator
                Operator mutation; // Mutation operator
                Operator selection; // Selection operator

                // Count number of operators
                int operatorCount = this.Architecture.Select(c => c.Interfaces.Select(o => o.Operators.Count()).Sum()).Sum();
                operatorCount += this.Architecture.Select(c => c.DependedInterfaces.Select(o => o.Operators.Count()).Sum()).Sum();
                AlgorithmOutput(string.Format("Operator count = {0}", operatorCount));
                Dictionary<string, object> parameters; // Operator parameters

                QualityIndicator indicators = null; // Object to get quality indicators
                
                // create var type
                switch (pType)
                {
                    case ChromosomeType.PT_Real:
                    case ChromosomeType.PT_Integer:
                        problem = new Kursawe("Real", operatorCount);
                        break;
                    case ChromosomeType.PT_Binary:
                        problem = new Kursawe("BinaryReal", operatorCount);
                        break;
                    default:
                        problem = new Kursawe("Real", operatorCount);
                        break;
                }
                // Also we can use these chromosom types
                //problem = new Kursawe("BinaryReal", 3);
                //problem = new Water("Real");
                //problem = new ZDT3("ArrayReal", 30);
                //problem = new ConstrEx("Real");
                //problem = new DTLZ1("Real");
                //problem = new OKA2("Real") ;

                // contruct algorithm
                algorithm = new JMetalCSharp.Metaheuristics.NSGAII.NSGAII(problem);

                // Algorithm parameters
                algorithm.SetInputParameter("populationSize", this.PopulationSize);
                algorithm.SetInputParameter("maxEvaluations", this.MaxEvaluation);

                // Mutation and Crossover for Real codification 
                parameters = new Dictionary<string, object>();
                parameters.Add("probability", 0.9);
                parameters.Add("distributionIndex", 20.0);
                crossover = CrossoverFactory.GetCrossoverOperator("SBXCrossover", parameters);

                parameters = new Dictionary<string, object>();
                parameters.Add("probability", 1.0 / problem.NumberOfVariables);
                parameters.Add("distributionIndex", 20.0);
                mutation = MutationFactory.GetMutationOperator("PolynomialMutation", parameters);

                // Selection Operator 
                parameters = null;
                selection = SelectionFactory.GetSelectionOperator("BinaryTournament2", parameters);

                // Add the operators to the algorithm
                algorithm.AddOperator("crossover", crossover);
                algorithm.AddOperator("mutation", mutation);
                algorithm.AddOperator("selection", selection);

                // Add the indicator object to the algorithm
                algorithm.SetInputParameter("indicators", indicators);

                // Execute the Algorithm
                long initTime = Environment.TickCount;
                SolutionSet population = algorithm.Execute();
                long estimatedTime = Environment.TickCount - initTime;

                // Result messages 
                AlgorithmOutput("Total execution time: " + estimatedTime + "ms");
                AlgorithmOutput("Variables values have been writen to file VAR");
                population.PrintVariablesToFile("VAR");
                AlgorithmOutput("Objectives values have been writen to file FUN");
                population.PrintObjectivesToFile("FUN");
                Console.WriteLine("Time: " + estimatedTime);
                Console.ReadLine();
                if (indicators != null)
                {
                    AlgorithmOutput("Quality indicators");
                    AlgorithmOutput("Hypervolume: " + indicators.GetHypervolume(population));
                    AlgorithmOutput("GD         : " + indicators.GetGD(population));
                    AlgorithmOutput("IGD        : " + indicators.GetIGD(population));
                    AlgorithmOutput("Spread     : " + indicators.GetSpread(population));
                    AlgorithmOutput("Epsilon    : " + indicators.GetEpsilon(population));

                    int evaluations = (int)algorithm.GetOutputParameter("evaluations");
                    AlgorithmOutput("Speed      : " + evaluations + " evaluations");
                }
            });
        }
        /// <summary>
        /// Config the Optimization application
        /// </summary>
        /// <param name="populationSize">Size of begin population</param>
        /// <param name="maxEvaluations">Maximum evaluation count</param>
        /// <param name="type">Type of variable</param>
        public void Configuration(int populationSize, int maxEvaluations, ChromosomeType type = ChromosomeType.PT_Real)
        {
            this.PopulationSize = populationSize;
            this.MaxEvaluation = maxEvaluations;
            pType = type;
        }
    }
}
