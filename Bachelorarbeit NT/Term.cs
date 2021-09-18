using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelorarbeit_NT
{
    public abstract class Term
    {

        public enum TermType
        {
            Unknown,
            QuadraticTwo,
        }

        /// <summary>
        /// @TODO Weitere Enums setzen um damit halt die ganzen dinger unterscheiden zu können.
        /// </summary>
        public decimal alpha { get; set; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="alpha">Irrational Constant</param>
        public Term() { }

        /// <summary>
        /// Calculates Quadratic Form
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public abstract decimal CalcQuadratic(Coordinate c);
        public abstract string Alpha(Coordinate c);

    }
}
