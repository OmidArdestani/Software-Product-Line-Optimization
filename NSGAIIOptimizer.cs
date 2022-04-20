using JMetalCSharp.Core;
using JMetalCSharp.Encoding.Variable;
using JMetalCSharp.Operators.Crossover;
using JMetalCSharp.Operators.Mutation;
using JMetalCSharp.Operators.Selection;
using JMetalCSharp.Problems;
using JMetalCSharp.Problems.Kursawe;
using JMetalCSharp.Problems.ZDT;
using JMetalCSharp.QualityIndicator;
using JMetalCSharp.Utils;
using JMetalCSharp.Utils.Wrapper;
using read_feature_model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyPLAOptimization
{
    class NSGAIIOptimizer
    {
        public event Func<bool> OptimizationFinished;
        public event Func<string, bool> AlgorithmOutput;
        public MyProblem problem; // The problem to solve
        private PLArchitecture Architecture = null;
        private int PopulationSize = 100;
        private SolutionSet population = null;
        public int MaxEvaluation { get; set; }
        private FeatureModel featureModel;
        private List<FeatureRelationship> featureRelationshipMatrix;
        public PLArchitecture BestPLA { get; set; }
        private List<KeyValuePair<List<string>, List<string>>> UsageOperationsRelationship = new List<KeyValuePair<List<string>, List<string>>> { };
        public ProgressBar ProccessProgressBar { get; set; }
        public NSGAIIOptimizer()
        {
            MaxEvaluation = 10;
        }
        /// <summary>
        /// Set the architecture that need optimization
        /// </summary>
        /// <param name="architecture">The Architecture that exported from Model</param>
        private void SetArchitecture(PLArchitecture architecture)
        {
            this.Architecture = architecture;
            // create a multi dimention matrix of interface relationships.
            // any operator in each interface depend on each operator in any dependecie interface.
            UsageOperationsRelationship.Clear();
            for (int c = 0; c < architecture.Components.Count; c++)
            {
                for (int i = 0; i < architecture.Components[c].Interfaces.Count; i++)
                {
                    var currentOperationsId = architecture.Components[c].Interfaces[i].Operations.Select(o => o.Id).ToList();
                    var dependencies = new List<string> { };
                    for (int d = 0; d < architecture.Components[c].DependedInterfaces.Count; d++)
                    {
                        dependencies.AddRange(architecture.Components[c].DependedInterfaces[d].Operations.Select(o => o.Id).ToList());
                    }
                    KeyValuePair<List<string>, List<string>> operatorDependencies = new KeyValuePair<List<string>, List<string>>(currentOperationsId, dependencies);
                    UsageOperationsRelationship.Add(operatorDependencies);
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
                ProccessProgressBar.Minimum = 0;
                ProccessProgressBar.Maximum = MaxEvaluation;
                JMetalCSharp.Metaheuristics.NSGAII.NSGAII algorithm; // The algorithm to use
                Operator crossover; // Crossover operator
                Operator mutation; // Mutation operator
                Operator selection; // Selection operator

                AlgorithmOutput("Optimization is running...");
                Dictionary<string, object> parameters; // Operator parameters

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
                crossover = new MSinglePointCrossover(parameters);

                parameters = new Dictionary<string, object>();
                parameters.Add("probability", 1.0 / problem.NumberOfVariables);
                //parameters.Add("distributionIndex", 20.0);
                parameters.Add("perturbation", 0.5); // half of my data range
                mutation = new MUniformMutation(parameters);

                // Selection Operator 
                parameters = null;
                selection = SelectionFactory.GetSelectionOperator("BinaryTournament2", parameters);

                // Add the operators to the algorithm
                algorithm.AddOperator("crossover", crossover);
                algorithm.AddOperator("mutation", mutation);
                algorithm.AddOperator("selection", selection);

                // Execute the Algorithm
                long initTime = Environment.TickCount;
                algorithm.ProcessProgress += GetProccessProgress;
                population = algorithm.Execute();
                long estimatedTime = Environment.TickCount - initTime;
                IComparer<Solution> comp = new MyComparator();
                BestPLA = problem.GenerateArchitecture(population.Best(comp));
                // Result messages 
                //AlgorithmOutput("Total execution time: " + estimatedTime + " ms");
                //AlgorithmOutput("Variables values have been writen to file VAR");
                //PrintVariablesToFile("VAR", population);
                //AlgorithmOutput("Objectives values have been writen to file FUN\n");
                //population.PrintObjectivesToFile("FUN");
                // output
                //AlgorithmOutput("Best PLA has:");
                //AlgorithmOutput("Components : " + BestPLA.ComponentCount);
                //AlgorithmOutput("Interfaces : " + BestPLA.InterfaceCount);
                //AlgorithmOutput("Operators : " + BestPLA.OperatorCount);
                OptimizationFinished();
            });
        }

        public void ExportVarData(string fileAddress)
        {
            PrintVariablesToFile(fileAddress, population);
        }
        public void ExportFuncData(string fileAddress)
        {
            population.PrintObjectivesToFile(fileAddress);
        }
        private void PrintVariablesToFile(string path, SolutionSet solutionSet)
        {
            try
            {
                if (solutionSet.Size() > 0)
                {
                    int numberOfVariables = ((ArrayReal)(solutionSet.SolutionsList.ElementAt(0).Variable[0])).Array.Count();
                    using (StreamWriter outFile = new StreamWriter(path))
                    {
                        foreach (Solution s in solutionSet.SolutionsList)
                        {
                            for (int i = 0; i < numberOfVariables; i++)
                            {
                                var firstRow = ((ArrayReal)(s.Variable[0])).Array[i];
                                var secondRow = ((ArrayReal)(s.Variable[1])).Array[i];
                                outFile.Write(string.Format("({0},{1})", firstRow, secondRow));
                            }
                            outFile.Write("\r\n");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("SolutionSet.PrintVariablesToFile", ex);
                Console.WriteLine(ex.StackTrace);
            }
        }
        public bool GetProccessProgress(int value)
        {
            ProccessProgressBar.Value = value;
            return true;
        }
        /// <summary>
        /// Config the Optimization application
        /// </summary>
        /// <param name="architecture">The architecture that is optimizing</param>
        /// <param name="maxEvaluations">Maximum evaluation count</param>
        public void Configuration(PLArchitecture architecture, FeatureModel featureModel, List<FeatureRelationship> featureRelationship, int maxEvaluations, int populationSize)
        {
            this.featureRelationshipMatrix = featureRelationship;
            this.Architecture = architecture;
            this.PopulationSize = populationSize;
            this.MaxEvaluation = maxEvaluations;
            this.featureModel = featureModel;
            this.SetArchitecture(architecture);

            problem = new MyProblem(this.Architecture, UsageOperationsRelationship, featureModel, featureRelationshipMatrix);
        }
    }
}
