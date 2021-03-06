using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelorarbeit_NT
{
    public class Zeta3 : Term
    {
        /// <summary>
        /// Implementation für alpha gleich zeta(3)
        /// </summary>
        public Zeta3()
        {
            alpha = 1.2020569031595942853997381615114499907649862923404988817922715553M;
        }

        public override decimal CalcQuadratic(Coordinate c)
        {
            return c.x * c.x + alpha * c.y * c.y;
        }
        public override string Alpha(Coordinate c)
        {
            return "Zeta3";
        }
    }
}
