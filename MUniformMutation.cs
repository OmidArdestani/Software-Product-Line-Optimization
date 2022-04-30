using JMetalCSharp.Core;
using JMetalCSharp.Encoding.SolutionType;
using JMetalCSharp.Encoding.Variable;
using JMetalCSharp.Utils;
using JMetalCSharp.Utils.Wrapper;
using MyPLAOptimization;
using System;
using System.Collections.Generic;

namespace JMetalCSharp.Operators.Mutation
{
    /// <summary>
    /// This class implements a uniform mutation operator.
    /// </summary>
    public class MUniformMutation : Mutation
    {
        #region Private Attributes
        /// <summary>
        /// Valid solution types to apply this operator
        /// </summary>
        private static readonly List<Type> VALID_TYPES = new List<Type>()
        {
            typeof(RealSolutionType),
            typeof(ArrayRealSolutionType)
        };

        /// <summary>
        /// Stores the value used in a uniform mutation operator
        /// </summary>
        private double? perturbation;

        private double? mutationProbability = null;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor 
        /// Creates a new uniform mutation operator instance
        /// </summary>
        /// <param name="parameters"></param>
        public MUniformMutation(Dictionary<string, object> parameters)
            : base(parameters)
        {
            Utils.Utils.GetDoubleValueFromParameter(parameters, "probability", ref mutationProbability);
            Utils.Utils.GetDoubleValueFromParameter(parameters, "perturbation", ref perturbation);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Performs the operation
        /// </summary>
        /// <param name="probability">Mutation probability</param>
        /// <param name="solution">The solution to mutate</param>
        private void DoMutation(double probability, Solution solution)
        {
            for (int var = 0; var < solution.Variable.Length; var++)
            {
                for (int var2 = 0; var2 < ((ArrayReal)solution.Variable[var]).Array.Length; var2++)
                {
                    // first row
                    if (JMetalRandom.NextDouble() < probability)
                    {
                        double rand = JMetalRandom.NextDouble();
                        double tmp = (rand - 0.5) * perturbation.Value;
                        tmp += ((ArrayReal)solution.Variable[var]).Array[var2];

                        if (tmp < ((ArrayReal)solution.Variable[var]).GetLowerBound(var2))
                        {
                            tmp = ((ArrayReal)solution.Variable[var]).GetLowerBound(var2);
                        }
                        else if (tmp > ((ArrayReal)solution.Variable[var]).GetUpperBound(var2))
                        {
                            tmp = ((ArrayReal)solution.Variable[var]).GetUpperBound(var2);
                        }
                        ((ArrayReal)solution.Variable[var]).Array[var2] = tmp;
                    }
                }
            }
        }

        #endregion

        #region Public Overrides

        /// <summary>
        /// Executes the operation
        /// </summary>
        /// <param name="obj">An object containing the solution to mutate</param>
        /// <returns></returns>
        public override object Execute(object obj)
        {
            Solution solution = (Solution)obj;
            DoMutation(mutationProbability.Value, solution);

            return solution;
        }

        #endregion
    }
}
