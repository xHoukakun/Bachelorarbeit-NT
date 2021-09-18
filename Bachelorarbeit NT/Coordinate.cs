using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelorarbeit_NT
{
    public class Coordinate
    {
        private Term t { get; set; }
        public decimal x = 1;
        public decimal y = 1;

        public Coordinate(Term _t, decimal _x, decimal _y)
        {
            y = _y;
            x = _x;
            t = _t;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>quadratic form of coordinate</returns>
        public decimal Calc()
        {
            return t.CalcQuadratic(this);
        }
        public string Alpha()
        {
            return t.Alpha(this);
        }
    }
}
