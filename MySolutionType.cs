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
	public class MyReal2DSolutionType : SolutionType
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="problem">Problem to solve</param>
		public MyReal2DSolutionType(Problem problem)
			: base(problem)
		{

		}

		public override Variable[] CreateVariables()
		{
			Variable[] variables = new Variable[2];

			for (int i = 0; i < 2; i++)
			{
				variables[i] = new ArrayReal(Problem.NumberOfVariables, Problem);
			}
			return variables;
		}
	}
}
