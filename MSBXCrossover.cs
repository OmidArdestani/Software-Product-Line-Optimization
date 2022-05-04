using JMetalCSharp.Core;
using JMetalCSharp.Encoding.SolutionType;
using JMetalCSharp.Encoding.Variable;
using JMetalCSharp.Utils;
using JMetalCSharp.Utils.Wrapper;
using System;
using System.Collections.Generic;

namespace JMetalCSharp.Operators.Crossover
{
	/// <summary>
	/// This class allows to apply a SBX crossover operator using two parent
	/// solutions.
	/// </summary>
	public class MSBXCrossover : Crossover
	{
		#region Private Attributes

		/// <summary>
		/// EPS defines the minimum difference allowed between real values
		/// </summary>
		private static readonly double EPS = 1.0e-14;

		private static readonly double ETA_C_DEFAULT = 20.0;
		private double crossoverProbability = 0.9;
		private double distributionIndex = ETA_C_DEFAULT;

		/// <summary>
		/// Valid solution types to apply this operator
		/// </summary>
		private static readonly List<Type> VALID_TYPES = new List<Type>()
        {
            typeof(RealSolutionType),
            typeof(ArrayRealSolutionType)
        };

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor Create a new SBX crossover operator whit a default index
		/// given by <code>DEFAULT_INDEX_CROSSOVER</code>
		/// </summary>
		/// <param name="parameters"></param>
		public MSBXCrossover(Dictionary<string, object> parameters)
			: base(parameters)
		{
			Utils.Utils.GetDoubleValueFromParameter(parameters, "probability", ref crossoverProbability);
			Utils.Utils.GetDoubleValueFromParameter(parameters, "distributionIndex", ref distributionIndex);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Perform the crossover operation.
		/// </summary>
		/// <param name="probability">Crossover probability</param>
		/// <param name="parent1">The first parent</param>
		/// <param name="parent2">The second parent</param>
		/// <returns>An array containing the two offsprings</returns>
		private Solution[] DoCrossover(double probability, Solution parent1, Solution parent2)
		{

			Solution[] offSpring = new Solution[2];

			offSpring[0] = new Solution(parent1);
			offSpring[1] = new Solution(parent2);

			int i;
			double rand;
			double y1, y2, yL, yu;
			double c1, c2;
			double alpha, beta, betaq;
			double valueX1, valueX2;
            //
            double y1_2, y2_2, yL_2, yu_2;
            double c1_2, c2_2;
            double alpha_2, beta_2, betaq_2;
            double valueX1_2, valueX2_2;

            int numberOfVariables = parent1.NumberOfVariables();

            if (JMetalRandom.NextDouble() <= probability)
            {
                for (i = 0; i < numberOfVariables; i++)
                {
                    valueX1 = ((ArrayReal)parent1.Variable[0]).Array[i];
                    valueX2 = ((ArrayReal)parent2.Variable[0]).Array[i];
                    valueX1_2 = ((ArrayReal)parent1.Variable[1]).Array[i];
                    valueX2_2 = ((ArrayReal)parent2.Variable[1]).Array[i];
                    if (JMetalRandom.NextDouble() <= 0.5)
                    {
                        if (Math.Abs(valueX1 - valueX2) > EPS)
                        {
                            if (valueX1 < valueX2)
                            {
                                y1 = valueX1;
                                y2 = valueX2;
                            }
                            else
                            {
                                y1 = valueX2;
                                y2 = valueX1;
                            }

                            yL = ((ArrayReal)parent1.Variable[0]).GetLowerBound(i);
                            yu = ((ArrayReal)parent1.Variable[0]).GetUpperBound(i);
                            rand = JMetalRandom.NextDouble();
                            beta = 1.0 + (2.0 * (y1 - yL) / (y2 - y1));
                            alpha = 2.0 - Math.Pow(beta, -(distributionIndex + 1.0));

                            if (rand <= (1.0 / alpha))
                            {
                                betaq = Math.Pow((rand * alpha), (1.0 / (distributionIndex + 1.0)));
                            }
                            else
                            {
                                betaq = Math.Pow((1.0 / (2.0 - rand * alpha)), (1.0 / (distributionIndex + 1.0)));
                            }

                            c1 = 0.5 * ((y1 + y2) - betaq * (y2 - y1));
                            beta = 1.0 + (2.0 * (yu - y2) / (y2 - y1));
                            alpha = 2.0 - Math.Pow(beta, -(distributionIndex + 1.0));

                            if (rand <= (1.0 / alpha))
                            {
                                betaq = Math.Pow((rand * alpha), (1.0 / (distributionIndex + 1.0)));
                            }
                            else
                            {
                                betaq = Math.Pow((1.0 / (2.0 - rand * alpha)), (1.0 / (distributionIndex + 1.0)));
                            }

                            c2 = 0.5 * ((y1 + y2) + betaq * (y2 - y1));

                            if (c1 < yL)
                            {
                                c1 = yL;
                            }

                            if (c2 < yL)
                            {
                                c2 = yL;
                            }

                            if (c1 > yu)
                            {
                                c1 = yu;
                            }

                            if (c2 > yu)
                            {
                                c2 = yu;
                            }

                            if (JMetalRandom.NextDouble() <= 0.5)
                            {
                                ((ArrayReal)parent1.Variable[0]).Array[i] = c2;
                                ((ArrayReal)parent2.Variable[0]).Array[i] = c1;
                            }
                            else
                            {
                                ((ArrayReal)parent1.Variable[0]).Array[i] = c1;
                                ((ArrayReal)parent2.Variable[0]).Array[i] = c2;
                            }
                        }
                        else
                        {
                            ((ArrayReal)parent1.Variable[0]).Array[i] = valueX1;
                            ((ArrayReal)parent2.Variable[0]).Array[i] = valueX2;
                        }
                        //
                        if (Math.Abs(valueX1_2 - valueX2_2) > EPS)
                        {
                            if (valueX1_2 < valueX2_2)
                            {
                                y1_2 = valueX1_2;
                                y2_2 = valueX2_2;
                            }
                            else
                            {
                                y1_2 = valueX2_2;
                                y2_2 = valueX1_2;
                            }

                            yL_2 = ((ArrayReal)parent1.Variable[1]).GetLowerBound(i);
                            yu_2 = ((ArrayReal)parent1.Variable[1]).GetUpperBound(i);
                            rand = JMetalRandom.NextDouble();
                            beta_2 = 1.0 + (2.0 * (y1_2 - yL_2) / (y2_2 - y1_2));
                            alpha_2 = 2.0 - Math.Pow(beta_2, -(distributionIndex + 1.0));

                            if (rand <= (1.0 / alpha_2))
                            {
                                betaq_2 = Math.Pow((rand * alpha_2), (1.0 / (distributionIndex + 1.0)));
                            }
                            else
                            {
                                betaq_2 = Math.Pow((1.0 / (2.0 - rand * alpha_2)), (1.0 / (distributionIndex + 1.0)));
                            }

                            c1_2 = 0.5 * ((y1_2 + y2_2) - betaq_2 * (y2_2 - y1_2));
                            beta_2 = 1.0 + (2.0 * (yu_2 - y2_2) / (y2_2 - y1_2));
                            alpha_2 = 2.0 - Math.Pow(beta_2, -(distributionIndex + 1.0));

                            if (rand <= (1.0 / alpha_2))
                            {
                                betaq_2 = Math.Pow((rand * alpha_2), (1.0 / (distributionIndex + 1.0)));
                            }
                            else
                            {
                                betaq_2 = Math.Pow((1.0 / (2.0 - rand * alpha_2)), (1.0 / (distributionIndex + 1.0)));
                            }

                            c2_2 = 0.5 * ((y1_2 + y2_2) + betaq_2 * (y2_2 - y1_2));

                            if (c1_2 < yL_2)
                            {
                                c1_2 = yL_2;
                            }

                            if (c2_2 < yL_2)
                            {
                                c2_2 = yL_2;
                            }

                            if (c1_2 > yu_2)
                            {
                                c1_2 = yu_2;
                            }

                            if (c2_2 > yu_2)
                            {
                                c2_2 = yu_2;
                            }

                            if (JMetalRandom.NextDouble() <= 0.5)
                            {
                                ((ArrayReal)parent1.Variable[1]).Array[i] = c2_2;
                                ((ArrayReal)parent2.Variable[1]).Array[i] = c1_2;
                            }
                            else
                            {
                                ((ArrayReal)parent1.Variable[1]).Array[i] = c1_2;
                                ((ArrayReal)parent2.Variable[1]).Array[i] = c2_2;
                            }
                        }
                        else
                        {
                            ((ArrayReal)parent1.Variable[1]).Array[i] = valueX1;
                            ((ArrayReal)parent2.Variable[1]).Array[i] = valueX2;
                        }
                    }
                }
            }
			return offSpring;
		}

		#endregion

		#region Public Overrides

		/// <summary>
		/// Executes the operation
		/// </summary>
		/// <param name="obj">An object containing an array of two parents</param>
		/// <returns>An object containing the offSprings</returns>
		public override object Execute(object obj)
		{
			Solution[] parents = (Solution[])obj;

			if (parents.Length != 2)
			{
				Logger.Log.Error("Exception in " + this.GetType().FullName + ".Execute()");
				Console.WriteLine("Exception in " + this.GetType().FullName + ".Execute()");
				throw new Exception("Exception in " + this.GetType().FullName + ".Execute()");
			}

			Solution[] offSpring;
			offSpring = DoCrossover(crossoverProbability, parents[0], parents[1]);

			return offSpring;
		}

		#endregion
	}
}
