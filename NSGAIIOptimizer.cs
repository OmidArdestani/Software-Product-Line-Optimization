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
        private PLArchitecture Architecture = null;
        private int PopulationSize = 100;
        private int MaxEvaluation = 10;
        private List<KeyValuePair<List<string>, List<string>>> UsageInterfaceRelationship = new List<KeyValuePair<List<string>, List<string>>> { };
        /// <summary>
        /// Set the architecture that need optimization
        /// </summary>
        /// <param name="architecture">The Architecture that exported from Model</param>
        private void SetArchitecture(PLArchitecture architecture)
        {
            this.Architecture = architecture;
            // create a multi dimention matrix of interface relationships.
            // any operator in each interface depend on each operator in any dependecie interface.
            UsageInterfaceRelationship.Clear();
            for (int c = 0; c < architecture.Components.Count; c++)
            {
                for (int i = 0; i < architecture.Components[c].Interfaces.Count; i++)
                {
                    var currentInterfaceId = architecture.Components[c].Interfaces[i].Operators.Select(o => o.Id).ToList();
                    var dependencies = new List<string> { };
                    for (int d = 0; d < architecture.Components[c].DependedInterfaces.Count; d++)
                    {
                        dependencies.AddRange(architecture.Components[c].DependedInterfaces[d].Operators.Select(o=>o.Id).ToList());
                    }
                    KeyValuePair<List<string>, List<string>> operatorDependencies = new KeyValuePair<List<string>, List<string>>(currentInterfaceId, dependencies);
                    UsageInterfaceRelationship.Add(operatorDependencies);
                }
            }
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

                AlgorithmOutput(string.Format("Operator count = {0}", PopulationSize));
                Dictionary<string, object> parameters; // Operator parameters

                problem = new MyProblem(this.Architecture,UsageInterfaceRelationship);
                //problem = new Kursawe("Real", 3);

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
                selection = new MySelectionOperator(parameters);
                //selection = SelectionFactory.GetSelectionOperator("BinaryTournament2", parameters);

                // Add the operators to the algorithm
                algorithm.AddOperator("crossover", crossover);
                algorithm.AddOperator("mutation", mutation);
                algorithm.AddOperator("selection", selection);

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
                AlgorithmOutput("Time: " + estimatedTime);
            });
        }
        /// <summary>
        /// Config the Optimization application
        /// </summary>
        /// <param name="architecture">The architecture that is optimizing</param>
        /// <param name="maxEvaluations">Maximum evaluation count</param>
        public void Configuration(PLArchitecture architecture, int maxEvaluations,int populationSize)
        {
            this.Architecture = architecture;
            this.PopulationSize = populationSize;
            this.MaxEvaluation = maxEvaluations;
            this.SetArchitecture(architecture);
        }
    }
}
