using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JMetalCSharp.Core;
using JMetalCSharp.Encoding.Variable;

namespace MyPLAOptimization
{
	/// <summary>
	/// Class representing a solution type composed of real variables
	/// </summary>
	public class Real2DSolutionType : SolutionType
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="problem">Problem to solve</param>
		public Real2DSolutionType(Problem problem)
			: base(problem)
		{

		}

		public override Variable[] CreateVariables()
		{
			Variable[] variables = new Variable[Problem.NumberOfVariables];

			for (int i = 0, li = Problem.NumberOfVariables; i < li; i++)
			{
				variables[i] = new ArrayReal(2, Problem);
			}
			return variables;
		}
	}
}
