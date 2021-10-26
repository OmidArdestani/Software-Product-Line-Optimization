using JMetalCSharp.Core;
using JMetalCSharp.Encoding.SolutionType;
using JMetalCSharp.Encoding.Variable;
using JMetalCSharp.Utils;
using MyPLAOptimization;
using System;
using System.Collections.Generic;

namespace JMetalCSharp.Operators.Crossover
{
    /// <summary>
    /// This class allows to apply a Single Point crossover operator using two parent
    /// solutions.
    /// </summary>
    public class MSinglePointCrossover : Crossover
    {

        #region Private Attributes
        /// <summary>
        /// Valid solution types to apply this operator
        /// </summary>
        private static readonly List<Type> VALID_TYPES = new List<Type>()
        {
            typeof(BinarySolutionType),
            typeof(BinaryRealSolutionType),
            typeof(IntSolutionType)
        };

        private double? crossoverProbability = null;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// Creates a new instance of the single point crossover operator
        /// </summary>
        /// <param name="parameters"></param>
        public MSinglePointCrossover(Dictionary<string, object> parameters)
            : base(parameters)
        {
            Utils.Utils.GetDoubleValueFromParameter(parameters, "probability", ref crossoverProbability);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Perform the crossover operation.
        /// </summary>
        /// <param name="probability">Crossover probability</param>
        /// <param name="parent1">The first parent</param>
        /// <param name="parent2">The second parent</param>
        /// <returns>An array containig the two offsprings</returns>
        private Solution[] DoCrossover(double probability, Solution parent1, Solution parent2)
        {
            Solution[] offSpring = new Solution[2];

            offSpring[0] = new Solution(parent1);
            offSpring[1] = new Solution(parent2);

            try
            {
                if (JMetalRandom.NextDouble() < probability)
                {
                    int crossoverPoint = JMetalRandom.Next(0, parent1.NumberOfVariables() - 1);
                    double valueX1;
                    double valueX2;
                    double valueX12;
                    double valueX22;
                    for (int i = crossoverPoint; i < parent1.NumberOfVariables(); i++)
                    {
                        valueX1 = ((ArrayReal)parent1.Variable[0]).Array[i];
                        valueX2 = ((ArrayReal)parent2.Variable[0]).Array[i];
                        ((ArrayReal)offSpring[0].Variable[0]).Array[i] = valueX2;
                        ((ArrayReal)offSpring[1].Variable[0]).Array[i] = valueX1;
                        valueX12 = ((ArrayReal)parent1.Variable[1]).Array[i];
                        valueX22 = ((ArrayReal)parent2.Variable[1]).Array[i];
                        ((ArrayReal)offSpring[0].Variable[1]).Array[i] = valueX22;
                        ((ArrayReal)offSpring[1].Variable[1]).Array[i] = valueX12;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error in: " + this.GetType().FullName + ".DoCrossover()", ex);
                Console.WriteLine("Error in " + this.GetType().FullName + ".DoCrossover()");
                throw new Exception("Exception in " + this.GetType().FullName + ".DoCrossover()");
            }
            return offSpring;
        }

        #endregion

        #region Public Overrides

        /// <summary>
        /// Executes the operation
        /// </summary>
        /// <param name="obj">An object containing an array of two solutions</param>
        /// <returns>An object containing an array with the offSprings</returns>
        public override object Execute(object obj)
        {
            Solution[] parents = (Solution[])obj;

            if (parents.Length < 2)
            {
                Logger.Log.Error("Error in " + this.GetType().FullName + ".Execute()");
                Console.WriteLine("Error in " + this.GetType().FullName + ".Execute()");
                throw new Exception("Exception in " + this.GetType().FullName + ".Execute()");
            }

            Solution[] offSpring;
            offSpring = DoCrossover(crossoverProbability.Value, parents[0], parents[1]);

            //-> Update the offSpring solutions
            for (int i = 0; i < offSpring.Length; i++)
            {
                offSpring[i].CrowdingDistance = 0.0;
                offSpring[i].Rank = 0;
            }
            return offSpring;
        }

        #endregion
    }
}
