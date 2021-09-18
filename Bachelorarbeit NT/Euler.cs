using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelorarbeit_NT
{/// <summary>
/// Implementation der Quadratischen form für alpha = e
/// </summary>
    public class Euler : Term
    {

        public Euler()
        {
            alpha = Convert.ToDecimal(Math.E);
        }

        public override decimal CalcQuadratic(Coordinate c)
        {
            return c.x * c.x + alpha * c.y * c.y; //diese Rechnung wird nachher von den Workern aufgerufen.
        }
        public override string Alpha(Coordinate c)
        {
            return "Euler";
        }
    }
}
