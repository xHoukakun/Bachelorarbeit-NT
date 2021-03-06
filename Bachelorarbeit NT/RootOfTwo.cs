using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelorarbeit_NT
{/// <summary>
/// Implementation der Quadratischen Form für wurzel 2
/// </summary>
    public class RootOfTwo : Term
    {

        public RootOfTwo()
        {
            alpha = Convert.ToDecimal(Math.Sqrt(2));
        }

        public override decimal CalcQuadratic(Coordinate c)
        {
            return c.x * c.x + alpha * c.y * c.y;
        }
        public override string Alpha(Coordinate c)
        {
            return "RootOfTwo";
        }
    }
}
