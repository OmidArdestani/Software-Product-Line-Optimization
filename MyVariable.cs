using JMetalCSharp.Core;
using JMetalCSharp.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPLAOptimization
{
    class Real2D : Variable
    {
        /// <summary>
        /// Get or Set the value of the <code>Real</code> encodings.variable.
        /// </summary>
        public new double[] Value { get; set; }
        /// <summary>
        /// Get or Set the lower bound of the <code>Real</code> encodings.variable.
        /// </summary>
        public new double LowerBound { get; set; }

        /// <summary>
        /// Get or Set the upper bound of the <code>Real</code> encodings.variable.
        /// </summary>
        public new double UpperBound { get; set; }

        public Real2D()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="lowerBound">Lower limit for the encodings.variable</param>
        /// <param name="upperBound">Upper limit for the encodings.variable</param>
        public Real2D(double lowerBound, double upperBound)
        {
            this.LowerBound = lowerBound;
            this.UpperBound = upperBound;
            Value = new double[2];
            this.Value[0] = JMetalRandom.NextDouble() * (upperBound - lowerBound) + lowerBound;
            this.Value[1] = JMetalRandom.NextDouble() * (upperBound - lowerBound) + lowerBound;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="lowerBound">Lower limit for the encodings.variable</param>
        /// <param name="upperBound">Upper limit for the encodings.variable</param>
        /// <param name="value">Value of the variable</param>
        public Real2D(double lowerBound, double upperBound, double[] value)
        {
            this.LowerBound = lowerBound;
            this.UpperBound = upperBound;
            this.Value = value;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="variable">The encodings.variable to copy.</param>
        public Real2D(Variable variable)
        {
            this.LowerBound = ((Real2D)variable).LowerBound;
            this.UpperBound = ((Real2D)variable).UpperBound;
            this.Value = ((Real2D)variable).Value;
        }

        /// <summary>
        /// Returns a exact copy of the <code>Real</code> encodings.variable
        /// </summary>
        /// <returns>the copy</returns>
        public override Variable DeepCopy()
        {
            JMetalCSharp.Core.Variable result;
            try
            {
                result = new Real2D(this);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(this.GetType().FullName + ".DeepCopy()", ex);
                Debug.WriteLine("Error in " + this.GetType().FullName + ".DeepCopy()");
                result = null;
            }

            return result;
        }

        // <summary>
        /// Returns a string representing the object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Join(",", Value);
        }

    }
}
